import Foundation

enum PhoneNumberFormatting {
    /// Murmansk city code used for local 6-digit university PBX numbers.
    static let murmanskAreaCode = "8152"

    /// Builds a `tel:` URL for Russian / Murmansk numbers.
    /// Local numbers like `21-38-81 (3045)` become `tel:+78152213881;ext=3045`.
    static func telURL(from raw: String) -> URL? {
        let parsed = parse(raw)
        guard let e164 = parsed.e164 else { return nil }
        if let extensionCode = parsed.extensionCode, !extensionCode.isEmpty {
            return URL(string: "tel:\(e164);ext=\(extensionCode)")
        }
        return URL(string: "tel:\(e164)")
    }

    static func parse(_ raw: String) -> (e164: String?, extensionCode: String?) {
        var working = raw.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !working.isEmpty else { return (nil, nil) }

        var extensionCode: String?
        if let match = working.range(of: #"\(\d{2,6}\)\s*$"#, options: .regularExpression) {
            extensionCode = String(working[match]).filter(\.isNumber)
            working.removeSubrange(match)
        } else if let match = working.range(
            of: #"(?i)(?:доб\.?|ext\.?|extension)\s*\d{2,6}\s*$"#,
            options: .regularExpression
        ) {
            extensionCode = String(working[match]).filter(\.isNumber)
            working.removeSubrange(match)
        }

        let digits = working.filter(\.isNumber)
        guard !digits.isEmpty else { return (nil, nil) }

        let e164: String?
        if digits.count == 6 {
            // Local Murmansk landline: 21-38-81 → +7 8152 213881
            e164 = "+7\(murmanskAreaCode)\(digits)"
        } else if digits.count == 10, digits.hasPrefix(murmanskAreaCode) || digits.hasPrefix("800") {
            e164 = "+7\(digits)"
        } else if digits.count == 11, digits.hasPrefix("8") {
            e164 = "+7\(digits.dropFirst())"
        } else if digits.count == 11, digits.hasPrefix("7") {
            e164 = "+\(digits)"
        } else if working.contains("+"), digits.count >= 11 {
            e164 = "+\(digits)"
        } else if digits.count >= 10 {
            e164 = digits.hasPrefix("7") ? "+\(digits)" : "+7\(digits)"
        } else {
            e164 = nil
        }

        return (e164, extensionCode)
    }
}
