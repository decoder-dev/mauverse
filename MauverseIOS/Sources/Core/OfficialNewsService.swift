import Foundation

actor OfficialNewsService {
    static let shared = OfficialNewsService()
    private var cache: [NewsFilter: [NewsItem]] = [:]
    private let cachePrefix = "mauverse.news.cache.v2."

    private init() {}

    func load(filter: NewsFilter) async throws -> [NewsItem] {
        if cache[filter] == nil, let persisted = persistedItems(for: filter) {
            cache[filter] = persisted
        }
        let path: String
        switch filter {
        case .all: path = "press/news/rss/"
        case .departments: path = "press/deps/rss/"
        case .sport: path = "press/sport/rss/"
        case .students: path = "press/community/rss/"
        case .science: path = "press/science/rss/"
        case .international: path = "press/inter/rss/"
        case .events: path = "press/events/rss/"
        case .other: path = "press/information/rss/"
        case .applicant: path = "abit/news/rss/"
        case .calendar: path = "press/calendar/rss/"
        }

        guard let url = URL(string: "https://mauniver.ru/\(path)") else {
            throw APIError.invalidResponse
        }
        do {
            let loaded: [NewsItem]
            do {
                loaded = try await fetch(url: url, cachePolicy: .reloadRevalidatingCacheData)
            } catch let error as URLError where Self.isTransient(error.code) {
                loaded = try await fetch(url: url, cachePolicy: .returnCacheDataElseLoad)
            }
            if !loaded.isEmpty {
                cache[filter] = loaded
                persist(loaded, for: filter)
            }
            return loaded
        } catch {
            if let cached = cache[filter], !cached.isEmpty {
                return cached
            }
            throw error
        }
    }

    func clearCache() {
        cache.removeAll()
        for filter in NewsFilter.allCases {
            UserDefaults.standard.removeObject(forKey: cachePrefix + String(filter.rawValue))
        }
    }

    private func fetch(url: URL, cachePolicy: URLRequest.CachePolicy) async throws -> [NewsItem] {
        var request = URLRequest(url: url, cachePolicy: cachePolicy, timeoutInterval: 30)
        request.timeoutInterval = 30
        request.setValue("MAUverse/1.12.3 (iOS)", forHTTPHeaderField: "User-Agent")
        request.setValue("application/rss+xml, application/xml, text/xml", forHTTPHeaderField: "Accept")

        let (data, response) = try await URLSession.shared.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw APIError.invalidResponse
        }
        guard (200..<300).contains(http.statusCode) else {
            throw APIError.server("Сайт МАУ временно недоступен: \(http.statusCode)")
        }

        let parser = RSSNewsParser()
        return try parser.parse(data)
            .uniqued(by: \.id)
            .filter { $0.title?.isEmpty == false && $0.link?.isEmpty == false }
    }

    private static func isTransient(_ code: URLError.Code) -> Bool {
        return switch code {
        case .timedOut, .networkConnectionLost, .cannotConnectToHost, .dnsLookupFailed:
            true
        default:
            false
        }
    }

    private func persistedItems(for filter: NewsFilter) -> [NewsItem]? {
        guard let data = UserDefaults.standard.data(forKey: cachePrefix + String(filter.rawValue))
        else { return nil }
        return try? JSONDecoder().decode([NewsItem].self, from: data)
    }

    private func persist(_ items: [NewsItem], for filter: NewsFilter) {
        guard let data = try? JSONEncoder().encode(items) else { return }
        UserDefaults.standard.set(data, forKey: cachePrefix + String(filter.rawValue))
    }
}

private final class RSSNewsParser: NSObject, XMLParserDelegate {
    private var items: [NewsItem] = []
    private var current: NewsItem?
    private var element = ""
    private var text = ""
    private var parsingError: Error?

    func parse(_ data: Data) throws -> [NewsItem] {
        let parser = XMLParser(data: data)
        parser.delegate = self
        guard parser.parse() else {
            throw parser.parserError ?? parsingError ?? APIError.invalidResponse
        }
        return items
    }

    func parser(
        _ parser: XMLParser,
        didStartElement elementName: String,
        namespaceURI: String?,
        qualifiedName qName: String?,
        attributes attributeDict: [String: String] = [:]
    ) {
        element = elementName
        text = ""
        if elementName == "item" {
            current = NewsItem(
                title: nil,
                description: nil,
                image: nil,
                link: nil,
                publish: nil
            )
        } else if elementName == "enclosure",
                  let url = attributeDict["url"],
                  attributeDict["type"]?.hasPrefix("image/") != false {
            current?.image = Self.absoluteURL(url)
        } else if (elementName == "media:content" || elementName == "media:thumbnail"),
                  let url = attributeDict["url"] {
            current?.image = Self.absoluteURL(url)
        }
    }

    func parser(_ parser: XMLParser, foundCharacters string: String) {
        text += string
    }

    func parser(
        _ parser: XMLParser,
        didEndElement elementName: String,
        namespaceURI: String?,
        qualifiedName qName: String?
    ) {
        guard current != nil else { return }
        let value = text.trimmingCharacters(in: .whitespacesAndNewlines)
        switch elementName {
        case "title": current?.title = HTMLTextCleaning.cleanRSS(value)
        case "link": current?.link = Self.absoluteURL(value)
        case "description":
            current?.description = HTMLTextCleaning.cleanRSS(value)
            if current?.image == nil {
                current?.image = Self.firstImageURL(in: value)
            }
        case "pubDate": current?.publish = Self.formattedDate(value)
        case "item":
            if let current { items.append(current) }
            current = nil
        default: break
        }
        text = ""
    }

    func parser(_ parser: XMLParser, parseErrorOccurred parseError: Error) {
        parsingError = parseError
    }

    private static func formattedDate(_ value: String) -> String {
        let input = DateFormatter()
        input.locale = Locale(identifier: "en_US_POSIX")
        input.dateFormat = "EEE, dd MMM yyyy HH:mm:ss Z"
        guard let date = input.date(from: value) else { return value }
        return date.formatted(
            .dateTime
                .locale(Locale(identifier: "ru_RU"))
                .day()
                .month(.wide)
                .year()
        )
    }

    private static func absoluteURL(_ value: String) -> String {
        guard let url = URL(string: value, relativeTo: URL(string: "https://mauniver.ru/")) else {
            return value
        }
        return url.absoluteURL.absoluteString
    }

    private static func firstImageURL(in html: String) -> String? {
        guard let expression = try? NSRegularExpression(
            pattern: #"<img[^>]+src=["']([^"']+)["']"#,
            options: [.caseInsensitive]
        ) else { return nil }
        let range = NSRange(html.startIndex..., in: html)
        guard let match = expression.firstMatch(in: html, range: range),
              let urlRange = Range(match.range(at: 1), in: html) else { return nil }
        return absoluteURL(String(html[urlRange]))
    }
}

private extension Array {
    func uniqued<Key: Hashable>(by key: (Element) -> Key) -> [Element] {
        var seen = Set<Key>()
        return filter { seen.insert(key($0)).inserted }
    }
}
