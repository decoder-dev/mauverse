import Foundation

enum UserRole: Int, Codable {
    case all = -1
    case student = 0
    case teacher = 1
}

struct UserDTO: Codable, Equatable {
    var userId: Int?
    var username: String?
    var firstName: String?
    var fullName: String?
    var role: Int?
    var creditBook: String?
    var groupId: String?
    var subGroupId: String?
    var scheduleGroupUID: String?
    var groupName: String?
    var speciality: String?
    var token: String?
    var privateToken: String?
    var error: String?
    var detail: String?

    var displayName: String {
        let value = fullName ?? firstName ?? username ?? "Студент"
        return value.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty ? "Студент" : value
    }

    enum CodingKeys: String, CodingKey {
        case userId = "userid"
        case username
        case firstName = "firstname"
        case fullName = "fullname"
        case role = "roleid"
        case creditBook = "credit_book"
        case groupId = "group_id"
        case subGroupId = "subgroup_id"
        case scheduleGroupUID
        case groupName = "groupname"
        case speciality
        case token
        case privateToken = "private_token"
        case error
        case detail
        case legacyUserId = "userId"
        case legacyFirstName = "firstName"
        case legacyFullName = "fullName"
        case legacyRole = "role"
        case legacyCreditBook = "creditBook"
        case legacyGroupId = "groupId"
        case apiGroupId = "groupid"
        case legacySubGroupId = "subGroupId"
        case legacyGroupName = "groupName"
        case legacyPrivateToken = "privateToken"
    }

    init(
        userId: Int? = nil,
        username: String? = nil,
        firstName: String? = nil,
        fullName: String? = nil,
        role: Int? = nil,
        creditBook: String? = nil,
        groupId: String? = nil,
        subGroupId: String? = nil,
        scheduleGroupUID: String? = nil,
        groupName: String? = nil,
        speciality: String? = nil,
        token: String? = nil,
        privateToken: String? = nil,
        error: String? = nil,
        detail: String? = nil
    ) {
        self.userId = userId
        self.username = username
        self.firstName = firstName
        self.fullName = fullName
        self.role = role
        self.creditBook = creditBook
        self.groupId = groupId
        self.subGroupId = subGroupId
        self.scheduleGroupUID = scheduleGroupUID
        self.groupName = groupName
        self.speciality = speciality
        self.token = token
        self.privateToken = privateToken
        self.error = error
        self.detail = detail
    }

    init(from decoder: Decoder) throws {
        let values = try decoder.container(keyedBy: CodingKeys.self)
        userId = values.decodeLossyIntIfPresent(forKey: .userId)
            ?? values.decodeLossyIntIfPresent(forKey: .legacyUserId)
        username = try values.decodeIfPresent(String.self, forKey: .username)
        firstName = try values.decodeIfPresent(String.self, forKey: .firstName)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyFirstName)
        fullName = try values.decodeIfPresent(String.self, forKey: .fullName)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyFullName)
        role = values.decodeLossyIntIfPresent(forKey: .role)
            ?? values.decodeLossyIntIfPresent(forKey: .legacyRole)
        creditBook = values.decodeLossyStringIfPresent(forKey: .creditBook)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyCreditBook)
        groupId = values.decodeLossyStringIfPresent(forKey: .groupId)
            ?? values.decodeLossyStringIfPresent(forKey: .apiGroupId)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyGroupId)
        subGroupId = values.decodeLossyStringIfPresent(forKey: .subGroupId)
            ?? values.decodeLossyStringIfPresent(forKey: .legacySubGroupId)
        scheduleGroupUID = try values.decodeIfPresent(String.self, forKey: .scheduleGroupUID)
            ?? values.decodeLossyStringIfPresent(forKey: .apiGroupId)
        groupName = try values.decodeIfPresent(String.self, forKey: .groupName)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyGroupName)
        speciality = try values.decodeIfPresent(String.self, forKey: .speciality)
        token = try values.decodeIfPresent(String.self, forKey: .token)
        privateToken = try values.decodeIfPresent(String.self, forKey: .privateToken)
            ?? values.decodeLossyStringIfPresent(forKey: .legacyPrivateToken)
        error = try values.decodeIfPresent(String.self, forKey: .error)
        detail = try values.decodeIfPresent(String.self, forKey: .detail)
    }

    func encode(to encoder: Encoder) throws {
        var values = encoder.container(keyedBy: CodingKeys.self)
        try values.encodeIfPresent(userId, forKey: .userId)
        try values.encodeIfPresent(username, forKey: .username)
        try values.encodeIfPresent(firstName, forKey: .firstName)
        try values.encodeIfPresent(fullName, forKey: .fullName)
        try values.encodeIfPresent(role, forKey: .role)
        try values.encodeIfPresent(creditBook, forKey: .creditBook)
        try values.encodeIfPresent(groupId, forKey: .groupId)
        try values.encodeIfPresent(subGroupId, forKey: .subGroupId)
        try values.encodeIfPresent(scheduleGroupUID, forKey: .scheduleGroupUID)
        try values.encodeIfPresent(groupName, forKey: .groupName)
        try values.encodeIfPresent(speciality, forKey: .speciality)
        try values.encodeIfPresent(token, forKey: .token)
        try values.encodeIfPresent(privateToken, forKey: .privateToken)
        try values.encodeIfPresent(error, forKey: .error)
        try values.encodeIfPresent(detail, forKey: .detail)
    }
}

