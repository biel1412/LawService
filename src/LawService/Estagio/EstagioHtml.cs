using System.Text;
using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.Th;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — HTML do sistema e da peça
public static class EstHtml
{
    public const string SYS_BLUE = "#123c69";
    public const string SYS_BLUE2 = "#1d5a96";

    public static JsonObject EstSettings() => Store.Settings;

    public static string NomeEstagiario()
    {
        var n = EstSettings().S("estagiario");
        return string.IsNullOrEmpty(n) ? "estagiário(a)" : n;
    }

    public static string CorrecaoUrl() => !string.IsNullOrEmpty(CORRECAO_URL) ? CORRECAO_URL : EstSettings().S("correcao_url");

    public static string Personalize(string html)
    {
        var nome = EstSettings().S("estagiario").Trim();
        return (html ?? "").Replace("%%NOME%%", Esc(nome.Length > 0 ? nome.Split(' ')[0] : "pessoal"));
    }

    public static readonly Dictionary<string, string> AREA_CURTA = new()
    {
        ["Direito do Consumidor"] = "Consumidor", ["Direito Civil"] = "Civil", ["Direito de Família"] = "Família",
        ["Direito Penal"] = "Penal", ["Direito do Trabalho"] = "Trabalho", ["Direito Previdenciário"] = "Previdenciário",
        ["Direito Administrativo"] = "Administrativo", ["Direito Tributário"] = "Tributário", ["Direito das Sucessões"] = "Sucessões",
    };

    public static string Curta(string area) => AREA_CURTA.TryGetValue(area, out var c) ? c : area;

    public static (string, string) PrazoChip(JsonObject c)
    {
        if (c.S("status") == "entregue") return ("Entregue", T["success"]);
        var pf = ParseDate(c.S("prazo_final"));
        int d = (pf - Today()).Days;
        if (d < 0) return ("Prazo vencido", T["danger"]);
        if (d == 0) return ("Vence hoje", T["danger"]);
        if (d <= 2) return ($"Vence em {d} dia{(d > 1 ? "s" : "")}", T["warn"]);
        return ($"Prazo {pf:dd/MM}", T["muted"]);
    }

    public static string AreaColor(string area) => area switch
    {
        "Direito Penal" => "#e5484d", "Direito do Trabalho" => "#2f9e44", "Direito Previdenciário" => "#0ca678",
        "Direito Administrativo" => "#1c7ed6", "Direito Tributário" => "#15aabf", "Direito de Família" => "#d6409f",
        "Direito do Consumidor" => "#f76707", "Direito das Sucessões" => "#a0522d", _ => "#8b5cf6",
    };

