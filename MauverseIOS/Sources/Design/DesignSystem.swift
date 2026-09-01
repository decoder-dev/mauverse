import SwiftUI

enum AppTheme: String, CaseIterable, Identifiable {
    case system
    case light
    case dark

    var id: String { rawValue }
    var title: String {
        switch self {
        case .system: "Как на iPhone"
        case .light: "Светлая"
        case .dark: "Тёмная"
        }
    }
    var colorScheme: ColorScheme? {
        switch self {
        case .system: nil
        case .light: .light
        case .dark: .dark
        }
    }
}

enum MauTheme {
    static let blue = Color(red: 0.09, green: 0.46, blue: 0.97)
    static let cyan = Color(red: 0.29, green: 0.78, blue: 1.0)
    static let violet = Color(red: 0.38, green: 0.42, blue: 0.92)
    static let navy = Color(red: 0.015, green: 0.075, blue: 0.13)
    static let success = Color(red: 0.16, green: 0.70, blue: 0.45)
    static let ink = Color(uiColor: .label)
    static let muted = Color(uiColor: .secondaryLabel)
    static let canvas = Color(uiColor: .systemBackground)
    static let card = Color(uiColor: .secondarySystemBackground)

    static let heroGradient = LinearGradient(
        colors: [blue, Color(red: 0.08, green: 0.27, blue: 0.68), cyan.opacity(0.85)],
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

/// Shared layout grid used across tab screens (mirrors Android LayoutMetrics).
enum MauLayout {
    static let pageHorizontal: CGFloat = 28
    static let pageTop: CGFloat = 20
    static let pageBottomTabClearance: CGFloat = 108
    static let gridGutter: CGFloat = 12
    static let gridRow: CGFloat = 12
    static let sectionStack: CGFloat = 22
    static let sectionHeaderBottom: CGFloat = 10
    static let cardPadding: CGFloat = 16
    static let cardMinHeight: CGFloat = 140
    static let maxContentWidth: CGFloat = 760

    static var twoColumnGrid: [GridItem] {
        [
            GridItem(.flexible(), spacing: gridGutter),
            GridItem(.flexible(), spacing: gridGutter),
        ]
    }
}

enum MauRadius {
    static let compact: CGFloat = 16
    static let card: CGFloat = 24
    static let hero: CGFloat = 30
}

enum MauMotion {
    static let snappy = Animation.snappy(duration: 0.28, extraBounce: 0.05)
    static let soft = Animation.spring(response: 0.42, dampingFraction: 0.86)
    static let press = Animation.spring(response: 0.22, dampingFraction: 0.72)
    static let orb = Animation.easeInOut(duration: 9.5).repeatForever(autoreverses: true)
    static let pulse = Animation.easeInOut(duration: 1.05).repeatForever(autoreverses: true)
}

/// Atmospheric canvas with drifting glass orbs (respects Reduce Motion).
struct MauBackground: View {
    @Environment(\.colorScheme) private var colorScheme
    @Environment(\.accessibilityReduceMotion) private var reduceMotion
    @State private var drift = false

    var body: some View {
        ZStack {
            MauTheme.canvas
            LinearGradient(
                colors: colorScheme == .dark
                    ? [MauTheme.navy.opacity(0.97), .black, MauTheme.blue.opacity(0.12)]
                    : [MauTheme.cyan.opacity(0.07), MauTheme.canvas, MauTheme.blue.opacity(0.05)],
                startPoint: .topTrailing,
                endPoint: .bottomLeading
            )
            Circle()
                .fill(MauTheme.cyan.opacity(colorScheme == .dark ? 0.16 : 0.12))
                .frame(width: 440, height: 440)
                .blur(radius: 78)
                .offset(x: drift ? 175 : 205, y: drift ? -330 : -360)
            Circle()
                .fill(MauTheme.blue.opacity(colorScheme == .dark ? 0.18 : 0.09))
                .frame(width: 400, height: 400)
                .blur(radius: 88)
                .offset(x: drift ? -175 : -210, y: drift ? 360 : 410)
            Circle()
                .fill(MauTheme.violet.opacity(colorScheme == .dark ? 0.10 : 0.05))
                .frame(width: 280, height: 280)
                .blur(radius: 70)
                .offset(x: drift ? 40 : -20, y: drift ? 120 : 160)
        }
        .ignoresSafeArea()
        .onAppear {
            guard !reduceMotion else { return }
            withAnimation(MauMotion.orb) { drift = true }
        }
    }
}

enum MauGlassStyle {
    case regular
    case thin
    case interactive
}

private struct MauGlassModifier: ViewModifier {
    let radius: CGFloat
    let style: MauGlassStyle

    @Environment(\.colorScheme) private var colorScheme
    @Environment(\.accessibilityReduceTransparency) private var reduceTransparency

    @ViewBuilder
    func body(content: Content) -> some View {
        if reduceTransparency {
            content
                .background(MauTheme.card.opacity(colorScheme == .dark ? 0.94 : 0.96),
                            in: RoundedRectangle(cornerRadius: radius, style: .continuous))
                .overlay { stroke }
        } else if #available(iOS 26.0, *) {
            glass26(content)
                .overlay { stroke.opacity(0.55) }
        } else {
            content
                .background(fallbackMaterial, in: RoundedRectangle(cornerRadius: radius, style: .continuous))
                .overlay { stroke }
        }
    }

    @available(iOS 26.0, *)
    @ViewBuilder
    private func glass26(_ content: Content) -> some View {
        let shape = RoundedRectangle(cornerRadius: radius, style: .continuous)
        // Keep to stable glassEffect APIs shipped with Xcode 26.5.
        content.glassEffect(.regular, in: shape)
    }

    private var fallbackMaterial: Material {
        switch style {
        case .thin: .thinMaterial
        case .regular, .interactive: .ultraThinMaterial
        }
    }

    private var stroke: some View {
        RoundedRectangle(cornerRadius: radius, style: .continuous)
            .strokeBorder(
                LinearGradient(
                    colors: colorScheme == .dark
                        ? [Color.white.opacity(0.28), Color.white.opacity(0.06)]
                        : [Color.white.opacity(0.85), Color.white.opacity(0.25)],
                    startPoint: .topLeading,
                    endPoint: .bottomTrailing
                ),
                lineWidth: 0.9
            )
    }
}

private struct MauPressStyle: ButtonStyle {
    @Environment(\.accessibilityReduceMotion) private var reduceMotion

    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .scaleEffect(configuration.isPressed && !reduceMotion ? 0.965 : 1)
            .opacity(configuration.isPressed ? 0.92 : 1)
            .animation(MauMotion.press, value: configuration.isPressed)
    }
}