struct ScheduleItem: Codable, Identifiable, Hashable {
    var id: Int?
    var name: String?
    var teacher: String?
    var room: String?
    var pairType: String?
    var startTime: String?
    var endTime: String?
    var date: String?
    var externalId: String?

    var stableID: String {
        externalId ?? "\(id ?? 0)-\(date ?? "")-\(startTime ?? "")-\(name ?? "")"
    }
}

struct ScheduleResponse: Decodable {
    let update: String?
    let success: Bool?
    let timetable: [ScheduleAPIItem]
}

struct ScheduleAPIItem: Decodable {
    let id: Int?
    let date: String?
    let slot: String?
    let dayOfWeek: String?
    let type: String?
    let disciplines: String?
    let room: String?
    let teacher: String?

    enum CodingKeys: String, CodingKey {
        case id, date, slot, type, disciplines, room, teacher
        case dayOfWeek = "day_of_week"
    }

    var appItem: ScheduleItem {
        let times = (slot ?? "").components(separatedBy: " - ")
        return ScheduleItem(
            id: id,
            name: disciplines,
            teacher: teacher,
            room: room,
            pairType: type,
            startTime: Self.normalizedTime(times.first),
            endTime: times.count > 1 ? Self.normalizedTime(times[1]) : nil,
            date: date,
            externalId: nil
        )
    }

    private static func normalizedTime(_ value: String?) -> String? {
        let trimmed = value?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
        guard !trimmed.isEmpty else { return nil }
        let parts = trimmed.split(separator: ":")
        guard parts.count >= 2, let hour = Int(parts[0]), let minute = Int(parts[1]) else {
            return trimmed
        }
        return String(format: "%02d:%02d", hour, minute)
    }
}

struct ScheduleFacultyResponse: Decodable {
    let success: Bool?
    let courses: [ScheduleFaculty]
}

struct ScheduleFaculty: Decodable, Identifiable {
    let facId: Int
    let facultee: String
    var id: Int { facId }

    enum CodingKeys: String, CodingKey {
        case facId = "fac_id"
        case facultee
    }
}

struct ScheduleGroupsResponse: Decodable {
    let success: Bool?
    let groups: [ScheduleGroup]
}

struct ScheduleGroup: Codable, Identifiable, Hashable {
    let groupId: Int
    let group: String
    let speciality: String?
    let uid: String
    var id: Int { groupId }

    enum CodingKeys: String, CodingKey {
        case groupId = "group_id"
        case group, speciality
        case uid = "UID"
    }
}

struct ScheduleTeachersResponse: Decodable {
    let success: Bool?
    let teachers: [ScheduleTeacher]
}

struct ScheduleTeacher: Decodable, Identifiable {
    let teacherId: Int
    let teacher: String
    let uid: String
    var id: Int { teacherId }

    enum CodingKeys: String, CodingKey {
        case teacherId = "teacher_id"
        case teacher
        case uid = "UID"
    }
}

struct NewsItem: Codable, Identifiable, Hashable {
    var title: String?
    var description: String?
    var image: String?
    var link: String?
    var publish: String?

    var id: String { link ?? "\(title ?? "")-\(publish ?? "")" }
}

struct GroupDTO: Codable, Identifiable, Hashable {
    var groupId: Int?
    var groupName: String?
    var name: String?
    var speciality: String?
    var id: String { "\(groupId ?? 0)-\(groupName ?? name ?? "")" }
    var title: String { groupName ?? name ?? "Группа" }
}

struct SubGroupItem: Codable, Identifiable, Hashable {
    var groupId: Int?
    var name: String?
    var id: String { "\(groupId ?? 0)-\(name ?? "")" }
}

struct SubGroupDTO: Codable {
    var groupId: Int?
    var speciality: String?
    var subGroups: [SubGroupItem]?
}

struct Department: Decodable, Identifiable, Hashable {
    var name: String?
    var id: Int?

    enum CodingKeys: String, CodingKey {
        case name, title, id, departmentId
    }

