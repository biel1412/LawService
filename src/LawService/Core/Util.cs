using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LawService;

public static class U
{
    public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    public static readonly CultureInfo PtBr = new("pt-BR");
    static readonly Random _rng = new();

    public static string Uid() => Guid.NewGuid().ToString("N")[..12];

    public static string NowIso() => DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss", Inv);

    public static DateTime Today() => DateTime.Today;

    public static string IsoDate(DateTime d) => d.ToString("yyyy-MM-dd", Inv);

    public static bool TryIso(string iso, out DateTime d)
    {
        d = default;
        if (string.IsNullOrWhiteSpace(iso)) return false;
        return DateTime.TryParse(iso, Inv, DateTimeStyles.None, out d);
    }

    public static DateTime ParseDate(string iso) => TryIso(iso, out var d) ? d.Date : DateTime.Today;

    public static string FmtDt(string iso, bool shortFmt = false)
    {
        if (string.IsNullOrEmpty(iso)) return "";
        if (!TryIso(iso, out var d)) return iso;
        if (shortFmt)
        {
            int delta = (Today() - d.Date).Days;
            if (delta == 0) return $"hoje, {d:HH:mm}";
            if (delta == 1) return $"ontem, {d:HH:mm}";
            return d.ToString("dd/MM/yyyy", Inv);
        }
        return d.ToString("dd/MM/yyyy 'às' HH:mm", Inv);
    }

    public static string Esc(string s) => WebUtility.HtmlEncode(s ?? "");

    public static string HtmlToPlain(string h)
    {
        if (string.IsNullOrEmpty(h)) return "";
        var s = Regex.Replace(h, @"(?is)<(style|script|head)[^>]*>.*?</\1>", "");
        s = Regex.Replace(s, @"(?i)<br\s*/?>", "\n");
        s = Regex.Replace(s, @"(?i)</(p|div|h[1-6]|li|tr|blockquote)>", "\n");
        s = Regex.Replace(s, "<[^>]+>", "");
        s = WebUtility.HtmlDecode(s).Replace(' ', ' ');
        s = Regex.Replace(s, "\n{3,}", "\n\n");
        return s.Trim('\n');
    }

    public static int Words(string s) => string.IsNullOrWhiteSpace(s) ? 0 : s.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;

