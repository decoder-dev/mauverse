import Combine
import Foundation

@MainActor
final class SessionStore: ObservableObject {
    @Published private(set) var user: UserDTO?
    @Published var isBusy = false
    @Published var errorMessage: String?

    private let storageKey = "mauverse.native.session"

    init() {
        if let data = UserDefaults.standard.data(forKey: storageKey) {
            user = try? JSONDecoder().decode(UserDTO.self, from: data)
        }
    }

    func signIn(username: String, password: String) async {
        guard !username.isEmpty, !password.isEmpty else {
            errorMessage = "Введите логин и пароль"
            return
        }
        isBusy = true
        errorMessage = nil
        defer { isBusy = false }
        do {
            let authenticated: UserDTO = try await APIClient.shared.post(
                "auth",
                body: LoginRequest(username: username, password: password)
            )
            if let error = authenticated.error ?? authenticated.detail {
                throw APIError.server(error)
            }
            user = authenticated
            persist()
        } catch {
            errorMessage = error.localizedDescription
        }
    }

    func update(_ transform: (inout UserDTO) -> Void) {
        guard var current = user else { return }
        transform(&current)
        user = current
        persist()
    }

    func signOut() {
        user = nil
        UserDefaults.standard.removeObject(forKey: storageKey)
    }

    private func persist() {
        guard let user, let data = try? JSONEncoder().encode(user) else { return }
        UserDefaults.standard.set(data, forKey: storageKey)
    }
}
