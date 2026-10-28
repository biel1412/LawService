using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO SIMULADO — motor de casos

/// <summary>Gerador aleatório com a mesma interface do random do Python.</summary>
public class Rnd
{
    readonly Random _r;
    public Rnd(int? seed = null) => _r = seed.HasValue ? new Random(seed.Value) : new Random();
    public int randint(int a, int b) => _r.Next(a, b + 1);
    public long randlong(long a, long b) => a + (long)(_r.NextDouble() * (b - a + 1));
    public double uniform(double a, double b) => a + (b - a) * _r.NextDouble();
    public double random() => _r.NextDouble();
    public T choice<T>(IList<T> l) => l[_r.Next(l.Count)];
    public string choice(string s) => s[_r.Next(s.Length)].ToString();
    public List<T> sample<T>(IList<T> l, int k)
    {
        var pool = l.ToList();
        var outp = new List<T>();
        for (int i = 0; i < k && pool.Count > 0; i++)
        {
            int j = _r.Next(pool.Count);
            outp.Add(pool[j]);
            pool.RemoveAt(j);
        }
        return outp;
    }
}

/// <summary>Pessoa física gerada para o caso.</summary>
public class Pes
{
    public string nome, primeiro, o, O, ele, do_, ao, nosso, Nosso, sr, cpf, prof, ec, init;
    public bool f;
}

public class Adv
{
    public string nome, oab;
}

public class UfInfo
{
    public string uf, cidade, estado, tj, tjc, trf, trfc, sj, jfo, trt, trtc, mun;

    public JsonObject ToJson() => new()
    {
        ["uf"] = uf, ["cidade"] = cidade, ["estado"] = estado, ["tj"] = tj, ["tjc"] = tjc, ["trf"] = trf, ["trfc"] = trfc, ["sj"] = sj,
        ["jfo"] = jfo, ["trt"] = trt, ["trtc"] = trtc, ["mun"] = mun,
    };
}

public class SocioInfo
{
    public string nome, curto, email, cargo, cor, oab;
    public bool f;

    public SocioInfo Clone() => (SocioInfo)MemberwiseClone();

    public JsonObject ToJson() => new()
    {
        ["nome"] = nome, ["curto"] = curto, ["email"] = email, ["cargo"] = cargo, ["cor"] = cor, ["f"] = f, ["oab"] = oab,
    };
}

public static class Est
{
    // Link do formulário de correção profissional (página de pagamento + formulário de dados).
    // Coloque aqui o seu link para que todos os alunos usem o mesmo. Se ficar vazio, o aluno pode
    // configurar em Estágio > engrenagem.
    public const string CORRECAO_URL = "";

    public const string FIRMA = "Almeida, Moraes & Duarte Advogados Associados";
    public const string FIRMA_DOMINIO = "amd.adv.br";
    public const string SISTEMA = "SIPE";
    public const string SISTEMA_NOME = "Processo Eletrônico";
    public const string OI = "Oi, %%NOME%%, tudo bem?";

    public static readonly Dictionary<string, SocioInfo> SOCIOS = new()
    {
        ["civel"] = new SocioInfo { nome = "Dra. Helena Moraes", curto = "Helena", email = "helena.moraes", cargo = "Sócia · Cível, Consumidor e Família", cor = "#d6409f", f = true },
        ["penal"] = new SocioInfo { nome = "Dr. Ricardo Almeida", curto = "Ricardo", email = "ricardo.almeida", cargo = "Sócio · Direito Penal", cor = "#e5484d", f = false },
        ["trabalho"] = new SocioInfo { nome = "Dra. Camila Duarte", curto = "Camila", email = "camila.duarte", cargo = "Sócia · Trabalhista e Previdenciário", cor = "#2f9e44", f = true },
        ["publico"] = new SocioInfo { nome = "Dr. Fábio Nogueira", curto = "Fábio", email = "fabio.nogueira", cargo = "Sócio · Direito Público e Tributário", cor = "#1c7ed6", f = false },
    };

