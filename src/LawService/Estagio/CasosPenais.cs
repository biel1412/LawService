using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — casos criminais
public static partial class Casos
{
    static (string vara, Pes prom, string mp) PenalBase(Ctx c, string classe, string[] assuntos)
    {
        var u = c.u;
        var juiz = c.juiz();
        var prom = c.pf();
        var vara = $"{c.ordm(1, 12)}ª Vara Criminal da Comarca de {u.cidade}";
        c.processo("estadual", classe, vara, assuntos, 0, juiz);
        c.meta["valor"] = null;
        var mp = $"MINISTÉRIO PÚBLICO DO ESTADO {u.estado.Up()}";
        c.parte("a", mp, "", new[] { $"{prom.nome} (Promotor{(prom.f ? "a" : "")} de Justiça)" });
        return (vara, prom, mp);
    }

    static Adv Prom(Pes prom) => new() { nome = prom.nome, oab = "Promotor(a) de Justiça" };

    public static JsonObject Furto(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var reu = c.pf(r.random() < .7 ? "M" : "F");
        var (vara, prom, mp) = PenalBase(c, "AÇÃO PENAL - PROCEDIMENTO ORDINÁRIO", new[] { "Furto (art. 155, caput, CP)" });
        var nosso = c.nossos_advs();
        c.parte("p", reu.nome, reu.cpf, nosso, true);
        var loja = r.choice(new[] { "Supermercado Boa Compra Ltda.", "Rede Mercado Popular Ltda.", "Hipermercado Central Ltda." });
        var itens = r.choice(new[] { ("três barras de chocolate, dois desodorantes e um pacote de café", 112.40),
            ("quatro peças de picanha embaladas", 148.90),
            ("dois xampus e três cremes de cabelo", 96.30) });
        var fiscal = c.pf();
        var pm1 = c.pf("M");
        var o = reu.o;
        var fatos = $"Em {Ctx.A(20)}, por volta das {r.randint(9, 20)}h, no interior do {loja}, em {u.cidade}, "
                    + $"{reu.nome} teria colocado na mochila {itens.Item1}, avaliados em <b>{Brl(itens.Item2)}</b>, e foi abordad{o} "
                    + "pelo fiscal de prevenção de perdas <b>antes de passar pelos caixas</b>, ainda na área interna da loja, "
                    + $"após ser monitorad{o} pelas câmeras desde que pegou os produtos. Os bens foram integralmente "
                    + "restituídos.";
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}";
        var apf = c.doc_s("APF1", "Auto de prisão em flagrante", Pn(
            fatos, $"Depoimento do condutor, soldado PM {Esc(pm1.nome)}: foi acionado pelo fiscal e encontrou {o} "
                   + $"conduzid{o} detid{o} na sala de segurança da loja.",
            $"Interrogatório: {o} conduzid{o} declarou estar desempregad{o} e que pegou os produtos \"para levar para "
            + "casa\". Primári" + o + "."));
        var aval = c.doc_s("AUTOAPREENS2", "Auto de apreensão, avaliação e restituição", Pn(
            $"Bens: {itens.Item1}. Valor de avaliação: {Brl(itens.Item2)}. Restituídos ao estabelecimento."));
        c.ev("DISTRIBUÍDO POR SORTEIO - AUTO DE PRISÃO EM FLAGRANTE", "AUTORIDADE POLICIAL", new[] { apf, aval }, detail: vara);
        c.ev("AUDIÊNCIA DE CUSTÓDIA REALIZADA - CONCEDIDA LIBERDADE PROVISÓRIA", "MAGISTRADO",
            new[] { c.doc_s("TERMOAUD1", "Termo de audiência de custódia", Pn(
                "Homologado o flagrante. Concedida liberdade provisória sem fiança, mediante comparecimento mensal em "
                + "juízo e proibição de frequentar o estabelecimento.")) }, gap: (1, 1));
        var antec = c.doc_s("CERTANTEC1", "Certidão de antecedentes criminais", Pn(
            $"Não constam condenações criminais em nome de {Esc(reu.nome)}. Consta um <b>inquérito policial em "
            + $"andamento</b> (nº {r.randint(1000, 9999)}/{Today().Year - 1}), por fato semelhante, sem denúncia oferecida."));
        c.ev("RELATÓRIO FINAL DO INQUÉRITO POLICIAL", "AUTORIDADE POLICIAL", new[] { antec }, gap: (15, 30));
        var den = c.doc_p("DENUNCIA1", "Denúncia", ender, P(
            $"O {mp}, por seu órgão de execução, oferece DENÚNCIA contra {Esc(reu.nome)}, CPF {reu.cpf}, pelos "
            + "seguintes fatos:", fatos,
            $"Assim agindo, {o} denunciad{o} incorreu nas sanções do <b>art. 155, caput, do Código Penal</b>. Requer o "
            + "recebimento da denúncia, a citação e o regular processamento, com a oitiva das testemunhas abaixo.",
            $"Rol de testemunhas: 1) {Esc(fiscal.nome)}, fiscal de prevenção de perdas; 2) {Esc(pm1.nome)}, "
            + "policial militar."), Prom(prom));
        int n_den = c.ev("DENÚNCIA", prom.nome.Up(), new[] { den }, gap: (5, 15));
        c.ev("DECISÃO - RECEBIDA A DENÚNCIA", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            "Presentes indícios de autoria e prova da materialidade, RECEBO a denúncia. Cite-se o acusado para "
            + "responder à acusação, por escrito, no prazo de 10 dias (arts. 396 e 396-A do CPP).")) }, gap: (3, 8));
        c.ev("MANDADO DE CITAÇÃO EXPEDIDO", "SERVIDOR", gap: (1, 2));
        int n_cit = c.ev("JUNTADA DE MANDADO DE CITAÇÃO CUMPRIDO", "OFICIAL DE JUSTIÇA", new[] { c.doc_s(
            "CERT1", "Certidão do oficial de justiça", Pn(
                $"Certifico que, nesta data, citei pessoalmente {Esc(reu.nome)}, que ficou ciente do inteiro teor da "
                + $"denúncia e declarou ter advogad{(c.socio.nome[2] == 'a' ? "a" : "o")} constituíd{(c.socio.f ? "a" : "o")}: "
                + $"{c.socio.nome}.")) }, gap: (3, 10));
        var teses = new[]
        {
            $"<b>Princípio da insignificância</b>: bens de {Brl(itens.Item2)} (menos de 10% do salário mínimo), "
            + "integralmente restituídos, sem violência — vetores do STF: mínima ofensividade, nenhuma periculosidade "
            + "social, reduzido grau de reprovabilidade e inexpressividade da lesão.",
            "Inquérito em andamento <b>não</b> é antecedente (Súmula 444 do STJ) e não demonstra habitualidade.",
            "Subsidiariamente: <b>tentativa</b> (art. 14, II, do CP) — a abordagem ocorreu antes dos caixas, sem "
            + "inversão da posse; e <b>furto privilegiado</b> (art. 155, § 2º): réu primário e coisa de pequeno valor.",
        };
        if (v == "resposta")
        {
            c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 10, ["contagem"] = "corridos" };
            c.tarefa("Resposta à acusação", "resposta_acusacao", 10, "corridos", "arts. 396 e 396-A do CPP", vara,
                code: "RESPOSTA");
            c.mail($"Novo cliente — furto em supermercado, resposta à acusação ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
                $"Pegamos a defesa d{o} {reu.sr} {reu.nome}, denunciad{o} por furto simples (evento {n_den}). "
                + $"{fatos}",
                $"{Cap(reu.ele)} foi citad{o} pessoalmente (evento {n_cit}) e o prazo para a <b>resposta à "
                + "acusação</b> já está correndo. Lembre: no processo penal os prazos são <b>contínuos</b> (art. 798 "
                + "do CPP), não se contam só dias úteis.",
                "Quero uma resposta à acusação forte, buscando a <b>absolvição sumária</b> (art. 397, III, do CPP). "
                + "Teses:" + c.dicas(teses.Concat(new[]
                {
                    "Observe que a pena mínima do furto simples é de 1 ano: a denúncia não traz proposta de "
                    + "<b>suspensão condicional do processo</b> (art. 89 da Lei 9.099/95) nem justificativa para não "
                    + "oferecer <b>acordo de não persecução penal</b> (art. 28-A do CPP). Requeira a remessa dos autos "
                    + "ao MP para manifestação.",
                    $"Arrole testemunhas (até 8 — art. 401 do CPP). {reu.O} cliente indicou "
                    + $"{c.pf().nome} (ex-empregador) para falar da conduta social.",
                }).ToArray()),
                c.prazo_frase(), c.assinaturaPenal());
            return c.build(reu.nome, "passivo", $"MP x {reu.nome} (furto)");
        }
        c.ev("RESPOSTA À ACUSAÇÃO", c.socio.nome.Up(), new[] { c.doc_p("RESPOSTA1", "Resposta à acusação", ender, P(
                "A defesa requer a absolvição sumária pela atipicidade material (insignificância) e arrola testemunhas."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }) }, gap: (5, 9));
        c.ev("DECISÃO - NÃO ACOLHIDA A ABSOLVIÇÃO SUMÁRIA", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            "As teses defensivas confundem-se com o mérito e dependem de instrução. Não sendo hipótese do art. 397 do "
            + "CPP, ratifico o recebimento da denúncia e designo audiência de instrução e julgamento.")) }, gap: (5, 15));
        var termo = c.doc_s("TERMOAUD1", "Termo de audiência de instrução e julgamento", Pn(
            $"Testemunha {Esc(fiscal.nome)} (fiscal): acompanhou o réu pelas câmeras desde a gôndola; abordou-o "
            + "\"antes de passar pelos caixas, perto do corredor de saída\"; os produtos foram devolvidos intactos.",
            $"Testemunha {Esc(pm1.nome)} (policial militar): chegou após a abordagem; o réu estava calmo e não "
            + "resistiu.",
            $"Interrogatório: {o} acusad{o} confessou ter pegado os produtos; disse que estava desempregad{o}, "
            + "com dois filhos pequenos, e se arrependeu.",
            "Encerrada a instrução, as partes requereram a apresentação de memoriais escritos, o que foi deferido "
            + "(art. 403, § 3º, do CPP): 5 dias para o MP e, após, 5 dias para a defesa."));
        int n_aij = c.ev("AUDIÊNCIA DE INSTRUÇÃO E JULGAMENTO REALIZADA", "MAGISTRADO", new[] { termo }, gap: (30, 60));
        int n_mp = c.ev("ALEGAÇÕES FINAIS - MINISTÉRIO PÚBLICO", prom.nome.Up(), new[] { c.doc_p(
            "MEMORIAIS1", "Memoriais", ender, P(
                "Materialidade e autoria comprovadas pelo auto de apreensão, pela prova oral e pela confissão. Não se "
                + "aplica o princípio da insignificância, pois o réu responde a outro procedimento por fato semelhante, "
                + "o que revela habitualidade. O crime consumou-se com a apreensão dos bens pelo agente (Súmula 582 do "
                + "STJ).", "Requer a condenação do acusado nas sanções do art. 155, caput, do Código Penal."),
            Prom(prom)) }, gap: (4, 6));
        if (v == "memoriais")
        {
            c.intimacao($"DEFESA: {reu.nome.Up()}", 5, "corridos");
            c.tarefa("Memoriais (alegações finais)", "memoriais_penal", 5, "corridos", "art. 403, § 3º, do CPP", vara,
                code: "ALEGACOES");
            c.mail($"Memoriais da defesa — {reu.nome.Split(' ')[0]} (furto)", "Olá, %%NOME%%.",
                $"Terminou a instrução no caso d{o} {reu.nome} (evento {n_aij}) e o MP já apresentou memoriais "
                + $"pedindo a condenação (evento {n_mp}). Agora é a nossa vez.",
                "Leia o termo da audiência: o fiscal confirmou que a abordagem foi <b>antes dos caixas</b> e que o "
                + "cliente foi monitorado o tempo todo. Isso é ouro para nós.",
                "Redija os <b>memoriais da defesa</b>, com pedidos principais e subsidiários bem organizados:"
                + c.dicas(teses.Concat(new[]
                {
                    "Rebata a Súmula 582 do STJ invocada pelo MP: não houve inversão da posse, nem por breve tempo.",
                    "Em caso de condenação: pena no mínimo legal, atenuante da <b>confissão espontânea</b> "
                    + "(art. 65, III, \"d\"), regime aberto e substituição por restritiva de direitos (art. 44).",
                }).ToArray()),
                c.prazo_frase(), c.assinaturaPenal());
            return c.build(reu.nome, "passivo", $"MP x {reu.nome} (furto)");
        }
        c.ev("ALEGAÇÕES FINAIS - DEFESA", c.socio.nome.Up(), new[] { c.doc_p("MEMORIAIS1", "Memoriais", ender, P(
                "A defesa reitera a atipicidade material e, subsidiariamente, a tentativa e o privilégio."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }) }, gap: (4, 6));
        c.ev("CONCLUSOS PARA SENTENÇA", gap: (1, 3));
        int ns = c.nxt;
        var sent = c.doc_j("SENT1", "Sentença", P(
            $"O Ministério Público denunciou {Esc(reu.nome)} como incurs{o} no art. 155, caput, do CP. Recebida a "
            + "denúncia, houve resposta à acusação, instrução e memoriais. É o relatório. Decido.",
            "A materialidade e a autoria estão comprovadas pela prova oral e pela confissão. Afasto o princípio da "
            + "insignificância: a existência de outro procedimento por fato semelhante revela reiteração delitiva. O "
            + "crime está consumado, pois o agente teve a posse dos bens, ainda que por breve tempo (Súmula 582 do STJ).",
            "Dosimetria. Na primeira fase, considero desfavoráveis os antecedentes, em razão do inquérito em curso, e "
            + "fixo a pena-base em 1 ano e 3 meses de reclusão e 12 dias-multa. Reconheço a atenuante da confissão, mas a "
            + "pena não pode ficar aquém do mínimo, e a fixo em 1 ano e 1 mês de reclusão e 11 dias-multa, que torno "
            + "definitiva à míngua de outras causas.",
            "Regime inicial aberto. Substituo a pena privativa de liberdade por duas restritivas de direitos "
            + "(prestação de serviços à comunidade e prestação pecuniária de um salário mínimo).",
            $"Ante o exposto, JULGO PROCEDENTE a denúncia para CONDENAR {Esc(reu.nome.Up())} como incurs{o} no "
            + "art. 155, caput, do Código Penal. Concedo o direito de recorrer em liberdade. Publique-se. Registre-se. "
            + "Intimem-se."));
        c.ev("SENTENÇA CONDENATÓRIA", "MAGISTRADO", new[] { sent }, gap: (10, 30));
        c.intimacao($"DEFESA: {reu.nome.Up()}", 5, "corridos");
        c.tarefa("Apelação criminal (interposição e razões)", "apelacao_penal", 5, "corridos",
            "art. 593, I, do CPP; razões no art. 600", vara, u.tj, "APELACAO");
        c.mail($"Sentença condenatória — apelação ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
            $"Saiu a sentença do caso d{o} {reu.nome} (evento {ns}): <b>condenação</b> por furto consumado, 1 ano e "
            + "1 mês de reclusão, regime aberto, substituída por restritivas. O juiz afastou a insignificância e ainda "
            + "aumentou a pena-base usando o <b>inquérito em andamento</b> como \"maus antecedentes\".",
            "Vamos apelar. Quero a petição de interposição <b>e as razões juntas</b> (assim não dependemos do prazo "
            + "de 8 dias do art. 600). Teses:" + c.dicas(teses.Concat(new[]
            {
                "Na dosimetria: violação da <b>Súmula 444 do STJ</b> — pena-base deve voltar ao mínimo.",
                "Mesmo que mantida a condenação, com o privilégio é possível aplicar só multa (art. 155, § 2º).",
            }).ToArray()),
            c.prazo_frase(), c.assinaturaPenal());
        return c.build(reu.nome, "passivo", $"MP x {reu.nome} (furto)");
    }

    public static JsonObject Drogas(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var reu = c.pf("M");
        var (vara, prom, mp) = PenalBase(c, "AÇÃO PENAL - PROCEDIMENTO ESPECIAL DA LEI ANTIDROGAS",
            new[] { "Tráfico de Drogas e Condutas Afins (art. 33 da Lei 11.343/06)" });
        var nosso = c.nossos_advs();
        c.parte("p", reu.nome, reu.cpf, nosso, true);
        var gramas = Math.Round(r.uniform(9, 19), 1).ToString("0.0#", Inv).Replace(".", ",");
        int porc = r.randint(6, 11);
        int dinheiro = r.choice(new[] { 47, 62, 85, 110 });
        var pm1 = c.pf("M");
        var pm2 = c.pf("M");
        var bairro = r.choice(new[] { "Vila Esperança", "Jardim das Flores", "Parque São José", "Vila Nova" });
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {c.orgao.Up()}";
        var fatos = $"Em {Ctx.A(25)}, durante patrulhamento no bairro {bairro}, policiais militares abordaram "
                    + $"{reu.nome}, {r.randint(19, 26)} anos, numa praça. Na revista pessoal, encontraram <b>{gramas} g de "
                    + $"maconha</b>, divididos em {porc} porções embaladas em plástico, e <b>R$ {dinheiro},00</b> em notas "
                    + "de valores variados. Não havia balança, anotações de contabilidade ou celular com mensagens de venda. "
                    + "O réu é primário, trabalha como ajudante de pedreiro e disse ser usuário.";
        c.ev("DISTRIBUÍDO POR SORTEIO - AUTO DE PRISÃO EM FLAGRANTE", "AUTORIDADE POLICIAL", new[] {
            c.doc_s("APF1", "Auto de prisão em flagrante", Pn(fatos, $"Condutor: sd. PM {Esc(pm1.nome)}: \"o local "
                                                                     + "é conhecido como ponto de venda de drogas\".")),
            c.doc_s("LAUDO2", "Laudo de constatação provisória", Pn("Substância: <i>Cannabis sativa</i> (maconha). "
                                                                   + $"Massa líquida: {gramas} g.")) },
            detail: c.orgao);
        c.ev("AUDIÊNCIA DE CUSTÓDIA REALIZADA - CONCEDIDA LIBERDADE PROVISÓRIA", "MAGISTRADO", gap: (1, 1));
        int n_den = c.ev("DENÚNCIA", prom.nome.Up(), new[] { c.doc_p("DENUNCIA1", "Denúncia", ender, P(
            $"O {mp} oferece DENÚNCIA contra {Esc(reu.nome)}: {fatos}",
            "Assim agindo, o denunciado incorreu nas sanções do <b>art. 33, caput, da Lei 11.343/2006</b>. A forma de "
            + "acondicionamento, o dinheiro em notas trocadas e o local conhecido como ponto de tráfico evidenciam a "
            + "destinação comercial."), Prom(prom)) }, gap: (10, 20));
        c.ev("NOTIFICAÇÃO PARA DEFESA PRÉVIA (ART. 55 DA LEI 11.343/06)", "SERVIDOR", gap: (2, 4));
        c.ev("DEFESA PRÉVIA", c.socio.nome.Up(), gap: (8, 10));
        c.ev("DECISÃO - RECEBIDA A DENÚNCIA", "MAGISTRADO", gap: (5, 10));
        c.ev("JUNTADA DE LAUDO PERICIAL DEFINITIVO", "PERITO", new[] { c.doc_s("LAUDO1", "Laudo toxicológico definitivo", Pn(
            $"Material examinado: {porc} porções de erva prensada. Resultado: POSITIVO para tetra-hidrocanabinol (THC). "
            + $"Massa líquida total: {gramas} g.")) }, gap: (10, 20));
        var test_def = c.pf();
        int n_aij = c.ev("AUDIÊNCIA DE INSTRUÇÃO E JULGAMENTO REALIZADA", "MAGISTRADO", new[] { c.doc_s(
                "TERMOAUD1", "Termo de audiência de instrução e julgamento", Pn(
                    $"Testemunha {Esc(pm1.nome)} (PM): fazia patrulhamento de rotina; não viu o réu vendendo nem "
                    + "entregando nada a ninguém; o local \"tem histórico\" de tráfico.",
                    $"Testemunha {Esc(pm2.nome)} (PM): confirmou a apreensão; não houve campana nem denúncia anônima "
                    + "específica contra o réu.",
                    $"Testemunha de defesa {Esc(test_def.nome)} (empregador): o réu trabalha com ele há dois anos, "
                    + "\"é trabalhador\" e recebe por diária, em dinheiro.",
                    "Interrogatório: o réu disse que é usuário há anos, que comprou a droga já porcionada para consumir no "
                    + "fim de semana e que o dinheiro era da diária de trabalho.",
                    "As partes requereram memoriais escritos, deferidos com prazo sucessivo de 5 dias.")) },
            gap: (20, 40));
        int n_mp = c.ev("ALEGAÇÕES FINAIS - MINISTÉRIO PÚBLICO", prom.nome.Up(), new[] { c.doc_p(
            "MEMORIAIS1", "Memoriais", ender, P(
                "A forma de acondicionamento, o dinheiro trocado e o local revelam o tráfico. O réu se dedica a "
                + "atividades criminosas, o que impede o redutor do § 4º do art. 33. Requer a condenação nas sanções do "
                + "art. 33, caput, da Lei 11.343/2006."), Prom(prom)) },
            gap: (4, 6));
        var teses = new[]
        {
            "<b>Desclassificação para o art. 28</b> da Lei 11.343/06, à luz dos critérios do § 2º: pequena "
            + "quantidade, ausência de balança, de anotações e de qualquer ato de venda presenciado.",
            "STF, <b>Tema 506</b> (RE 635.659): presume-se usuário quem porta até <b>40 g de maconha</b> — aqui eram "
            + $"{gramas} g. Discuta o alcance dessa tese no caso concreto.",
            "Subsidiariamente, <b>tráfico privilegiado</b> (art. 33, § 4º): réu primário, de bons antecedentes, com "
            + "trabalho lícito comprovado — redução de 2/3.",
            "Com o privilégio: regime aberto e substituição por restritivas de direitos (o STF afastou a vedação legal "
            + "— HC 97.256 e Resolução 5/2012 do Senado).",
        };
        if (v == "memoriais")
        {
            c.intimacao($"DEFESA: {reu.nome.Up()}", 5, "corridos");
            c.tarefa("Memoriais (alegações finais)", "memoriais_penal", 5, "corridos",
                "art. 403, § 3º, do CPP, aplicado por analogia (art. 57 da Lei 11.343/06)",
                c.orgao, code: "ALEGACOES");
            c.mail($"Memoriais — tráfico x uso ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
                $"No caso do {reu.nome}, denunciado por tráfico (evento {n_den}), a instrução terminou (evento "
                + $"{n_aij}) e o MP já pediu a condenação (evento {n_mp}). Precisamos dos <b>memoriais da defesa</b>.",
                "A prova é fraca para o tráfico: os próprios policiais disseram que não viram nenhuma venda e que não "
                + "havia investigação prévia. O laudo definitivo confirma a pequena quantidade. Teses:" + c.dicas(teses),
                c.prazo_frase(), c.assinaturaPenal());
            return c.build(reu.nome, "passivo", $"MP x {reu.nome} (Lei 11.343)");
        }
        c.ev("ALEGAÇÕES FINAIS - DEFESA", c.socio.nome.Up(), gap: (4, 6));
        int ns = c.nxt;
        var sent = c.doc_j("SENT1", "Sentença", P(
            $"O Ministério Público denunciou {Esc(reu.nome)} como incurso no art. 33, caput, da Lei 11.343/06. Após "
            + "a instrução, as partes apresentaram memoriais. É o relatório. Decido.",
            "A materialidade está comprovada pelo laudo definitivo. A autoria é certa. A forma de acondicionamento em "
            + "porções individuais, o dinheiro em notas trocadas e a apreensão em local conhecido como ponto de tráfico "
            + "afastam a tese de uso próprio.",
            "Deixo de aplicar a causa de diminuição do § 4º do art. 33, pois as circunstâncias da apreensão indicam "
            + "dedicação a atividades criminosas.",
            "Pena-base no mínimo legal: 5 anos de reclusão e 500 dias-multa, que torno definitiva. Regime inicial "
            + "semiaberto. Incabível a substituição (art. 44, I, do CP).",
            $"Ante o exposto, JULGO PROCEDENTE a denúncia para CONDENAR {Esc(reu.nome.Up())} nas sanções do art. "
            + "33, caput, da Lei 11.343/2006. Concedo o direito de recorrer em liberdade. Publique-se. Intimem-se."));
        c.ev("SENTENÇA CONDENATÓRIA", "MAGISTRADO", new[] { sent }, gap: (10, 30));
        c.intimacao($"DEFESA: {reu.nome.Up()}", 5, "corridos");
        c.tarefa("Apelação criminal (interposição e razões)", "apelacao_penal", 5, "corridos",
            "art. 593, I, do CPP; razões no art. 600", c.orgao, u.tj, "APELACAO");
        c.mail($"Condenação por tráfico — apelação ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
            $"Saiu a sentença do {reu.nome} (evento {ns}): <b>condenado por tráfico</b>, 5 anos em regime "
            + "semiaberto, sem o privilégio do § 4º. O juiz usou o local da abordagem e as notas trocadas para dizer "
            + "que ele \"se dedica a atividades criminosas\" — sem nenhuma prova concreta disso.",
            "Vamos <b>apelar</b>, com interposição e razões na mesma peça. Teses:" + c.dicas(teses.Concat(new[]
            {
                "Fundamentação inidônea para negar o privilégio: local da abordagem e dinheiro trocado (que o "
                + "empregador explicou) não provam dedicação ao crime.",
            }).ToArray()),
            c.prazo_frase(), c.assinaturaPenal());
        return c.build(reu.nome, "passivo", $"MP x {reu.nome} (Lei 11.343)");
    }
}
