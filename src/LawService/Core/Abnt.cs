using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static LawService.U;

namespace LawService;

/// <summary>TCC — citações e referências no padrão ABNT (NBR 10520 e NBR 6023).</summary>
public static class Abnt
{
    public static readonly string[] MESES_ABNT = { "jan.", "fev.", "mar.", "abr.", "maio", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez." };

    public static readonly (string key, string text)[] TIPOS_FONTE =
    {
        ("livro", "Livro"), ("capitulo", "Capítulo de livro"), ("artigo", "Artigo de periódico"),
        ("lei", "Legislação"), ("juris", "Jurisprudência"), ("site", "Site / documento online"),
        ("tese", "Tese / dissertação / TCC"),
    };

    public static readonly (string key, string text, string color)[] STATUS_PARTE =
    {
        ("afazer", "A fazer", "#a3a8b8"), ("escrevendo", "Escrevendo", "#f08c00"),
        ("revisar", "Revisar", "#1c7ed6"), ("pronto", "Pronto", "#2f9e44"),
    };

    public const double CM = 37.8; // px por centímetro no documento (96 dpi)

    public static string TipoNome(string k) => TIPOS_FONTE.FirstOrDefault(t => t.key == k).text ?? "Fonte";

    public static JsonObject TccDefault()
    {
        var n = NowIso();
        JsonObject Parte(string t) => new()
        {
            ["id"] = Uid(), ["titulo"] = t, ["html"] = "", ["plain"] = "", ["status"] = "afazer", ["meta"] = 0, ["updated"] = n,
        };
        return new JsonObject
        {
            ["meta"] = new JsonObject
            {
                ["titulo"] = "", ["subtitulo"] = "", ["autor"] = "", ["orientador"] = "", ["instituicao"] = "", ["curso"] = "",
                ["cidade"] = "", ["ano"] = Today().Year.ToString(), ["meta_palavras"] = 12000, ["fonte"] = "Times New Roman",
                ["autor_maiusc"] = true, ["natureza"] = "",
            },
            ["parts"] = new JsonArray(new[] { "Introdução", "Capítulo 1", "Capítulo 2", "Capítulo 3", "Conclusão" }.Select(t => (JsonNode)Parte(t)).ToArray()),
            ["cits"] = new JsonArray(),
            ["ordem"] = "manual",
        };
    }

    public static List<(string sob, string pre)> AutoresLista(string s)
    {
        var outp = new List<(string, string)>();
        foreach (var a0 in Regex.Split(s ?? "", ";|\n"))
        {
            var a = a0.Trim();
            if (a.Length == 0) continue;
            if (a.Contains(','))
            {
                var p = a.Split(',', 2);
                outp.Add((p[0].Trim(), p[1].Trim()));
            }
            else
            {
                var ps = a.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                outp.Add(ps.Length == 1 ? (ps[0], "") : (ps[^1], string.Join(" ", ps[..^1])));
            }
        }
        return outp;
    }

    static bool Entidade(JsonNode c)
    {
        var a = c.S("autores").Trim();
        var t = c.S("tipo");
        return t == "lei" || t == "juris" || a.Contains('.') || (a == a.ToUpperInvariant() && a.Any(char.IsLetter) && a.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length <= 3 && !a.Contains(','));
    }

    public static string RefAutor(JsonNode c)
    {
        var a = c.S("autores").Trim();
        if (a.Length == 0) return "";
        if (Entidade(c))
        {
            var partes = a.Split('.').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
            return string.Join(". ", new[] { partes[0].ToUpperInvariant() }.Concat(partes.Skip(1)));
        }
        var lst = AutoresLista(a);
        var fmt = lst.Select(x => $"{x.sob.ToUpperInvariant()}, {x.pre}".TrimEnd(',', ' ')).ToList();
        return fmt.Count > 3 ? fmt[0] + " et al" : string.Join("; ", fmt);
    }

    public static string CitAutor(JsonNode c, bool maiusc = true)
    {
        var a = c.S("autores").Trim();
        if (a.Length == 0)
        {
            var t = c.S("titulo").Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            return t.Length > 0 ? t[0].ToUpperInvariant() + " ..." : "[AUTOR]";
        }
        string bse;
        if (Entidade(c)) bse = a.Split('.')[0].Trim();
        else
        {
            var lst = AutoresLista(a).Select(x => x.sob).ToList();
            bse = lst.Count > 3 ? lst[0] + " et al." : string.Join("; ", lst);
        }
        return maiusc ? bse.ToUpperInvariant().Replace(" ET AL.", " et al.") : bse;
    }

    public static string AnoTxt(JsonNode c)
    {
        var a = c.S("ano").Trim();
        return a.Length > 0 ? a : "[s. d.]";
    }

    public static string CitacaoTxt(JsonNode c, string modo = null, bool maiusc = true, bool pagina = true)
    {
        var pg = c.S("pagina").Trim();
        var pgt = "";
        if (pagina && pg.Length > 0)
            pgt = Regex.IsMatch(pg, @"^(p|f|cap|art|n)\.", RegexOptions.IgnoreCase) ? $", {pg}" : $", p. {pg}";
        return $"({CitAutor(c, maiusc)}, {AnoTxt(c)}{pgt})";
    }

    public static string AcessoTxt(string s)
    {
        if (DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return $"{d.Day} {MESES_ABNT[d.Month - 1]} {d.Year}";
        return s ?? "";
    }

    public static string ReferenciaHtml(JsonNode c)
    {
        var tipo = c.S("tipo", "livro");
        if (tipo.Length == 0) tipo = "livro";
        var a = RefAutor(c);
        var t = Esc(c.S("titulo").Trim().Length > 0 ? c.S("titulo").Trim() : "[Título]");
        var sub = Esc(c.S("subtitulo").Trim());
        var ed = c.S("edicao").Trim();
        var local = Esc(c.S("local").Trim());
        var edit = Esc(c.S("editora").Trim());
        var vol = Esc(c.S("volume").Trim());
        var ano = Esc(AnoTxt(c));
        var aut = a.Length > 0 ? $"{Esc(a)}. " : "";
        var titB = $"<b>{t}</b>" + (sub.Length > 0 ? $": {sub}" : "");
        var titN = t + (sub.Length > 0 ? $": {sub}" : "");
        var eds = ed.Length > 0 && ed.All(char.IsDigit) ? $"{ed}. ed. " : ed.Length > 0 ? $"{Esc(ed)}. " : "";
        var loc = local.Length > 0 ? local : "[<i>S. l.</i>]";
        var semEd = edit.Length > 0 ? edit : "[<i>s. n.</i>]";
        string r = tipo switch
        {
            "livro" => $"{aut}{titB}. {eds}{loc}: {semEd}, {ano}.",
            "capitulo" => $"{aut}{titN}. <i>In</i>: {Esc(c.S("obra_autor"))} <b>{Esc(c.S("obra").Length > 0 ? c.S("obra") : "[Obra]")}"
                          + $"</b>. {eds}{loc}: {semEd}, {ano}.{(vol.Length > 0 ? " " + vol + "." : "")}",
            "artigo" => $"{aut}{titN}. <b>{(edit.Length > 0 ? edit : "[Periódico]")}</b>, {loc}{(vol.Length > 0 ? ", " + vol : "")}, {ano}.",
            "lei" => $"{aut}{titN}. <b>{(edit.Length > 0 ? edit : "Diário Oficial da União")}</b>: {(local.Length > 0 ? local : "Brasília, DF")}, {ano}.",
            "juris" => $"{aut}{titN}.{(vol.Length > 0 ? " " + vol + "." : "")} {(local.Length > 0 ? local : "Brasília, DF")}, {ano}.",
            "tese" => $"{aut}{titB}. {ano}. {(vol.Length > 0 ? vol : "Trabalho de Conclusão de Curso")} – {(edit.Length > 0 ? edit : "[Instituição]")}, {loc}, {ano}.",
            _ => $"{aut}{titB}. {loc}{(edit.Length > 0 ? ": " + edit : "")}, {ano}.",
        };
        var url = c.S("url").Trim();
        if (url.Length > 0) r += $" Disponível em: {Esc(url)}. Acesso em: {AcessoTxt(c.S("acesso"))}.";
        return Regex.Replace(r, @"\s{2,}", " ");
    }

    public static string ReferenciaPlain(JsonNode c) => HtmlToPlain(ReferenciaHtml(c));

    public static string FonteKey(JsonNode c) => Regex.Replace(ReferenciaPlain(c).ToLowerInvariant(), @"\W+", "");

    public static string TccBody(string html)
    {
        var m = Regex.Match(html ?? "", "<body[^>]*>(.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return m.Success ? m.Groups[1].Value : (html ?? "");
    }

    public static string Norm(string s)
    {
        var d = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in d)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && ch < 128) sb.Append(ch);
        return sb.ToString().ToLowerInvariant();
    }
}