    public static readonly UfInfo[] SIM_UFS =
    {
        new() { uf = "RS", cidade = "Porto Alegre", estado = "do Rio Grande do Sul", tj = "Tribunal de Justiça do Estado do Rio Grande do Sul", tjc = "21",
            trf = "Tribunal Regional Federal da 4ª Região", trfc = "04", sj = "Seção Judiciária do Rio Grande do Sul", jfo = "7100",
            trt = "Tribunal Regional do Trabalho da 4ª Região", trtc = "04", mun = "Município de Porto Alegre" },
        new() { uf = "PR", cidade = "Curitiba", estado = "do Paraná", tj = "Tribunal de Justiça do Estado do Paraná", tjc = "16",
            trf = "Tribunal Regional Federal da 4ª Região", trfc = "04", sj = "Seção Judiciária do Paraná", jfo = "7000",
            trt = "Tribunal Regional do Trabalho da 9ª Região", trtc = "09", mun = "Município de Curitiba" },
        new() { uf = "SC", cidade = "Florianópolis", estado = "de Santa Catarina", tj = "Tribunal de Justiça do Estado de Santa Catarina", tjc = "24",
            trf = "Tribunal Regional Federal da 4ª Região", trfc = "04", sj = "Seção Judiciária de Santa Catarina", jfo = "7200",
            trt = "Tribunal Regional do Trabalho da 12ª Região", trtc = "12", mun = "Município de Florianópolis" },
        new() { uf = "SP", cidade = "São Paulo", estado = "de São Paulo", tj = "Tribunal de Justiça do Estado de São Paulo", tjc = "26",
            trf = "Tribunal Regional Federal da 3ª Região", trfc = "03", sj = "Seção Judiciária de São Paulo", jfo = "6301",
            trt = "Tribunal Regional do Trabalho da 2ª Região", trtc = "02", mun = "Município de São Paulo" },
        new() { uf = "MG", cidade = "Belo Horizonte", estado = "de Minas Gerais", tj = "Tribunal de Justiça do Estado de Minas Gerais", tjc = "13",
            trf = "Tribunal Regional Federal da 6ª Região", trfc = "06", sj = "Seção Judiciária de Minas Gerais", jfo = "3800",
            trt = "Tribunal Regional do Trabalho da 3ª Região", trtc = "03", mun = "Município de Belo Horizonte" },
    };

    public static readonly string[] NOMES_M = { "João Pedro", "Carlos Eduardo", "Rafael", "Lucas", "Marcos", "André", "Felipe", "Gustavo", "Rodrigo",
        "Paulo Henrique", "Eduardo", "Thiago", "Bruno", "Diego", "Leandro", "Vinícius", "Matheus", "Daniel",
        "Fábio", "Roberto", "Sérgio", "Otávio", "Renan", "Alexandre" };
    public static readonly string[] NOMES_F = { "Mariana", "Fernanda", "Juliana", "Camila", "Patrícia", "Aline", "Beatriz", "Larissa", "Vanessa",
        "Renata", "Luciana", "Gabriela", "Tatiane", "Daniela", "Priscila", "Simone", "Cristina", "Letícia",
        "Natália", "Bruna", "Carolina", "Sabrina", "Débora", "Elaine" };
    public static readonly string[] SOBRENOMES = { "Silva", "Souza", "Oliveira", "Pereira", "Costa", "Rodrigues", "Nascimento", "Lima", "Araújo",
        "Fernandes", "Carvalho", "Gomes", "Martins", "Rocha", "Ribeiro", "Barbosa", "Moreira", "Cardoso",
        "Mendes", "Freitas", "Vieira", "Pinto", "Correia", "Campos", "Machado", "Rezende", "Batista",
        "Monteiro", "Cavalcanti", "Dias", "Ramos", "Lopes", "Farias", "Prado", "Brandão", "Siqueira" };
    public static readonly (string, string)[] PROFISSOES = { ("vendedor", "vendedora"), ("auxiliar administrativo", "auxiliar administrativa"),
        ("motorista", "motorista"), ("professor", "professora"), ("técnico de enfermagem", "técnica de enfermagem"),
        ("eletricista", "eletricista"), ("cozinheiro", "cozinheira"), ("operador de caixa", "operadora de caixa"),
        ("recepcionista", "recepcionista"), ("analista de sistemas", "analista de sistemas") };

