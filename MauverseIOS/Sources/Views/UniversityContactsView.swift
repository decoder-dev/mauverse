import SwiftUI
import UIKit

struct UniversityContactsView: View {
    @State private var copiedMessage: String?

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Контакты и реквизиты")
                            .font(.system(size: 30, weight: .bold, design: .rounded))
                        Text("Приёмная комиссия, филиалы и платежи")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    if let url = URL(string: UniversityPortalURLs.requisites) {
                        NavigationLink {
                            InAppBrowserView(url: url, title: "Адреса и реквизиты")
                        } label: {
                            HStack {
                                Label("Страница на сайте МАУ", systemImage: "safari.fill")
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

                    MauSectionHeader(title: "Приёмная комиссия")
                    ForEach(UniversityPortalCatalog.admissionContacts) { block in
                        contactCard(block)
                    }

                    MauSectionHeader(title: "Реквизиты университета")
                    ForEach(UniversityPortalCatalog.universityRequisites) { block in
                        contactCard(block)
                    }
                }
                .padding(20)
                .padding(.bottom, 30)
            }
        }
        .navigationTitle("Контакты")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
        .overlay(alignment: .bottom) {
            if let copiedMessage {
                Text(copiedMessage)
                    .font(.footnote.weight(.semibold))
                    .padding(.horizontal, 14)
                    .padding(.vertical, 10)
                    .background(.ultraThinMaterial, in: Capsule())
                    .padding(.bottom, 24)
                    .transition(.move(edge: .bottom).combined(with: .opacity))
            }
        }
        .animation(.easeInOut(duration: 0.2), value: copiedMessage)
    }

    private func contactCard(_ block: UniversityContactBlock) -> some View {
        VStack(alignment: .leading, spacing: 12) {
            Text(block.title)
                .font(.headline)
                .foregroundStyle(MauTheme.ink)
            Text(block.details)
                .font(.caption)
                .foregroundStyle(MauTheme.muted)
                .fixedSize(horizontal: false, vertical: true)

            HStack(spacing: 10) {
                if let phone = block.phone, let url = phoneURL(phone) {
                    Link(destination: url) {
                        Label("Позвонить", systemImage: "phone.fill")
                    }
                    .buttonStyle(.borderedProminent)
                    .controlSize(.small)
                }
                Button {
                    copyText(block.details, label: "Скопировано")
                } label: {
                    Label("Копировать", systemImage: "doc.on.doc")
                }
                .buttonStyle(.bordered)
                .controlSize(.small)
            }

            if let email = block.email {
                Button {
                    copyText(email, label: "E-mail скопирован")
                } label: {
                    Label(email, systemImage: "envelope")
                        .font(.caption.weight(.semibold))
                }
                .buttonStyle(.plain)
                .foregroundStyle(MauTheme.blue)
            }
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(16)
        .mauSurface(radius: 20)
    }

    private func phoneURL(_ phone: String) -> URL? {
        let digits = phone.filter { $0.isNumber || $0 == "+" }
        return URL(string: "tel:\(digits)")
    }

    private func copyText(_ value: String, label: String) {
        UIPasteboard.general.string = value
        copiedMessage = label
        Task {
            try? await Task.sleep(nanoseconds: 1_600_000_000)
            if copiedMessage == label { copiedMessage = nil }
        }
    }
}
