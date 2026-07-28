namespace mau.Controls;

/// <summary>
/// Marks a border as a glass surface. The iOS handler renders the native
/// material while other platforms keep the regular MAUI background.
/// </summary>
public static class LiquidGlass
{
    public static readonly BindableProperty UsesLiquidGlassProperty =
        BindableProperty.CreateAttached(
            "UsesLiquidGlass",
            typeof(bool),
            typeof(LiquidGlass),
            false,
            propertyChanged: OnUsesLiquidGlassChanged);

    public static bool GetUsesLiquidGlass(BindableObject view) =>
        (bool)view.GetValue(UsesLiquidGlassProperty);

    public static void SetUsesLiquidGlass(BindableObject view, bool value) =>
        view.SetValue(UsesLiquidGlassProperty, value);

    static void OnUsesLiquidGlassChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Border border)
            border.Handler?.UpdateValue(UsesLiquidGlassProperty.PropertyName);
    }
}