    public static string Up(this string s) => (s ?? "").ToUpperInvariant();

    public static string GenCpf(Rnd r)
    {
        var n = Enumerable.Range(0, 9).Select(_ => r.randint(0, 9)).ToList();
        for (int k = 0; k < 2; k++)
        {
            int s = 0;
            for (int i = 0; i < n.Count; i++) s += n[i] * (n.Count + 1 - i);
            int d = (s * 10) % 11;
            n.Add(d == 10 ? 0 : d);
        }
        var st = string.Concat(n);
        return $"{st[..3]}.{st[3..6]}.{st[6..9]}-{st[9..]}";
    }

    public static string GenCnpj(Rnd r)
    {
        var n = Enumerable.Range(0, 8).Select(_ => r.randint(0, 9)).Concat(new[] { 0, 0, 0, 1 }).ToList();
        foreach (var w in new[] { new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 } })
        {
            int s = 0;
            for (int i = 0; i < w.Length; i++) s += n[i] * w[i];
            s %= 11;
            n.Add(s < 2 ? 0 : 11 - s);
        }
        var st = string.Concat(n);
        return $"{st[..2]}.{st[2..5]}.{st[5..8]}/{st[8..12]}-{st[12..]}";
    }

    public static int CnjDv(string base_) => 98 - (int)(BigInteger.Parse(base_ + "00") % 97);

    /// <summary>Número único do processo (Res. CNJ 65/2008) com dígito verificador válido.</summary>
    public static string GenCnj(Rnd r, string j, string tr, string origem, int year)
    {
        int seq = r.randint(5000000, 5099999);
        var b = $"{seq:D7}{year:D4}{j}{tr}{origem}";
        int dv = CnjDv(b);
        return $"{seq:D7}-{dv:D2}.{year}.{j}.{tr}.{origem}";
    }

    public static string P(params string[] paras) => Pp(true, paras);
    public static string Pn(params string[] paras) => Pp(false, paras);

    static string Pp(bool indent, string[] paras)
    {
        var st = indent ? "text-indent:36px; " : "";
        return string.Concat(paras.Select(p => $"<p style=\"{st}margin:0 0 10px 0; line-height:150%;\" align=\"justify\">{p}</p>"));
    }

    public static string NossoCli(Pes p) => $"{p.nosso} nov{p.o}";

    public static Adv AdvNosso(Ctx c) => new() { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab };

    public static string EndJuiz(Ctx c) => $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {c.meta.S("orgao").Up()}";
}

/// <summary>Constrói um caso: partes, eventos com datas úteis, documentos, tarefa e e-mail do sócio.</summary>
public class Ctx
{
    public readonly Rnd r;
    public readonly UfInfo u;
    public readonly SocioInfo socio;
    public readonly string area;
    public readonly List<JsonObject> events = new();
    public readonly List<JsonObject> anexos = new();
    public JsonObject prazo;
    readonly HashSet<string> used = new();
    DateTime cur;
    public readonly JsonObject meta;
    List<string> emailParas = new();
    string subject = "";
    (string, string)? _renum;

    public Ctx(Rnd rnd, string socioKey, string area)
    {
        r = rnd;
        u = rnd.choice(Est.SIM_UFS);
        socio = Est.SOCIOS[socioKey].Clone();
        socio.oab = $"OAB/{u.uf} {rnd.randint(41, 129)}.{rnd.randint(100, 999)}";
        this.area = area;
        cur = Today().AddDays(-600);
        meta = new JsonObject
        {
            ["area"] = area, ["segredo"] = false, ["lab"] = new JsonArray("AUTOR", "RÉU"), ["ativo"] = new JsonArray(), ["passivo"] = new JsonArray(),
        };
    }

