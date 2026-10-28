using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static LawService.Th;
using static LawService.U;

namespace LawService;

/// <summary>Dados do aplicativo — mesmo formato do estudos.json da versão Python.</summary>
public static class Store
{
    public static readonly HashSet<string> IMG_EXT = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    public static JsonObject Data = new();

    public static JsonObject Settings => Data.O("settings");

    /// <summary>Lê o JSON salvo no navegador (ou cria os dados de exemplo) e completa o que faltar.</summary>
    public static JsonObject LoadData(string json)
    {
        JsonObject data = null;
        if (!string.IsNullOrWhiteSpace(json))
        {
            try { data = JsonNode.Parse(json) as JsonObject; }
            catch { data = null; }
        }
        data ??= SampleData();
        return Normalize(data);
    }

    public static JsonObject Normalize(JsonObject data)
    {
        foreach (var k in new[] { "notebooks", "questions", "decks" })
            if (data[k] is not JsonArray) data[k] = new JsonArray();
        foreach (var k in new[] { "area_colors", "settings" })
            if (data[k] is not JsonObject) data[k] = new JsonObject();
        if (data["estagio"] is not JsonObject) data["estagio"] = new JsonObject { ["cases"] = new JsonArray() };
        if (data.I("version", 1) < 2) MigrateV1(data);
        return data;
    }

    public static string Serialize(JsonObject data = null) => (data ?? Data).ToJsonString(J.Compact);

