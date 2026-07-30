import SwiftUI

struct LoginView: View {
    @EnvironmentObject private var session: SessionStore
    @State private var username = ""
    @State private var password = ""
    @FocusState private var focused: Field?

    private enum Field { case username, password }

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 26) {
                    Spacer(minLength: 54)
                    VStack(spacing: 14) {
                        ZStack {
                            RoundedRectangle(cornerRadius: 30, style: .continuous)
                                .fill(MauTheme.heroGradient)
                            Image(systemName: "snowflake")
                                .font(.system(size: 45, weight: .medium))
                                .foregroundStyle(.white)
                        }
                        .frame(width: 96, height: 96)
                        .overlay {
                            RoundedRectangle(cornerRadius: 30, style: .continuous)
                                .stroke(.white.opacity(0.24), lineWidth: 1)
                        }
                        .shadow(color: MauTheme.blue.opacity(0.28), radius: 28, y: 14)

                        Text("MAUverse")
                            .font(.system(size: 40, weight: .bold, design: .rounded))
                        Text("Цифровая среда Арктического университета")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                            .multilineTextAlignment(.center)
                    }

                    VStack(spacing: 16) {
                        TextField("Логин ЭИОС", text: $username)
                            .textContentType(.username)
                            .textInputAutocapitalization(.never)
                            .autocorrectionDisabled()
                            .focused($focused, equals: .username)
                            .submitLabel(.next)
                            .onSubmit { focused = .password }
                            .padding(18)
                            .background(MauTheme.card.opacity(0.78), in: RoundedRectangle(cornerRadius: 18))

                        SecureField("Пароль", text: $password)
                            .textContentType(.password)
                            .focused($focused, equals: .password)
                            .submitLabel(.go)
                            .onSubmit { signIn() }
                            .padding(18)
                            .background(MauTheme.card.opacity(0.78), in: RoundedRectangle(cornerRadius: 18))

                        if let error = session.errorMessage {
                            Text(error)
                                .font(.footnote)
                                .foregroundStyle(.red)
                                .frame(maxWidth: .infinity, alignment: .leading)
                        }

                        Button(action: signIn) {
                            HStack {
                                if session.isBusy { ProgressView().tint(.white) }
                                Text(session.isBusy ? "Входим…" : "Войти")
                                    .fontWeight(.semibold)
                            }
                            .frame(maxWidth: .infinity)
                            .padding(.vertical, 17)
                        }
                        .buttonStyle(.plain)
                        .foregroundStyle(.white)
                        .background(MauTheme.heroGradient, in: RoundedRectangle(cornerRadius: 18))
                        .shadow(color: MauTheme.blue.opacity(0.22), radius: 14, y: 7)
                        .disabled(session.isBusy)
                    }
                    .padding(20)
                    .mauSurface(radius: 28)

                    Text("Используйте данные от электронной информационно-образовательной среды МАУ")
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                        .multilineTextAlignment(.center)
                        .padding(.horizontal, 24)
                }
                .padding(.horizontal, 20)
                .padding(.bottom, 30)
            }
        }
    }

    private func signIn() {
        focused = nil
        Task { await session.signIn(username: username, password: password) }
    }
}