    // ── pessoas e valores
    public Pes pf(string sexo = null, string prof = null)
    {
        sexo ??= r.choice("MF");
        bool f = sexo == "F";
        string nome;
        while (true)
        {
            nome = $"{r.choice(f ? Est.NOMES_F : Est.NOMES_M)} {string.Join(" ", r.sample(Est.SOBRENOMES, 2))}";
            if (!used.Contains(nome)) break;
        }
        used.Add(nome);
        if (prof == null)
        {
            var pr = r.choice(Est.PROFISSOES);
            prof = f ? pr.Item2 : pr.Item1;
        }
        return new Pes
        {
            nome = nome, primeiro = nome.Split(' ')[0], f = f, o = f ? "a" : "o", O = f ? "A" : "O", ele = f ? "ela" : "ele", do_ = f ? "da" : "do",
            ao = f ? "à" : "ao", nosso = f ? "nossa" : "nosso", Nosso = f ? "Nossa" : "Nosso", sr = f ? "Sra." : "Sr.", cpf = Est.GenCpf(r),
            prof = prof, ec = r.choice(new[] { "solteir", "casad", "divorciad" }) + (f ? "a" : "o"),
            init = string.Join(".", nome.Split(' ').Select(w => w[0].ToString())) + ".",
        };
    }

    public Adv adv()
    {
        var p = pf();
        return new Adv { nome = p.nome, oab = $"OAB/{u.uf} {r.randint(20, 140)}.{r.randint(100, 999)}" };
    }

    public JsonObject juiz(string cargoM = "Juiz de Direito", string cargoF = "Juíza de Direito")
    {
        var p = pf();
        return new JsonObject { ["nome"] = p.nome, ["cargo"] = p.f ? cargoF : cargoM };
    }

    public double money(double a, double b) => Math.Round(r.uniform(a, b), 2);

    public string ordm(int a = 1, int b = 8) => $"{r.randint(a, b)}";

    // ── tokens de data (resolvidos no fim): §Dn§ evento n, §An§ n dias antes do evento 1, §Bn§ n dias antes de hoje
    public int nxt => events.Count + 1;

    public string D(int? n = null) => $"§D{n ?? nxt}§";

    public static string A(int n) => $"§A{n}§";

    public static string B(int n) => $"§B{n}§";

    // ── eventos
    public int ev(string desc, string user = "SISTEMA", IEnumerable<JsonObject> docs = null, string detail = "", (int, int)? gap = null)
    {
        var g = gap ?? (1, 6);
        if (events.Count > 0) cur = g.Item2 > 0 ? AddBd(cur, r.randint(g.Item1, g.Item2)) : cur;
        int n = nxt;
        var e = new JsonObject
        {
            ["n"] = n, ["date"] = IsoDate(cur),
            ["time"] = $"{r.randint(8, 18):D2}:{r.randint(0, 59):D2}:{r.randint(0, 59):D2}",
            ["desc"] = desc, ["user"] = user, ["detail"] = detail,
            ["docs"] = new JsonArray((docs ?? Enumerable.Empty<JsonObject>()).Cast<JsonNode>().ToArray()),
        };
        events.Add(e);
        return n;
    }

    public int intimacao(string quem, int dias, string contagem = "uteis", (int, int)? gap = null, bool nossa = true)
    {
        int n1 = ev("INTIMAÇÃO ELETRÔNICA - EXPEDIDA/CERTIFICADA", "SISTEMA", detail: $"{quem} — Prazo: {dias} dias", gap: (0, 1));
        int n2 = ev("CONFIRMADA A INTIMAÇÃO ELETRÔNICA", "SISTEMA", detail: $"Refer. ao Evento {n1}", gap: gap ?? (1, 4));
        if (nossa) prazo = new JsonObject { ["ev"] = n2, ["dias"] = dias, ["contagem"] = contagem };
        return n2;
    }

    public int citacao(string quem, (int, int)? gap = null)
    {
        int n1 = ev("EXPEDIDA/CERTIFICADA A CITAÇÃO ELETRÔNICA", detail: quem, gap: gap ?? (1, 3));
        return ev("CONFIRMADA A CITAÇÃO ELETRÔNICA", detail: $"Refer. ao Evento {n1}", gap: (1, 5));
    }