    public static string ProcHtml(JsonObject c)
    {
        var m = c.O("meta");
        if (!c.B("processo_existe"))
            return "<table width='100%' cellpadding='22' cellspacing='0'><tr><td bgcolor='#f4f7fb' align='center'>"
                   + $"<p style='font-size:13pt; color:{SYS_BLUE};'><b>Nenhum processo vinculado</b></p>"
                   + "<p style='color:#55606e;'>Este caso ainda não foi ajuizado. A peça que você vai redigir dará início "
                   + "à ação. Depois da entrega, o processo será distribuído e aparecerá aqui com os seus eventos.</p>"
                   + "<p style='color:#55606e;'>Os documentos do cliente estão anexados ao e-mail.</p></td></tr></table>";
        const string TD_K = "bgcolor='#eef3f8' style='color:#33475b;'";
        const string TD_V = "bgcolor='#ffffff'";

        string Sec(string t) => "<table width='100%' cellspacing='0' cellpadding='7' style='margin-top:16px;'><tr>"
                                + $"<td bgcolor='{SYS_BLUE}'><span style='color:#ffffff; font-weight:700; font-size:9.5pt;'>{t}</span>"
                                + "</td></tr></table>";

        var evs = c.A("eventos").List();
        var autuacao = evs.Count > 0 ? FmtBr(evs[0].S("date")) : "";
        var juiz = m.O("juiz");
        var vnum = J.Num(m.Get("valor"));
        var valor = vnum.HasValue ? Brl(vnum.Value) : "—";
        var outp = new StringBuilder();
        if (m.B("segredo"))
            outp.Append("<table width='100%' cellpadding='6' cellspacing='0'><tr><td bgcolor='#fdecec' align='center'>"
                        + "<b style='color:#c92a2a;'>PROCESSO EM SEGREDO DE JUSTIÇA</b></td></tr></table>");
        outp.Append(Sec("Capa do processo"));
        var rows = new (string, string, string, string)[]
        {
            ("Nº do processo", $"<b>{m.S("numero")}</b>", "Classe da ação", Esc(m.S("classe"))),
            ("Competência", Esc(m.S("competencia")), "Data de autuação", autuacao),
            ("Situação", "MOVIMENTO", "Órgão julgador", Esc(m.S("orgao"))),
            ("Juiz(a)", Esc(juiz.S("nome")), "Valor da causa", valor),
            ("Assuntos", Esc(string.Join("; ", m.A("assuntos").Strs())), "Área", Esc(c.S("area"))),
        };
        outp.Append("<table width='100%' cellspacing='1' cellpadding='6' bgcolor='#c9d6e3'>" + string.Concat(rows.Select(r =>
            $"<tr><td {TD_K} width='15%'>{r.Item1}</td><td {TD_V} width='35%'>{r.Item2}</td><td {TD_K} width='15%'>{r.Item3}</td>"
            + $"<td {TD_V}>{r.Item4}</td></tr>")) + "</table>");
        outp.Append(Sec("Partes e representantes"));

        string Polo(JsonArray lst, string lab)
        {
            var h = new StringBuilder();
            foreach (var p in lst.Objs())
            {
                var tag = p.B("cliente")
                    ? " <span style='background-color:#e7f6eb; color:#2b8a3e;'>&nbsp;cliente do escritório&nbsp;</span>" : "";
                var doc = p.S("doc").Length > 0 ? $"<br><span style='color:#55606e;'>{Esc(p.S("doc"))}</span>" : "";
                var advs = string.Concat(p.A("advs").Strs().Select(a => $"<br><span style='color:#55606e;'>Representante: {Esc(a)}</span>"));
                h.Append($"<p style='margin:0 0 8px 0;'><b>{Esc(p.S("nome").Up())}</b>{tag}{doc}{advs}</p>");
            }
            return h.Length > 0 ? h.ToString() : "<p>—</p>";
        }
        var lab = m.A("lab").Strs();
        var (la, lp) = (lab[0], lab[1]);
        outp.Append("<table width='100%' cellspacing='1' cellpadding='8' bgcolor='#c9d6e3'><tr>"
                    + $"<td {TD_K} width='50%'><b>{la}</b></td><td {TD_K}><b>{lp}</b></td></tr><tr>"
                    + $"<td {TD_V} valign='top'>{Polo(m.A("ativo"), la)}</td><td {TD_V} valign='top'>{Polo(m.A("passivo"), lp)}"
                    + "</td></tr></table>");
        var abertos = evs.Where(e => e.Get("prazo") is JsonObject).ToList();
        if (abertos.Count > 0)
        {
            outp.Append(Sec("Prazos"));
            outp.Append("<table width='100%' cellspacing='1' cellpadding='6' bgcolor='#c9d6e3'><tr>" + string.Concat(
                            new[] { "Evento", "Prazo", "Início da contagem", "Data final", "Status" }.Select(h => $"<td {TD_K}><b>{h}</b></td>"))
                        + "</tr>" + string.Concat(abertos.Select(e =>
                        {
                            var p = e.O("prazo");
                            bool ab = p.S("status") == "ABERTO";
                            return $"<tr><td bgcolor='{(ab ? "#fff8db" : "#ffffff")}' align='center'>{e.I("n")}</td>"
                                   + $"<td {TD_V}>{p.I("dias")} dias {(p.S("contagem") == "uteis" ? "úteis" : "corridos")}</td>"
                                   + $"<td {TD_V}>{FmtBr(p.S("inicio"))}</td><td {TD_V}><b>{FmtBr(p.S("fim"))}</b></td>"
                                   + $"<td {TD_V}><b style='color:{(ab ? "#e67700" : "#2b8a3e")};'>{p.S("status")}</b></td></tr>";
                        })) + "</table>");
        }
        outp.Append(Sec($"Eventos ({evs.Count})"));
        var head = string.Concat(new[] { "Ev.", "Data/Hora", "Descrição", "Usuário", "Documentos" }
            .Select(h => $"<td bgcolor='#dfe8f1' style='color:{SYS_BLUE};'><b>{h}</b></td>"));
        var body = new StringBuilder();
        int i = 0;
        foreach (var e in Enumerable.Reverse(evs))
        {
            var bg = i % 2 == 0 ? "#ffffff" : "#f5f8fc";
            i++;
            var pz = e.Get("prazo") as JsonObject;
            if (pz != null && pz.S("status") == "ABERTO") bg = "#fff8db";
            var dsc = e.S("desc");
            var desc = $"<b>{Esc(dsc)}</b>";
            if (new[] { "SENTENÇA", "DECISÃO", "PROCEDÊNCIA" }.Any(dsc.Contains)) desc = $"<b style='color:#0b4f8a;'>{Esc(dsc)}</b>";
            if (e.S("detail").Length > 0) desc += $"<br><span style='color:#5c6b7a;'>{Esc(e.S("detail"))}</span>";
            if (pz != null)
                desc += $"<br><span style='color:#a35400;'>Prazo: {pz.I("dias")} dias "
                        + $"({(pz.S("contagem") == "uteis" ? "úteis" : "corridos")}) · Status: {pz.S("status")} · "
                        + $"Data inicial: {FmtBr(pz.S("inicio"))} · Data final: {FmtBr(pz.S("fim"))}</span>";
            var docsL = e.A("docs").Objs().Select(d => $"<a href='doc:{e.I("n")}:{d.S("code")}' style='color:#0b5cad; text-decoration:none;'>"
                                                       + $"<b>{d.S("code")}</b></a> <span style='color:#5c6b7a;'>{Esc(d.S("title"))}</span>").ToList();
            var docs = docsL.Count > 0 ? string.Join("<br>", docsL) : "<span style='color:#9aa5b1;'>—</span>";
            body.Append($"<tr><td bgcolor='{bg}' align='center' width='8%'><b>{e.I("n")}</b></td>"
                        + $"<td bgcolor='{bg}' width='13%'>{FmtBr(e.S("date"))}<br><span style='color:#5c6b7a;'>"
                        + $"{e.S("time")}</span></td><td bgcolor='{bg}'>{desc}</td>"
                        + $"<td bgcolor='{bg}' width='14%' style='color:#33475b;'>{Esc(e.S("user"))}</td>"
                        + $"<td bgcolor='{bg}' width='22%'>{docs}</td></tr>");
        }
        outp.Append($"<table width='100%' cellspacing='1' cellpadding='7' bgcolor='#c9d6e3'><tr>{head}</tr>" + body + "</table>");
        return outp.ToString();
    }

