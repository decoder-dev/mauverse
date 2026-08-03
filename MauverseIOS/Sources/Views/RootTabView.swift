import SwiftUI

struct RootTabView: View {
    @State private var selectedTab = 0

    var body: some View {
        TabView(selection: $selectedTab) {
            NavigationStack { HomeView(selectedTab: $selectedTab) }
                .tabItem { Label("Главная", systemImage: "house.fill") }
                .tag(0)
            NavigationStack { ScheduleView() }
                .tabItem { Label("Расписание", systemImage: "calendar") }
                .tag(1)
            NavigationStack { ServicesView() }
                .tabItem { Label("Сервисы", systemImage: "square.grid.2x2.fill") }
                .tag(2)
            NavigationStack { NewsView() }
                .tabItem { Label("Новости", systemImage: "newspaper.fill") }
                .tag(3)
            NavigationStack { ProfileView() }
                .tabItem { Label("Профиль", systemImage: "person.crop.circle.fill") }
                .tag(4)
        }
        .tint(MauTheme.blue)
        .sensoryFeedback(.selection, trigger: selectedTab)
        .animation(MauMotion.soft, value: selectedTab)
    }
}
