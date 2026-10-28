using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — casos cíveis
public static partial class Casos
{
    public static JsonObject Consumidor(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        bool banco = r.random() < .5;
        var re_nome = banco ? r.choice(new[] { "Banco Meridiano S.A.", "Banco Horizonte Digital S.A." })
            : r.choice(new[] { "Conecta Telecom S.A.", "Vivaz Telecomunicações S.A." });
        var re_cnpj = GenCnpj(r);
        var produto = banco ? "um cartão de crédito" : "uma linha de telefonia móvel pós-paga";
        var divida = c.money(380, 2900);
        var contrato = r.randint(100000000, 999999999).ToString();
        var loja = r.choice(new[] { "numa loja de eletrodomésticos", "numa loja de móveis", "numa ótica", "numa loja de materiais de construção" });
        double dm = r.choice(new[] { 8000, 10000, 12000, 15000 });
        bool ini = v == "inicial";
        Func<int, string> Tq = ini ? Ctx.B : Ctx.A;
        var juiz = c.juiz();
        var adv_re = c.adv();
        var nosso = c.nossos_advs();
        c.processo("estadual", "PROCEDIMENTO DO JUIZADO ESPECIAL CÍVEL",
            $"{c.ordm(1, 6)}º Juizado Especial Cível da Comarca de {u.cidade}",
            new[] { "Inclusão Indevida em Cadastro de Inadimplentes", "Indenização por Dano Moral", "Declaração de Inexistência de Débito" },
            divida + dm, juiz, ("REQUERENTE", "REQUERIDO"), "Juizado Especial Cível");
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", re_nome, re_cnpj, new[] { $"{adv_re.nome} ({adv_re.oab})" });
        var fatos = $"Em {Tq(40)}, {cli.nome} tentou fazer uma compra parcelada {loja} e teve o crédito recusado. "
                    + "Ao consultar os cadastros de proteção ao crédito, descobriu uma anotação incluída a pedido de "
                    + $"<b>{re_nome}</b>, referente ao contrato nº {contrato}, no valor de <b>{Brl(divida)}</b>, com vencimento "
                    + $"em {Tq(150)}. {cli.O} cliente afirma que nunca contratou {produto} com a empresa, nunca recebeu "
                    + "cobrança anterior e não reconhece o endereço cadastrado.";
        var tit = $"{cli.nome} x {re_nome}";
        var reCurto = $"{re_nome.Split(' ')[0]} {re_nome.Split(' ')[1]}";
        if (ini)
        {
            c.anexo("Relato do atendimento", Pn(
                $"<b>Data do atendimento:</b> {Tq(2)}<br><b>Cliente:</b> {Esc(cli.nome)}, CPF {cli.cpf}, "
                + $"{Esc(cli.prof)}, residente em {u.cidade}/{u.uf}.", fatos,
                $"No mesmo dia, {cli.o} cliente ligou para a central da empresa (protocolo nº "
                + $"{r.randlong(1000000000L, 9999999999L)}). Foi informad{cli.o} de que o débito \"consta como regular\" e "
                + $"que seria necessário enviar boletim de ocorrência. Registrou ocorrência policial por fraude em {Tq(30)}.",
                $"Não há outras anotações em nome {cli.do_} cliente. {cli.O} cliente relata constrangimento na loja, "
                + "diante de outros consumidores, e quer a retirada urgente do nome do cadastro."));
            c.anexo("Consulta ao cadastro de inadimplentes", Pn(
                $"<b>CONSULTA DE CPF</b> — {cli.cpf} — emitida em {Tq(38)}",
                $"Credor: <b>{re_nome}</b> (CNPJ {re_cnpj})<br>Contrato: {contrato}<br>Valor: {Brl(divida)}<br>"
                + $"Vencimento: {Tq(150)}<br>Data de inclusão: {Tq(120)}<br>Natureza: {(banco ? "cartão de crédito" : "telefonia")}",
                "Outras anotações: <b>nenhuma</b>."));
            c.anexo("Boletim de ocorrência", Pn(
                $"Comunicante: {Esc(cli.nome)}. Fato: uso indevido de dados pessoais para contratação de {produto} "
                + $"junto a {re_nome}, sem autorização. Data do registro: {Tq(30)}."));
            c.tarefa("Petição inicial — ação declaratória de inexistência de débito c/c indenização por danos morais",
                "inicial", 5, "interno", "Não há prazo processual, mas o cliente está com o nome negativado — quanto antes, melhor.",
                $"Juizado Especial Cível da Comarca de {u.cidade}/{u.uf}", code: "INIC");
            c.mail($"Novo cliente — negativação indevida ({cli.nome.Split(' ')[0]} x {reCurto})", OI,
                $"Atendi ontem {cli.o} {cli.nome}, {NossoCli(cli)} cliente. {fatos}",
                "Deixei em anexo o relato do atendimento, a consulta ao cadastro e o boletim de ocorrência. "
                + "Vamos ajuizar no <b>Juizado Especial Cível</b> (o valor cabe no teto de 40 salários mínimos).",
                "Preciso que você redija a <b>petição inicial</b>. Alguns pontos que não podem faltar:"
                + c.dicas(
                    "Pedido de <b>tutela de urgência</b> para retirada imediata do nome do cadastro (art. 300 do CPC).",
                    "Relação de consumo: responsabilidade objetiva (art. 14 do CDC) e <b>inversão do ônus da prova</b> "
                    + "(art. 6º, VIII, do CDC) — não dá para exigir prova de fato negativo do cliente.",
                    banco ? "Fraude praticada por terceiro é <b>fortuito interno</b>: Súmula 479 do STJ."
                        : "Fraude praticada por terceiro é <b>risco da atividade</b> (fortuito interno) e não afasta a responsabilidade da operadora.",
                    $"Dano moral <i>in re ipsa</i> pela negativação indevida. Vamos pedir {Brl(dm)}.",
                    "Pedidos: declaração de inexistência do débito, exclusão definitiva, indenização e confirmação "
                    + "da tutela. Atenção ao valor da causa."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", tit);
        }

        // ── histórico processual comum
        var inic = c.doc_p("INIC1", "Petição inicial", $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}",
            P($"{Esc(cli.nome)}, {cli.ec}, {Esc(cli.prof)}, CPF {cli.cpf}, vem propor AÇÃO "
              + "DECLARATÓRIA DE INEXISTÊNCIA DE DÉBITO C/C INDENIZAÇÃO POR DANOS MORAIS, com pedido de tutela de "
              + $"urgência, em face de {re_nome}, CNPJ {re_cnpj}.", fatos,
                "Requer a concessão de tutela de urgência para exclusão do nome dos cadastros, a inversão do ônus "
                + $"da prova, a declaração de inexistência do débito e a condenação da ré ao pagamento de {Brl(dm)} "
                + "a título de danos morais."),
            new Adv { nome = !c.socio.f ? c.socio.nome[4..].Trim() : c.socio.nome[5..].Trim(), oab = c.socio.oab }, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic, c.procuracao(cli), c.doc_s(
            "OUT3", "Consulta ao cadastro de inadimplentes", Pn($"Credor: {re_nome}. Contrato {contrato}. Valor "
                                                               + $"{Brl(divida)}. Outras anotações: nenhuma.")) },
            detail: c.orgao);
        c.ev("CONCLUSOS PARA DECISÃO/DESPACHO", gap: (1, 2));
        c.ev("DECISÃO INTERLOCUTÓRIA - DEFERIDA A ANTECIPAÇÃO DE TUTELA", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            "Vistos.",
            "Presentes a probabilidade do direito — a parte autora nega a contratação e apresentou boletim de "
            + "ocorrência — e o perigo de dano decorrente da manutenção do nome em cadastro restritivo, DEFIRO a tutela "
            + "de urgência (art. 300 do CPC) para determinar que a ré providencie a exclusão da anotação no prazo de 5 "
            + "dias, sob pena de multa diária de R$ 200,00, limitada a R$ 6.000,00.",
            "Relação de consumo. Inverto o ônus da prova (art. 6º, VIII, do CDC).",
            "Cite-se e intime-se para audiência de conciliação. Intimem-se.")) }, gap: (2, 5));
        c.citacao($"{c.lab1}: {re_nome.Up()}");
        c.ev("AUDIÊNCIA DE CONCILIAÇÃO REALIZADA - SEM ACORDO", "CONCILIADOR", new[] { c.doc_s("TERMOAUD1", "Termo de audiência", Pn(
            "Aberta a audiência, presentes a parte autora, acompanhada de advogado, e a preposta da ré, acompanhada de "
            + $"advogado. Proposta a conciliação, a ré ofereceu {Brl(Math.Round(dm * .2 / 100, MidpointRounding.ToEven) * 100)}, recusados pela parte autora. "
            + "Conciliação inexitosa. A ré apresentará contestação no prazo de 15 dias; após, réplica em 15 dias.")) }, gap: (15, 25));
        c.ev("CONTESTAÇÃO", adv_re.nome.Up(), new[] { c.doc_p("CONT1", "Contestação",
                $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}", P(
                    $"{re_nome} apresenta CONTESTAÇÃO. A contratação foi regular, realizada por canal digital, com "
                    + "validação de dados pessoais, conforme telas sistêmicas anexas. O débito é legítimo e a inscrição "
                    + "configura exercício regular de direito (art. 188, I, do Código Civil).",
                    "Subsidiariamente, eventual fraude decorreu de culpa exclusiva de terceiro (art. 14, § 3º, II, do CDC). "
                    + "Inexiste dano moral indenizável; caso reconhecido, requer fixação em patamar mínimo.",
                    "Requer a improcedência dos pedidos."), adv_re),
            c.doc_s("OUT2", "Telas sistêmicas", Pn("Capturas de tela do sistema interno da ré, sem assinatura do "
                                                   + "consumidor, indicando contratação digital e endereço diverso do "
                                                   + "domicílio da parte autora.")) }, gap: (10, 14));
        c.ev("RÉPLICA", c.socio.nome.Up(), new[] { c.doc_p("REPLICA1", "Réplica",
            $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}", P(
                "Telas sistêmicas são documentos unilaterais e não comprovam a contratação. Não há contrato assinado, "
                + "gravação ou biometria. Reitera os pedidos iniciais."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }) }, gap: (8, 14));
        c.ev("CONCLUSOS PARA SENTENÇA", gap: (1, 3));
        int ns = c.nxt;
        var base_fund = P(
            "Dispensado o relatório, nos termos do art. 38 da Lei 9.099/95. Decido.",
            "Trata-se de relação de consumo, com inversão do ônus da prova já deferida. A parte ré não juntou contrato "
            + "assinado, gravação de contratação ou qualquer elemento idôneo de que a parte autora tenha anuído com a "
            + "contratação. Telas sistêmicas, produzidas unilateralmente, não se prestam a esse fim. A eventual fraude "
            + "praticada por terceiro constitui fortuito interno, inerente ao risco da atividade, e não afasta a "
            + "responsabilidade objetiva do fornecedor (art. 14 do CDC" + (banco ? "; Súmula 479 do STJ)." : ")."),
            "Assim, o débito deve ser declarado inexistente.");
        if (v == "recurso_inominado")
        {
            var outro = r.choice(new[] { "Lojas Primavera Ltda.", "Magazine Vitória Ltda.", "Financeira Atlas S.A." });
            var proc_outro = GenCnj(r, "8", u.tjc, "0001", Today().Year - 2);
            var sent = c.doc_j("SENT1", "Sentença", base_fund + P(
                "Quanto ao dano moral, contudo, o extrato juntado pela própria parte ré (evento 7) indica que, à época "
                + $"da inscrição, havia anotação anterior em nome da parte autora, lançada por {outro}. Aplica-se a "
                + "Súmula 385 do STJ: \"Da anotação irregular em cadastro de proteção ao crédito, não cabe indenização por "
                + "dano moral, quando preexistente legítima inscrição, ressalvado o direito ao cancelamento\".",
                "Ante o exposto, JULGO PARCIALMENTE PROCEDENTES os pedidos para: a) DECLARAR a inexistência do débito "
                + $"referente ao contrato nº {contrato}; b) CONFIRMAR a tutela de urgência e determinar a exclusão "
                + "definitiva da anotação. JULGO IMPROCEDENTE o pedido de indenização por danos morais.",
                "Sem custas e honorários nesta instância (art. 55 da Lei 9.099/95). Publique-se. Registre-se. Intimem-se."));
            c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - PROCEDÊNCIA EM PARTE", "MAGISTRADO", new[] { sent }, gap: (10, 25));
            c.intimacao($"REQUERENTE: {cli.nome.Up()}", 10);
            c.intimacao($"REQUERIDO: {re_nome.Up()}", 10, gap: (0, 1), nossa: false);
            c.prazo = new JsonObject { ["ev"] = c.nxt - 3, ["dias"] = 10, ["contagem"] = "uteis" };
            c.tarefa("Recurso inominado", "recurso_inominado", 10, "uteis", "art. 42 da Lei 9.099/95",
                c.orgao, $"Turma Recursal dos Juizados Especiais Cíveis — {u.tj}", "RECINOM");
            c.mail($"URGENTE — sentença no caso {cli.nome.Split(' ')[0]} x {reCurto} (proc. {c.numero})", OI,
                $"Saiu a sentença no processo d{cli.o} {cli.nome} (evento {ns}) e ganhamos só pela metade: o juiz "
                + "declarou a dívida inexistente e mandou excluir o nome, mas <b>negou o dano moral</b> com base na "
                + "Súmula 385 do STJ, por causa de uma anotação anterior.",
                $"Acontece que essa anotação anterior, de {outro}, também era indevida: {cli.o} cliente a "
                + $"contestou no processo nº {proc_outro}, e ela já tinha sido <b>excluída por decisão judicial</b> "
                + "antes da sentença. Ou seja, não era uma inscrição \"legítima\", que é o que a súmula exige.",
                "Quero recorrer só da parte do dano moral. Dê uma olhada nos eventos do processo e redija o "
                + "<b>recurso inominado</b>. Pontos de atenção:" + c.dicas(
                    "Recurso endereçado ao juízo de origem, com as razões à Turma Recursal.",
                    "Tempestividade e <b>preparo</b> (art. 42, § 1º, da Lei 9.099/95: até 48 horas após a "
                    + "interposição, independentemente de intimação). O cliente vai recolher.",
                    "Afastar a Súmula 385: a jurisprudência do STJ admite flexibilizá-la quando as inscrições "
                    + "anteriores estão sendo questionadas judicialmente.",
                    $"Pedir a reforma para condenar a ré em danos morais ({Brl(dm)}, como na inicial)."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", tit);
        }
        if (v == "contrarrazoes")
        {
            double val = r.choice(new[] { 6000, 7000, 8000 });
            var sent = c.doc_j("SENT1", "Sentença", base_fund + P(
                "O dano moral decorrente da inscrição indevida é presumido (<i>in re ipsa</i>). Não havia outras "
                + $"anotações. Considerando a extensão do dano e o caráter pedagógico, fixo a indenização em {Brl(val)}.",
                "Ante o exposto, JULGO PROCEDENTES os pedidos para: a) DECLARAR a inexistência do débito; b) CONFIRMAR "
                + $"a tutela de urgência; c) CONDENAR a ré ao pagamento de {Brl(val)} a título de danos morais, com "
                + "correção monetária desde esta data (Súmula 362 do STJ) e juros desde o evento danoso (Súmula 54 do STJ).",
                "Sem custas e honorários (art. 55 da Lei 9.099/95). Publique-se. Registre-se. Intimem-se."));
            c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - PROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (10, 25));
            c.intimacao($"REQUERENTE: {cli.nome.Up()}", 10, nossa: false);
            c.intimacao($"REQUERIDO: {re_nome.Up()}", 10, gap: (0, 1), nossa: false);
            c.ev("RECURSO INOMINADO", adv_re.nome.Up(), new[] { c.doc_p("RECINOM1", "Recurso inominado",
                    $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}", P(
                        $"{re_nome} interpõe RECURSO INOMINADO. A sentença desconsiderou que a contratação foi realizada "
                        + "com dados pessoais corretos da parte autora, o que demonstra cautela da recorrente. Se houve "
                        + "fraude, a culpa é exclusiva de terceiro, excludente prevista no art. 14, § 3º, II, do CDC.",
                        "A mera inscrição, sem outros desdobramentos, configura mero aborrecimento. Subsidiariamente, o "
                        + $"valor de {Brl(val)} é excessivo e gera enriquecimento sem causa; requer redução para R$ 2.000,00.",
                        "Requer o provimento do recurso para julgar improcedentes os pedidos."), adv_re),
                c.doc_s("CUSTAS2", "Comprovante de preparo", Pn("Guia de preparo recursal paga.")) },
                gap: (6, 9));
            c.ev("ATO ORDINATÓRIO PRATICADO", "SERVIDOR", detail: "Intime-se a parte recorrida para contrarrazões",
                gap: (1, 3));
            c.intimacao($"REQUERENTE: {cli.nome.Up()}", 10);
            c.tarefa("Contrarrazões ao recurso inominado", "contrarrazoes", 10, "uteis",
                "art. 42, § 2º, da Lei 9.099/95", c.orgao,
                $"Turma Recursal dos Juizados Especiais Cíveis — {u.tj}", "CONTRAZ");
            c.mail($"Contrarrazões — {cli.nome.Split(' ')[0]} x {reCurto}", OI,
                $"Boa notícia primeiro: no processo d{cli.o} {cli.nome} a sentença foi <b>totalmente "
                + $"favorável</b> (evento {ns}) — dívida declarada inexistente e {Brl(val)} de dano moral.",
                $"Como era de esperar, {re_nome} recorreu (evento {ns + 5}). Alegam culpa exclusiva de terceiro, "
                + "\"mero aborrecimento\" e pedem para reduzir o valor para R$ 2.000,00. Fomos intimados para "
                + "<b>contrarrazões</b>, que vou deixar com você.",
                "Leia o recurso com atenção e rebata ponto a ponto:" + c.dicas(
                    "Fraude de terceiro é fortuito interno — não rompe o nexo causal" + (banco ? " (Súmula 479 do STJ)." : "."),
                    "Dano moral presumido na negativação indevida; não havia outras anotações.",
                    "O valor fixado é proporcional e tem caráter pedagógico — não há motivo para reduzir.",
                    "Lembre que em contrarrazões não dá para pedir aumento do valor (isso exigiria recurso próprio)."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", tit);
        }
        // embargos de declaração
        {
            double val = r.choice(new[] { 5000, 6000, 7000 });
            var sent = c.doc_j("SENT1", "Sentença", P(
                "Dispensado o relatório, nos termos do art. 38 da Lei 9.099/95. Decido.",
                "A parte ré não comprovou a contratação. Telas sistêmicas são documentos unilaterais. A inscrição foi "
                + "indevida e o dano moral, nessa hipótese, é presumido. Fixo a indenização em " + Brl(val) + ".",
                $"Ante o exposto, JULGO PROCEDENTE o pedido para CONDENAR a ré ao pagamento de {Brl(val)} a título de "
                + "danos morais, corrigidos monetariamente.",
                "Sem custas e honorários (art. 55 da Lei 9.099/95). Publique-se. Registre-se. Intimem-se."));
            c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - PROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (10, 25));
            c.intimacao($"REQUERENTE: {cli.nome.Up()}", 5);
            c.tarefa("Embargos de declaração", "embargos", 5, "uteis", "arts. 48 e 49 da Lei 9.099/95",
                c.orgao, code: "EMBDECL");
            c.mail($"Embargos de declaração — sentença omissa ({cli.nome.Split(' ')[0]})", OI,
                $"A sentença do caso d{cli.o} {cli.nome} saiu (evento {ns}). Condenou {re_nome} em {Brl(val)} de "
                + "dano moral, ótimo, mas leia com atenção: o juiz <b>não decidiu</b> todos os nossos pedidos.",
                "Pelo que vi, a sentença:" + c.dicas(
                    $"não <b>declarou a inexistência do débito</b> do contrato nº {contrato} (pedido principal!);",
                    "não <b>confirmou a tutela de urgência</b>, nem determinou a exclusão definitiva da anotação;",
                    "falou só em \"corrigidos monetariamente\", sem definir o <b>termo inicial da correção e dos juros</b> "
                    + "(Súmulas 362 e 54 do STJ)."),
                "Precisamos de <b>embargos de declaração</b> apontando essas omissões (art. 48 da Lei 9.099/95 c/c art. "
                + "1.022, II, do CPC). Seja objetivo: embargos não servem para rediscutir o mérito, só para integrar a "
                + "decisão. Peça também que fique registrada a interrupção do prazo para recurso (art. 50 da Lei 9.099/95).",
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", tit);
        }
    }

    public static JsonObject Transito(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var autor = c.pf();
        var reu = c.pf();
        var juiz = c.juiz();
        var adv_x = c.adv();
        var mat = c.money(7800, 24500);
        var lc = c.money(2400, 6800);
        var nosso = c.nossos_advs();
        bool cliente_autor = v is "replica" or "memoriais" or "apelacao" or "contrarrazoes";
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        c.processo("estadual", "PROCEDIMENTO COMUM CÍVEL", vara,
            new[] { "Acidente de Trânsito", "Indenização por Dano Material", "Lucros Cessantes" }, mat + lc, juiz);
        c.parte("a", autor.nome, autor.cpf, cliente_autor ? nosso : new List<string> { $"{adv_x.nome} ({adv_x.oab})" }, cliente_autor);
        c.parte("p", reu.nome, reu.cpf, cliente_autor ? new List<string> { $"{adv_x.nome} ({adv_x.oab})" } : nosso, !cliente_autor);
        var ruas = r.sample(new[] { "Av. Brasil", "Rua das Acácias", "Av. Independência", "Rua XV de Novembro",
            "Av. Santos Dumont", "Rua Marechal Deodoro", "Av. dos Andradas" }, 2);
        var (rua1, rua2) = (ruas[0], ruas[1]);
        var carros = r.sample(new[] { "um Fiat Argo", "um VW Polo", "um Chevrolet Onix", "um Hyundai HB20",
            "um Toyota Corolla", "um Renault Kwid" }, 2);
        var (carro_a, carro_b) = (carros[0], carros[1]);
        var fatos = $"Em {Ctx.A(70)}, por volta das {r.randint(7, 20)}h, no cruzamento da {rua1} com a {rua2}, em "
                    + $"{u.cidade}, {carro_a} conduzido por {autor.nome} (autor{(autor.f ? "a" : "")}) foi "
                    + $"atingido na lateral por {carro_b} conduzido por {reu.nome}. Segundo a inicial, o réu avançou o "
                    + "sinal vermelho. A parte autora pede o conserto do veículo e lucros cessantes, pois trabalha como "
                    + "motorista de aplicativo e ficou sem o carro durante o reparo.";
        var adv_aut = cliente_autor ? new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab } : adv_x;
        var adv_reu = cliente_autor ? adv_x : new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab };
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}";
        var inic = c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{Esc(autor.nome)}, CPF {autor.cpf}, propõe AÇÃO DE INDENIZAÇÃO POR DANOS MATERIAIS E LUCROS "
            + $"CESSANTES em face de {Esc(reu.nome)}, CPF {reu.cpf}.", fatos,
            $"O orçamento de menor valor, entre três obtidos, totaliza {Brl(mat)}. Os lucros cessantes, apurados pela "
            + $"média de ganhos dos três meses anteriores, somam {Brl(lc)}. Há boletim de ocorrência.",
            "Com base nos arts. 186 e 927 do Código Civil e no art. 44 do Código de Trânsito Brasileiro, requer a "
            + $"condenação do réu ao pagamento de {Brl(mat + lc)}, com correção e juros."), adv_aut, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_aut.nome.Up(), new[] { inic, c.procuracao(autor), c.doc_s(
                "BO3", "Boletim de ocorrência de trânsito", Pn(
                    $"Data: {Ctx.A(70)}. Local: {rua1} x {rua2}. Versão do condutor 1 ({Esc(autor.nome)}): \"o outro "
                    + $"veículo cruzou com o sinal fechado\". Versão do condutor 2 ({Esc(reu.nome)}): \"o sinal estava "
                    + "amarelo e o outro carro estava em alta velocidade\". Sem vítimas.")),
            c.doc_s("ORC4", "Orçamentos", Pn($"Três orçamentos de oficinas: {Brl(mat)}, {Brl(mat * 1.08)} e "
                                             + $"{Brl(mat * 1.15)}.")) }, detail: vara);
        c.ev("DESPACHO", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Despacho", P(
            "Recebo a inicial. Designo audiência de conciliação (art. 334 do CPC). Cite-se. O prazo para contestação "
            + "observará o art. 335 do CPC.")) }, gap: (3, 8));
        c.citacao($"RÉU: {reu.nome.Up()}");
        int n_aud = c.ev("AUDIÊNCIA DE CONCILIAÇÃO REALIZADA - SEM ACORDO", "CONCILIADOR",
            new[] { c.doc_s("TERMOAUD1", "Termo de audiência de conciliação", Pn(
                "Presentes as partes e seus advogados. Tentada a conciliação, não houve acordo. Fica aberto ao "
                + "réu o prazo de 15 dias úteis para contestação, contado desta audiência (art. 335, I, do CPC).")) }, gap: (20, 35));
        var seg = r.choice(new[] { "Seguradora Porto Firme S.A.", "Aliança Sul Seguros S.A.", "Rota Segura Seguros S.A." });
        if (v == "contestacao")
        {
            c.prazo = new JsonObject { ["ev"] = n_aud, ["dias"] = 15, ["contagem"] = "uteis" };
            c.tarefa("Contestação", "contestacao", 15, "uteis", "art. 335, I, do CPC", vara, code: "CONT");
            c.mail($"Contestação — acidente de trânsito ({reu.nome.Split(' ')[0]})", OI,
                $"{reu.O} {reu.nome}, cliente nov{reu.o}, está sendo processad{reu.o} por "
                + $"{autor.nome} por causa de um acidente de trânsito. {fatos}",
                $"A audiência de conciliação foi ontem e não teve acordo (evento {n_aud}), então o prazo de "
                + "contestação já está correndo. Conversei com o cliente e a versão dele é outra:" + c.dicas(
                    "O sinal <b>estava amarelo</b> quando ele entrou no cruzamento; o carro da parte autora vinha em "
                    + "velocidade acima da permitida (há um vídeo de câmera de um comércio da esquina).",
                    $"Há uma testemunha: {c.pf().nome}, que estava no ponto de ônibus.",
                    $"O cliente tem seguro com cobertura para danos a terceiros na <b>{seg}</b>."),
                "Redija a <b>contestação</b>. Sugestões:" + c.dicas(
                    "Culpa exclusiva da vítima ou, ao menos, <b>culpa concorrente</b> (art. 945 do CC).",
                    $"Avalie a <b>denunciação da lide</b> à {seg} (art. 125, II, do CPC).",
                    "Impugne os valores: escolha do orçamento, ausência de notas fiscais do conserto e falta de "
                    + "prova concreta dos lucros cessantes (extratos da plataforma?).",
                    "Especifique provas: vídeo, testemunha e depoimento pessoal."),
                c.prazo_frase(), c.assinatura());
            return c.build(reu.nome, "passivo", $"{autor.nome} x {reu.nome}");
        }
        var cont = c.doc_p("CONT1", "Contestação", ender, P(
            $"{Esc(reu.nome)} apresenta CONTESTAÇÃO.",
            "<b>Preliminarmente</b>, alega ilegitimidade ativa: o veículo está registrado em nome de "
            + $"{r.choice(new[] { "Banco Meridiano S.A. (alienação fiduciária)", "cônjuge da parte autora" })}, de modo que "
            + "apenas o proprietário poderia pedir indenização.",
            "No mérito, sustenta que o sinal estava amarelo e que a parte autora trafegava em velocidade excessiva, "
            + "caracterizando culpa exclusiva da vítima. Impugna os orçamentos, por ausência de nota fiscal, e os lucros "
            + "cessantes, por falta de prova dos rendimentos.",
            "Requer o acolhimento da preliminar ou a improcedência dos pedidos."), adv_reu);
        int n_cont = c.ev("CONTESTAÇÃO", adv_reu.nome.Up(), new[] { cont }, gap: (10, 14));
        if (v == "replica")
        {
            c.ev("ATO ORDINATÓRIO PRATICADO", "SERVIDOR", detail: "Intimação da parte autora para réplica",
                gap: (2, 5));
            c.intimacao($"AUTOR: {autor.nome.Up()}", 15);
            c.tarefa("Réplica (impugnação à contestação)", "replica", 15, "uteis", "arts. 350 e 351 do CPC", vara,
                code: "REPLICA");
            c.mail($"Réplica — {autor.nome.Split(' ')[0]} x {reu.nome.Split(' ')[0]}", OI,
                $"No processo d{autor.o} {autor.nome} (acidente de trânsito), o réu contestou (evento "
                + $"{n_cont}) e fomos intimados para <b>réplica</b>.",
                "Ele levantou uma <b>preliminar de ilegitimidade ativa</b> porque o carro não está no nome do "
                + "cliente, e no mérito diz que o sinal estava amarelo e que o cliente estava rápido demais.",
                "Pontos para a réplica:" + c.dicas(
                    "Rebater a preliminar: quem tem a posse direta e sofreu o prejuízo pode pedir indenização "
                    + "(o cliente usa o carro para trabalhar e pagou o conserto).",
                    "No mérito, valorizar o boletim de ocorrência e o fato de o réu ter cruzado no amarelo/vermelho "
                    + "— quem chega ao cruzamento no amarelo deve parar se puder (art. 44 do CTB).",
                    "Defender os orçamentos (escolhemos o menor de três) e juntar extratos da plataforma para os "
                    + "lucros cessantes.",
                    "Especificar provas: testemunhas e depoimento pessoal do réu."),
                c.prazo_frase(), c.assinatura());
            return c.build(autor.nome, "ativo", $"{autor.nome} x {reu.nome}");
        }
        c.ev("RÉPLICA", adv_aut.nome.Up(), new[] { c.doc_p("REPLICA1", "Réplica", ender, P(
            "A parte autora, possuidora direta do veículo, é legítima para pleitear a reparação. No mérito, o réu "
            + "admitiu ter ingressado no cruzamento com o sinal amarelo. Reitera os pedidos."), adv_aut) }, gap: (10, 14));
        c.ev("DECISÃO DE SANEAMENTO E ORGANIZAÇÃO", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão de saneamento", P(
            "Rejeito a preliminar de ilegitimidade ativa: o possuidor direto do veículo, que suportou o prejuízo, tem "
            + "legitimidade para buscar a reparação. Fixo como pontos controvertidos a dinâmica do acidente, a extensão dos "
            + "danos materiais e a existência de lucros cessantes. Defiro prova testemunhal. Designo audiência de "
            + "instrução e julgamento.")) }, gap: (15, 30));
        var test1 = c.pf();
        var test2 = c.pf();
        var termo = c.doc_s("TERMOAUD1", "Termo de audiência de instrução e julgamento", Pn(
            "Depoimento pessoal da parte autora: confirmou que o sinal estava verde para si e que trafegava a cerca de "
            + "50 km/h.",
            "Depoimento pessoal do réu: disse que \"o sinal tinha acabado de ficar amarelo\" e que não conseguiu frear.",
            $"Testemunha {Esc(test1.nome)} (arrolada pela autora), motorista que aguardava no cruzamento: afirmou "
            + "que o sinal para o réu \"já estava vermelho\" e que o carro da autora não estava em velocidade alta.",
            $"Testemunha {Esc(test2.nome)} (arrolada pelo réu), vendedor de loja próxima: disse ter ouvido a batida, "
            + "mas admitiu que não viu o semáforo no momento da colisão.",
            "Encerrada a instrução, as partes requereram a substituição dos debates orais por razões finais escritas, "
            + "o que foi deferido, com prazos sucessivos de 15 dias úteis, primeiro para a parte autora (art. 364, § 2º, "
            + "do CPC)."));
        int n_ins = c.ev("AUDIÊNCIA DE INSTRUÇÃO E JULGAMENTO REALIZADA", "MAGISTRADO", new[] { termo }, gap: (30, 50));
        if (v == "memoriais")
        {
            c.intimacao($"AUTOR: {autor.nome.Up()}", 15);
            c.tarefa("Memoriais (razões finais escritas)", "memoriais", 15, "uteis", "art. 364, § 2º, do CPC", vara,
                code: "ALEGACOES");
            c.mail($"Memoriais — {autor.nome.Split(' ')[0]} x {reu.nome.Split(' ')[0]} (instrução encerrada)", OI,
                $"Tivemos a audiência de instrução no caso do acidente d{autor.o} {autor.nome} (evento {n_ins}). "
                + "Foi muito bem para nós. Leia o termo da audiência, que resume os depoimentos.",
                "O juiz converteu os debates orais em <b>memoriais</b> e nós falamos primeiro. Monte assim:" + c.dicas(
                    "Breve síntese do processo (inicial, contestação, saneamento).",
                    $"Análise da prova: a testemunha {test1.nome} viu o sinal vermelho; a testemunha do réu "
                    + "<b>não viu</b> o semáforo; o próprio réu admitiu o amarelo.",
                    "Ônus da prova (art. 373, I, do CPC) cumprido pela parte autora; nexo causal e culpa do réu "
                    + "(arts. 186 e 927 do CC; art. 44 do CTB).",
                    "Danos materiais (menor de três orçamentos) e lucros cessantes comprovados.",
                    "Pedido final de procedência integral."),
                c.prazo_frase(), c.assinatura());
            return c.build(autor.nome, "ativo", $"{autor.nome} x {reu.nome}");
        }
        c.ev("ALEGAÇÕES FINAIS", adv_aut.nome.Up(), new[] { c.doc_p("ALEGACOES1", "Memoriais", ender, P(
            "A prova testemunhal confirmou a versão da parte autora. Reitera a procedência."), adv_aut) }, gap: (12, 15));
        c.ev("ALEGAÇÕES FINAIS", adv_reu.nome.Up(), new[] { c.doc_p("ALEGACOES1", "Memoriais", ender, P(
            "Não há prova segura da dinâmica do acidente. Requer a improcedência."), adv_reu) }, gap: (12, 15));
        c.ev("CONCLUSOS PARA SENTENÇA", gap: (1, 3));
        int ns = c.nxt;
        if (v == "apelacao")
        {
            var sent = c.doc_j("SENT1", "Sentença", P(
                $"{Esc(autor.nome)} ajuizou ação de indenização contra {Esc(reu.nome)}, narrando colisão em "
                + "cruzamento semaforizado. O réu contestou, imputando culpa exclusiva à parte autora. Houve instrução.",
                "É o relatório. Decido.",
                "Incumbe à parte autora a prova do fato constitutivo de seu direito (art. 373, I, do CPC). As versões "
                + "das partes são conflitantes e o boletim de ocorrência apenas reproduz declarações unilaterais. A prova "
                + "testemunhal é dividida: uma testemunha afirma que o sinal estava vermelho; a outra não presenciou o "
                + "semáforo. Diante da dúvida sobre a dinâmica do acidente, não há como imputar culpa ao réu.",
                "Ante o exposto, JULGO IMPROCEDENTES os pedidos. Condeno a parte autora ao pagamento das custas e de "
                + "honorários advocatícios de 10% sobre o valor da causa (art. 85, § 2º, do CPC). Publique-se. "
                + "Intimem-se."));
            c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - IMPROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (15, 35));
            c.intimacao($"AUTOR: {autor.nome.Up()}", 15);
            c.tarefa("Apelação", "apelacao", 15, "uteis", "art. 1.003, § 5º, do CPC", vara,
                $"{u.tj}", "APELACAO");
            c.mail($"Sentença desfavorável — apelação ({autor.nome.Split(' ')[0]} x {reu.nome.Split(' ')[0]})", OI,
                $"Más notícias: a sentença do caso d{autor.o} {autor.nome} julgou <b>improcedentes</b> os pedidos "
                + $"(evento {ns}). O juiz disse que a prova ficou \"dividida\" e aplicou o art. 373, I, do CPC.",
                $"Discordo totalmente. Só uma testemunha viu o semáforo — a nossa ({test1.nome}) — e ela foi "
                + "categórica. A testemunha do réu admitiu que não viu o sinal, e o próprio réu confessou que entrou "
                + "no amarelo. Isso não é prova dividida.",
                "Vamos de <b>apelação</b>. Estruture assim:" + c.dicas(
                    "Petição de interposição ao juízo de 1º grau + razões ao Tribunal (art. 1.010 do CPC).",
                    "Tempestividade e <b>preparo</b> (art. 1.007 do CPC) — o cliente vai recolher.",
                    "Erro na valoração da prova: depoimentos, confissão do réu, art. 44 do CTB.",
                    "Pedido de reforma para julgar procedentes os pedidos e inverter a sucumbência."),
                c.prazo_frase(), c.assinatura());
            return c.build(autor.nome, "ativo", $"{autor.nome} x {reu.nome}");
        }
        // contrarrazões à apelação do réu
        {
            var sent = c.doc_j("SENT1", "Sentença", P(
                $"{Esc(autor.nome)} ajuizou ação de indenização contra {Esc(reu.nome)}. Houve contestação e instrução.",
                "É o relatório. Decido.",
                "A prova oral demonstrou que o réu ingressou no cruzamento com o semáforo vermelho. A única testemunha que "
                + "presenciou o sinal foi firme nesse sentido, e o próprio réu admitiu que o sinal já estava amarelo. Presentes "
                + "a conduta culposa, o dano e o nexo causal (arts. 186 e 927 do CC).",
                $"Os danos materiais estão comprovados pelo menor de três orçamentos ({Brl(mat)}). Os lucros cessantes foram "
                + $"demonstrados pelos extratos da plataforma ({Brl(lc)}).",
                $"Ante o exposto, JULGO PROCEDENTES os pedidos para condenar o réu ao pagamento de {Brl(mat + lc)}, com "
                + "correção monetária e juros legais. Condeno o réu em custas e honorários de 10% sobre a condenação. "
                + "Publique-se. Intimem-se."));
            c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - PROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (15, 35));
            c.intimacao($"AUTOR: {autor.nome.Up()}", 15, nossa: false);
            c.intimacao($"RÉU: {reu.nome.Up()}", 15, gap: (0, 1), nossa: false);
            int n_ap = c.ev("APELAÇÃO", adv_reu.nome.Up(), new[] { c.doc_p("APELACAO1", "Apelação", ender, P(
                    $"{Esc(reu.nome)} interpõe APELAÇÃO. A sentença baseou-se em uma única testemunha, que seria amiga da "
                    + "parte autora. A confissão de que o sinal estava amarelo não equivale a avanço do sinal vermelho.",
                    "Subsidiariamente, deve ser reconhecida culpa concorrente (art. 945 do CC), pois a autora trafegava em "
                    + "velocidade incompatível. Os lucros cessantes não foram provados, pois os extratos não indicam despesas.",
                    "Requer a reforma da sentença para julgar improcedentes os pedidos ou, subsidiariamente, reduzir a "
                    + "condenação pela metade."), adv_reu), c.doc_s("CUSTAS2", "Comprovante de preparo",
                    Pn("Preparo recolhido.")) }, gap: (10, 14));
            c.ev("ATO ORDINATÓRIO PRATICADO", "SERVIDOR", detail: "Intime-se o apelado para contrarrazões (art. 1.010, § 1º, CPC)");
            c.intimacao($"AUTOR: {autor.nome.Up()}", 15);
            c.tarefa("Contrarrazões à apelação", "contrarrazoes", 15, "uteis", "art. 1.010, § 1º, do CPC", vara,
                u.tj, "CONTRAZ");
            c.mail($"Contrarrazões de apelação — {autor.nome.Split(' ')[0]} x {reu.nome.Split(' ')[0]}", OI,
                $"Ganhamos em 1º grau o caso do acidente d{autor.o} {autor.nome} (evento {ns}), com condenação de "
                + $"{Brl(mat + lc)}. O réu <b>apelou</b> (evento {n_ap}).",
                "Ele ataca a credibilidade da nossa testemunha (diz que seria amiga do cliente — o que nunca foi alegado "
                + "antes nem contraditado na audiência), pede culpa concorrente e questiona os lucros cessantes.",
                "Redija as <b>contrarrazões</b>:" + c.dicas(
                    "Testemunha não foi contraditada no momento oportuno (art. 457, § 1º, do CPC): preclusão.",
                    "Confissão do réu sobre o sinal amarelo + art. 44 do CTB.",
                    "Não há prova de velocidade excessiva da parte autora: culpa concorrente não demonstrada.",
                    "Lucros cessantes comprovados pelos extratos; pedir também a <b>majoração dos honorários</b> em "
                    + "grau recursal (art. 85, § 11, do CPC)."),
                c.prazo_frase(), c.assinatura());
            return c.build(autor.nome, "ativo", $"{autor.nome} x {reu.nome}");
        }
    }

    public static JsonObject Alimentos(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var pai = c.pf("M");
        var mae = c.pf("F");
        var filho = c.pf();
        int idade = r.randint(4, 12);
        var sal = c.money(3200, 5200);
        var vara = $"{c.ordm(1, 6)}ª Vara de Família da Comarca de {u.cidade}";
        var juiz = c.juiz();
        var adv_m = c.adv();
        var nosso = c.nossos_advs();
        c.processo("estadual", "ALIMENTOS - LEI ESPECIAL Nº 5.478/68", vara, new[] { "Fixação", "Alimentos" },
            Math.Round(sal * .3 * 12, 2), juiz, segredo: true);
        c.parte("a", $"{filho.init} (menor), representad{filho.o} por {mae.init}", "***", new[] { $"{adv_m.nome} ({adv_m.oab})" });
        c.parte("p", pai.nome, pai.cpf, nosso, true);
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}";
        int outro_filho = r.randint(1, 3);
        var inic = c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{filho.init}, com {idade} anos, representad{filho.o} por sua genitora {mae.init}, propõe AÇÃO DE "
            + $"ALIMENTOS em face de {Esc(pai.nome)}, seu genitor.",
            $"O réu trabalha como {Esc(pai.prof)} e teria renda superior a {Brl(sal * 1.4)}. A criança tem despesas "
            + "com escola, plano de saúde, alimentação e vestuário. Requer a fixação de alimentos provisórios de 30% dos "
            + "rendimentos brutos do réu (art. 4º da Lei 5.478/68) e, ao final, alimentos definitivos no mesmo patamar."),
            adv_m, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_m.nome.Up(), new[] { inic }, detail: $"{vara} — SEGREDO DE JUSTIÇA");
        int n_dec = c.ev("DECISÃO INTERLOCUTÓRIA - ALIMENTOS PROVISÓRIOS FIXADOS", "MAGISTRADO", new[] { c.doc_j(
            "DESPADEC1", "Decisão", P(
                "Vistos. Comprovada a relação de parentesco, FIXO alimentos provisórios em 30% (trinta por cento) dos "
                + "rendimentos brutos do réu, incluídos 13º salário e férias, a serem descontados em folha (art. 4º da Lei "
                + "5.478/68). Oficie-se ao empregador.",
                "Designo audiência de conciliação, instrução e julgamento. Cite-se o réu, com as advertências do art. 7º "
                + "da Lei 5.478/68.")) }, gap: (2, 4));
        int n_cit = c.citacao($"RÉU: {pai.nome.Up()}");
        var gastos = Brl(c.money(900, 1400));
        var dicas_base = new[]
        {
            "Binômio <b>necessidade × possibilidade</b> (art. 1.694, § 1º, do CC): o salário bruto do cliente é "
            + $"{Brl(sal)}, e o líquido é bem menor.",
            $"Ele tem outr{(outro_filho == 1 ? "o filho" : "os " + outro_filho + " filhos")} do atual "
            + $"relacionamento e paga aluguel ({gastos}).",
            "Ele não se nega a pagar: oferece um valor compatível (sugiro 18% dos rendimentos líquidos, ou o "
            + "percentual que você justificar), mais metade das despesas escolares.",
            "O processo tramita em <b>segredo de justiça</b> — identifique a criança só pelas iniciais.",
        };
        if (v == "agravo")
        {
            c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 15, ["contagem"] = "uteis" };
            c.tarefa("Agravo de instrumento (contra os alimentos provisórios)", "agravo", 15, "uteis",
                "arts. 1.003, § 5º, e 1.015, I, do CPC", vara, u.tj, "AGRAVO");
            c.mail($"Alimentos provisórios — agravo de instrumento ({pai.nome.Split(' ')[0]})", OI,
                $"Recebemos um novo cliente, o {pai.sr} {pai.nome}. Ele foi citado numa ação de alimentos "
                + $"movida pel{filho.o} filh{filho.o} (evento {n_cit}), e o juiz já fixou <b>alimentos "
                + $"provisórios de 30% dos rendimentos brutos</b> (evento {n_dec}). Com o desconto em folha, ele "
                + "não vai conseguir pagar as próprias contas.",
                "Quero atacar essa decisão já, sem esperar a audiência. Redija o <b>agravo de instrumento</b>:"
                + c.dicas(dicas_base.Concat(new[]
                {
                    "O agravo é endereçado <b>diretamente ao Tribunal</b> (art. 1.016 do CPC) — confira as peças "
                    + "obrigatórias do art. 1.017.",
                    "Peça <b>efeito suspensivo / antecipação da tutela recursal</b> (art. 1.019, I) para reduzir o "
                    + "percentual desde já.",
                }).ToArray()),
                c.prazo_frase(), c.assinatura());
            return c.build(pai.nome, "passivo", $"Alimentos — {filho.init} x {pai.nome}");
        }
        c.tarefa("Contestação (ação de alimentos)", "contestacao", r.randint(7, 10), "audiencia",
            "art. 9º da Lei 5.478/68", vara, code: "CONT");
        c.mail($"Ação de alimentos — contestação ({pai.nome.Split(' ')[0]})", OI,
            $"Novo cliente na área de família: {pai.sr} {pai.nome}, réu numa ação de alimentos movida "
            + $"pel{filho.o} filh{filho.o} de {idade} anos, representad{filho.o} pela mãe. Os provisórios "
            + $"foram fixados em 30% dos rendimentos brutos (evento {n_dec}) e ele já foi citado (evento {n_cit}).",
            "Na Lei de Alimentos, a defesa é apresentada na <b>audiência de conciliação, instrução e julgamento</b>, "
            + "mas vamos juntar a contestação escrita no sistema antes. Pontos:" + c.dicas(dicas_base.Concat(new[]
            {
                "Pedir a revisão dos provisórios já na contestação e indicar as provas (contracheques, contrato de "
                + "aluguel, certidão de nascimento dos outros filhos).",
            }).ToArray()),
            c.prazo_frase(), c.assinatura());
        return c.build(pai.nome, "passivo", $"Alimentos — {filho.init} x {pai.nome}");
    }

    public static JsonObject Saude(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf("F");
        var plano = r.choice(new[] { "Vida Plena Saúde Ltda.", "Unimais Assistência Médica S.A.", "Bem Viver Planos de Saúde S.A." });
        var cnpj = GenCnpj(r);
        var med = "capecitabina";
        var custo = c.money(4200, 9800);
        var medica = c.pf("F");
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        var juiz = c.juiz();
        var nosso = c.nossos_advs();
        var adv_p = c.adv();
        c.processo("estadual", "PROCEDIMENTO COMUM CÍVEL", vara, new[] { "Planos de Saúde", "Tratamento Médico-Hospitalar",
            "Indenização por Dano Moral" }, custo * 12 + 15000, juiz);
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", plano, cnpj, new[] { $"{adv_p.nome} ({adv_p.oab})" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var fatos = $"{cli.nome}, {r.randint(38, 66)} anos, beneficiária de plano individual da {plano} há "
                    + $"{r.randint(4, 15)} anos, foi diagnosticada com câncer de mama. A oncologista Dra. {medica.nome} "
                    + $"prescreveu quimioterapia oral com <b>{med}</b> (custo mensal aproximado de {Brl(custo)}). Em {Tq(12)}, "
                    + "o plano negou a cobertura por escrito, alegando que se trata de \"medicamento de uso domiciliar, "
                    + "excluído do contrato (art. 10, VI, da Lei 9.656/98)\".";
        var dicas = new[]
        {
            "Súmula 608 do STJ: o CDC se aplica aos planos de saúde (exceto autogestão).",
            "A Lei 9.656/98 obriga a cobertura de <b>antineoplásicos orais</b> (art. 12, I, \"c\", e II, \"g\"), "
            + "exceção expressa à exclusão de medicamentos domiciliares do art. 10, VI.",
            "<b>Tutela de urgência</b> (art. 300 do CPC): risco evidente — o tratamento não pode esperar.",
            "Dano moral pela recusa indevida de cobertura em momento de fragilidade.",
        };
        if (v == "inicial")
        {
            c.anexo("Relatório médico", Pn($"Paciente {Esc(cli.nome)}. Diagnóstico: neoplasia maligna de mama. "
                                          + $"Indicação: {med} via oral, ciclos de 21 dias, com início <b>imediato</b>. O "
                                          + $"atraso compromete o prognóstico. Dra. {Esc(medica.nome)} — CRM/{u.uf} "
                                          + $"{r.randint(10000, 49999)}."));
            c.anexo("Negativa do plano", Pn("Prezada beneficiária, informamos que a solicitação de cobertura para o "
                                           + $"medicamento {med} foi NEGADA, por tratar-se de medicamento para tratamento "
                                           + $"domiciliar, excluído nos termos do art. 10, VI, da Lei 9.656/98. Data: "
                                           + $"{Tq(12)}."));
            c.tarefa("Petição inicial — obrigação de fazer com tutela de urgência c/c danos morais", "inicial", 2,
                "interno", "Caso urgente: a paciente precisa começar o tratamento.", vara, code: "INIC");
            c.mail($"URGENTE — negativa de quimioterapia oral ({cli.nome.Split(' ')[0]})", OI,
                $"Recebi hoje a {cli.sr} {cli.nome} com a filha. {fatos}",
                "Em anexo estão o relatório médico e a negativa por escrito. Vamos ajuizar uma <b>ação de obrigação de "
                + "fazer com pedido de tutela de urgência</b>, cumulada com danos morais (sugiro R$ 15.000,00). "
                + "Fundamentos:" + c.dicas(dicas.Concat(new[]
                {
                    "Pedido de tutela para fornecimento do medicamento em 48h, sob pena de multa diária.",
                    "Valor da causa: 12 meses de tratamento + dano moral (art. 292 do CPC).",
                }).ToArray()),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x {plano}");
        }
        var inic = c.doc_p("INIC1", "Petição inicial", $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}", P(
                $"{Esc(cli.nome)} propõe AÇÃO DE OBRIGAÇÃO DE FAZER C/C INDENIZAÇÃO POR DANOS MORAIS, com pedido de tutela "
                + $"de urgência, em face de {plano}.", fatos,
                "Requer tutela de urgência para fornecimento imediato do medicamento e, ao final, a confirmação da tutela e "
                + "indenização por danos morais de R$ 15.000,00."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic, c.procuracao(cli),
            c.doc_s("LAUDO3", "Relatório médico", Pn($"Indicação de {med} com início imediato.")),
            c.doc_s("OUT4", "Negativa do plano", Pn("Negativa com base no art. 10, VI, da Lei 9.656/98.")) },
            detail: vara);
        c.ev("CONCLUSOS PARA DECISÃO/DESPACHO", gap: (1, 1));
        int n_dec = c.ev("DECISÃO INTERLOCUTÓRIA - INDEFERIDA A ANTECIPAÇÃO DE TUTELA", "MAGISTRADO", new[] { c.doc_j(
            "DESPADEC1", "Decisão", P(
                "Vistos. Não vislumbro, neste momento, a probabilidade do direito. O contrato exclui expressamente "
                + "medicamentos de uso domiciliar, nos termos do art. 10, VI, da Lei 9.656/98, e a questão demanda "
                + "contraditório e, eventualmente, prova técnica.",
                "INDEFIRO a tutela de urgência. Cite-se a ré. Intimem-se.")) }, gap: (1, 3));
        c.intimacao($"AUTOR: {cli.nome.Up()}", 15, gap: (0, 1));
        c.tarefa("Agravo de instrumento com pedido de antecipação da tutela recursal", "agravo", 15, "uteis",
            "arts. 1.003, § 5º, e 1.015, I, do CPC", vara, u.tj, "AGRAVO");
        c.meta.O("tarefa")["interno"] = 2;
        c.mail($"URGENTE — tutela negada, agravo de instrumento ({cli.nome.Split(' ')[0]} x plano)", OI,
            $"O juiz <b>indeferiu a tutela de urgência</b> no caso da {cli.sr} {cli.nome} (evento {n_dec}). "
            + "Ele aceitou o argumento de que remédio de uso domiciliar está excluído — ignorou a exceção legal dos "
            + "antineoplásicos orais.",
            "Ela não pode esperar o fim do processo. Precisamos de um <b>agravo de instrumento</b> com pedido de "
            + "<b>antecipação da tutela recursal</b> (art. 1.019, I, do CPC). Prazo legal de 15 dias úteis, mas quero "
            + "protocolar em no máximo 2 dias. Pontos:" + c.dicas(dicas.Take(3).Concat(new[]
            {
                "Cabimento: decisão sobre tutela provisória (art. 1.015, I) — endereçamento direto ao Tribunal.",
                "Juntar as peças obrigatórias do art. 1.017 e comprovar a urgência com o relatório médico.",
            }).ToArray()),
            "O prazo legal vence em §PF§, mas me mande a minuta até <b>§LIMX§</b>.", c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x {plano}");
    }

    public static JsonObject Despejo(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var locador = c.pf();
        var locat = c.pf();
        int aluguel = r.choice(new[] { 1450, 1680, 1900, 2200, 2650, 3100 });
        int meses = r.choice(new[] { 3, 4 });
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        var adv_x = c.adv();
        var end = $"{r.choice(new[] { "Rua", "Av." })} {r.choice(new[] { "Ipiranga", "Getúlio Vargas", "Bento Gonçalves", "Afonso Pena", "Paulista" })}, nº {r.randint(100, 2400)}, apto {r.randint(101, 1204)}";
        int total = aluguel * meses;
        if (v == "inicial")
        {
            c.processo("estadual", "DESPEJO POR FALTA DE PAGAMENTO CUMULADO COM COBRANÇA", vara,
                new[] { "Despejo por Falta de Pagamento", "Locação de Imóvel" }, aluguel * 12);
            c.parte("a", locador.nome, locador.cpf, nosso, true);
            c.parte("p", locat.nome, locat.cpf, new List<string>());
            c.anexo("Contrato de locação (resumo)", Pn(
                $"Locador: {Esc(locador.nome)}. Locatári{locat.o}: {Esc(locat.nome)}. Imóvel residencial: "
                + $"{end}, {u.cidade}/{u.uf}. Prazo: 30 meses, iniciado em {Ctx.B(400)}. Aluguel: {Brl(aluguel)}, "
                + "vencimento todo dia 5. Encargos: condomínio e IPTU por conta do locatário. Garantia: <b>nenhuma</b> "
                + "(o fiador original faleceu e não foi substituído). Multa moratória de 10%."));
            c.anexo("Demonstrativo do débito", Pn(
                $"Aluguéis em aberto: {meses} meses × {Brl(aluguel)} = {Brl(total)}. Condomínios em aberto: "
                + $"{Brl(meses * 480)}. Notificação extrajudicial enviada em {Ctx.B(20)}, sem resposta."));
            c.tarefa("Petição inicial — despejo por falta de pagamento c/c cobrança", "inicial", 4, "interno",
                "Não há prazo processual.", vara, code: "INIC");
            c.mail($"Novo cliente — despejo por falta de pagamento ({locador.nome.Split(' ')[0]})", OI,
                $"Fechamos contrato com {locador.o} {locador.sr} {locador.nome}, proprietári{locador.o} "
                + $"de um apartamento alugado para {locat.nome}. {locat.O} inquilin{locat.o} não paga há "
                + $"<b>{meses} meses</b> ({Brl(total)} só de aluguel, fora condomínio).",
                "Deixei o resumo do contrato e o demonstrativo em anexo. Vamos ajuizar <b>despejo por falta de "
                + "pagamento cumulado com cobrança</b> (art. 62, I, da Lei 8.245/91). Pontos:" + c.dicas(
                    "O contrato está <b>sem garantia</b>: isso abre a possibilidade de <b>liminar de desocupação em 15 "
                    + "dias</b> (art. 59, § 1º, IX), mediante caução de três aluguéis. Peça!",
                    "Cálculo discriminado do débito (art. 62, I) — aluguéis, condomínio, multa de 10%, correção e juros.",
                    "Mencione a possibilidade de purgação da mora pelo réu (art. 62, II) e já requeira a citação com "
                    + "essa advertência.",
                    "Valor da causa: 12 meses de aluguel (art. 58, III)."),
                c.prazo_frase(), c.assinatura());
            return c.build(locador.nome, "ativo", $"{locador.nome} x {locat.nome}");
        }
        var juiz = c.juiz();
        c.processo("estadual", "DESPEJO POR FALTA DE PAGAMENTO CUMULADO COM COBRANÇA", vara,
            new[] { "Despejo por Falta de Pagamento", "Locação de Imóvel" }, aluguel * 12, juiz);
        c.parte("a", locador.nome, locador.cpf, new[] { $"{adv_x.nome} ({adv_x.oab})" });
        c.parte("p", locat.nome, locat.cpf, nosso, true);
        var iptu = c.money(900, 1900);
        var pago_mes = r.choice(U.MESES.Take(9).ToList());
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}";
        var inic = c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{Esc(locador.nome)} propõe AÇÃO DE DESPEJO POR FALTA DE PAGAMENTO C/C COBRANÇA em face de "
            + $"{Esc(locat.nome)}, locatári{locat.o} do imóvel situado na {end}.",
            $"{locat.O} ré{(locat.f ? "" : "u")} deixou de pagar {meses} aluguéis. O débito atualizado, "
            + $"conforme planilha, é de {Brl(total + meses * 480 + iptu + aluguel * .1 * meses)}, incluindo aluguéis, "
            + $"condomínios, IPTU do exercício ({Brl(iptu)}) e multa de 10%.",
            "Requer a rescisão do contrato, o despejo e a condenação ao pagamento do débito."), adv_x, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_x.nome.Up(), new[] { inic, c.doc_s("CONTR3", "Contrato de locação", Pn(
                $"Aluguel {Brl(aluguel)}. Encargos por conta do locatário. Multa moratória de 10%.")),
            c.doc_s("CALC4", "Planilha do débito", Pn($"Inclui IPTU {Brl(iptu)} e aluguel de {pago_mes}.")) },
            detail: vara);
        c.ev("DESPACHO", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Despacho", P(
            "Cite-se o réu para, no prazo de 15 dias, contestar ou requerer a purgação da mora, nos termos do art. 62, "
            + "II, da Lei 8.245/91.")) }, gap: (3, 8));
        int n_cit = c.citacao($"RÉU: {locat.nome.Up()}");
        c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 15, ["contagem"] = "uteis" };
        c.tarefa("Contestação com pedido de purgação da mora", "contestacao", 15, "uteis",
            "art. 335 do CPC c/c art. 62, II, da Lei 8.245/91", vara, code: "CONT");
        c.mail($"Ação de despejo — defesa d{locat.o} {locat.nome.Split(' ')[0]}", OI,
            $"{locat.O} {locat.sr} {locat.nome} nos procurou: foi citad{locat.o} numa <b>ação de despejo "
            + $"por falta de pagamento</b> (evento {n_cit}). {Cap(locat.ele)} admite que atrasou alguns "
            + "meses (perdeu o emprego), mas já está trabalhando de novo e <b>quer ficar no imóvel</b>.",
            "Analisando a planilha do locador, achei dois problemas:" + c.dicas(
                $"Cobram o IPTU do ano ({Brl(iptu)}), que {locat.o} cliente <b>já pagou</b> (temos as guias quitadas).",
                $"Incluíram o aluguel de {pago_mes}, pago por PIX (temos o comprovante)."),
            "Redija a <b>contestação com pedido de purgação da mora</b>:" + c.dicas(
                "Purgação da mora no prazo da contestação (art. 62, II, da Lei 8.245/91): depósito judicial do "
                + "valor correto, com multa, juros, custas e honorários.",
                "Impugnação específica dos valores indevidos, com o cálculo do que é realmente devido.",
                $"{locat.O} cliente não purgou a mora nos últimos 24 meses (art. 62, parágrafo único).",
                "Pedido para evitar a rescisão e o despejo, com a extinção do pedido de despejo após o depósito."),
            c.prazo_frase(), c.assinatura());
        return c.build(locat.nome, "passivo", $"{locador.nome} x {locat.nome}");
    }

    /// <summary>Primeira letra maiúscula (como o capitalize() do Python).</summary>
    public static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..].ToLowerInvariant();
}
