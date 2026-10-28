namespace LawService;

/// <summary>Utilidades de interface (cores recentes, menus de cor).</summary>
public static class Ui
{
    public static List<string> Recent(string key)
    {
        if (key == null) return new List<string>();
        return Store.Settings.A("recent_" + key).Strs();
    }

    public static void PushRecent(string key, string c, int cols)
    {
        if (key == null || string.IsNullOrEmpty(c)) return;
        var r = Recent(key);
        r.Remove(c);
        r.Insert(0, c);
        Store.Settings["recent_" + key] = J.Arr(r.Take(cols));
    }

    /// <summary>Submenu com paleta de cores.</summary>
    public static MItem ColorSub(string text, string icon, ColorSpec spec) => new()
    {
        Icon = icon, Text = text,
        Children = new List<MItem> { new() { Content = close => b =>
        {
            b.OpenComponent<Components.ColorGrid>(0);
            b.AddAttribute(1, "Spec", spec);
            b.AddAttribute(2, "Close", close);
            b.CloseComponent();
        } } },
    };

    /// <summary>Menu que é só a paleta.</summary>
    public static List<MItem> ColorMenu(ColorSpec spec) => new()
    {
        new() { Content = close => b =>
        {
            b.OpenComponent<Components.ColorGrid>(0);
            b.AddAttribute(1, "Spec", spec);
            b.AddAttribute(2, "Close", close);
            b.CloseComponent();
        } },
    };

    public static string Px(double v) => v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "px";
    public static string Pt(double pt) => Px(pt * 96.0 / 72.0);
}
