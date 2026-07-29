import Foundation

actor OfficialNewsService {
    static let shared = OfficialNewsService()
    private var cache: [NewsFilter: [NewsItem]] = [:]

    private init() {}

    func load(filter: NewsFilter) async throws -> [NewsItem] {
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
            if !loaded.isEmpty { cache[filter] = loaded }
            return loaded
        } catch {
            if let cached = cache[filter], !cached.isEmpty {
                return cached
            }
            throw error
        }
    }

    private func fetch(url: URL, cachePolicy: URLRequest.CachePolicy) async throws -> [NewsItem] {
        var request = URLRequest(url: url, cachePolicy: cachePolicy, timeoutInterval: 30)
        request.timeoutInterval = 30
        request.setValue("MAUverse/1.9.2 (iOS)", forHTTPHeaderField: "User-Agent")
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
    }

    private static func isTransient(_ code: URLError.Code) -> Bool {
        return switch code {
        case .timedOut, .networkConnectionLost, .cannotConnectToHost, .dnsLookupFailed:
            true
        default:
            false
        }
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
            current?.image = url
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
        case "title": current?.title = value.cleanedRSS
        case "link": current?.link = value
        case "description": current?.description = value.cleanedRSS
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
}

private extension String {
    var cleanedRSS: String {
        replacingOccurrences(of: "<[^>]+>", with: " ", options: .regularExpression)
            .replacingOccurrences(of: "&nbsp;", with: " ")
            .replacingOccurrences(of: "&mdash;", with: "—")
            .replacingOccurrences(of: "&ndash;", with: "–")
            .replacingOccurrences(of: "&laquo;", with: "«")
            .replacingOccurrences(of: "&raquo;", with: "»")
            .replacingOccurrences(of: "&amp;", with: "&")
            .replacingOccurrences(of: "\\s+", with: " ", options: .regularExpression)
            .trimmingCharacters(in: .whitespacesAndNewlines)
    }
}