    // ─────────────────────────── migração da versão Tkinter
    public static string RunsToHtml(JsonArray runs)
    {
        var lines = new List<List<(string kind, JsonNode val, HashSet<string> g)>> { new() };
        foreach (var r in runs.Objs())
        {
            var g = new HashSet<string>(r.A("g").Strs());
            if (r.Has("img"))
            {
                lines[^1].Add(("img", r, g));
                continue;
            }
            var parts = r.S("t").Split('\n');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) lines.Add(new());
                if (parts[i].Length > 0) lines[^1].Add(("t", JsonValue.Create(parts[i]), g));
            }
        }
        var outp = new StringBuilder();
        foreach (var line in lines)
        {
            var tags = new HashSet<string>(line.SelectMany(s => s.g));
            var tag = new[] { "h1", "h2", "h3" }.FirstOrDefault(tags.Contains) ?? "p";
            var bstyle = new List<string> { "margin-top:0px", "margin-bottom:4px", "line-height:135%" };
            if (tags.Contains("center")) bstyle.Add("text-align:center");
            if (tags.Contains("right")) bstyle.Add("text-align:right");
            var box = tags.FirstOrDefault(x => x.StartsWith("box_"));
            if (box != null) bstyle.Add($"background-color:{box[4..]}");
            var segs = new StringBuilder();
            foreach (var (kind, val, g) in line)
            {
                if (kind == "img")
                {
                    int w = val.I("w", 0);
                    segs.Append($"<img src=\"{Esc(val.S("img"))}\" width=\"{(w > 0 ? w : 400)}\" />");
                    continue;
                }
                var st = new List<string>();
                if (g.Contains("b") || tag != "p") st.Add("font-weight:700");
                if (g.Contains("i")) st.Add("font-style:italic");
                var deco = new List<string>();
                foreach (var (k, d) in new[] { ("u", "underline"), ("s", "line-through"), ("done", "line-through") })
                    if (g.Contains(k) && !deco.Contains(d)) deco.Add(d);
                if (deco.Count > 0) st.Add("text-decoration:" + string.Join(" ", deco));
                foreach (var x in g)
                {
                    if (x.StartsWith("fg_")) st.Add($"color:{x[3..]}");
                    else if (x.StartsWith("bg_")) st.Add($"background-color:{x[3..]}");
                    else if (x.StartsWith("sz_")) st.Add($"font-size:{x[3..]}pt");
                    else if (x.StartsWith("fam_")) st.Add($"font-family:'{x[4..]}'");
                }
                if (g.Contains("code")) st.Add($"font-family:'{MONO_FONT}'; color:#c2255c; background-color:#f1f3f7");
                if (tag != "p") st.Add($"font-size:{HEAD_PT[tag[1] - '0']}pt");
                var txt = Esc(J.Str(val));
                segs.Append(st.Count > 0 ? $"<span style=\"{string.Join("; ", st)}\">{txt}</span>" : txt);
            }
            var inner = segs.Length > 0 ? segs.ToString() : "<br />";
            outp.Append($"<{tag} style=\"{string.Join("; ", bstyle)}\">{inner}</{tag}>");
        }
        return "<html><body>" + outp + "</body></html>";
    }

    public static void MigrateV1(JsonObject data)
    {
        void Walk(JsonArray pages)
        {
            foreach (var p in pages.Objs())
            {
                if (p.Has("content") && !p.Has("html"))
                {
                    p["html"] = RunsToHtml(p["content"] as JsonArray ?? new JsonArray());
                    p.Remove("content");
                    p["plain"] = "";
                }
                if (!p.Has("html")) p["html"] = "";
                Walk(p.A("children"));
            }
        }
        foreach (var nb in data.A("notebooks").Objs()) Walk(nb.A("pages"));
        foreach (var q in data.A("questions").Objs())
        {
            if (q.Has("fonte") && !q.Has("banca"))
            {
                q["banca"] = q.S("fonte");
                q.Remove("fonte");
            }
            if (q["alternativas"] is not JsonArray)
            {
                var alts = q.S("alternativas");
                var lst = new List<string>();
                foreach (var ln0 in alts.Split('\n'))
                {
                    var ln = Regex.Replace(ln0, @"^\s*[A-Ea-e]\s*[\)\.\-:]\s*", "").Trim();
                    if (ln.Length > 0) lst.Add(ln);
                }
                q["alternativas"] = J.Arr(lst.Take(5));
            }
            var g = q.S("gabarito").Trim();
            var gl = g.ToLowerInvariant();
            if (new[] { "certo", "errado", "c", "e" }.Contains(gl) && q.A("alternativas").Count == 0)
            {
                q["tipo"] = "ce";
                q["gabarito"] = gl.StartsWith("c") ? "C" : "E";
            }
            else
            {
                q["tipo"] = "me";
                q["gabarito"] = g.Length > 0 ? g[..1].ToUpperInvariant() : "";
            }
            if (!q.Has("ano")) q["ano"] = "";
            if (!q.Has("resposta")) q["resposta"] = null;
            if (!q.Has("historico")) q["historico"] = new JsonArray();
        }
        int[] iv = { 0, 1, 2, 4, 8, 16, 32 };
        foreach (var d in data.A("decks").Objs())
            foreach (var c in d.A("cards").Objs())
            {
                if (c.Has("box"))
                {
                    int b = c.I("box");
                    c.Remove("box");
                    c["interval"] = iv[Math.Min(Math.Max(b, 0), 6)];
                    c["ease"] = 2.5;
                    c["reps"] = c.I("right") + c.I("wrong");
                    c["lapses"] = c.I("wrong");
                    c.Remove("wrong");
                    c.Remove("right");
                }
            }
        data["version"] = 2;
    }

    public static void FillPlain(JsonObject data)
    {
        void Walk(JsonArray pages)
        {
            foreach (var p in pages.Objs())
            {
                if (!p.Ok("plain") && p.Ok("html"))
                {
                    var t = HtmlToPlain(p.S("html"));
                    p["plain"] = t.Length > 20000 ? t[..20000] : t;
                }
                Walk(p.A("children"));
            }
        }
        foreach (var nb in data.A("notebooks").Objs()) Walk(nb.A("pages"));
    }

    // ─────────────────────────── dados de exemplo
    public static JsonObject SampleData()
    {
        var n = NowIso();
        const string Pa = "style=\"margin-top:0px; margin-bottom:4px; line-height:135%;\"";

        string H(int level, string text) =>
            $"<h{level} style=\"margin-top:14px; margin-bottom:6px;\"><span style=\"font-size:{HEAD_PT[level]}pt; font-weight:700;\">{text}</span></h{level}>";

        string Callout(string color, string inner) =>
            $"<table border=\"0\" width=\"100%\" cellpadding=\"14\" bgcolor=\"{color}\" style=\"margin-top:6px; margin-bottom:8px;\"><tr><td><p {Pa}>{inner}</p></td></tr></table>";

        var welcome =
            H(1, "Bem-vindo ao seu caderno")
            + $"<p {Pa}>Este editor funciona como o OneNote. Selecione um trecho e use a barra acima — ou a barrinha "
            + "que aparece sobre a seleção — para aplicar <b>negrito</b>, <i>itálico</i>, "
            + "<span style=\"color:#e03131; font-weight:700;\">cores</span>, "
            + "<span style=\"background-color:#fff3a3;\">marca-texto</span> e "
            + "<span style=\"background-color:#a5d8ff;\">muito mais</span>.</p>"
            + Callout("#eef2ff", "<b>Dica:</b> cole prints com <b>Ctrl+V</b>. Clique com o botão direito numa imagem "
                                 + "para mudar o tamanho.")
            + H(2, "Atalhos de digitação")
            + $"<ul style=\"margin-top:0px;\"><li {Pa}>Digite <b>- </b> no começo da linha para criar uma lista</li>"
            + $"<li {Pa}>Digite <b>1. </b> para uma lista numerada e <b>[] </b> para tarefas</li>"
            + $"<li {Pa}>Digite <b># </b> para um título e <b>&gt; </b> para uma citação</li>"
            + $"<li {Pa}><b>Ctrl+Shift+V</b> cola sem formatação</li></ul>"
            + H(2, "Checklist")
            + $"<ul style=\"margin-top:0px;\"><li class=\"checked\" {Pa}><span style=\"text-decoration:line-through; "
            + "color:#a3a8b8;\">Abrir o aplicativo</span></li>"
            + $"<li class=\"unchecked\" {Pa}>Criar meu primeiro caderno</li>"
            + $"<li class=\"unchecked\" {Pa}>Cadastrar 5 questões de Direito Penal</li></ul>";
        var teoria =
            H(1, "Teoria do Crime")
            + $"<p {Pa}>Pelo conceito analítico (corrente tripartite), crime é fato "
            + "<span style=\"color:#1971c2; font-weight:700;\">típico</span>, "
            + "<span style=\"color:#2f9e44; font-weight:700;\">ilícito</span> e "
            + "<span style=\"color:#e03131; font-weight:700;\">culpável</span>.</p>"
            + Callout("#e7f5ff", "<b>Fato típico:</b> conduta, resultado, nexo causal e tipicidade.")
            + Callout("#ebfbee", "<b>Ilicitude:</b> contrariedade ao ordenamento — excludentes do art. 23 do CP.")
            + Callout("#fff0f6", "<b>Culpabilidade:</b> imputabilidade, potencial consciência da ilicitude e "
                                 + "exigibilidade de conduta diversa.");
        var tipicidade =
            H(1, "Tipicidade")
            + $"<p {Pa}><b>Tipicidade formal:</b> adequação do fato à norma penal.</p>"
            + $"<p {Pa}><b>Tipicidade material:</b> lesão ou perigo de lesão relevante ao bem jurídico "
            + "<span style=\"color:#868e96; font-style:italic;\">(princípio da insignificância)</span>.</p>";

        JsonObject Page(string title, string color, string html, params JsonObject[] children) => new()
        {
            ["id"] = Uid(), ["title"] = title, ["color"] = color, ["html"] = html, ["plain"] = "",
            ["children"] = new JsonArray(children.Cast<JsonNode>().ToArray()), ["created"] = n, ["updated"] = n,
        };

        JsonObject Card(string f, string b) => new()
        {
            ["id"] = Uid(), ["front"] = f, ["back"] = b, ["interval"] = 0, ["ease"] = 2.5, ["due"] = "", ["reps"] = 0,
            ["lapses"] = 0, ["created"] = n,
        };

        return new JsonObject
        {
            ["version"] = 2,
            ["notebooks"] = new JsonArray(
                new JsonObject
                {
                    ["id"] = Uid(), ["name"] = "Meu caderno", ["color"] = "#5b5ef0", ["created"] = n,
                    ["pages"] = new JsonArray(Page("Bem-vindo", "#5b5ef0", welcome)),
                },
                new JsonObject
                {
                    ["id"] = Uid(), ["name"] = "Direito Penal", ["color"] = "#e5484d", ["created"] = n,
                    ["pages"] = new JsonArray(Page("Teoria do Crime", "#e5484d", teoria, Page("Tipicidade", "#f76707", tipicidade))),
                }),
            ["questions"] = new JsonArray(
                new JsonObject
                {
                    ["id"] = Uid(), ["area"] = "Direito Penal", ["tema"] = "Teoria do Crime", ["banca"] = "Exemplo", ["ano"] = "2026",
                    ["tipo"] = "me",
                    ["enunciado"] = "De acordo com a corrente tripartite, adotada majoritariamente pela doutrina brasileira, o crime é composto por:",
                    ["alternativas"] = J.Arr(new[] { "Fato típico e ilícito.", "Fato típico, ilícito e culpável.",
                        "Fato típico, culpável e punível.", "Conduta, resultado e nexo causal.",
                        "Tipicidade, antijuridicidade e punibilidade." }),
                    ["gabarito"] = "B", ["resultado"] = "", ["resposta"] = null, ["historico"] = new JsonArray(), ["created"] = n,
                    ["updated"] = n,
                    ["comments"] = new JsonArray(new JsonObject
                    {
                        ["id"] = Uid(), ["date"] = n,
                        ["text"] = "A culpabilidade integra o conceito analítico de crime na corrente tripartite.",
                    }),
                },
                new JsonObject
                {
                    ["id"] = Uid(), ["area"] = "Direito Constitucional", ["tema"] = "Direitos Fundamentais", ["banca"] = "Exemplo",
                    ["ano"] = "2025", ["tipo"] = "ce",
                    ["enunciado"] = "As normas definidoras dos direitos e garantias fundamentais têm aplicação imediata.",
                    ["alternativas"] = new JsonArray(), ["gabarito"] = "C", ["resultado"] = "", ["resposta"] = null,
                    ["historico"] = new JsonArray(), ["created"] = n, ["updated"] = n, ["comments"] = new JsonArray(),
                }),
            ["area_colors"] = new JsonObject { ["Direito Penal"] = "#e5484d", ["Direito Constitucional"] = "#1c7ed6" },
            ["decks"] = new JsonArray(new JsonObject
            {
                ["id"] = Uid(), ["name"] = "Direito Penal", ["color"] = "#e5484d", ["created"] = n,
                ["cards"] = new JsonArray(
                    Card("Quais são os elementos do fato típico?", "Conduta, resultado, nexo causal e tipicidade."),
                    Card("O que é crime impossível? (art. 17 do CP)",
                        "Quando, por ineficácia absoluta do meio ou por absoluta impropriedade do objeto, é impossível consumar-se o crime."),
                    Card("Diferença entre dolo direto e dolo eventual?",
                        "No dolo direto o agente quer o resultado; no eventual, assume o risco de produzi-lo (art. 18, I, do CP).")),
            }),
            ["settings"] = new JsonObject(),
        };
    }
}