    // ── documentos
    string _header()
    {
        var m = meta;
        var lines = new List<string> { "PODER JUDICIÁRIO" };
        if (m.S("justica") == "federal") lines.AddRange(new[] { "JUSTIÇA FEDERAL", u.sj.Up() });
        else if (m.S("justica") == "trabalho") lines.AddRange(new[] { "JUSTIÇA DO TRABALHO", u.trt.Up() });
        else lines.Add(u.tj.Up());
        lines.Add(m.S("orgao"));
        return string.Join("<br>", lines.Select((x, i) => i < lines.Count - 1 ? $"<b>{Esc(x)}</b>" : Esc(x)));
    }

    string _partes_lines()
    {
        var lab = meta.A("lab").Strs();
        var a = string.Join(", ", meta.A("ativo").Objs().Select(x => x.S("nome")));
        var p = string.Join(", ", meta.A("passivo").Objs().Select(x => x.S("nome")));
        return $"<b>{lab[0]}</b>: {Esc(a.Up())}<br><b>{lab[1]}</b>: {Esc(p.Up())}";
    }

    public JsonObject doc_j(string code, string titulo, string paras, int? n = null)
    {
        int nn = n ?? nxt;
        var cod = $"{r.randlong(100000000000L, 999999999999L)}v{r.randint(2, 9)}";
        var crc = $"{r.randlong(0, 0xFFFFFFFFL):X8}";
        var juiz = meta.O("juiz");
        var html = $"<p align=\"center\" style=\"margin:0 0 18px 0; line-height:140%;\">{_header()}</p>"
                   + $"<p style=\"margin:0 0 4px 0;\"><b>{Esc(meta.S("classe"))} Nº {meta.S("numero")}/{u.uf}</b></p>"
                   + $"<p style=\"margin:0 0 18px 0; line-height:150%;\">{_partes_lines()}</p>"
                   + $"<p align=\"center\" style=\"margin:0 0 16px 0;\"><b>{Esc(titulo.Up())}</b></p>"
                   + paras
                   + "<p style=\"margin:24px 0 0 0; color:#555; font-size:8.5pt; line-height:140%;\">Documento eletrônico "
                   + $"assinado por <b>{Esc(juiz.S("nome").Up())}</b>, <b>{Esc(juiz.S("cargo"))}</b>, "
                   + "na forma do artigo 1º, inciso III, da Lei 11.419, de 19 de dezembro de 2006. A conferência da "
                   + $"autenticidade do documento está disponível no sistema {Est.SISTEMA} (ambiente de simulação), mediante o "
                   + $"preenchimento do código verificador <b>{cod}</b> e do código CRC <b>{crc}</b>.<br>"
                   + $"Data e hora: {D(nn)}.</p>";
        return new JsonObject { ["code"] = code, ["title"] = titulo, ["html"] = html };
    }

    public JsonObject doc_p(string code, string titulo, string enderecamento, string paras, Adv adv, int? n = null, bool fecho = true)
    {
        int nn = n ?? nxt;
        var html = $"<p style=\"margin:0 0 26px 0; line-height:150%;\"><b>{Esc(enderecamento)}</b></p>"
                   + $"<p style=\"margin:0 0 18px 0;\"><b>Processo nº {meta.S("numero")}</b></p>"
                   + $"<p align=\"center\" style=\"margin:0 0 16px 0;\"><b>{Esc(titulo.Up())}</b></p>" + paras;
        if (fecho)
            html += "<p style=\"margin:16px 0 0 0;\">Nestes termos, pede deferimento.</p>"
                    + $"<p style=\"margin:12px 0 0 0;\">{Esc(u.cidade)}/{u.uf}, {D(nn)}.</p>"
                    + $"<p style=\"margin:26px 0 0 0;\"><b>{Esc(adv.nome.Up())}</b><br>{Esc(adv.oab)}</p>";
        return new JsonObject { ["code"] = code, ["title"] = titulo, ["html"] = html };
    }

