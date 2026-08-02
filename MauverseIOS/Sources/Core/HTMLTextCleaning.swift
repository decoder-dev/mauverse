import Foundation

enum HTMLTextCleaning {
    /// Decodes HTML entities (`&thinsp;`, `&#160;`, …) and strips tags for RSS titles/bodies.
    static func cleanRSS(_ value: String) -> String {
        let decoded = decodeEntities(value)
        return decoded
            .replacingOccurrences(of: "<[^>]+>", with: " ", options: .regularExpression)
            .replacingOccurrences(of: "[\\u00A0\\u2000-\\u200B\\u202F\\u205F\\u3000]+", with: " ", options: .regularExpression)
            .replacingOccurrences(of: "\\s+", with: " ", options: .regularExpression)
            .trimmingCharacters(in: .whitespacesAndNewlines)
    }

    static func decodeEntities(_ value: String) -> String {
        guard !value.isEmpty else { return value }

        var result = value
        let named: [(String, String)] = [
            ("&nbsp;", " "),
            ("&thinsp;", " "),
            ("&ensp;", " "),
            ("&emsp;", " "),
            ("&mdash;", "—"),
            ("&ndash;", "–"),
            ("&laquo;", "«"),
            ("&raquo;", "»"),
            ("&quot;", "\""),
            ("&apos;", "'"),
            ("&#39;", "'"),
            ("&lt;", "<"),
            ("&gt;", ">"),
            ("&amp;", "&")
        ]
        // Decode named entities first; &amp; last among pairs that can nest.
        for (entity, replacement) in named where entity != "&amp;" {
            result = result.replacingOccurrences(of: entity, with: replacement, options: .caseInsensitive)
        }

        // Numeric decimal &#160; and hex &#x2009;
        result = replaceNumericEntities(result)
        result = result.replacingOccurrences(of: "&amp;", with: "&", options: .caseInsensitive)
        return result
    }

    private static func replaceNumericEntities(_ value: String) -> String {
        var output = value
        let patterns: [(String, (String) -> String?)] = [
            (#"&#(\d+);"#, { Int($0).flatMap(unicodeScalarString) }),
            (#"&#x([0-9a-fA-F]+);"#, { UInt32($0, radix: 16).flatMap(unicodeScalarString) })
        ]
        for (pattern, decoder) in patterns {
            guard let regex = try? NSRegularExpression(pattern: pattern) else { continue }
            let range = NSRange(output.startIndex..., in: output)
            let matches = regex.matches(in: output, range: range).reversed()
            for match in matches {
                guard match.numberOfRanges >= 2,
                      let full = Range(match.range(at: 0), in: output),
                      let body = Range(match.range(at: 1), in: output),
                      let replacement = decoder(String(output[body])) else { continue }
                output.replaceSubrange(full, with: replacement)
            }
        }
        return output
    }

    private static func unicodeScalarString(_ value: Int) -> String? {
        guard let scalar = UnicodeScalar(value) else { return nil }
        return String(Character(scalar))
    }

    private static func unicodeScalarString(_ value: UInt32) -> String? {
        guard let scalar = UnicodeScalar(value) else { return nil }
        return String(Character(scalar))
    }
}
