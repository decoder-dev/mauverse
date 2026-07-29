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
    var groupId: Int?
    var subGroupId: Int?
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
            startTime: times.first,
            endTime: times.count > 1 ? times[1] : nil,
            date: date,
            externalId: nil
        )
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

struct ScheduleGroup: Decodable, Identifiable, Hashable {
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

struct Department: Codable, Identifiable, Hashable {
    var name: String?
    var id: Int?
}

struct Telephone: Codable, Identifiable, Hashable {
    var title: String?
    var person: String?
    var phone: String?
    var phone2: String?
    var fax: String?
    var building2: String?
    var room: String?
    var depEmail: String?
    var id: String { "\(title ?? "")-\(person ?? "")-\(phone ?? "")" }
}

struct Debt: Codable, Identifiable, Hashable {
    var firstName: String?
    var surname: String?
    var lastName: String?
    var group: String?
    var discipline: String?
    var markType: String?
    var id: String { "\(discipline ?? "")-\(markType ?? "")-\(group ?? "")" }
}

struct Semester: Codable, Identifiable, Hashable {
    var semesterNumber: Int?
    var semester: String?
    var semesterSubtitle: String?
    var debts: [Debt]?
    var id: String { "\(semesterNumber ?? 0)-\(semester ?? "")" }
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
