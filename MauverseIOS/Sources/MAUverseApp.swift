import SwiftUI

@main
struct MAUverseApp: App {
    @StateObject private var session = SessionStore()

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
            .preferredColorScheme(.light)
        }
    }
}