    /// <summary>Encontra um documento pelo link ("doc:n:CODE" ou "anexo:CODE").</summary>
    public static (JsonObject doc, JsonObject ev) FindDoc(JsonObject c, string key)
    {
        if (key.StartsWith("anexo:"))
        {
            var code = key.Split(':', 2)[1];
            return (c.A("anexos").Objs().FirstOrDefault(a => a.S("code") == code), null);
        }
        var parts = key.Split(':');
        if (parts.Length < 3) return (null, null);
        var ev = c.A("eventos").Objs().FirstOrDefault(e => e.I("n").ToString() == parts[1]);
        if (ev == null) return (null, null);
        return (ev.A("docs").Objs().FirstOrDefault(d => d.S("code") == parts[2]), ev);
    }

    public static string Enderecamento(JsonObject c, string alvo = "a_quo")
    {
        var m = c.O("meta");
        var t = m.O("tarefa");
        var org = t.S("a_quo");
        if (alvo == "ad_quem")
        {
            var aq = t.S("ad_quem");
            if (aq.StartsWith("Turma Recursal")) return $"EGRÉGIA {aq.Up()}";
            return $"EGRÉGIO {aq.Up()}";
        }
        if (m.S("justica") == "federal") return $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) FEDERAL DA {org.Up()}";
        if (m.S("justica") == "trabalho") return $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DA {org.Up()}";
        if (new[] { "1º", "2º", "3º", "4º", "5º", "6º", "Juizado" }.Any(org.StartsWith))
            return $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {org.Up()}";
        return $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {org.Up()}";
    }

    /// <summary>Esqueleto da peça (endereçamento, qualificação e tópicos).</summary>
    public static string EstruturaPeca(JsonObject c)
    {
        var m = c.O("meta");
        var t = m.O("tarefa");
        var tipo = t.S("tipo");
        var cli = c.S("cliente");
        var outros = (c.S("polo") == "ativo" ? m.A("passivo") : m.A("ativo")).Objs().Select(p => p.S("nome")).ToList();
        var outro = outros.Count > 0 ? outros[0] : "[parte contrária]";
        var num = c.B("processo_existe") ? m.S("numero") : "";
        var socio = c.O("socio");

        string Ph(string s) => $"<span style='color:#8a8f9e;'><i>[{s}]</i></span>";
        string H(string s) => $"<p style='margin:18px 0 8px 0;'><b>{s}</b></p>";
        string Par(string s) => $"<p align='justify' style='margin:0 0 10px 0; line-height:150%; text-indent:48px;'>{s}</p>";
        string End(string s) => $"<p style='margin:0 0 30px 0; line-height:150%;'><b>{Esc(s)}</b></p>";

        var proc = num.Length > 0 ? $"<p style='margin:0 0 24px 0;'><b>Processo nº {num}</b></p>" : "";
        var fecho = "<p style='margin:20px 0 0 0;'>Nestes termos, pede deferimento.</p>"
                    + $"<p style='margin:10px 0 0 0;'>{Esc(c.O("uf").S("cidade"))}/{c.O("uf").S("uf")}, {FdateExt(Today())}.</p>"
                    + $"<p style='margin:30px 0 0 0;'><b>{Esc(socio.S("nome").Split(' ', 2)[1].Up())}</b><br>"
                    + $"{socio.S("oab")}</p>";
        var q = $"<b>{Esc(cli.Up())}</b>, já qualificad{"o(a)"} nos autos do processo em epígrafe";
        if (tipo is "inicial" or "inicial_trab" or "ms")
        {
            var acao = tipo switch
            {
                "inicial_trab" => "RECLAMAÇÃO TRABALHISTA", "ms" => "MANDADO DE SEGURANÇA COM PEDIDO LIMINAR", _ => Ph("NOME DA AÇÃO"),
            };
            return End(Enderecamento(c))
                   + Par($"<b>{Esc(cli.Up())}</b>, {Ph("nacionalidade, estado civil, profissão, CPF, endereço")}, por "
                         + "seu advogado infra-assinado (procuração anexa), vem respeitosamente à presença de Vossa "
                         + $"Excelência propor {acao} em face de <b>{Esc(outro.Up())}</b>, "
                         + $"{Ph("qualificação")}, pelos fatos e fundamentos a seguir expostos.")
                   + H("I – DOS FATOS") + Par(Ph("Narre os fatos de forma cronológica e objetiva"))
                   + H("II – DO DIREITO") + Par(Ph("Fundamentos jurídicos: lei, súmulas, jurisprudência"))
                   + (tipo != "inicial_trab" ? H("III – DA TUTELA DE URGÊNCIA / LIMINAR") + Par(Ph("Probabilidade do direito e perigo de dano")) : "")
                   + H("IV – DOS PEDIDOS") + Par("Diante do exposto, requer:") + Par(Ph("a) ...; b) ...; c) ..."))
                   + Par($"Dá-se à causa o valor de {Ph("R$")}.") + fecho;
        }
        if (tipo == "hc")
            return End($"EXCELENTÍSSIMO(A) SENHOR(A) DESEMBARGADOR(A) PRESIDENTE DO {t.S("ad_quem").Up()}")
                   + Par($"<b>{Esc(socio.S("nome").Split(' ', 2)[1].Up())}</b>, advogado(a), "
                         + $"{socio.S("oab")}, vem impetrar <b>HABEAS CORPUS COM PEDIDO LIMINAR</b> em favor de "
                         + $"<b>{Esc(cli.Up())}</b>, {Ph("qualificação; local onde está preso")}, contra ato do(a) MM. "
                         + $"Juiz(a) de Direito da {Esc(t.S("a_quo"))} (autoridade coatora), nos autos nº {num}, com "
                         + "fundamento no art. 5º, LXVIII, da CF e nos arts. 647 e 648 do CPP.")
                   + H("I – DOS FATOS E DA DECISÃO IMPUGNADA") + Par(Ph("Desenvolva"))
                   + H("II – DO CONSTRANGIMENTO ILEGAL") + Par(Ph("fundamentação inidônea; cautelares diversas; condições pessoais"))
                   + H("III – DO PEDIDO LIMINAR") + Par(Ph("plausibilidade e urgência"))
                   + H("IV – DOS PEDIDOS") + Par(Ph("liminar; informações; parecer do MP; concessão da ordem"))
                   + fecho;
        var nomes = new Dictionary<string, string>
        {
            ["contestacao"] = "CONTESTAÇÃO", ["contestacao_trab"] = "CONTESTAÇÃO", ["replica"] = "RÉPLICA",
            ["excecao"] = "EXCEÇÃO DE PRÉ-EXECUTIVIDADE", ["resposta_acusacao"] = "RESPOSTA À ACUSAÇÃO",
            ["embargos"] = "EMBARGOS DE DECLARAÇÃO", ["memoriais"] = "MEMORIAIS",
            ["memoriais_penal"] = "MEMORIAIS (ALEGAÇÕES FINAIS)", ["embargos_monitoria"] = "EMBARGOS À MONITÓRIA",
            ["embargos_execucao"] = "EMBARGOS À EXECUÇÃO", ["revogacao"] = "PEDIDO DE REVOGAÇÃO DA PRISÃO PREVENTIVA",
            ["primeiras_declaracoes"] = "PRIMEIRAS DECLARAÇÕES",
        };
        if (nomes.TryGetValue(tipo, out var nome))
        {
            var secoes = new Dictionary<string, string[]>
            {
                ["contestacao"] = new[] { "I – SÍNTESE DA INICIAL", "II – PRELIMINARES", "III – DO MÉRITO", "IV – DAS PROVAS", "V – DOS PEDIDOS" },
                ["contestacao_trab"] = new[] { "I – SÍNTESE DA INICIAL", "II – PRELIMINARES / PREJUDICIAIS",
                    "III – DO MÉRITO (impugnação específica de cada pedido)", "IV – DOS PEDIDOS" },
                ["replica"] = new[] { "I – SÍNTESE DA CONTESTAÇÃO", "II – DA PRELIMINAR", "III – DO MÉRITO", "IV – DAS PROVAS", "V – DOS REQUERIMENTOS" },
                ["excecao"] = new[] { "I – DO CABIMENTO", "II – DA PRESCRIÇÃO", "III – DOS PEDIDOS" },
                ["resposta_acusacao"] = new[] { "I – SÍNTESE DA ACUSAÇÃO", "II – PRELIMINARMENTE", "III – DA ABSOLVIÇÃO SUMÁRIA",
                    "IV – DO ROL DE TESTEMUNHAS", "V – DOS PEDIDOS" },
                ["embargos"] = new[] { "I – DA TEMPESTIVIDADE", "II – DAS OMISSÕES", "III – DOS PEDIDOS" },
                ["memoriais"] = new[] { "I – SÍNTESE PROCESSUAL", "II – DA PROVA PRODUZIDA", "III – DO DIREITO", "IV – DOS PEDIDOS" },
                ["memoriais_penal"] = new[] { "I – SÍNTESE PROCESSUAL", "II – DA PROVA PRODUZIDA", "III – TESE PRINCIPAL",
                    "IV – TESES SUBSIDIÁRIAS", "V – DOS PEDIDOS" },
                ["embargos_monitoria"] = new[] { "I – DA TEMPESTIVIDADE", "II – SÍNTESE DA INICIAL", "III – DO MÉRITO",
                    "IV – DO EXCESSO (valor que entende devido — art. 702, § 2º)", "V – DAS PROVAS", "VI – DOS PEDIDOS" },
                ["embargos_execucao"] = new[] { "I – DA TEMPESTIVIDADE", "II – SÍNTESE DA EXECUÇÃO",
                    "III – DO EXCESSO DE EXECUÇÃO (demonstrativo — art. 917, § 3º)", "IV – DO EFEITO SUSPENSIVO", "V – DAS PROVAS",
                    "VI – DOS PEDIDOS" },
                ["revogacao"] = new[] { "I – SÍNTESE", "II – DA AUSÊNCIA DOS REQUISITOS DA PREVENTIVA", "III – DOS FATOS NOVOS",
                    "IV – DAS MEDIDAS CAUTELARES DIVERSAS / PRISÃO DOMICILIAR", "V – DOS PEDIDOS" },
                ["primeiras_declaracoes"] = new[] { "I – DO AUTOR DA HERANÇA", "II – DO CÔNJUGE E DOS HERDEIROS",
                    "III – DOS BENS (com valores)", "IV – DA MEAÇÃO E DA HERANÇA", "V – DOS REQUERIMENTOS" },
            }[tipo];
            var prea = tipo != "primeiras_declaracoes"
                ? $"{q}, que lhe move {Esc(outro.Up())}, vem, por seu advogado, apresentar "
                : $"<b>{Esc(cli.Up())}</b>, inventariante nomeada nos autos do inventário em epígrafe ({Esc(outro)}), vem, por seu advogado, apresentar ";
            return End(Enderecamento(c)) + proc
                   + Par(prea + $"<b>{nome}</b>, com fundamento no {Esc(t.S("fundamento"))}, pelos motivos a seguir.")
                   + string.Concat(secoes.Select(s => H(s) + Par(Ph("Desenvolva")))) + fecho;
        }
        // recursos com interposição + razões / agravo direto ao tribunal
        var nomeR = tipo switch
        {
            "apelacao" => "APELAÇÃO", "apelacao_penal" => "APELAÇÃO", "recurso_inominado" => "RECURSO INOMINADO",
            "ro" => "RECURSO ORDINÁRIO", "contrarrazoes" => "CONTRARRAZÕES", "agravo" => "AGRAVO DE INSTRUMENTO", _ => t.S("peca").Up(),
        };
        if (tipo == "agravo")
            return End($"EXCELENTÍSSIMO(A) SENHOR(A) DESEMBARGADOR(A) PRESIDENTE DO {t.S("ad_quem").Up()}")
                   + Par($"<b>{Esc(cli.Up())}</b>, {Ph("qualificação")}, vem interpor <b>AGRAVO DE INSTRUMENTO</b>, "
                         + "com pedido de antecipação da tutela recursal, contra a decisão proferida nos autos do "
                         + $"processo nº {num}, em trâmite na {Esc(t.S("a_quo"))}, em que litiga com "
                         + $"{Esc(outro.Up())}.")
                   + H("I – DO CABIMENTO E DA TEMPESTIVIDADE") + Par(Ph("art. 1.015 e prazo"))
                   + H("II – DAS PEÇAS OBRIGATÓRIAS (art. 1.017 do CPC)") + Par(Ph("liste os documentos"))
                   + H("III – NOMES E ENDEREÇOS DOS ADVOGADOS (art. 1.016, IV)") + Par(Ph("advogados das partes"))
                   + H("IV – SÍNTESE DA DECISÃO AGRAVADA") + Par(Ph("Desenvolva"))
                   + H("V – DAS RAZÕES PARA A REFORMA") + Par(Ph("Desenvolva"))
                   + H("VI – DA ANTECIPAÇÃO DA TUTELA RECURSAL (art. 1.019, I)") + Par(Ph("Desenvolva"))
                   + H("VII – DOS PEDIDOS") + Par(Ph("Desenvolva")) + fecho;
        var razoes = tipo == "contrarrazoes" ? "CONTRARRAZÕES" : $"RAZÕES DE {nomeR}";
        var secs = tipo == "contrarrazoes"
            ? new[] { "I – SÍNTESE DO PROCESSO", "II – DA MANUTENÇÃO DA SENTENÇA", "III – DOS PEDIDOS" }
            : new[] { "I – DA TEMPESTIVIDADE E DO PREPARO", "II – SÍNTESE DO PROCESSO E DA SENTENÇA", "III – DAS RAZÕES PARA A REFORMA", "IV – DOS PEDIDOS" };
        var aq2 = t.S("ad_quem");
        var interp = End(Enderecamento(c)) + proc
                     + Par($"{q}, vem, por seu advogado, {(tipo == "contrarrazoes" ? "apresentar" : "interpor")} "
                           + $"<b>{nomeR}</b> {(tipo == "contrarrazoes" ? "ao recurso interposto por " + Esc(outro.Up()) : "contra a sentença proferida")}"
                           + $", com fundamento no {Esc(t.S("fundamento"))}, requerendo o recebimento e a remessa das razões "
                           + $"anexas {(aq2.StartsWith("Turma") ? "à" : "ao")} {Esc(aq2)}.")
                     + fecho;
        var raz = $"<p style='margin:40px 0 20px 0;' align='center'><b>{razoes}</b></p>"
                  + Par($"<b>{(tipo == "contrarrazoes" ? "Recorrido" : "Recorrente")}:</b> {Esc(cli)}<br>"
                        + $"<b>{(tipo == "contrarrazoes" ? "Recorrente" : "Recorrido")}:</b> {Esc(outro)}<br>"
                        + $"<b>Processo de origem:</b> {num} — {Esc(t.S("a_quo"))}")
                  + End(Enderecamento(c, "ad_quem"))
                  + string.Concat(secs.Select(s => H(s) + Par(Ph("Desenvolva")))) + fecho;
        return interp + raz;
    }
}
