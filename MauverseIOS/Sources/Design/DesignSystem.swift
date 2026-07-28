import SwiftUI

enum MauTheme {
    static let blue = Color(red: 0.16, green: 0.43, blue: 0.95)
    static let ink = Color(red: 0.08, green: 0.10, blue: 0.16)
    static let muted = Color(red: 0.42, green: 0.46, blue: 0.54)
    static let canvas = Color(red: 0.965, green: 0.97, blue: 0.985)
    static let lavender = Color(red: 0.88, green: 0.90, blue: 1.0)
}

struct MauBackground: View {
    var body: some View {
        ZStack {
            MauTheme.canvas
            Circle()
                .fill(Color.blue.opacity(0.12))
                .frame(width: 360, height: 360)
                .blur(radius: 40)
                .offset(x: 170, y: -330)
            Circle()
                .fill(Color.purple.opacity(0.10))
                .frame(width: 330, height: 330)
                .blur(radius: 55)
                .offset(x: -180, y: 360)
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

