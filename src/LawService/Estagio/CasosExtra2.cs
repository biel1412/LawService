using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — casos adicionais (parte 2)
public static partial class Casos
{
    public static JsonObject Hc(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var reu = c.pf("M");
        var (vara, prom, mp) = PenalBase(c, "AUTO DE PRISÃO EM FLAGRANTE", new[] { "Furto Qualificado (art. 155, § 4º, I, CP)" });
        c.parte("p", reu.nome, reu.cpf, c.nossos_advs(), true);
        var loja = r.choice(new[] { "uma loja de celulares", "uma farmácia", "uma loja de conveniência" });
        var valor = c.money(900, 2800);
        int filha = r.randint(3, 9);
        var fatos = $"Em {Ctx.A(6)}, de madrugada, {reu.nome}, {r.randint(22, 34)} anos, foi preso em flagrante após "
                    + $"arrombar a porta de {loja} em {u.cidade} e subtrair mercadorias avaliadas em {Brl(valor)}, todas "
                    + "recuperadas. Não houve violência ou grave ameaça. É <b>primário</b>, tem residência fixa e trabalha como "
                    + $"{Esc(reu.prof)}.";
        c.ev("DISTRIBUÍDO - AUTO DE PRISÃO EM FLAGRANTE", "AUTORIDADE POLICIAL", new[] { c.doc_s("APF1", "Auto de prisão em flagrante",
                Pn(fatos)), c.doc_s("CERTANTEC2", "Certidão de antecedentes", Pn(
                $"Nada consta em nome de {Esc(reu.nome)}.")) }, detail: vara);
        int n_dec = c.ev("DECISÃO - FLAGRANTE HOMOLOGADO E CONVERTIDO EM PRISÃO PREVENTIVA", "MAGISTRADO", new[] { c.doc_j(
            "DESPADEC1", "Decisão — audiência de custódia", P(
                "Homologo o auto de prisão em flagrante, formalmente regular.",
                "O crime imputado é grave e gera intranquilidade social, sendo cada vez mais comuns os furtos a "
                + "estabelecimentos comerciais nesta comarca. A segregação é necessária para garantia da ordem pública e "
                + "para resguardar a credibilidade da Justiça. As medidas cautelares diversas mostram-se insuficientes.",
                "CONVERTO a prisão em flagrante em PRISÃO PREVENTIVA (arts. 310, II, 312 e 313, I, do CPP). Expeça-se "
                + "mandado de prisão.")) }, gap: (1, 1));
        c.ev("MANDADO DE PRISÃO CUMPRIDO", "SISTEMA", gap: (0, 1));
        if (v == "hc")
        {
            c.tarefa("Habeas corpus com pedido liminar", "hc", 2, "interno",
                "Habeas corpus não tem prazo, mas o cliente está preso: é urgente.", vara, u.tj, "HC");
            c.mail($"URGENTE — preso preventivamente, habeas corpus ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
                $"A família do {reu.nome} nos procurou. {fatos}",
                $"Na audiência de custódia o juiz converteu o flagrante em <b>preventiva</b> (evento {n_dec}) com "
                + "fundamentação genérica: gravidade do crime, \"intranquilidade social\", credibilidade da Justiça. "
                + "Nada sobre o caso concreto.",
                "Vamos impetrar <b>habeas corpus</b> no Tribunal de Justiça, com pedido liminar. Teses:" + c.dicas(
                    "Fundamentação inidônea: gravidade abstrata e clamor social não justificam a preventiva; a decisão "
                    + "precisa indicar fatos concretos e contemporâneos (arts. 312, § 2º, e 315, § 2º, do CPP).",
                    "Prisão como <i>ultima ratio</i>: o juiz não explicou por que as cautelares do art. 319 seriam "
                    + "insuficientes (art. 282, § 6º).",
                    "Condições pessoais favoráveis: primário, emprego, residência fixa; crime sem violência, bens "
                    + "recuperados.",
                    "Autoridade coatora: o juízo da vara criminal; competência do TJ. Junte a decisão e os documentos "
                    + "(HC não admite dilação probatória)."),
                c.prazo_frase(), c.assinaturaPenal());
            return c.build(reu.nome, "passivo", $"HC — {reu.nome}");
        }
        c.tarefa("Pedido de revogação da prisão preventiva", "revogacao", 2, "interno",
            "Não há prazo, mas o cliente está preso: é urgente.", vara, code: "PET");
        c.mail($"URGENTE — revogação da preventiva ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
            $"Novo cliente preso: {reu.nome}. {fatos}",
            $"A preventiva foi decretada na custódia (evento {n_dec}). Antes de ir ao Tribunal, vamos pedir ao próprio "
            + "juiz a <b>revogação</b>, porque temos fatos novos que ele não conhecia:" + c.dicas(
                "Carteira de trabalho assinada e declaração do empregador, que mantém a vaga.",
                "Comprovante de residência em nome dele há 6 anos.",
                $"Ele é <b>pai solo</b> de uma menina de {filha} anos (a mãe faleceu); a criança está com uma vizinha. "
                + "Temos certidão de nascimento e de óbito."),
            "Redija o <b>pedido de revogação da prisão preventiva</b> (art. 316 do CPP):" + c.dicas(
                "Ausência de fundamentação concreta (art. 315, § 2º) e de contemporaneidade (art. 312, § 2º).",
                "Subsidiariamente, substituição por cautelares diversas (arts. 282, § 5º, e 319).",
                "Ainda subsidiariamente, <b>prisão domiciliar</b>: homem único responsável por filho de até 12 anos "
                + "(art. 318, VI, do CPP)."),
            c.prazo_frase(), c.assinaturaPenal());
        return c.build(reu.nome, "passivo", $"MP x {reu.nome} (preventiva)");
    }

