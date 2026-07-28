using mau.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace mau.Platforms.iOS;

internal static class LiquidGlassStyling
{
    const nint GlassViewTag = 0x4D415547;

    internal static void Configure()
    {
        BorderHandler.Mapper.AppendToMapping(
            LiquidGlass.UsesLiquidGlassProperty.PropertyName,
            static (handler, view) => LiquidGlassStyling.Apply(handler.PlatformView, view));

        // Dynamic theme changes remap Background, so re-apply the transparent
        // native host after MAUI has updated its brush.
        BorderHandler.Mapper.AppendToMapping(
            nameof(IView.Background),
            static (handler, view) => LiquidGlassStyling.Apply(handler.PlatformView, view));
    }

    static void Apply(UIView platformView, IBorderView virtualView)
    {
        var existingGlass = platformView.ViewWithTag(GlassViewTag) as UIVisualEffectView;
        if (virtualView is not BindableObject bindableView ||
            !LiquidGlass.GetUsesLiquidGlass(bindableView))
        {
            existingGlass?.RemoveFromSuperview();
            return;
        }

        if (existingGlass is null)
        {
            existingGlass = CreateGlassView(virtualView);
            existingGlass.Tag = GlassViewTag;
            existingGlass.UserInteractionEnabled = false;
            existingGlass.TranslatesAutoresizingMaskIntoConstraints = false;
            platformView.InsertSubview(existingGlass, 0);

            NSLayoutConstraint.ActivateConstraints(
            [
                existingGlass.LeadingAnchor.ConstraintEqualTo(platformView.LeadingAnchor),
                existingGlass.TrailingAnchor.ConstraintEqualTo(platformView.TrailingAnchor),
                existingGlass.TopAnchor.ConstraintEqualTo(platformView.TopAnchor),
                existingGlass.BottomAnchor.ConstraintEqualTo(platformView.BottomAnchor)
            ]);
        }

        platformView.BackgroundColor = UIColor.Clear;
        platformView.Layer.BackgroundColor = UIColor.Clear.CGColor;
        platformView.ClipsToBounds = true;
    }

    static UIVisualEffectView CreateGlassView(IBorderView virtualView)
    {
        UIVisualEffect effect;
        if (OperatingSystem.IsIOSVersionAtLeast(26))
        {
            var glass = UIGlassEffect.Create(UIGlassEffectStyle.Regular);
            glass.Interactive =
                virtualView is Microsoft.Maui.Controls.View view &&
                view.GestureRecognizers.Count > 0;
            glass.TintColor = UIColor.FromRGBA(0, 140, 250, 18);
            effect = glass;
        }
        else
        {
            effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.SystemMaterial);
        }

        return new UIVisualEffectView(effect);
    }
}
