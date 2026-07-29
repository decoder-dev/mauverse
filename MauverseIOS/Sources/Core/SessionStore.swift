import Combine
import Foundation

@MainActor
final class SessionStore: ObservableObject {
    @Published private(set) var user: UserDTO?
    @Published var isBusy = false
    @Published var errorMessage: String?

    private let storageKey = "mauverse.native.session"
    private var sessionObserver: NSObjectProtocol?

    init() {
        if let data = KeychainStore.read(account: storageKey) {
            user = try? JSONDecoder().decode(UserDTO.self, from: data)
        } else if let legacy = UserDefaults.standard.data(forKey: storageKey) {
            user = try? JSONDecoder().decode(UserDTO.self, from: legacy)
            if user != nil {
                KeychainStore.write(legacy, account: storageKey)
                UserDefaults.standard.removeObject(forKey: storageKey)
            }
        }
        sessionObserver = NotificationCenter.default.addObserver(
            forName: .mauverseSessionExpired,
            object: nil,
            queue: .main
        ) { [weak self] _ in
            Task { @MainActor in
                self?.signOut()
                self?.errorMessage = "Сессия истекла. Войдите в аккаунт повторно"
            }
        }
    }

    deinit {
        if let sessionObserver {
            NotificationCenter.default.removeObserver(sessionObserver)
        }
    }

    func signIn(username: String, password: String) async {
        let normalizedUsername = username.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !normalizedUsername.isEmpty, !password.isEmpty else {
            errorMessage = "Введите логин и пароль"
            return
        }
        isBusy = true
        errorMessage = nil
        defer { isBusy = false }
        do {
            var authenticated: UserDTO = try await APIClient.shared.post(
                "auth",
                body: LoginRequest(username: normalizedUsername, password: password)
            )
            guard authenticated.token?.isEmpty == false else {
                throw APIError.server(
                    authenticated.detail
                    ?? authenticated.error
                    ?? "Сервер не вернул токен авторизации"
                )
            }
            authenticated.username = authenticated.username ?? normalizedUsername
            if authenticated.scheduleGroupUID?.isEmpty != false {
                authenticated.scheduleGroupUID = authenticated.groupId
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
        KeychainStore.delete(account: storageKey)
        UserDefaults.standard.removeObject(forKey: storageKey)
    }

    private func persist() {
        guard let user, let data = try? JSONEncoder().encode(user) else { return }
        KeychainStore.write(data, account: storageKey)
    }
}