    public JsonObject doc_s(string code, string titulo, string html) => new()
    {
        ["code"] = code, ["title"] = titulo,
        ["html"] = $"<p align=\"center\" style=\"margin:0 0 16px 0;\"><b>{Esc(titulo.Up())}</b></p>" + html,
    };

    public JsonObject procuracao(Pes cli, string code = "PROC2")
    {
        var nomeSocio = socio.nome.StartsWith("Dra.") ? socio.nome[5..] : socio.nome[4..];
        return doc_s(code, "Procuração", Est.Pn(
            $"<b>OUTORGANTE:</b> {Esc(cli.nome)}, {cli.ec}, {Esc(cli.prof)}, inscrit{cli.o} no CPF sob o "
            + $"nº {cli.cpf}, residente em {Esc(u.cidade)}/{u.uf}.",
            $"<b>OUTORGADOS:</b> {Esc(nomeSocio)}, {socio.oab}, e demais advogados de {Est.FIRMA}.",
            "<b>PODERES:</b> para o foro em geral, com a cláusula <i>ad judicia et extra</i>, podendo propor ações, "
            + "contestar, recorrer, transigir, desistir, receber e dar quitação, e substabelecer."));
    }

    public void anexo(string titulo, string html) =>
        anexos.Add(new JsonObject { ["code"] = $"ANEXO{anexos.Count + 1}", ["title"] = titulo, ["html"] = html });

    // ── metadados e tarefa
    public void processo(string justica, string classe, string orgao, string[] assuntos, double? valor, JsonObject juiz = null,
        (string, string)? lab = null, string competencia = "", bool segredo = false, int? ano = null)
    {
        int year = ano ?? Today().Year - r.choice(new[] { 0, 1, 1, 2 });
        string num;
        if (justica == "federal") num = Est.GenCnj(r, "4", u.trfc, u.jfo, year);
        else if (justica == "trabalho") num = Est.GenCnj(r, "5", u.trtc, $"{r.randint(1, 30):D4}", year);
        else num = Est.GenCnj(r, "8", u.tjc, $"{r.choice(new[] { 1, 1, 1, 8, 10, 21, 33 }):D4}", year);
        var lb = lab ?? ("AUTOR", "RÉU");
        meta["justica"] = justica;
        meta["classe"] = classe;
        meta["orgao"] = orgao;
        meta["assuntos"] = J.Arr(assuntos);
        meta["valor"] = valor;
        meta["lab"] = new JsonArray(lb.Item1, lb.Item2);
        meta["juiz"] = juiz ?? this.juiz();
        meta["competencia"] = string.IsNullOrEmpty(competencia) ? orgao : competencia;
        meta["segredo"] = segredo;
        meta["numero"] = num;
    }

    public void parte(string polo, string nome, string doc, IEnumerable<string> advs, bool cliente = false) =>
        meta.A(polo == "a" ? "ativo" : "passivo").Add(new JsonObject
        {
            ["nome"] = nome, ["doc"] = doc, ["advs"] = J.Arr(advs), ["cliente"] = cliente,
        });

    public List<string> nossos_advs() =>
        new() { $"{(!socio.f ? socio.nome[4..].Trim() : socio.nome[5..].Trim())} ({socio.oab})" };

    public void tarefa(string peca, string tipo, int dias, string contagem, string fundamento, string a_quo, string ad_quem = "",
        string code = "PET", int? interno = null)
    {
        meta["tarefa"] = new JsonObject
        {
            ["peca"] = peca, ["tipo"] = tipo, ["dias"] = dias, ["contagem"] = contagem, ["fundamento"] = fundamento, ["a_quo"] = a_quo,
            ["ad_quem"] = ad_quem, ["code"] = code, ["interno"] = interno,
        };
    }