extension View {
    /// Primary Liquid Glass surface (cards, sheets, chrome).
    func mauGlass(radius: CGFloat = MauRadius.card, style: MauGlassStyle = .regular) -> some View {
        modifier(MauGlassModifier(radius: radius, style: style))
    }

    /// Secondary translucent surface — prefers glass over opaque fills.
    func mauSurface(radius: CGFloat = MauRadius.card) -> some View {
        mauGlass(radius: radius, style: .thin)
    }

    func mauPressable() -> some View {
        buttonStyle(MauPressStyle())
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

    /// Standard tab-root scroll insets: 28 pt sides, 12 pt grid gutter, 108 pt tab clearance.
    func mauTabPageContent(maxWidth: CGFloat = MauLayout.maxContentWidth) -> some View {
        frame(maxWidth: maxWidth)
            .frame(maxWidth: .infinity)
            .padding(.horizontal, MauLayout.pageHorizontal)
            .padding(.top, MauLayout.pageTop)
            .padding(.bottom, MauLayout.pageBottomTabClearance)
    }

    /// Trailing inset for horizontal chip/card rows inside tab pages.
    func mauHorizontalScrollTrailingInset() -> some View {
        padding(.trailing, MauLayout.gridGutter)
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
            .shadow(color: color.opacity(0.28), radius: 8, y: 3)
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
            .background(color.opacity(0.14), in: Capsule())
            .overlay {
                Capsule().strokeBorder(color.opacity(0.22), lineWidth: 0.6)
            }
    }
}

struct SkeletonCard: View {
    @Environment(\.accessibilityReduceMotion) private var reduceMotion
    @State private var pulse = false
    var height: CGFloat = 150

    var body: some View {
        VStack(alignment: .leading, spacing: 13) {
            RoundedRectangle(cornerRadius: 14, style: .continuous)
                .frame(height: height)
            RoundedRectangle(cornerRadius: 6, style: .continuous)
                .frame(height: 18)
                .padding(.trailing, 40)
            RoundedRectangle(cornerRadius: 6, style: .continuous)
                .frame(width: 150, height: 12)
        }
        .foregroundStyle(MauTheme.muted.opacity(pulse ? 0.10 : 0.22))
        .padding(16)
        .mauGlass(radius: MauRadius.card, style: .thin)
        .onAppear {
            guard !reduceMotion else { pulse = true; return }
            withAnimation(MauMotion.pulse) { pulse = true }
        }
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
        .mauGlass(radius: MauRadius.compact, style: .interactive)
    }
}

struct EmptyState: View {
    let icon: String
    let title: String
    let message: String

    var body: some View {
        VStack(spacing: 12) {
            Image(systemName: icon)
                .font(.system(size: 35, weight: .medium))
                .foregroundStyle(MauTheme.blue)
            Text(title).font(.headline)
            Text(message)
                .font(.subheadline)
                .foregroundStyle(MauTheme.muted)
                .multilineTextAlignment(.center)
        }
        .frame(maxWidth: .infinity)
        .padding(28)
        .mauGlass(radius: MauRadius.card)
    }
}

/// Groups sibling glass views so iOS 26 can morph materials together.
struct MauGlassStack<Content: View>: View {
    @ViewBuilder var content: () -> Content

    var body: some View {
        if #available(iOS 26.0, *) {
            GlassEffectContainer { content() }
        } else {
            content()
        }
    }
}
