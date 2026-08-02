import SwiftUI

struct CampusNavigatorView: View {
    @State private var searchText = ""

    private var filteredGroups: [CampusBuildingGroup] {
        let query = searchText.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !query.isEmpty else { return UniversityPortalCatalog.campusGroups }
        return UniversityPortalCatalog.campusGroups.compactMap { group in
            let buildings = group.buildings.filter {
                $0.title.localizedCaseInsensitiveContains(query)
                    || $0.address.localizedCaseInsensitiveContains(query)
            }
            guard !buildings.isEmpty else { return nil }
            return CampusBuildingGroup(
                title: group.title,
                transportTip: group.transportTip,
                buildings: buildings
            )
        }
    }

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Навигатор по корпусам")
                            .font(.system(size: 30, weight: .bold, design: .rounded))
                        Text("Маршрут в 2GIS и панорама Яндекс.Карт")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    TextField("Поиск корпуса или адреса", text: $searchText)
                        .textFieldStyle(.roundedBorder)

                    if let url = URL(string: UniversityPortalURLs.campusNavigatorSite) {
                        NavigationLink {
                            InAppBrowserView(url: url, title: "Навигатор на сайте")
                        } label: {
                            HStack {
                                Label("Официальный навигатор МАУ", systemImage: "map.fill")
                                    .font(.subheadline.weight(.semibold))
                                Spacer()
                                Image(systemName: "arrow.up.right")
                                    .font(.caption.bold())
                            }
                            .foregroundStyle(MauTheme.blue)
                            .padding(16)
                            .mauSurface(radius: 18)
                        }
                        .buttonStyle(.plain)
                    }

                    if filteredGroups.isEmpty {
                        EmptyState(
                            icon: "magnifyingglass",
                            title: "Ничего не найдено",
                            message: "Попробуйте другое название корпуса"
                        )
                    } else {
                        ForEach(filteredGroups) { group in
                            MauSectionHeader(title: group.title)
                            Text(group.transportTip)
                                .font(.caption)
                                .foregroundStyle(MauTheme.muted)
                                .padding(.bottom, 4)

                            ForEach(group.buildings) { building in
                                buildingCard(building)
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 30)
            }
        }
        .navigationTitle("Корпуса")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
    }

    private func buildingCard(_ building: CampusBuilding) -> some View {
        VStack(alignment: .leading, spacing: 12) {
            Text(building.title)
                .font(.headline)
                .foregroundStyle(MauTheme.ink)
            Text(building.address)
                .font(.subheadline)
                .foregroundStyle(MauTheme.muted)

            HStack(spacing: 10) {
                if let route = routeURL(for: building) {
                    Link(destination: route) {
                        Label("Маршрут", systemImage: "point.topleft.down.to.point.bottomright.curvepath")
                    }
                    .buttonStyle(.borderedProminent)
                    .controlSize(.small)
                }
                if let panorama = panoramaURL(for: building) {
                    Link(destination: panorama) {
                        Label("Панорама", systemImage: "pano.fill")
                    }
                    .buttonStyle(.bordered)
                    .controlSize(.small)
                }
            }
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(16)
        .mauSurface(radius: 18)
    }

    private func routeURL(for building: CampusBuilding) -> URL? {
        let encoded = building.searchQuery.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed)
            ?? building.searchQuery
        return URL(string: "https://2gis.ru/\(building.mapCity)/search/\(encoded)")
    }

    private func panoramaURL(for building: CampusBuilding) -> URL? {
        var components = URLComponents(string: "https://yandex.ru/maps/")
        components?.queryItems = [
            URLQueryItem(name: "text", value: building.searchQuery),
            URLQueryItem(name: "l", value: "stv")
        ]
        return components?.url
    }
}
