import Foundation

enum APIError: LocalizedError {
    case invalidResponse
    case server(String)
    case network(String)

    var errorDescription: String? {
        return switch self {
        case .invalidResponse: "Сервер вернул некорректный ответ"
        case .server(let message): message
        case .network(let message): message
        }
    }
}

extension Notification.Name {
    static let mauverseSessionExpired = Notification.Name("mauverse.session.expired")
}

final class APIClient {
    static let shared = APIClient()
    private let baseURL: URL
    private let session: URLSession

    private init() {
        if let configured = Bundle.main.object(forInfoDictionaryKey: "MAUVERSE_API_BASE_URL") as? String,
           let url = URL(string: configured), !configured.isEmpty {
            baseURL = url
        } else {
            baseURL = URL(string: "https://app.mauniver.ru/dev/mauverse/")!
        }
        let configuration = URLSessionConfiguration.default
        configuration.timeoutIntervalForRequest = 30
        configuration.waitsForConnectivity = true
        session = URLSession(configuration: configuration)
    }

    func get<Response: Decodable>(
        _ path: String,
        query: [URLQueryItem] = [],
        user: UserDTO? = nil
    ) async throws -> Response {
        var components = URLComponents(url: baseURL.appendingPathComponent(path), resolvingAgainstBaseURL: false)!
        components.queryItems = query.isEmpty ? nil : query
        var request = URLRequest(url: components.url!)
        request.httpMethod = "GET"
        addHeaders(to: &request, user: user)
        return try await execute(request, retryOnTransient: true)
    }

    func post<Response: Decodable, Body: Encodable>(
        _ path: String,
        body: Body,
        user: UserDTO? = nil,
        retryOnTransient: Bool = false
    ) async throws -> Response {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "POST"
        let encoder = JSONEncoder()
        encoder.keyEncodingStrategy = .convertToSnakeCase
        request.httpBody = try encoder.encode(body)
        addHeaders(to: &request, user: user)
        return try await execute(request, retryOnTransient: retryOnTransient)
    }

    func post<Response: Decodable>(
        _ path: String,
        user: UserDTO? = nil,
        retryOnTransient: Bool = false
    ) async throws -> Response {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "POST"
        addHeaders(to: &request, user: user)
        return try await execute(request, retryOnTransient: retryOnTransient)
    }

    private func addHeaders(to request: inout URLRequest, user: UserDTO?) {
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue("application/json", forHTTPHeaderField: "Accept")
        if let token = user?.token ?? user?.privateToken {
            request.setValue(token, forHTTPHeaderField: "X-Auth-Token")
        }
        if let username = user?.username {
            request.setValue(username, forHTTPHeaderField: "X-Auth-Username")
        }
    }

    private func execute<Response: Decodable>(
        _ request: URLRequest,
        retryOnTransient: Bool
    ) async throws -> Response {
        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await performRequest(
                request,
                retryOnTransient: retryOnTransient
            )
        } catch let error as URLError {
            throw APIError.network(Self.networkMessage(for: error))
        }
        guard let http = response as? HTTPURLResponse else { throw APIError.invalidResponse }
        if http.statusCode == 401 {
            if request.value(forHTTPHeaderField: "X-Auth-Token") != nil {
                NotificationCenter.default.post(name: .mauverseSessionExpired, object: nil)
                throw APIError.server("Сессия истекла. Войдите в аккаунт повторно")
            }
        }
        guard (200..<300).contains(http.statusCode) else {
            throw APIError.server(Self.serverMessage(status: http.statusCode, data: data))
        }
        guard !data.isEmpty else { throw APIError.invalidResponse }
        do {
            return try decoder.decode(Response.self, from: data)
        } catch {
            throw APIError.server("Сервис вернул данные в новом формате. Обновите приложение")
        }
    }

    private func performRequest(
        _ request: URLRequest,
        retryOnTransient: Bool
    ) async throws -> (Data, URLResponse) {
        let attempts = retryOnTransient ? 2 : 1
        var lastError: Error?
        for attempt in 0..<attempts {
            do {
                let result = try await session.data(for: request)
                if retryOnTransient,
                   attempt + 1 < attempts,
                   let http = result.1 as? HTTPURLResponse,
                   [502, 503, 504].contains(http.statusCode) {
                    try await Task.sleep(for: .milliseconds(350))
                    continue
                }
                return result
            } catch let error as URLError where retryOnTransient && Self.isTransient(error.code) {
                lastError = error
                if attempt + 1 < attempts {
                    try await Task.sleep(for: .milliseconds(350))
                }
            }
        }
        throw lastError ?? APIError.invalidResponse
    }

    fileprivate static func serverMessage(status: Int, data: Data) -> String {
        if let object = try? JSONSerialization.jsonObject(with: data) as? [String: Any] {
            if let detail = object["detail"] as? String, !detail.isEmpty { return detail }
            if let error = object["error"] as? String, !error.isEmpty { return error }
            if let validation = object["detail"] as? [[String: Any]] {
                let messages = validation.compactMap { $0["msg"] as? String }
                if !messages.isEmpty {
                    return "Проверьте введённые данные: \(messages.joined(separator: ", "))"
                }
            }
        }
        return switch status {
        case 400: "Сервер не принял запрос"
        case 403: "Недостаточно прав для просмотра этих данных"
        case 404: "Запрошенные данные не найдены"
        case 422: "Проверьте заполнение обязательных полей"
        case 429: "Слишком много запросов. Попробуйте немного позже"
        case 500...599: "Сервис МАУ временно недоступен"
        default: "Ошибка сервера: \(status)"
        }
    }

    private static func networkMessage(for error: URLError) -> String {
        return switch error.code {
        case .notConnectedToInternet: "Нет подключения к интернету"
        case .timedOut: "Сервер не ответил вовремя"
        case .cannotFindHost, .cannotConnectToHost, .dnsLookupFailed:
            "Не удалось подключиться к сервису МАУ"
        case .cancelled: "Запрос отменён"
        default: "Ошибка сети. Попробуйте ещё раз"
        }
    }

    private static func isTransient(_ code: URLError.Code) -> Bool {
        [.timedOut, .networkConnectionLost, .cannotConnectToHost, .dnsLookupFailed]
            .contains(code)
    }

    private var decoder: JSONDecoder {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }
}

