import SwiftUI

enum AppTheme: String, CaseIterable, Identifiable {
    case system
    case light
    case dark

    var id: String { rawValue }
    var title: String {
        return switch self {
        case .system: "Как на iPhone"
        case .light: "Светлая"
        case .dark: "Тёмная"
        }
    }
    var colorScheme: ColorScheme? {
        return switch self {
        case .system: nil
        case .light: .light
        case .dark: .dark
        }
    }
}

enum MauTheme {
    static let blue = Color(red: 0.09, green: 0.46, blue: 0.97)
    static let cyan = Color(red: 0.29, green: 0.78, blue: 1.0)
    static let violet = Color(red: 0.47, green: 0.40, blue: 1.0)
    static let navy = Color(red: 0.015, green: 0.075, blue: 0.13)
    static let success = Color(red: 0.16, green: 0.70, blue: 0.45)
    static let ink = Color(uiColor: .label)
    static let muted = Color(uiColor: .secondaryLabel)
    static let canvas = Color(uiColor: .systemBackground)
    static let card = Color(uiColor: .secondarySystemBackground)
    static let lavender = Color(red: 0.88, green: 0.90, blue: 1.0)

    static let heroGradient = LinearGradient(
        colors: [blue, Color(red: 0.08, green: 0.27, blue: 0.68), violet],
        startPoint: .topLeading,
        endPoint: .bottomTrailing
    )
}

enum MauSpacing {
    static let xs: CGFloat = 6
    static let sm: CGFloat = 10
    static let md: CGFloat = 16
    static let lg: CGFloat = 22
    static let xl: CGFloat = 30
}

enum MauRadius {
    static let compact: CGFloat = 16
    static let card: CGFloat = 24
    static let hero: CGFloat = 30
}

struct MauBackground: View {
    @Environment(\.colorScheme) private var colorScheme

    var body: some View {
        ZStack {
            MauTheme.canvas
            LinearGradient(
                colors: colorScheme == .dark
                    ? [MauTheme.navy.opacity(0.96), .black, Color.purple.opacity(0.10)]
                    : [Color.blue.opacity(0.055), MauTheme.canvas, Color.purple.opacity(0.045)],
                startPoint: .topTrailing,
                endPoint: .bottomLeading
            )
            Circle()
                .fill(MauTheme.cyan.opacity(colorScheme == .dark ? 0.13 : 0.10))
                .frame(width: 420, height: 420)
                .blur(radius: 70)
                .offset(x: 190, y: -350)
            Circle()
                .fill(MauTheme.violet.opacity(colorScheme == .dark ? 0.15 : 0.075))
                .frame(width: 390, height: 390)
                .blur(radius: 80)
                .offset(x: -190, y: 390)
        }
        .ignoresSafeArea()
    }
}

private struct MauGlassModifier: ViewModifier {
    let radius: CGFloat

    @ViewBuilder
    func body(content: Content) -> some View {
        if #available(iOS 26.0, *) {
            content.glassEffect(.regular, in: RoundedRectangle(cornerRadius: radius, style: .continuous))
        } else {
            content
                .background(.ultraThinMaterial, in: RoundedRectangle(cornerRadius: radius, style: .continuous))
                .overlay {
                    RoundedRectangle(cornerRadius: radius, style: .continuous)
                        .stroke(Color.white.opacity(0.75), lineWidth: 1)
                }
        }
    }
}

extension View {
    func mauGlass(radius: CGFloat = 24) -> some View {
        modifier(MauGlassModifier(radius: radius))
    }

    func pageTitle(_ title: String, subtitle: String? = nil) -> some View {
        VStack(alignment: .leading, spacing: 7) {
            Text(title)
                .font(.system(size: 34, weight: .bold, design: .rounded))
                .foregroundStyle(MauTheme.ink)
            if let subtitle {
                Text(subtitle)
                    .font(.subheadline)
                    .foregroundStyle(MauTheme.muted)
            }
        }
        .frame(maxWidth: .infinity, alignment: .leading)
    }

    func mauSurface(radius: CGFloat = MauRadius.card) -> some View {
        background(MauTheme.card.opacity(0.78), in: RoundedRectangle(cornerRadius: radius, style: .continuous))
            .overlay {
                RoundedRectangle(cornerRadius: radius, style: .continuous)
                    .stroke(Color.primary.opacity(0.07), lineWidth: 0.75)
            }
    }
}

struct IconTile: View {
    let systemName: String
    var color = MauTheme.blue

    var body: some View {
        Image(systemName: systemName)
            .font(.system(size: 20, weight: .semibold))
            .foregroundStyle(.white)
            .frame(width: 44, height: 44)
            .background(color.gradient, in: RoundedRectangle(cornerRadius: 14, style: .continuous))
    }
}

struct MauSectionHeader: View {
    let title: String
    var action: String?

    var body: some View {
        HStack(alignment: .firstTextBaseline) {
            Text(title)
                .font(.title3.bold())
                .foregroundStyle(MauTheme.ink)
            Spacer()
            if let action {
                Text(action)
                    .font(.subheadline.weight(.semibold))
                    .foregroundStyle(MauTheme.blue)
            }
        }
    }
}

struct MauStatusPill: View {
    let title: String
    var icon = "checkmark.circle.fill"
    var color = MauTheme.success

    var body: some View {
        Label(title, systemImage: icon)
            .font(.caption.weight(.semibold))
            .foregroundStyle(color)
            .padding(.horizontal, 11)
            .padding(.vertical, 7)
            .background(color.opacity(0.12), in: Capsule())
    }
}

struct SkeletonCard: View {
    @State private var pulse = false

    var body: some View {
        VStack(alignment: .leading, spacing: 13) {
            RoundedRectangle(cornerRadius: 14)
                .frame(height: 150)
            RoundedRectangle(cornerRadius: 6)
                .frame(height: 18)
                .padding(.trailing, 40)
            RoundedRectangle(cornerRadius: 6)
                .frame(width: 150, height: 12)
        }
        .foregroundStyle(MauTheme.muted.opacity(pulse ? 0.12 : 0.23))
        .padding(16)
        .mauSurface()
        .animation(.easeInOut(duration: 0.9).repeatForever(autoreverses: true), value: pulse)
        .onAppear { pulse = true }
    }
}

struct LoadingOverlay: View {
    let title: String

    var body: some View {
        HStack(spacing: 12) {
            ProgressView()
            Text(title).font(.subheadline.weight(.medium))
        }
        .padding(.horizontal, 18)
        .padding(.vertical, 14)
        .mauGlass(radius: 18)
    }
}

struct EmptyState: View {
    let icon: String
    let title: String
    let message: String

    var body: some View {
        VStack(spacing: 12) {
            Image(systemName: icon)
                .font(.system(size: 35))
                .foregroundStyle(MauTheme.blue)
            Text(title).font(.headline)
            Text(message)
                .font(.subheadline)
                .foregroundStyle(MauTheme.muted)
                .multilineTextAlignment(.center)
        }
        .frame(maxWidth: .infinity)
        .padding(28)
        .mauGlass()
    }
}
