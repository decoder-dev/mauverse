import SwiftUI

@main
struct MAUverseApp: App {
    @StateObject private var session = SessionStore()
    @AppStorage("mauverse.appearance") private var appearance = AppTheme.system.rawValue

    var body: some Scene {
        WindowGroup {
            Group {
                if session.user == nil {
                    LoginView()
                } else {
                    RootTabView()
                }
            }
            .environmentObject(session)
            .preferredColorScheme(AppTheme(rawValue: appearance)?.colorScheme)
        }
    }
}
