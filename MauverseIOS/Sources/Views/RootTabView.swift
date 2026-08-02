import SwiftUI

struct RootTabView: View {
    var body: some View {
        TabView {
            NavigationStack { HomeView() }
                .tabItem { Label("Главная", systemImage: "house.fill") }
            NavigationStack { ScheduleView() }
                .tabItem { Label("Расписание", systemImage: "calendar") }
            NavigationStack { ServicesView() }
                .tabItem { Label("Сервисы", systemImage: "square.grid.2x2.fill") }
            NavigationStack { NewsView() }
                .tabItem { Label("Новости", systemImage: "newspaper.fill") }
            NavigationStack { ProfileView() }
                .tabItem { Label("Профиль", systemImage: "person.crop.circle.fill") }
        }
        .tint(MauTheme.blue)
    }
}