    public static string Collapse(string s) => string.Join(" ", (s ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

    public static string Elide(string s, int n) => s.Length > n ? s[..n] + "…" : s;

    public static T Choice<T>(IList<T> l) => l[_rng.Next(l.Count)];

    // ─────────────────────────── datas e valores (estágio)
    public static readonly string[] MESES = { "janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto",
        "setembro", "outubro", "novembro", "dezembro" };

    public static string Brl(double v) => "R$ " + v.ToString("#,##0.00", PtBr);

    public static bool Weekend(DateTime d) => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;

    public static DateTime AddBd(DateTime d, int n)
    {
        while (n > 0)
        {
            d = d.AddDays(1);
            if (!Weekend(d)) n--;
        }
        return d;
    }

    public static DateTime SubBd(DateTime d, int n)
    {
        while (n > 0)
        {
            d = d.AddDays(-1);
            if (!Weekend(d)) n--;
        }
        return d;
    }

    public static DateTime WeekdayFwd(DateTime d)
    {
        while (Weekend(d)) d = d.AddDays(1);
        return d;
    }

    /// <summary>Segunda=0 … domingo=6 (como o weekday() do Python).</summary>
    public static int Wd(DateTime d) => ((int)d.DayOfWeek + 6) % 7;

    public static string Fdate(DateTime d) => d.ToString("dd/MM/yyyy", Inv);

    public static string FdateExt(DateTime d) => $"{d.Day} de {MESES[d.Month - 1]} de {d.Year}";

    public static string FmtBr(string iso) => TryIso(iso, out var d) ? Fdate(d) : (iso ?? "");

    public static string Fmt(double v, string f = "0.##") => v.ToString(f, Inv);

    public static string Thousands(int v) => v.ToString("#,##0", Inv).Replace(",", ".");
}

/// <summary>Atalhos para trabalhar com o JSON (equivalente aos dicts do Python).</summary>
public static class J
{
    public static string Str(JsonNode v)
    {
        if (v is null) return null;
        if (v is JsonValue jv)
        {
            if (jv.TryGetValue<string>(out var s)) return s;
            if (jv.TryGetValue<JsonElement>(out var el))
                return el.ValueKind switch
                {
                    JsonValueKind.String => el.GetString(),
                    JsonValueKind.Number => el.GetRawText(),
                    JsonValueKind.True => "True",
                    JsonValueKind.False => "False",
                    _ => null
                };
            return jv.ToJsonString().Trim('"');
        }
        return v.ToJsonString();
    }

    public static double? Num(JsonNode v)
    {
        if (v is not JsonValue jv) return null;
        if (jv.TryGetValue<int>(out var i)) return i;
        if (jv.TryGetValue<long>(out var l)) return l;
        if (jv.TryGetValue<double>(out var d)) return d;
        if (jv.TryGetValue<decimal>(out var m)) return (double)m;
        if (jv.TryGetValue<float>(out var f)) return f;
        if (jv.TryGetValue<JsonElement>(out var el))
        {
            if (el.ValueKind == JsonValueKind.Number) return el.GetDouble();
            if (el.ValueKind == JsonValueKind.String && double.TryParse(el.GetString(), NumberStyles.Any, U.Inv, out var x)) return x;
            if (el.ValueKind == JsonValueKind.True) return 1;
            if (el.ValueKind == JsonValueKind.False) return 0;
        }
        if (jv.TryGetValue<string>(out var s) && double.TryParse(s, NumberStyles.Any, U.Inv, out var y)) return y;
        if (jv.TryGetValue<bool>(out var bo)) return bo ? 1 : 0;
        return null;
    }

    public static JsonNode Get(this JsonNode n, string k) => n is JsonObject o && o.TryGetPropertyValue(k, out var v) ? v : null;

    public static string S(this JsonNode n, string k, string d = "") => Str(n.Get(k)) ?? d;

    public static double D(this JsonNode n, string k, double d = 0) => Num(n.Get(k)) ?? d;

    public static int I(this JsonNode n, string k, int d = 0) => (int)Math.Round(Num(n.Get(k)) ?? d);

    public static bool B(this JsonNode n, string k, bool d = false)
    {
        var v = n.Get(k);
        if (v is null) return d;
        if (v is JsonValue jv)
        {
            if (jv.TryGetValue<bool>(out var b)) return b;
            if (jv.TryGetValue<JsonElement>(out var el))
            {
                if (el.ValueKind == JsonValueKind.True) return true;
                if (el.ValueKind == JsonValueKind.False || el.ValueKind == JsonValueKind.Null) return false;
                if (el.ValueKind == JsonValueKind.String) return !string.IsNullOrEmpty(el.GetString());
                if (el.ValueKind == JsonValueKind.Number) return el.GetDouble() != 0;
            }
            if (jv.TryGetValue<string>(out var s)) return !string.IsNullOrEmpty(s);
            var num = Num(v);
            if (num != null) return num != 0;
        }
        if (v is JsonArray a) return a.Count > 0;
        if (v is JsonObject o) return o.Count > 0;
        return d;
    }

    /// <summary>Verdadeiro se o valor existe e não é vazio (como "if x.get(k)" no Python).</summary>
    public static bool Ok(this JsonNode n, string k) => n.B(k);

    public static JsonArray A(this JsonNode n, string k)
    {
        var o = (JsonObject)n;
        if (o.TryGetPropertyValue(k, out var v) && v is JsonArray a) return a;
        a = new JsonArray();
        o[k] = a;
        return a;
    }

    public static JsonObject O(this JsonNode n, string k)
    {
        var o = (JsonObject)n;
        if (o.TryGetPropertyValue(k, out var v) && v is JsonObject x) return x;
        x = new JsonObject();
        o[k] = x;
        return x;
    }

    public static bool Has(this JsonNode n, string k) => n is JsonObject o && o.ContainsKey(k);

    public static IEnumerable<JsonObject> Objs(this JsonArray a) => a?.OfType<JsonObject>() ?? Enumerable.Empty<JsonObject>();

    public static List<JsonObject> List(this JsonArray a) => a.Objs().ToList();

    public static List<string> Strs(this JsonArray a) => a?.Select(Str).Where(x => x != null).ToList() ?? new List<string>();

    public static void Detach(JsonNode n)
    {
        switch (n?.Parent)
        {
            case JsonArray a: a.Remove(n); break;
            case JsonObject o:
                var key = o.FirstOrDefault(kv => ReferenceEquals(kv.Value, n)).Key;
                if (key != null) o.Remove(key);
                break;
        }
    }

    /// <summary>Substitui o conteúdo de um array por estes nós (soltando-os dos pais atuais).</summary>
    public static void Reset(JsonArray arr, IEnumerable<JsonNode> items)
    {
        var lst = items.ToList();
        foreach (var it in lst) Detach(it);
        arr.Clear();
        foreach (var it in lst) arr.Add(it);
    }

    public static int IndexOf(this JsonArray a, JsonNode n)
    {
        for (int i = 0; i < a.Count; i++) if (ReferenceEquals(a[i], n)) return i;
        return -1;
    }

    public static void InsertAt(this JsonArray a, int i, JsonNode n)
    {
        Detach(n);
        a.Insert(Math.Max(0, Math.Min(i, a.Count)), n);
    }

    public static void Push(this JsonArray a, JsonNode n)
    {
        Detach(n);
        a.Add(n);
    }

    public static JsonArray Arr(IEnumerable<string> items) => new(items.Select(s => (JsonNode)JsonValue.Create(s)).ToArray());

    public static readonly JsonSerializerOptions Opts = new()
    {
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static readonly JsonSerializerOptions Compact = new()
    {
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>String JS/JSON segura para usar em ExecuteScriptAsync.</summary>
    public static string Js(string s) => JsonSerializer.Serialize(s ?? "", Compact);
}