    init(from decoder: Decoder) throws {
        let values = try decoder.container(keyedBy: CodingKeys.self)
        name = try values.decodeIfPresent(String.self, forKey: .name)
            ?? values.decodeLossyStringIfPresent(forKey: .title)
        id = values.decodeLossyIntIfPresent(forKey: .id)
            ?? values.decodeLossyIntIfPresent(forKey: .departmentId)
    }
}

struct Telephone: Decodable, Identifiable, Hashable {
    var title: String?
    var person: String?
    var phone: String?
    var phone2: String?
    var fax: String?
    var building2: String?
    var room: String?
    var depEmail: String?
    var id: String { "\(title ?? "")-\(person ?? "")-\(phone ?? "")" }

    enum CodingKeys: String, CodingKey {
        case title, post, person, name, phone, telephone, phone2, fax, building2, building, room
        case depEmail, email
    }

    init(from decoder: Decoder) throws {
        let values = try decoder.container(keyedBy: CodingKeys.self)
        title = try values.decodeIfPresent(String.self, forKey: .title)
            ?? values.decodeLossyStringIfPresent(forKey: .post)
        person = try values.decodeIfPresent(String.self, forKey: .person)
            ?? values.decodeLossyStringIfPresent(forKey: .name)
        phone = try values.decodeIfPresent(String.self, forKey: .phone)
            ?? values.decodeLossyStringIfPresent(forKey: .telephone)
        phone2 = try values.decodeIfPresent(String.self, forKey: .phone2)
        fax = try values.decodeIfPresent(String.self, forKey: .fax)
        building2 = try values.decodeIfPresent(String.self, forKey: .building2)
            ?? values.decodeLossyStringIfPresent(forKey: .building)
        room = try values.decodeIfPresent(String.self, forKey: .room)
        depEmail = try values.decodeIfPresent(String.self, forKey: .depEmail)
            ?? values.decodeLossyStringIfPresent(forKey: .email)
    }
}

struct Debt: Codable, Identifiable, Hashable {
    var firstName: String?
    var surname: String?
    var lastName: String?
    var group: String?
    var discipline: String?
    var markType: String?
    var id: String { "\(discipline ?? "")-\(markType ?? "")-\(group ?? "")" }

    enum CodingKeys: String, CodingKey {
        case firstName = "firstname"
        case surname
        case lastName = "lastname"
        case group
        case discipline
        case markType = "marktype"
    }
}

struct Semester: Codable, Identifiable, Hashable {
    var semesterNumber: Int?
    var semester: String?
    var semesterSubtitle: String?
    var debts: [Debt]?
    var id: String { "\(semesterNumber ?? 0)-\(semester ?? "")" }

    enum CodingKeys: String, CodingKey {
        case semesterNumber = "semesternumber"
        case semester
        case semesterSubtitle
        case debts
    }
}

struct SemesterResponse: Codable {
    var semesters: [Semester]?
    var error: String?
    var detail: String?
}

struct DebtResponse: Codable {
    var debts: [Debt]?
    var error: String?
    var detail: String?
}

struct StudentFormField: Codable, Identifiable, Hashable {
    let title: String
    let value: String
    var id: String { title }
}

struct StudentFormRequest: Encodable {
    let sender: String
    let username: String
    let text: [StudentFormField]

    enum CodingKeys: String, CodingKey {
        case sender = "from"
        case username
        case text
    }
}

struct SuccessResponse: Codable {
    var success: Bool?
    var error: String?
    var detail: String?
}

struct Teacher: Codable, Identifiable, Hashable {
    var id: Int?
    var teacherId: Int?
    var name: String?
    var fullName: String?
    var email: String?
    var phone: String?
    var post: String?
    var extras: String?
    var stableID: String { "\(teacherId ?? id ?? 0)-\(fullName ?? name ?? "")" }
}

struct APIMessage: Codable {
    var error: String?
    var detail: String?
    var statusCode: Int?
}

enum NewsFilter: Int, CaseIterable, Identifiable {
    case all = 0, departments, sport, students, science, international, events, other
    var id: Int { rawValue }
    var title: String {
        return switch self {
        case .all: "Все"
        case .departments: "Подразделения"
        case .sport: "Спорт"
        case .students: "Студентам"
        case .science: "Наука"
        case .international: "Международное"
        case .events: "События"
        case .other: "Другое"
        }
    }
}

private extension KeyedDecodingContainer {
    func decodeLossyStringIfPresent(forKey key: Key) -> String? {
        if let value = try? decode(String.self, forKey: key) {
            return value
        }
        if let value = try? decode(Int.self, forKey: key) {
            return String(value)
        }
        return nil
    }

    func decodeLossyIntIfPresent(forKey key: Key) -> Int? {
        if let value = try? decode(Int.self, forKey: key) {
            return value
        }
        if let value = try? decode(String.self, forKey: key) {
            return Int(value)
        }
        return nil
    }
}
