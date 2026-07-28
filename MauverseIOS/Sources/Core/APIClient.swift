import Foundation

enum APIError: LocalizedError {
    case invalidResponse
    case server(String)

    var errorDescription: String? {
        switch self {
        case .invalidResponse: "Сервер вернул некорректный ответ"
        case .server(let message): message
        }
    }
}

struct EmptyBody: Encodable {}

final class APIClient {
    static let shared = APIClient()
    private let baseURL = URL(string: "https://app.mauniver.ru/dev/mauverse/")!
    private let session: URLSession

    private init() {
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
        return try await execute(request)
    }

    func post<Response: Decodable, Body: Encodable>(
        _ path: String,
        body: Body,
        user: UserDTO? = nil
    ) async throws -> Response {
        var request = URLRequest(url: baseURL.appendingPathComponent(path))
        request.httpMethod = "POST"
        request.httpBody = try JSONEncoder().encode(body)
        addHeaders(to: &request, user: user)
        return try await execute(request)
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

    private func execute<Response: Decodable>(_ request: URLRequest) async throws -> Response {
        let (data, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else { throw APIError.invalidResponse }
        guard (200..<300).contains(http.statusCode) else {
            let detail = (try? decoder.decode(APIMessage.self, from: data))
            throw APIError.server(detail?.detail ?? detail?.error ?? "Ошибка сервера: \(http.statusCode)")
        }
        do {
            return try decoder.decode(Response.self, from: data)
        } catch {
            throw APIError.server("Не удалось прочитать ответ сервера")
        }
    }

    private var decoder: JSONDecoder {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return decoder
    }
}

struct LoginRequest: Encodable { let username: String; let password: String }
struct GroupRequest: Encodable { let groupName: String }
struct DepartmentRequest: Encodable { let departmentId: Int; let name: String }
struct TeacherRequest: Encodable { let name: String }
struct DebtRequest: Encodable { let creditBook: String }
struct ScheduleRequest: Encodable {
    let startDate: String
    let endDate: String
    let groupId: Int?
    let subgroupId: Int?
}

