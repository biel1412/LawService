using System.Globalization;

namespace LawService;

/// <summary>Tema escuro com vermelho-bordô (cor do Direito) e paletas.</summary>
public static class Th
{
    public const string APP_NAME = "LawService";

    public static readonly Dictionary<string, string> T = new()
    {
        ["bg"] = "#121113", ["surface"] = "#1b191c", ["surface2"] = "#211f23", ["hover"] = "#2a272c", ["pressed"] = "#332f35",
        ["border"] = "#2e2a30", ["border2"] = "#3d383f",
        ["text"] = "#f1edef", ["text2"] = "#d4cdd1", ["muted"] = "#a39aa0", ["faint"] = "#766d73",
        ["accent"] = "#c23a48", ["accent2"] = "#7a1626", ["accent_hover"] = "#d24c5a",
        ["accent_soft"] = "#341a1f", ["accent_soft2"] = "#47222a", ["sel"] = "#5e2832",
        ["danger"] = "#f0595c", ["danger_soft"] = "#3a191b", ["success"] = "#46b65a", ["success_soft"] = "#17301d",
        ["warn"] = "#f0a03a", ["warn_soft"] = "#3a2a14", ["info"] = "#4c9ee8", ["info_soft"] = "#15283b",
        ["rail1"] = "#1f0a10", ["rail2"] = "#0c0708", ["rail_text"] = "#c4b2b8", ["rail_hover"] = "rgba(255,255,255,0.06)",
        ["paper"] = "#ffffff", ["paper_text"] = "#1a1d2b", ["scroll"] = "#4a4349", ["scroll_hover"] = "#625a60",
    };

    // cores de "entidades" (cadernos, páginas, áreas, baralhos)
    public static readonly string[] PALETTE = { "#5b5ef0", "#8b5cf6", "#d6409f", "#e5484d", "#f76707", "#f5a524",
        "#2f9e44", "#0ca678", "#15aabf", "#1c7ed6", "#495057", "#a0522d" };
    // cores do texto (linha base + tons)
    static readonly string[] _TXT = { "#1a1d2b", "#e03131", "#f76707", "#f59f00", "#2f9e44", "#0c8599", "#1971c2", "#5f3dc4", "#ae3ec9", "#c2255c" };
    // marca-texto
    public static readonly string[] HIGHLIGHTS = { "#fff3a3", "#ffe066", "#ffd8a8", "#ffc9c9", "#fcc2d7", "#eebefa", "#d0bfff",
        "#bac8ff", "#a5d8ff", "#99e9f2", "#96f2d7", "#b2f2bb", "#d8f5a2", "#e9ecef" };
    // blocos de destaque
    public static readonly string[] CALLOUTS = { "#eef2ff", "#e7f5ff", "#e3fafc", "#e6fcf5", "#ebfbee", "#fff9db",
        "#fff4e6", "#fff0f6", "#f8f0fc", "#f1f3f5" };

    public static string UI_FONT = "Segoe UI";
    public static string MONO_FONT = "Consolas";
    public const double BASE_PT = 11;
    public static readonly Dictionary<int, double> HEAD_PT = new() { [1] = 22, [2] = 17, [3] = 14 };
    public const int QUOTE_MARGIN = 26;
    public const string LETTERS = "ABCDE";
    public const string SERIF = "Times New Roman";

    /// <summary>Cor em RGB (0–255).</summary>
    public static (int r, int g, int b) Rgb(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return (128, 128, 128);
        s = s.Trim();
        if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var p = s[(s.IndexOf('(') + 1)..s.IndexOf(')')].Split(',');
            return (int.Parse(p[0].Trim()), int.Parse(p[1].Trim()), int.Parse(p[2].Trim()));
        }
        if (s.StartsWith("#")) s = s[1..];
        if (s.Length == 3) s = string.Concat(s.Select(ch => $"{ch}{ch}"));
        if (s.Length == 8) s = s[2..];
        if (s.Length != 6 || !int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v)) return (128, 128, 128);
        return ((v >> 16) & 255, (v >> 8) & 255, v & 255);
    }

    public static string Hex(int r, int g, int b) => $"#{r:x2}{g:x2}{b:x2}";

    public static string Mix(string c1, string c2, double t)
    {
        var a = Rgb(c1);
        var b = Rgb(c2);
        return Hex((int)Math.Round(a.r + (b.r - a.r) * t), (int)Math.Round(a.g + (b.g - a.g) * t), (int)Math.Round(a.b + (b.b - a.b) * t));
    }

    /// <summary>Cor com transparência (CSS rgba).</summary>
    public static string Alpha(string c, double a)
    {
        var x = Rgb(c);
        return $"rgba({x.r},{x.g},{x.b},{a.ToString("0.###", CultureInfo.InvariantCulture)})";
    }

    /// <summary>Fundo suave da cor c sobre a superfície do tema.</summary>
    public static string Tint(string c, double t) => Mix(c, T["surface"], Math.Max(0.0, t - 0.06));

    /// <summary>Cor c clareada para texto/ícone legível sobre fundo escuro.</summary>
    public static string Ink(string c, double t = .3) => Mix(c, "#ffffff", 0.38);

    public static List<string> TextPalette()
    {
        var rows = new List<string[]>
        {
            _TXT.ToArray(),
            _TXT.Select(c => Mix(c, "#ffffff", 0.45)).ToArray(),
            _TXT.Select(c => Mix(c, "#ffffff", 0.72)).ToArray(),
            _TXT.Select(c => Mix(c, "#000000", 0.35)).ToArray(),
        };
        rows[1][0] = "#6b7186";
        rows[2][0] = "#a3a8b8";
        rows[3][0] = "#000000";
        return rows.SelectMany(r => r).ToList();
    }

    public static string Grad(string c1, string c2, string dir = "135deg") => $"linear-gradient({dir}, {c1}, {c2})";
}
