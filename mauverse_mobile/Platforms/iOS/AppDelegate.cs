using Foundation;

using System.Diagnostics.CodeAnalysis;

namespace mau.Platforms.iOS
{
    [Register("AppDelegate")]
    [SuppressMessage(
        "Naming",
        "CA1711:Identifiers should not have incorrect suffix",
        Justification = "UIKit discovers the application delegate by its conventional registered name.")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