actor ScheduleAPIClient {
    static let shared = ScheduleAPIClient()
    private let baseURL = URL(string: "https://api-schedule.mauniver.ru/")!
    private let session: URLSession
    private var groupsCache: [ScheduleGroup]?
    private var scheduleCache: [String: [ScheduleItem]] = [:]
    private let groupsCacheKey = "mauverse.schedule.groups.v1"
    private let groupsCacheDateKey = "mauverse.schedule.groups.date.v1"
    private let scheduleCachePrefix = "mauverse.schedule.items.v1."

    private init() {
        let configuration = URLSessionConfiguration.default
        configuration.timeoutIntervalForRequest = 30
        configuration.waitsForConnectivity = true
        session = URLSession(configuration: configuration)
    }

    func schedule(uid: String, start: String, end: String) async throws -> [ScheduleItem] {
        let key = "\(uid)|\(start)|\(end)"
        do {
            let response: ScheduleResponse = try await get(
                "groups/\(escaped(uid))/schedule/\(start)/\(end)"
            )
            guard response.success != false else {
                throw APIError.server("Расписание временно не готово")
            }
            let result = response.timetable.map(\.appItem)
            scheduleCache[key] = result
            if let data = try? JSONEncoder().encode(result) {
                UserDefaults.standard.set(data, forKey: persistedScheduleKey(key))
            }
            return result
        } catch {
            if let cached = scheduleCache[key] { return cached }
            if let data = UserDefaults.standard.data(forKey: persistedScheduleKey(key)),
               let cached = try? JSONDecoder().decode([ScheduleItem].self, from: data) {
                scheduleCache[key] = cached
                return cached
            }
            throw error
        }
    }

    func groups() async throws -> [ScheduleGroup] {
        if let groupsCache { return groupsCache }
        if let cached = persistedGroups(), isGroupsCacheFresh {
            groupsCache = cached
            return cached
        }
        let faculties: ScheduleFacultyResponse = try await get("faculties")
        var result: [ScheduleGroup] = []
        for faculty in faculties.courses {
            let response: ScheduleGroupsResponse = try await get(
                "faculties/\(faculty.facId)/groups/main"
            )
            result.append(contentsOf: response.groups)
        }
        let sorted = result.sorted {
            $0.group.localizedStandardCompare($1.group) == .orderedAscending
        }
        groupsCache = sorted
        if let data = try? JSONEncoder().encode(sorted) {
            UserDefaults.standard.set(data, forKey: groupsCacheKey)
            UserDefaults.standard.set(Date(), forKey: groupsCacheDateKey)
        }
        return sorted
    }

    func findGroup(named name: String) async throws -> ScheduleGroup? {
        let normalized = name.trimmingCharacters(in: .whitespacesAndNewlines)
        return try await groups().first {
            $0.group.compare(normalized, options: [.caseInsensitive, .diacriticInsensitive]) == .orderedSame
        }
    }

    func teachers(matching name: String) async throws -> [ScheduleTeacher] {
        let query = [URLQueryItem(name: "name", value: name)]
        let response: ScheduleTeachersResponse = try await get("teachers/search", query: query)
        return response.teachers
    }

    func clearCache() {
        groupsCache = nil
        scheduleCache.removeAll()
        UserDefaults.standard.removeObject(forKey: groupsCacheKey)
        UserDefaults.standard.removeObject(forKey: groupsCacheDateKey)
        for key in UserDefaults.standard.dictionaryRepresentation().keys
        where key.hasPrefix(scheduleCachePrefix) {
            UserDefaults.standard.removeObject(forKey: key)
        }
    }

    private func get<Response: Decodable>(
        _ path: String,
        query: [URLQueryItem] = []
    ) async throws -> Response {
        guard let token = Bundle.main.object(forInfoDictionaryKey: "MAUverseScheduleToken") as? String,
              !token.isEmpty,
              !token.contains("$(") else {
            throw APIError.server("Токен API расписания не настроен")
        }
        var components = URLComponents(
            url: baseURL.appendingPathComponent(path),
            resolvingAgainstBaseURL: false
        )!
        components.queryItems = query.isEmpty ? nil : query
        var request = URLRequest(url: components.url!)
        request.httpMethod = "GET"
        request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Accept")

        let data: Data
        let response: URLResponse
        do {
            (data, response) = try await scheduleData(for: request)
        } catch let error as URLError {
            throw APIError.network(APIClientNetworkMessage.message(for: error))
        }
        guard let http = response as? HTTPURLResponse else { throw APIError.invalidResponse }
        guard (200..<300).contains(http.statusCode) else {
            let message = switch http.statusCode {
            case 401, 403: "Токен API расписания недействителен"
            case 404: "Данные расписания не найдены"
            default: APIClient.serverMessage(status: http.statusCode, data: data)
            }
            throw APIError.server(message)
        }
        do {
            let decoder = JSONDecoder()
            return try decoder.decode(Response.self, from: data)
        } catch {
            throw APIError.server("API расписания вернул данные в новом формате")
        }
    }

    private func scheduleData(for request: URLRequest) async throws -> (Data, URLResponse) {
        var lastError: Error?
        for attempt in 0..<2 {
            do {
                let result = try await session.data(for: request)
                if attempt == 0,
                   let http = result.1 as? HTTPURLResponse,
                   [502, 503, 504].contains(http.statusCode) {
                    try await Task.sleep(for: .milliseconds(350))
                    continue
                }
                return result
            } catch let error as URLError where [
                .timedOut, .networkConnectionLost, .cannotConnectToHost, .dnsLookupFailed
            ].contains(error.code) {
                lastError = error
                if attempt == 0 { try await Task.sleep(for: .milliseconds(350)) }
            }
        }
        throw lastError ?? APIError.invalidResponse
    }

    private var isGroupsCacheFresh: Bool {
        guard let date = UserDefaults.standard.object(forKey: groupsCacheDateKey) as? Date else {
            return false
        }
        return Date().timeIntervalSince(date) < 6 * 60 * 60
    }

    private func persistedGroups() -> [ScheduleGroup]? {
        guard let data = UserDefaults.standard.data(forKey: groupsCacheKey) else { return nil }
        return try? JSONDecoder().decode([ScheduleGroup].self, from: data)
    }

    private func persistedScheduleKey(_ key: String) -> String {
        scheduleCachePrefix + key.replacingOccurrences(of: "|", with: ".")
    }

    private func escaped(_ value: String) -> String {
        value.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? value
    }
}

private enum APIClientNetworkMessage {
    static func message(for error: URLError) -> String {
        return switch error.code {
        case .notConnectedToInternet: "Нет подключения к интернету"
        case .timedOut: "API расписания не ответил вовремя"
        case .cannotFindHost, .cannotConnectToHost, .dnsLookupFailed:
            "Не удалось подключиться к API расписания"
        default: "Ошибка сети при загрузке расписания"
        }
    }
}

struct LoginRequest: Encodable { let username: String; let password: String }
struct GroupRequest: Encodable { let groupName: String }
struct DepartmentRequest: Encodable { let departmentId: Int; let name: String }
struct TeacherRequest: Encodable { let name: String }
struct DebtRequest: Encodable {
    let creditBook: String
    var semesterNumber: Int? = nil
}
struct ScheduleRequest: Encodable {
    let startDate: String
    let endDate: String
    let groupId: Int?
    let subgroupId: Int?
}