    // ── finalização: ajusta datas para "agora", calcula prazos e troca os tokens
    public JsonObject build(string cliente, string polo, string titulo)
    {
        var t = meta.O("tarefa");
        var hoje = Today();
        DateTime first;
        var contagem = t.S("contagem");
        int dias = t.I("dias");
        if (events.Count > 0)
        {
            var last = ParseDate(events[^1].S("date"));
            int back = dias >= 10 || (contagem != "uteis" && contagem != "corridos") ? r.randint(1, 3) : r.randint(0, 1);
            var target = back == 0 ? WeekdayFwd(hoje) : SubBd(hoje, back);
            if (target > hoje) target = SubBd(hoje, 1);
            int shift = (target - last).Days;
            DateTime? prev = null;
            foreach (var e in events)
            {
                var d = WeekdayFwd(ParseDate(e.S("date")).AddDays(shift));
                if (prev.HasValue && d < prev.Value) d = prev.Value;
                e["date"] = IsoDate(d);
                prev = d;
            }
            first = ParseDate(events[0].S("date"));
            var num = meta.S("numero");
            var mm = Regex.Match(num, @"^(\d{7})-\d{2}\.(\d{4})\.(\d)\.(\d{2})\.(\d{4})$");
            if (mm.Success && int.Parse(mm.Groups[2].Value) != first.Year)
            {
                var b = $"{mm.Groups[1].Value}{first.Year}{mm.Groups[3].Value}{mm.Groups[4].Value}{mm.Groups[5].Value}";
                var novo = $"{mm.Groups[1].Value}-{Est.CnjDv(b):D2}.{first.Year}.{mm.Groups[3].Value}.{mm.Groups[4].Value}.{mm.Groups[5].Value}";
                _renum = (num, novo);
            }
        }
        else first = hoje;
        var dates = events.ToDictionary(e => e.I("n"), e => ParseDate(e.S("date")));
        DateTime pf;
        DateTime? pi = null;
        if ((contagem == "uteis" || contagem == "corridos") && prazo != null)
        {
            var bse = dates[prazo.I("ev")];
            if (contagem == "uteis")
            {
                pi = AddBd(bse, 1);
                pf = AddBd(bse, dias);
            }
            else
            {
                pi = bse.AddDays(1);
                pf = WeekdayFwd(bse.AddDays(dias));
            }
            var ev0 = events[prazo.I("ev") - 1];
            ev0["prazo"] = new JsonObject
            {
                ["dias"] = dias, ["contagem"] = contagem, ["inicio"] = IsoDate(pi.Value), ["fim"] = IsoDate(pf), ["status"] = "ABERTO",
            };
        }
        else pf = AddBd(hoje, dias);
        int left = (pf - hoje).Days;
        var lim = left > 4 ? SubBd(pf, 2) : left > 1 ? SubBd(pf, 1) : pf;
        if (t.I("interno") > 0) lim = AddBd(hoje, t.I("interno"));

        string Repl(string s)
        {
            if (s == null) return s;
            if (_renum is { } rn) s = s.Replace(rn.Item1, rn.Item2);
            if (!s.Contains('§')) return s;
            return Regex.Replace(s, "§(D|A|B|PF|PI|LIMX|LIM|AUD)(\\d*)§", m =>
            {
                var k = m.Groups[1].Value;
                var v = m.Groups[2].Value;
                switch (k)
                {
                    case "D": return Fdate(dates.TryGetValue(int.Parse(v), out var dd) ? dd : first);
                    case "A": return Fdate(WeekdayFwd(first.AddDays(-int.Parse(v))));
                    case "B":
                        int bv = int.Parse(v);
                        return Fdate(bv < 5 ? WeekdayFwd(hoje.AddDays(-bv)) : hoje.AddDays(-bv));
                    case "PF": return Fdate(pf);
                    case "PI": return Fdate(pi ?? hoje);
                    case "LIM": return Fdate(lim);
                    case "LIMX":
                        var wd = new[] { "segunda", "terça", "quarta", "quinta", "sexta", "sábado", "domingo" }[Wd(lim)];
                        return $"{wd}-feira ({Fdate(lim)})".Replace("sábado-feira", "sábado").Replace("domingo-feira", "domingo");
                    case "AUD": return Fdate(pf);
                }
                return m.Value;
            });
        }

        JsonNode Deep(JsonNode o)
        {
            switch (o)
            {
                case JsonObject ob:
                    var no = new JsonObject();
                    foreach (var kv in ob) no[kv.Key] = Deep(kv.Value);
                    return no;
                case JsonArray ar:
                    return new JsonArray(ar.Select(Deep).ToArray());
                case JsonValue jv when jv.TryGetValue<string>(out var s):
                    return JsonValue.Create(Repl(s));
                case null:
                    return null;
                default:
                    return o.DeepClone();
            }
        }

        var anexCodes = anexos.Select(a => a.S("code")).ToList();
        var caseObj = new JsonObject
        {
            ["id"] = Uid(), ["created"] = NowIso(), ["status"] = "novo", ["lido"] = false, ["area"] = area,
            ["socio"] = socio.ToJson(), ["uf"] = u.ToJson(), ["titulo"] = titulo, ["cliente"] = cliente, ["polo"] = polo,
            ["meta"] = Deep(meta), ["eventos"] = Deep(new JsonArray(events.Select(e => e.DeepClone()).ToArray())),
            ["anexos"] = Deep(new JsonArray(anexos.Select(e => e.DeepClone()).ToArray())),
            ["prazo_final"] = IsoDate(pf), ["limite"] = IsoDate(lim),
            ["processo_existe"] = events.Count > 0,
            ["emails"] = new JsonArray(new JsonObject
            {
                ["de"] = socio.nome, ["email"] = $"{socio.email}@{Est.FIRMA_DOMINIO}", ["para"] = "você", ["data"] = NowIso(),
                ["assunto"] = Repl(subject), ["html"] = Repl(string.Concat(emailParas)), ["anexos"] = J.Arr(anexCodes), ["tipo"] = "socio",
            }),
            ["peca_html"] = "", ["peca_atualizada"] = null, ["correcao"] = false,
        };
        return caseObj;
    }

