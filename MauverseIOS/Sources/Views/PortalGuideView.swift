import SwiftUI

struct PortalGuideView: View {
    let title: String
    let subtitle: String
    let sections: [PortalSection]

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    VStack(alignment: .leading, spacing: 6) {
                        Text(title)
                            .font(.system(size: 30, weight: .bold, design: .rounded))
                        Text(subtitle)
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    ForEach(sections) { section in
                        MauSectionHeader(title: section.title)
                        VStack(spacing: 10) {
                            ForEach(section.links) { item in
                                if let url = URL(string: item.url) {
                                    NavigationLink {
                                        InAppBrowserView(url: url, title: item.title)
                                    } label: {
                                        PortalLinkRow(link: item)
                                    }
                                    .buttonStyle(.plain)
                                }
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 30)
            }
        }
        .navigationTitle(title)
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
    }
}

struct PortalLinkRow: View {
    let link: PortalLink

    var body: some View {
        HStack(spacing: 14) {
            IconTile(systemName: link.systemImage, color: MauTheme.blue)
            VStack(alignment: .leading, spacing: 4) {
                Text(link.title)
                    .font(.subheadline.weight(.semibold))
                    .foregroundStyle(MauTheme.ink)
                    .multilineTextAlignment(.leading)
                Text(link.subtitle)
                    .font(.caption)
                    .foregroundStyle(MauTheme.muted)
                    .multilineTextAlignment(.leading)
            }
            Spacer(minLength: 8)
            Image(systemName: "chevron.right")
                .font(.caption.bold())
                .foregroundStyle(MauTheme.muted)
        }
        .padding(16)
        .mauSurface(radius: 20)
    }
}

struct DigitalServicesView: View {
    var body: some View {
        PortalGuideView(
            title: "Цифровые сервисы",
            subtitle: "ЭИОС, почта, библиотека и заявки МАУ",
            sections: UniversityPortalCatalog.digitalSections
        )
    }
}