    public static JsonObject Embriaguez(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var reu = c.pf();
        var (vara, prom, mp) = PenalBase(c, "AÇÃO PENAL - PROCEDIMENTO SUMÁRIO",
            new[] { "Crimes de Trânsito — Embriaguez ao Volante (art. 306 do CTB)" });
        c.parte("p", reu.nome, reu.cpf, c.nossos_advs(), true);
        var pm1 = c.pf("M");
        var pm2 = c.pf("F");
        var o = reu.o;
        var fatos = $"Em {Ctx.A(80)}, por volta das {r.randint(22, 23)}h, {reu.nome} foi abordad{o} numa blitz em {u.cidade}. "
                    + "Recusou-se a fazer o teste do etilômetro. Os agentes lavraram <b>termo de constatação de sinais de "
                    + "alteração da capacidade psicomotora</b> assinalando apenas \"odor de álcool no hálito\" e \"olhos "
                    + "vermelhos\"; no mesmo termo, marcaram como <b>normais</b> a fala, o equilíbrio, a orientação e a "
                    + "coordenação. Não houve acidente nem direção anormal relatada.";
        var ender = EndJuiz(c);
        c.ev("DISTRIBUÍDO - AUTO DE PRISÃO EM FLAGRANTE", "AUTORIDADE POLICIAL", new[] {
            c.doc_s("APF1", "Auto de prisão em flagrante", Pn(fatos, "Fiança arbitrada pela autoridade policial e "
                                                                    + "recolhida. Liberad" + o + ".")),
            c.doc_s("TERMO2", "Termo de constatação de sinais de alteração da capacidade psicomotora", Pn(
                "Sonolência: NÃO · Olhos vermelhos: SIM · Vômito: NÃO · Soluços: NÃO · Desordem nas vestes: NÃO · "
                + "Odor de álcool no hálito: SIM · Agressividade: NÃO · Arrogância: NÃO · Exaltação: NÃO · Ironia: NÃO · "
                + "Falante: NÃO · Dispersão: NÃO · Orientação: NORMAL · Memória: NORMAL · Capacidade motora e verbal: "
                + "NORMAL. Recusou o etilômetro.")) }, detail: vara);
        int n_den = c.ev("DENÚNCIA", prom.nome.Up(), new[] { c.doc_p("DENUNCIA1", "Denúncia", ender, P(
            $"O {mp} oferece DENÚNCIA contra {Esc(reu.nome)}: {fatos}",
            $"Assim agindo, {o} denunciad{o} incorreu no <b>art. 306 da Lei 9.503/97</b>."), Prom(prom)) }, gap: (20, 40));
        c.ev("DECISÃO - RECEBIDA A DENÚNCIA", "MAGISTRADO", gap: (3, 8));
        int n_cit = c.ev("JUNTADA DE MANDADO DE CITAÇÃO CUMPRIDO", "OFICIAL DE JUSTIÇA", gap: (5, 12));
        var teses = new[]
        {
            "O art. 306, § 1º, II, exige <b>sinais que indiquem alteração da capacidade psicomotora</b>; o próprio "
            + "termo marcou fala, equilíbrio e coordenação como normais. Odor e olhos vermelhos, isolados, não provam a "
            + "alteração.",
            "A recusa ao etilômetro não pode ser usada como prova contra o acusado (direito de não produzir prova "
            + "contra si); ela gera apenas a infração administrativa do art. 165-A do CTB.",
            "<i>In dubio pro reo</i>.",
        };
        if (v == "resposta")
        {
            c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 10, ["contagem"] = "corridos" };
            c.tarefa("Resposta à acusação", "resposta_acusacao", 10, "corridos", "arts. 396 e 396-A do CPP", vara,
                code: "RESPOSTA");
            c.mail($"Embriaguez ao volante — resposta à acusação ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
                $"Pegamos a defesa d{o} {reu.nome}, denunciad{o} por embriaguez ao volante (evento {n_den}). {fatos}",
                $"{Cap(reu.ele)} foi citad{o} (evento {n_cit}). Redija a <b>resposta à acusação</b>, buscando a "
                + "absolvição sumária por falta de justa causa. Teses:" + c.dicas(teses.Concat(new[]
                {
                    "A pena mínima do art. 306 é de 6 meses: cabe <b>suspensão condicional do processo</b> (art. 89 da "
                    + "Lei 9.099/95), e o MP também não justificou a ausência de proposta de <b>ANPP</b> (art. 28-A do "
                    + "CPP). Requeira a remessa ao MP.",
                    $"Rol de testemunhas: {c.pf().nome}, que jantou com {reu.o} cliente e {reu.o} viu beber "
                    + "apenas uma taça de vinho.",
                }).ToArray()),
                c.prazo_frase(), c.assinaturaPenal());
            return c.build(reu.nome, "passivo", $"MP x {reu.nome} (art. 306 CTB)");
        }
        c.ev("RESPOSTA À ACUSAÇÃO", c.socio.nome.Up(), gap: (5, 9));
        int n_aij = c.ev("AUDIÊNCIA DE INSTRUÇÃO E JULGAMENTO REALIZADA", "MAGISTRADO", new[] { c.doc_s(
            "TERMOAUD1", "Termo de audiência", Pn(
                $"Testemunha {Esc(pm1.nome)} (agente): \"tinha cheiro de bebida e os olhos vermelhos\"; perguntado, "
                + $"disse que {o} condutor{(reu.f ? "a" : "")} \"falava normalmente\" e \"desceu do carro sem dificuldade\".",
                $"Testemunha {Esc(pm2.nome)} (agente): não se recorda da abordagem; confirmou a assinatura no termo.",
                $"Interrogatório: {o} acusad{o} disse ter tomado uma taça de vinho no jantar, duas horas antes, e que "
                + "recusou o etilômetro por orientação de um conhecido.",
                "As partes requereram memoriais, deferidos com prazo sucessivo de 5 dias.")) }, gap: (40, 80));
        c.ev("ALEGAÇÕES FINAIS - MINISTÉRIO PÚBLICO", prom.nome.Up(), new[] { c.doc_p("MEMORIAIS1", "Memoriais", ender, P(
            "O termo de constatação e o depoimento do agente comprovam a embriaguez. A recusa ao etilômetro reforça a "
            + "conclusão. Requer a condenação nas sanções do art. 306 do CTB."), Prom(prom)) }, gap: (4, 6));
        c.intimacao($"DEFESA: {reu.nome.Up()}", 5, "corridos");
        c.tarefa("Memoriais (alegações finais)", "memoriais_penal", 5, "corridos", "art. 403, § 3º, do CPP", vara,
            code: "ALEGACOES");
        c.mail($"Memoriais — embriaguez ao volante ({reu.nome.Split(' ')[0]})", "Olá, %%NOME%%.",
            $"Encerrada a instrução no caso d{o} {reu.nome} (evento {n_aij}). O MP pediu a condenação e usou a recusa "
            + "ao bafômetro como reforço.",
            "O próprio agente admitiu na audiência que a fala e o equilíbrio estavam normais. Redija os "
            + "<b>memoriais da defesa</b>:" + c.dicas(teses.Concat(new[]
            {
                "Subsidiariamente: pena mínima, regime aberto e substituição por restritiva de direitos (art. 44 do CP).",
            }).ToArray()),
            c.prazo_frase(), c.assinaturaPenal());
        return c.build(reu.nome, "passivo", $"MP x {reu.nome} (art. 306 CTB)");
    }

    public static JsonObject Vicio(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var (prod, preco) = r.choice(new[] { ("notebook", c.money(3800, 6200)), ("geladeira frost free", c.money(3600, 5900)),
            ("smart TV de 55 polegadas", c.money(2900, 4800)) });
        var loja = r.choice(new[] { "Magazine Vitória Ltda.", "Eletro Center Comércio Ltda.", "Casa & Tecnologia S.A." });
        var fab = r.choice(new[] { "Nexus Eletrônicos do Brasil Ltda.", "Polar Eletrodomésticos S.A.", "Vértice Tecnologia Ltda." });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var nosso = c.nossos_advs();
        var juiz = c.juiz();
        c.processo("estadual", "PROCEDIMENTO DO JUIZADO ESPECIAL CÍVEL",
            $"{c.ordm(1, 6)}º Juizado Especial Cível da Comarca de {u.cidade}",
            new[] { "Produto Impróprio", "Rescisão do Contrato e Devolução do Dinheiro" }, preco + 4000, juiz,
            ("REQUERENTE", "REQUERIDO"), "Juizado Especial Cível");
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", loja, GenCnpj(r), new List<string>());
        c.parte("p", fab, GenCnpj(r), new List<string>());
        var fatos = $"{cli.nome} comprou um(a) {prod} por {Brl(preco)} na {loja}, em {Tq(210)}, com garantia contratual de "
                    + $"1 ano do fabricante ({fab}). Em {Tq(80)}, o aparelho parou de funcionar. Foi levado à assistência técnica "
                    + $"autorizada em {Tq(75)} (ordem de serviço anexa) e só foi devolvido em {Tq(30)} — <b>45 dias depois</b> — "
                    + "com o mesmo defeito. Novas reclamações por e-mail ficaram sem solução.";
        if (v == "inicial")
        {
            c.anexo("Nota fiscal e ordem de serviço", Pn($"NF de {Tq(210)}: {prod}, {Brl(preco)}. Ordem de serviço aberta "
                                                         + $"em {Tq(75)}; devolução em {Tq(30)}: \"defeito não solucionado — "
                                                         + "aguardando peça\"."));
            c.tarefa("Petição inicial (Juizado Especial Cível) — vício do produto", "inicial", 5, "interno",
                "Sem prazo processual, mas fique de olho no prazo decadencial do art. 26 do CDC.",
                c.orgao, code: "INIC");
            c.mail($"Novo cliente — produto com defeito ({cli.nome.Split(' ')[0]})", OI, fatos,
                "Vamos ajuizar no <b>Juizado Especial Cível</b> contra a loja e o fabricante. Pontos:" + c.dicas(
                    "Responsabilidade <b>solidária</b> de todos os fornecedores pelo vício do produto (art. 18 do CDC).",
                    "Passado o prazo de 30 dias sem conserto, o consumidor escolhe: troca, <b>restituição imediata da "
                    + "quantia paga</b>, corrigida, ou abatimento (art. 18, § 1º). O cliente quer o dinheiro de volta.",
                    "Decadência: 90 dias para bens duráveis (art. 26, II), obstada pela reclamação comprovada (art. 26, "
                    + "§ 2º, I) — e a garantia legal só começa a correr depois da contratual.",
                    "Dano moral é discutível em vício de produto: sustente o desvio produtivo (tempo perdido) com "
                    + "moderação, R$ 4.000,00.",
                    "Inversão do ônus da prova (art. 6º, VIII)."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x {loja}");
        }
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}";
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{Esc(cli.nome)} propõe ação em face de {loja} e {fab}. {fatos} Pede a restituição de {Brl(preco)} e "
            + "R$ 4.000,00 de danos morais."), AdvNosso(c), n: 1) }, detail: c.orgao);
        c.citacao($"REQUERIDOS: {loja.Up()} E {fab.Up()}");
        c.ev("AUDIÊNCIA DE CONCILIAÇÃO REALIZADA - SEM ACORDO", "CONCILIADOR", gap: (15, 25));
        c.ev("CONTESTAÇÃO", "ADVOGADO DO FABRICANTE", new[] { c.doc_s("CONT1", "Contestação do fabricante", Pn(
            "Prejudicial de mérito: decadência. O produto é durável e o prazo de 90 dias (art. 26, II, do CDC) é contado "
            + "da entrega. A ação foi ajuizada mais de 90 dias após a compra. No mérito, nega o vício.")) },
            gap: (5, 12));
        int ns = c.nxt;
        c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - DECADÊNCIA", "MAGISTRADO", new[] { c.doc_j("SENT1", "Sentença", P(
            "Dispensado o relatório (art. 38 da Lei 9.099/95). Decido.",
            "Tratando-se de produto durável, o direito de reclamar pelos vícios aparentes ou de fácil constatação caduca em "
            + "90 dias, contados da entrega efetiva do produto (art. 26, II e § 1º, do CDC). A compra ocorreu há mais de 90 "
            + "dias do ajuizamento.",
            "Ante o exposto, RECONHEÇO A DECADÊNCIA e julgo extinto o processo com resolução de mérito (art. 487, II, do "
            + "CPC). Sem custas e honorários (art. 55 da Lei 9.099/95). Intimem-se.")) }, gap: (10, 25));
        c.intimacao($"REQUERENTE: {cli.nome.Up()}", 10);
        c.tarefa("Recurso inominado", "recurso_inominado", 10, "uteis", "art. 42 da Lei 9.099/95", c.orgao,
            $"Turma Recursal dos Juizados Especiais Cíveis — {u.tj}", "RECINOM");
        c.mail($"Decadência reconhecida — recurso inominado ({cli.nome.Split(' ')[0]})", OI,
            $"No caso do {prod} d{cli.o} {cli.nome}, o juiz acolheu a <b>decadência</b> (evento {ns}), contando 90 "
            + "dias da data da compra. Isso está errado por três motivos. Redija o <b>recurso inominado</b>:" + c.dicas(
                "Vício <b>oculto</b>: o prazo começa quando o defeito fica evidenciado (art. 26, § 3º, do CDC), não na "
                + "entrega — o aparelho funcionou por meses.",
                "A reclamação comprovada perante o fornecedor <b>obsta</b> a decadência até a resposta negativa "
                + "(art. 26, § 2º, I) — temos a ordem de serviço e os e-mails.",
                "A garantia legal é complementar à contratual (art. 50) — o prazo do art. 26 só corre depois de "
                + "encerrada a garantia de 1 ano, entendimento do STJ.",
                "Pedido: afastar a decadência e, estando a causa madura, julgar procedentes os pedidos; ou anular a "
                + "sentença para análise do mérito. Atenção ao preparo."),
            $"Fatos: {fatos}", c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x {loja}");
    }

    public static JsonObject Voo(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var cia = r.choice(new[] { "Aerovia Linhas Aéreas S.A.", "Céu Azul Transportes Aéreos S.A." });
        var destino = r.choice(new[] { "Recife", "Salvador", "Fortaleza", "Manaus", "Natal" });
        int horas = r.randint(13, 19);
        var motivo = !cli.f ? r.choice(new[] { "o casamento do irmão, do qual seria padrinho", "uma entrevista de emprego presencial",
                "o velório da avó" })
            : r.choice(new[] { "o casamento da irmã, do qual seria madrinha", "uma entrevista de emprego presencial",
                "o velório da avó" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var nosso = c.nossos_advs();
        c.processo("estadual", "PROCEDIMENTO DO JUIZADO ESPECIAL CÍVEL",
            $"{c.ordm(1, 6)}º Juizado Especial Cível da Comarca de {u.cidade}",
            new[] { "Atraso de Voo", "Indenização por Dano Moral", "Indenização por Dano Material" }, 12000.0,
            lab: ("REQUERENTE", "REQUERIDO"), competencia: "Juizado Especial Cível");
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", cia, GenCnpj(r), new List<string>());
        var gastos = c.money(240, 680);
        var fatos = $"Em {Tq(50)}, {cli.nome} tinha voo de {u.cidade} para {destino} às {r.randint(7, 11)}h. O voo foi "
                    + $"cancelado por \"readequação da malha aérea\" e {cli.ele} só embarcou <b>{horas} horas depois</b>, "
                    + $"pernoitando no aeroporto: a companhia não forneceu hotel nem alimentação adequada. Gastou {Brl(gastos)} "
                    + $"com refeições e transporte e perdeu {motivo}.";
        var dicas = new[]
        {
            "Responsabilidade objetiva do transportador (art. 14 do CDC); \"readequação da malha\" é fortuito interno.",
            "Resolução ANAC 400/2016: assistência material — alimentação a partir de 2 horas e hospedagem em caso de "
            + "pernoite (art. 27). Nada disso foi fornecido.",
            "Atenção: o STJ entende que o dano moral por atraso de voo <b>não é presumido</b>, e o art. 251-A do Código "
            + "Brasileiro de Aeronáutica (Lei 14.034/2020) exige demonstração efetiva do prejuízo. Por isso, detalhe as "
            + "circunstâncias concretas: duração, pernoite sem assistência e o compromisso perdido.",
            $"Dano material: {Brl(gastos)}, com comprovantes.",
        };
        if (v == "inicial")
        {
            c.anexo("Cartões de embarque, comprovantes e prints", Pn(fatos, "Prints do aplicativo da companhia com o "
                                                                           + "cancelamento; notas fiscais das refeições; "
                                                                           + "convite/documento do compromisso perdido."));
            c.tarefa("Petição inicial (Juizado Especial Cível) — cancelamento de voo", "inicial", 5, "interno",
                "Sem prazo processual.", c.orgao, code: "INIC");
            c.mail($"Novo cliente — voo cancelado ({cli.nome.Split(' ')[0]})", OI, fatos,
                "Vamos ajuizar no <b>Juizado Especial Cível</b>. Pontos:" + c.dicas(dicas.Concat(new[]
                {
                    "Pedido de dano moral: R$ 10.000,00.",
                }).ToArray()), c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x {cia}");
        }
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DO {c.orgao.Up()}";
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{Esc(cli.nome)} propõe ação indenizatória em face de {cia}. {fatos}"), AdvNosso(c), n: 1) },
            detail: c.orgao);
        c.citacao($"REQUERIDO: {cia.Up()}");
        c.ev("AUDIÊNCIA DE CONCILIAÇÃO REALIZADA - SEM ACORDO", "CONCILIADOR", gap: (15, 25));
        c.ev("CONTESTAÇÃO", "ADVOGADO DA REQUERIDA", new[] { c.doc_s("CONT1", "Contestação", Pn(
            "O cancelamento decorreu de readequação da malha, motivo operacional. O passageiro foi reacomodado no primeiro "
            + "voo disponível. Mero aborrecimento.")) }, gap: (5, 10));
        int ns = c.nxt;
        c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - PROCEDÊNCIA EM PARTE", "MAGISTRADO", new[] { c.doc_j("SENT1", "Sentença", P(
                "Dispensado o relatório (art. 38 da Lei 9.099/95). Decido.",
                $"Comprovados os gastos de {Brl(gastos)}, devidos a título de dano material.",
                "Quanto ao dano moral, o atraso de voo, por si só, não gera dano presumido. A parte autora foi reacomodada e "
                + "chegou ao destino. Os transtornos narrados configuram mero aborrecimento do cotidiano.",
                $"JULGO PARCIALMENTE PROCEDENTES os pedidos para condenar a ré ao pagamento de {Brl(gastos)} a título de danos "
                + "materiais. JULGO IMPROCEDENTE o pedido de danos morais. Sem custas e honorários. Intimem-se.")) },
            gap: (10, 25));
        c.intimacao($"REQUERENTE: {cli.nome.Up()}", 10);
        c.tarefa("Recurso inominado", "recurso_inominado", 10, "uteis", "art. 42 da Lei 9.099/95", c.orgao,
            $"Turma Recursal dos Juizados Especiais Cíveis — {u.tj}", "RECINOM");
        c.mail($"Voo cancelado — dano moral negado, recurso ({cli.nome.Split(' ')[0]})", OI,
            $"A sentença do caso d{cli.o} {cli.nome} (evento {ns}) só deu o dano material e chamou de \"mero "
            + $"aborrecimento\" {horas} horas de atraso com pernoite no aeroporto sem assistência e a perda de {motivo}.",
            "O juiz aplicou a tese de que o dano não é presumido — correto em abstrato —, mas ignorou que nós "
            + "<b>provamos</b> as circunstâncias concretas. Redija o <b>recurso inominado</b>:" + c.dicas(dicas.Skip(1).Concat(new[]
            {
                "Pedido de reforma para condenar em danos morais (R$ 10.000,00) e preparo.",
            }).ToArray()),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x {cia}");
    }

    public static JsonObject Rescisao(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var emp = c.pf();
        var empresa = r.choice(new[] { "Construtora Alicerce Ltda.", "Transportes Rota Sul Ltda.", "Restaurante Sabor da Terra Ltda." });
        int sal = r.choice(new[] { 2300, 2650, 3100, 3480 });
        int anos = r.randint(3, 7);
        var vara = $"{c.ordm(1, 30)}ª Vara do Trabalho de {u.cidade}";
        c.processo("trabalho", "AÇÃO TRABALHISTA - RITO ORDINÁRIO", vara, new[] { "Rescisão Indireta", "FGTS" }, sal * 14.0,
            lab: ("RECLAMANTE", "RECLAMADO"));
        c.parte("a", emp.nome, emp.cpf, c.nossos_advs(), true);
        c.parte("p", empresa, GenCnpj(r), new List<string>());
        int meses_fgts = r.randint(9, 16);
        var fatos = $"{emp.nome} trabalha para {empresa} há {anos} anos, com salário de {Brl(sal)}. O extrato do FGTS "
                    + $"mostra que a empresa <b>não deposita há {meses_fgts} meses</b>, e os salários vêm sendo pagos com atraso "
                    + "de 10 a 20 dias nos últimos 6 meses (temos os extratos bancários). O cliente quer sair da empresa sem "
                    + "perder os direitos de uma dispensa sem justa causa.";
        c.anexo("Extrato do FGTS e extratos bancários", Pn(fatos));
        c.tarefa("Reclamação trabalhista — rescisão indireta", "inicial_trab", 4, "interno",
            "Sem prazo processual, mas o cliente precisa decidir se continua trabalhando enquanto a ação tramita.",
            vara, code: "INIC");
        c.mail($"Rescisão indireta — FGTS não depositado ({emp.nome.Split(' ')[0]})", OI, fatos,
            "Vamos pedir a <b>rescisão indireta</b>. Redija a reclamação trabalhista:" + c.dicas(
                "Art. 483, \"d\", da CLT (empregador não cumpre as obrigações do contrato): ausência reiterada de "
                + "depósitos do FGTS e atraso salarial são falta grave, segundo a jurisprudência do TST.",
                "Nessa hipótese, o empregado pode optar por continuar trabalhando até a decisão (art. 483, § 3º). "
                + "Diga na inicial qual foi a opção (o cliente vai se afastar a partir do ajuizamento).",
                "Pedidos líquidos (art. 840, § 1º): aviso prévio proporcional, 13º e férias + 1/3 proporcionais, "
                + "depósitos do FGTS em atraso + multa de 40%, guias do seguro-desemprego, baixa na CTPS.",
                "Justiça gratuita."),
            c.prazo_frase(), c.assinatura());
        return c.build(emp.nome, "ativo", $"{emp.nome} x {empresa}");
    }

    public static JsonObject Bpc(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf("F");
        int idade = r.randint(66, 74);
        var marido = c.pf("M");
        var vara = $"{c.ordm(1, 12)}ª Vara Federal de {u.cidade} (Juizado Especial Federal)";
        c.processo("federal", "PROCEDIMENTO DO JUIZADO ESPECIAL CÍVEL", vara, new[] { "Benefício Assistencial (art. 203, V, CF)" },
            20000.0, c.juiz("Juiz Federal", "Juíza Federal"), competencia: "Juizado Especial Federal Cível");
        c.parte("a", cli.nome, cli.cpf, c.nossos_advs(), true);
        c.parte("p", "INSTITUTO NACIONAL DO SEGURO SOCIAL - INSS", "29.979.036/0001-40", new[] { "Procuradoria Federal" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var fatos = $"{cli.nome}, {idade} anos, nunca contribuiu para a Previdência (foi dona de casa a vida toda). Mora "
                    + $"apenas com o marido, {marido.nome}, de {idade + r.randint(1, 5)} anos, que recebe <b>aposentadoria de "
                    + "um salário mínimo</b>. Pediu o <b>BPC/LOAS ao idoso</b> (DER em " + Tq(90) + "), indeferido porque o INSS "
                    + "considerou a renda do marido e calculou renda per capita de meio salário mínimo.";
        var dicas = new[]
        {
            "Requisitos: 65 anos ou mais e família sem meios de prover a subsistência (art. 20 da Lei 8.742/93; art. 203, "
            + "V, da CF).",
            "O benefício previdenciário de até 1 salário mínimo recebido por idoso <b>não entra</b> no cálculo da renda "
            + "familiar (art. 20, § 14, da Lei 8.742/93, incluído pela Lei 13.982/2020). Sem a aposentadoria do marido, a "
            + "renda per capita é zero.",
            "O STF já declarou que o critério de 1/4 do salário mínimo não é absoluto: a miserabilidade pode ser provada "
            + "por outros meios (gastos com remédios, moradia).",
        };
        if (v == "inicial")
        {
            c.anexo("Documentos", Pn(fatos, $"Indeferimento do INSS em {Tq(60)}; CadÚnico atualizado; carta de concessão da "
                                            + "aposentadoria do marido; receitas e notas de medicamentos de uso contínuo."));
            c.tarefa("Petição inicial (Juizado Especial Federal) — BPC/LOAS", "inicial", 5, "interno",
                "Sem prazo processual; a cliente está sem renda própria.", vara, code: "INIC");
            c.mail($"BPC/LOAS negado — {cli.nome.Split(' ')[0]}", OI, fatos,
                "Vamos ao <b>Juizado Especial Federal</b>. Redija a <b>petição inicial</b>:" + c.dicas(dicas.Concat(new[]
                {
                    "Pedido de concessão desde a DER, com atrasados, e de tutela de urgência (idosa, verba alimentar).",
                    "Requeira estudo socioeconômico (perícia social).",
                }).ToArray()),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x INSS");
        }
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { c.doc_p("INIC1", "Petição inicial",
                $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) FEDERAL DA {vara.Up()}", P($"{Esc(cli.nome)} requer o benefício "
                                                                                + $"assistencial ao idoso. {fatos}"),
                AdvNosso(c), n: 1) }, detail: vara);
        c.ev("JUNTADA DE LAUDO SOCIOECONÔMICO", "PERITO", new[] { c.doc_s("LAUDO1", "Estudo social", Pn(
            "A família reside em casa própria, simples, de alvenaria, com móveis antigos em bom estado. Gastos mensais: "
            + "alimentação, contas de consumo e medicamentos de uso contínuo da autora (parte não fornecida pelo SUS). "
            + "Renda: aposentadoria do marido, de um salário mínimo.")) }, gap: (30, 60));
        int ns = c.nxt;
        c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - IMPROCEDÊNCIA", "MAGISTRADO", new[] { c.doc_j("SENT1", "Sentença", P(
            "Dispensado o relatório. Decido.",
            "A renda familiar per capita corresponde a metade do salário mínimo, acima do limite legal. Ademais, o estudo "
            + "social revela casa própria em bom estado de conservação, o que afasta a situação de miserabilidade.",
            "JULGO IMPROCEDENTE o pedido. Sem custas e honorários. Intimem-se.")) }, gap: (10, 25));
        c.intimacao($"AUTOR: {cli.nome.Up()}", 10);
        c.tarefa("Recurso inominado (Turma Recursal Federal)", "recurso_inominado", 10, "uteis",
            "art. 42 da Lei 9.099/95 c/c art. 1º da Lei 10.259/2001", vara, $"Turma Recursal da {u.sj}", "RECINOM");
        c.mail($"BPC negado em sentença — recurso inominado ({cli.nome.Split(' ')[0]})", OI,
            $"A sentença do BPC da {cli.sr} {cli.nome} foi de improcedência (evento {ns}). O juiz <b>contou a "
            + "aposentadoria do marido</b> na renda e ainda disse que casa própria afasta a miserabilidade.",
            "Redija o <b>recurso inominado</b>:" + c.dicas(dicas.Skip(1).Concat(new[]
            {
                "Casa própria simples não descaracteriza a vulnerabilidade; o laudo mostra gastos com remédios e renda "
                + "per capita zero após a exclusão legal.",
                "Pedido de reforma, com concessão desde a DER; parte com gratuidade, sem preparo.",
            }).ToArray()),
            $"Fatos: {fatos}", c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x INSS");
    }

    public static JsonObject Itbi(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var preco = c.money(380000, 640000);
        var refv = Math.Round(preco * r.uniform(1.25, 1.45), 2);
        var pago = Math.Round(refv * .03, 2);
        var devido = Math.Round(preco * .03, 2);
        var vara = $"Juizado Especial da Fazenda Pública da Comarca de {u.cidade}";
        c.processo("estadual", "PROCEDIMENTO DO JUIZADO ESPECIAL DA FAZENDA PÚBLICA", vara,
            new[] { "ITBI - Imposto de Transmissão Intervivos de Bens Móveis e Imóveis", "Repetição de indébito" },
            Math.Round(pago - devido, 2), competencia: "Juizado Especial da Fazenda Pública");
        c.parte("a", cli.nome, cli.cpf, c.nossos_advs(), true);
        c.parte("p", u.mun.Up(), GenCnpj(r), new[] { "Procuradoria-Geral do Município" });
        var fatos = $"{cli.nome} comprou um apartamento em {u.cidade} por <b>{Brl(preco)}</b> (escritura de {Ctx.B(200)}). "
                    + "Para registrar, a prefeitura exigiu o ITBI calculado sobre o \"<b>valor venal de referência</b>\" fixado "
                    + $"unilateralmente pelo Município, de {Brl(refv)}. {cli.O} cliente pagou {Brl(pago)}; sobre o preço real da "
                    + $"compra, o imposto seria de {Brl(devido)}. A diferença paga a maior é de <b>{Brl(pago - devido)}</b>.";
        c.anexo("Escritura e guia do ITBI", Pn(fatos));
        c.tarefa("Petição inicial — repetição de indébito (ITBI)", "inicial", 5, "interno",
            "O prazo para pedir a restituição é de 5 anos do pagamento (art. 168, I, do CTN); estamos tranquilos.",
            vara, code: "INIC");
        c.mail($"ITBI pago a maior — restituição ({cli.nome.Split(' ')[0]})", OI, fatos,
            "Vamos pedir a restituição no <b>Juizado Especial da Fazenda Pública</b> (Lei 12.153/2009). Pontos:"
            + c.dicas(
                "<b>STJ, Tema 1.113</b>: a base de cálculo do ITBI é o valor do imóvel transmitido em condições normais "
                + "de mercado, não vinculada ao IPTU; o valor declarado pelo contribuinte goza de presunção de "
                + "veracidade, e o Município <b>não pode arbitrar previamente</b> a base com valor de referência fixado "
                + "unilateralmente.",
                "Repetição do indébito (art. 165, I, do CTN), com correção e juros (no tributário municipal, conforme "
                + "a legislação local; muitos municípios adotam a Selic).",
                "Junte escritura, guia e comprovante de pagamento; não há necessidade de prévio pedido administrativo.",
                "Atenção ao polo passivo: o próprio Município."),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x {u.mun}");
    }

    public static JsonObject Buraco(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var dano = c.money(2100, 5600);
        var rua = r.choice(new[] { "Av. Sertório", "Rua Augusta", "Av. Cristóvão Colombo", "Rua Padre Anchieta", "Av. Amazonas" });
        var vara = $"Juizado Especial da Fazenda Pública da Comarca de {u.cidade}";
        c.processo("estadual", "PROCEDIMENTO DO JUIZADO ESPECIAL DA FAZENDA PÚBLICA", vara,
            new[] { "Responsabilidade Civil do Estado", "Indenização por Dano Material" }, dano,
            competencia: "Juizado Especial da Fazenda Pública");
        c.parte("a", cli.nome, cli.cpf, c.nossos_advs(), true);
        c.parte("p", u.mun.Up(), GenCnpj(r), new[] { "Procuradoria-Geral do Município" });
        int prot = r.randint(3, 6);
        var fatos = $"Em {Ctx.B(25)}, à noite e com chuva, {cli.nome} passou com o carro por um buraco profundo na {rua}, em "
                    + $"{u.cidade}, sem nenhuma sinalização. Estouraram dois pneus, entortaram as rodas e a suspensão foi "
                    + $"danificada: conserto de <b>{Brl(dano)}</b> (nota fiscal). Pelo canal de atendimento da prefeitura, havia "
                    + $"<b>{prot} protocolos de reclamação</b> sobre o mesmo buraco nos 4 meses anteriores, sem providência.";
        c.anexo("Boletim de ocorrência, fotos, nota fiscal e protocolos", Pn(fatos));
        c.tarefa("Petição inicial — responsabilidade civil do Município", "inicial", 5, "interno",
            "Prescrição de 5 anos contra a Fazenda (Decreto 20.910/32) — sem urgência, mas não vamos deixar parado.",
            vara, code: "INIC");
        c.mail($"Buraco na via — indenização contra o Município ({cli.nome.Split(' ')[0]})", OI, fatos,
            "Vamos ao <b>Juizado Especial da Fazenda Pública</b>. Redija a <b>petição inicial</b>:" + c.dicas(
                "Responsabilidade do Município pela conservação das vias (art. 37, § 6º, da CF) e o art. 1º, § 3º, do "
                + "CTB: os órgãos do Sistema Nacional de Trânsito respondem objetivamente por danos causados por ação, "
                + "omissão ou erro na manutenção que garanta o trânsito seguro.",
                "Mesmo para quem defende responsabilidade subjetiva por omissão, há <b>omissão específica</b> e falha do "
                + "serviço comprovada pelos protocolos anteriores.",
                "Nexo causal: fotos, boletim de ocorrência e nota fiscal.",
                "Pedido: danos materiais com correção e juros; valor da causa dentro do teto de 60 salários mínimos."),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x {u.mun}");
    }

    // ────────────────────────────────────────────────────────── catálogo de variantes
    public static readonly (string fam, string var, string area, string socio, Func<Ctx, string, JsonObject> fn)[] CASOS =
    {
        ("consumidor", "inicial", "Direito do Consumidor", "civel", Consumidor),
        ("consumidor", "recurso_inominado", "Direito do Consumidor", "civel", Consumidor),
        ("consumidor", "contrarrazoes", "Direito do Consumidor", "civel", Consumidor),
        ("consumidor", "embargos", "Direito do Consumidor", "civel", Consumidor),
        ("saude", "inicial", "Direito do Consumidor", "civel", Saude),
        ("saude", "agravo", "Direito do Consumidor", "civel", Saude),
        ("transito", "contestacao", "Direito Civil", "civel", Transito),
        ("transito", "replica", "Direito Civil", "civel", Transito),
        ("transito", "memoriais", "Direito Civil", "civel", Transito),
        ("transito", "apelacao", "Direito Civil", "civel", Transito),
        ("transito", "contrarrazoes", "Direito Civil", "civel", Transito),
        ("despejo", "inicial", "Direito Civil", "civel", Despejo),
        ("despejo", "contestacao", "Direito Civil", "civel", Despejo),
        ("alimentos", "agravo", "Direito de Família", "civel", Alimentos),
        ("alimentos", "contestacao", "Direito de Família", "civel", Alimentos),
        ("furto", "resposta", "Direito Penal", "penal", Furto),
        ("furto", "memoriais", "Direito Penal", "penal", Furto),
        ("furto", "apelacao", "Direito Penal", "penal", Furto),
        ("drogas", "memoriais", "Direito Penal", "penal", Drogas),
        ("drogas", "apelacao", "Direito Penal", "penal", Drogas),
        ("trabalho", "inicial", "Direito do Trabalho", "trabalho", Trabalho),
        ("trabalho", "contestacao", "Direito do Trabalho", "trabalho", Trabalho),
        ("trabalho", "ro", "Direito do Trabalho", "trabalho", Trabalho),
        ("previdenciario", "inicial", "Direito Previdenciário", "trabalho", Previdenciario),
        ("previdenciario", "recurso", "Direito Previdenciário", "trabalho", Previdenciario),
        ("mandado", "inicial", "Direito Administrativo", "publico", Mandado),
        ("mandado", "agravo", "Direito Administrativo", "publico", Mandado),
        ("execfiscal", "excecao", "Direito Tributário", "publico", ExecFiscal),
        ("divorcio", "inicial", "Direito de Família", "civel", Divorcio),
        ("divorcio", "contestacao", "Direito de Família", "civel", Divorcio),
        ("inventario", "inicial", "Direito das Sucessões", "civel", Inventario),
        ("inventario", "primeiras_declaracoes", "Direito das Sucessões", "civel", Inventario),
        ("usucapiao", "inicial", "Direito Civil", "civel", Usucapiao),
        ("usucapiao", "apelacao", "Direito Civil", "civel", Usucapiao),
        ("monitoria", "inicial", "Direito Civil", "civel", Monitoria),
        ("monitoria", "embargos", "Direito Civil", "civel", Monitoria),
        ("embexec", "embargos", "Direito Civil", "civel", EmbExec),
        ("vicio", "inicial", "Direito do Consumidor", "civel", Vicio),
        ("vicio", "recurso", "Direito do Consumidor", "civel", Vicio),
        ("voo", "inicial", "Direito do Consumidor", "civel", Voo),
        ("voo", "recurso", "Direito do Consumidor", "civel", Voo),
        ("hc", "hc", "Direito Penal", "penal", Hc),
        ("hc", "revogacao", "Direito Penal", "penal", Hc),
        ("embriaguez", "resposta", "Direito Penal", "penal", Embriaguez),
        ("embriaguez", "memoriais", "Direito Penal", "penal", Embriaguez),
        ("rescisao", "inicial", "Direito do Trabalho", "trabalho", Rescisao),
        ("bpc", "inicial", "Direito Previdenciário", "trabalho", Bpc),
        ("bpc", "recurso", "Direito Previdenciário", "trabalho", Bpc),
        ("itbi", "inicial", "Direito Tributário", "publico", Itbi),
        ("buraco", "inicial", "Direito Administrativo", "publico", Buraco),
    };

    public static readonly List<string> AREAS_ESTAGIO = CASOS.Select(x => x.area).Distinct().ToList();

    public static readonly (string key, string text)[] FASES =
    {
        ("inicial", "Petição inicial"), ("defesa", "Defesa"), ("recurso", "Recurso"), ("memoriais", "Memoriais"),
        ("outras", "Outras peças (HC, revogação de prisão, inventário)"),
    };

    public static string FaseDe(string tipo)
    {
        if (tipo is "inicial" or "inicial_trab" or "ms") return "inicial";
        if (tipo is "contestacao" or "contestacao_trab" or "resposta_acusacao" or "replica" or "excecao" or "embargos_monitoria"
            or "embargos_execucao") return "defesa";
        if (tipo is "hc" or "revogacao" or "primeiras_declaracoes") return "outras";
        if (tipo is "memoriais" or "memoriais_penal") return "memoriais";
        return "recurso";
    }

    public static JsonObject GerarCaso(string area = null, string fase = null, ISet<(string, string)> evitar = null)
    {
        var rnd = new Rnd();
        var pool = CASOS.Where(x => area == null || x.area == area).ToList();
        if (fase != null)
        {
            var tmp = pool.Where(x => FaseDe(GerarUm(x, new Rnd(1)).O("meta").O("tarefa").S("tipo")) == fase).ToList();
            if (tmp.Count > 0) pool = tmp;
        }
        var fresh = pool.Where(x => evitar == null || !evitar.Contains((x.fam, x.var))).ToList();
        if (fresh.Count == 0) fresh = pool;
        return GerarUm(rnd.choice(fresh), rnd);
    }

    public static JsonObject GerarUm((string fam, string var, string area, string socio, Func<Ctx, string, JsonObject> fn) item, Rnd rnd)
    {
        var c = new Ctx(rnd, item.socio, item.area);
        var caso = item.fn(c, item.var);
        caso["familia"] = item.fam;
        caso["variante"] = item.var;
        return caso;
    }
}