    // ── e-mail
    public void mail(string subj, params string[] paras)
    {
        subject = subj;
        emailParas = paras.Select(p => $"<p style=\"margin:0 0 12px 0; line-height:155%;\">{p}</p>").ToList();
    }

    public string dicas(params string[] itens) =>
        "<ul style='margin:0 0 12px 18px;'>" + string.Concat(itens.Select(i => $"<li style='margin-bottom:6px; line-height:150%;'>{i}</li>")) + "</ul>";

    public string prazo_frase()
    {
        var t = meta.O("tarefa");
        var cont = t.S("contagem");
        var c = cont == "uteis" ? "dias úteis" : cont == "corridos" ? "dias corridos" : "dias";
        if (cont == "uteis" || cont == "corridos")
            return $"O prazo é de <b>{t.I("dias")} {c}</b> ({t.S("fundamento")}) e vence em <b>§PF§</b>. "
                   + "Me mande a minuta até <b>§LIMX§</b> para eu revisar com calma.";
        if (cont == "audiencia")
            return "A audiência está marcada para <b>§AUD§</b> e a defesa precisa estar no sistema antes dela "
                   + $"({t.S("fundamento")}). Me mande a minuta até <b>§LIMX§</b>.";
        return $"{t.S("fundamento")} Preciso da minuta até <b>§LIMX§</b>.";
    }

    public string assinatura()
    {
        var s = socio;
        return $"Abraço,<br><b>{s.nome}</b><br><span style='color:#6b7186'>{s.cargo}<br>{Est.FIRMA}<br>{s.oab}</span>";
    }

    /// <summary>Atalho para a assinatura usada nos casos criminais ("Abraço," + resto).</summary>
    public string assinaturaPenal() => "Abraço,<br>" + assinatura().Split("<br>", 2)[1];

    // atalhos de leitura da meta
    public string orgao => meta.S("orgao");
    public string numero => meta.S("numero");
    public string lab0 => meta.A("lab").Strs()[0];
    public string lab1 => meta.A("lab").Strs()[1];
}
