using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — trabalho, previdência e público
public static partial class Casos
{
    public static JsonObject Trabalho(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var emp = c.pf();
        var empresa = r.choice(new[] { "Lojas Primavera Comércio de Roupas Ltda.", "Distribuidora Rota Sul Ltda.",
            "Comercial Estrela Utilidades Ltda.", "Rede Bom Lar Móveis Ltda." });
        var cnpj = GenCnpj(r);
        var funcao = !emp.f ? r.choice(new[] { "vendedor", "estoquista", "operador de caixa" })
            : r.choice(new[] { "vendedora", "estoquista", "operadora de caixa" });
        int sal = r.choice(new[] { 1980, 2150, 2340, 2600, 2890 });
        int anos = r.randint(2, 4);
        var vara = $"{c.ordm(1, 30)}ª Vara do Trabalho de {u.cidade}";
        var juiz = c.juiz("Juiz do Trabalho", "Juíza do Trabalho");
        var nosso = c.nossos_advs();
        var adv_x = c.adv();
        c.processo("trabalho", "AÇÃO TRABALHISTA - RITO ORDINÁRIO", vara,
            new[] { "Horas Extras", "Intervalo Intrajornada", "Verbas Rescisórias" }, Math.Round(sal * 9.5, 2), juiz,
            ("RECLAMANTE", "RECLAMADO"));
        bool cli_emp = v == "contestacao";
        c.parte("a", emp.nome, emp.cpf, cli_emp ? new List<string> { $"{adv_x.nome} ({adv_x.oab})" } : nosso, !cli_emp);
        c.parte("p", empresa, cnpj, cli_emp ? nosso : new List<string> { $"{adv_x.nome} ({adv_x.oab})" }, cli_emp);
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var jornada = "das 9h às 19h, de segunda a sábado, com apenas 30 minutos de intervalo";
        var fatos = $"{emp.nome} trabalhou para {empresa} como {funcao}, de {Tq(365 * anos + 90)} a {Tq(90)}, com salário de "
                    + $"{Brl(sal)}. Afirma que cumpria jornada {jornada}, sem receber horas extras, e que foi dispensad{emp.o} "
                    + "sem justa causa.";
        var ender = $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DA {vara.Up()}";
        if (v == "inicial")
        {
            c.anexo("Relato do atendimento", Pn(fatos,
                "Os cartões de ponto mostravam sempre os mesmos horários (9h00 / 18h00), mas "
                + $"{emp.o} cliente diz que era obrigad{emp.o} a bater o ponto às 18h e continuar trabalhando até as 19h. A empresa tem cerca de 60 "
                + $"empregados. O TRCT foi pago com 15 dias de atraso. Testemunha: um ex-colega, {c.pf().nome}."));
            c.anexo("Documentos", Pn($"CTPS digital; contracheques; TRCT (pago em {Tq(75)}); aviso prévio indenizado "
                                     + $"(dispensa comunicada em {Tq(90)})."));
            c.tarefa("Reclamação trabalhista (petição inicial)", "inicial_trab", 5, "interno",
                "A dispensa foi há cerca de 3 meses — estamos tranquilos com a prescrição bienal, mas não vamos "
                + "deixar parado.", vara, code: "INIC");
            c.mail($"Nov{emp.o} cliente — reclamação trabalhista ({emp.nome.Split(' ')[0]})", OI,
                $"Atendi {emp.o} {emp.nome}. {fatos}",
                "O relato do atendimento e a lista de documentos estão em anexo. Redija a <b>reclamação "
                + "trabalhista</b>. Atenção:" + c.dicas(
                    "Desde a Reforma, os pedidos precisam ser <b>certos, determinados e com indicação de valor</b> "
                    + "(art. 840, § 1º, da CLT) — faça uma estimativa de cada pedido.",
                    "Horas extras com adicional de 50% (art. 7º, XVI, da CF; art. 59 da CLT) e reflexos.",
                    "Cartões \"britânicos\" (horários sempre iguais) são inválidos como prova: <b>Súmula 338, III, "
                    + "do TST</b>.",
                    "Intervalo intrajornada suprimido: pagamento só do período suprimido, com 50%, natureza "
                    + "indenizatória (art. 71, § 4º, da CLT).",
                    "Multa do art. 477, § 8º, da CLT pelo atraso das verbas rescisórias.",
                    "Justiça gratuita (art. 790, §§ 3º e 4º, da CLT)."),
                c.prazo_frase(), c.assinatura());
            return c.build(emp.nome, "ativo", $"{emp.nome} x {empresa}");
        }
        var inic = c.doc_p("INIC1", "Reclamação trabalhista", ender, P(
                $"{Esc(emp.nome)} propõe RECLAMAÇÃO TRABALHISTA em face de {empresa}.", fatos,
                "Pede horas extras com adicional de 50% e reflexos, uma hora diária pelo intervalo suprimido, multa do art. "
                + "477, § 8º, da CLT, adicional de insalubridade em grau médio (manuseio de produtos de limpeza) e justiça "
                + "gratuita, com valores estimados conforme planilha."),
            cli_emp ? adv_x : new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", (cli_emp ? adv_x.nome : c.socio.nome).Up(), new[] { inic }, detail: vara);
        c.ev("NOTIFICAÇÃO DA RECLAMADA - EXPEDIDA", "SISTEMA", detail: "Audiência inicial designada", gap: (2, 5));
        int n_not = c.ev("CONFIRMADA A NOTIFICAÇÃO", "SISTEMA", gap: (2, 6));
        if (v == "contestacao")
        {
            c.tarefa("Contestação trabalhista", "contestacao_trab", r.randint(8, 12), "audiencia",
                "art. 847, parágrafo único, da CLT", vara, code: "CONT");
            c.mail($"Reclamação trabalhista contra {empresa.Split(' ')[0]} {empresa.Split(' ')[1]} — contestação", OI,
                $"Nossa cliente {empresa} foi notificada de uma reclamação trabalhista (evento {n_not}) proposta "
                + $"por {emp.nome}, ex-{funcao}. Leia a inicial no evento 1.",
                "Conversei com o RH. A versão da empresa:" + c.dicas(
                    "Ponto eletrônico com horários <b>variáveis</b> (temos os espelhos). As poucas horas extras "
                    + "foram pagas nos contracheques ou compensadas em banco de horas com acordo individual escrito.",
                    "Intervalo de 1 hora, pré-assinalado no ponto (art. 74, § 2º, da CLT).",
                    "A função não envolvia limpeza; o manuseio eventual de produtos domésticos não gera insalubridade "
                    + "(não consta no Anexo 13 da NR-15). Temos o PGR e o LTCAT.",
                    "As verbas rescisórias foram pagas dentro de 10 dias (comprovante de transferência)."),
                "Redija a <b>contestação</b>, impugnando especificamente cada pedido e cada valor. Inclua o pedido "
                + "de perícia se entender necessário (art. 195 da CLT) e lembre de rebater a justiça gratuita "
                + "se houver fundamento. A defesa vai pelo sistema antes da audiência.",
                c.prazo_frase(), c.assinatura());
            return c.build(emp.nome, "passivo", $"{emp.nome} x {empresa}");
        }
        int n_cont = c.ev("CONTESTAÇÃO", adv_x.nome.Up(), new[] { c.doc_p("CONT1", "Contestação", ender, P(
                "A reclamada nega a jornada alegada. Os cartões de ponto anexos registram a jornada real, das 9h às 18h, "
                + "com 1 hora de intervalo. Horas extras eventuais foram pagas. Requer a improcedência."), adv_x),
            c.doc_s("PONTO2", "Cartões de ponto", Pn("Todos os registros: entrada 09:00, intervalo 12:00-13:00, saída "
                                                    + "18:00, sem qualquer variação de minutos, durante todo o contrato.")) },
            gap: (15, 25));
        var testemunha = c.pf();
        c.ev("AUDIÊNCIA DE INSTRUÇÃO REALIZADA", "MAGISTRADO", new[] { c.doc_s("ATA1", "Ata de audiência", Pn(
            "Depoimento do preposto: não soube dizer como era feito o registro de ponto.",
            $"Testemunha do reclamante, {Esc(testemunha.nome)}: \"a gente batia o ponto às 18h e voltava para "
            + "arrumar a loja até umas 19h; o almoço era de meia hora porque a loja ficava cheia\".",
            "Sem outras provas. Razões finais remissivas. Conciliação recusada.")) }, gap: (30, 60));
        int ns = c.nxt;
        var sent = c.doc_j("SENT1", "Sentença", P(
            $"{Esc(emp.nome)} ajuizou reclamação trabalhista contra {empresa}. Houve contestação e instrução. É o "
            + "relatório. Decido.",
            "Horas extras e intervalo. A reclamada juntou cartões de ponto, que gozam de presunção de veracidade. A "
            + "prova testemunhal produzida pelo reclamante é única e insuficiente para infirmá-los. Indefiro.",
            "Insalubridade. A perícia técnica concluiu pela ausência de agentes insalubres. Indefiro.",
            "Multa do art. 477, § 8º, da CLT. O TRCT foi pago após o prazo de 10 dias. Defiro.",
            "Ante o exposto, JULGO PROCEDENTES EM PARTE os pedidos para condenar a reclamada ao pagamento da multa do "
            + "art. 477, § 8º, da CLT. Defiro a justiça gratuita. Honorários sucumbenciais recíprocos de 10%, com "
            + "exigibilidade suspensa para o reclamante (ADI 5.766). Custas pela reclamada. Intimem-se."));
        c.ev("SENTENÇA - PROCEDENTE EM PARTE", "MAGISTRADO", new[] { sent }, gap: (10, 25));
        c.intimacao($"RECLAMANTE: {emp.nome.Up()}", 8);
        c.tarefa("Recurso ordinário", "ro", 8, "uteis", "art. 895, I, c/c art. 775 da CLT",
            vara, u.trt, "RO");
        c.mail($"Sentença trabalhista — recurso ordinário ({emp.nome.Split(' ')[0]})", OI,
            $"A sentença do caso d{emp.o} {emp.nome} saiu (evento {ns}). Ganhamos só a multa do art. 477; "
            + "o juiz <b>negou as horas extras e o intervalo</b> dizendo que os cartões de ponto prevalecem.",
            $"Só que, se você abrir os cartões (evento {n_cont}, documento PONTO2), vai ver que <b>todos os dias têm exatamente os mesmos "
            + "horários</b>: 9h00, 12h00, 13h00, 18h00. Isso é o famoso \"ponto britânico\". Além disso, a nossa "
            + "testemunha foi firme e o preposto não soube explicar nada.",
            "Redija o <b>recurso ordinário</b>:" + c.dicas(
                "Interposição ao juízo de origem e razões ao TRT.",
                "<b>Súmula 338, III, do TST</b>: cartões com horários uniformes são inválidos e invertem o ônus da "
                + "prova, que passa a ser do empregador — que não produziu prova nenhuma.",
                "Prova testemunhal + desconhecimento do preposto (efeitos da confissão, art. 843, § 1º, da CLT).",
                "Pedir horas extras, intervalo suprimido (art. 71, § 4º) e reflexos. Não recorra da insalubridade "
                + "(o laudo foi bem feito).",
                "Reclamante beneficiário da justiça gratuita: sem preparo."),
            c.prazo_frase(), c.assinatura());
        return c.build(emp.nome, "ativo", $"{emp.nome} x {empresa}");
    }

    public static JsonObject Previdenciario(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var prof = cli.f ? r.choice(new[] { "costureira", "auxiliar de limpeza", "cozinheira" })
            : r.choice(new[] { "pedreiro", "carregador", "auxiliar de produção" });
        cli.prof = prof;
        var doenca = r.choice(new[] { "hérnia de disco lombar (CID M51.1) com radiculopatia",
            "tendinite crônica do ombro direito (CID M75.1)",
            "síndrome do túnel do carpo bilateral (CID G56.0)" });
        var nb = $"{r.randint(600, 650)}.{r.randint(100, 999)}.{r.randint(100, 999)}-{r.randint(0, 9)}";
        var vara = $"{c.ordm(1, 12)}ª Vara Federal de {u.cidade} (Juizado Especial Federal)";
        var juiz = c.juiz("Juiz Federal", "Juíza Federal");
        var nosso = c.nossos_advs();
        c.processo("federal", "PROCEDIMENTO DO JUIZADO ESPECIAL CÍVEL", vara,
            new[] { "Auxílio por Incapacidade Temporária", "Benefícios em Espécie" }, Math.Round(2600.0 * 14, 2), juiz,
            ("AUTOR", "RÉU"), "Juizado Especial Federal Cível");
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", "INSTITUTO NACIONAL DO SEGURO SOCIAL - INSS", "29.979.036/0001-40", new[] { "Procuradoria Federal" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var fatos = $"{cli.nome}, {r.randint(41, 57)} anos, {prof}, segurad{cli.o} empregad{cli.o} com "
                    + $"{r.randint(6, 18)} anos de contribuição, está afastad{cli.o} do trabalho por {doenca}. O pedido de "
                    + $"<b>auxílio por incapacidade temporária</b> (NB {nb}, DER {Tq(70)}) foi indeferido pelo INSS por "
                    + "\"não constatação de incapacidade laborativa\" na perícia administrativa.";
        if (v == "inicial")
        {
            c.anexo("Documentos médicos", Pn(
                $"Atestado do ortopedista (emitido em {Tq(20)}): {doenca}; afastamento por 120 dias; contraindicado "
                + "esforço físico e movimentos repetitivos. Exames de imagem compatíveis."));
            c.anexo("Comunicação de decisão do INSS", Pn(
                $"NB {nb}. Em atenção ao seu pedido de auxílio por incapacidade temporária, informamos que não foi "
                + "reconhecido o direito ao benefício, tendo em vista que não foi constatada, em exame realizado pela "
                + $"Perícia Médica Federal, incapacidade para o seu trabalho ou atividade habitual. Data: {Tq(55)}."));
            c.tarefa("Petição inicial (Juizado Especial Federal)", "inicial", 5, "interno",
                "Não há prazo processual, mas o cliente está sem renda.", vara, code: "INIC");
            c.mail($"Previdenciário — auxílio por incapacidade negado ({cli.nome.Split(' ')[0]})", OI,
                $"Atendi {cli.o} {cli.nome}. {fatos}",
                "Em anexo estão os documentos médicos e a carta de indeferimento. Vamos ajuizar no <b>Juizado "
                + "Especial Federal</b> (Lei 10.259/2001). Redija a <b>petição inicial</b>:" + c.dicas(
                    "Requisitos do benefício: qualidade de segurado, carência de 12 contribuições (art. 25, I, da "
                    + "Lei 8.213/91) e incapacidade para a atividade habitual por mais de 15 dias (art. 59).",
                    "Prévio requerimento administrativo feito (STF, Tema 350) — junte o indeferimento.",
                    "Pedido de <b>perícia médica judicial</b> com especialista e quesitos.",
                    "Pagamento desde a DER, com correção e juros, e tutela de urgência para implantação.",
                    "Gratuidade da justiça."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x INSS");
        }
        var inic = c.doc_p("INIC1", "Petição inicial", $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) FEDERAL DA {vara.Up()}", P(
                $"{Esc(cli.nome)} propõe ação previdenciária em face do INSS. {fatos}",
                "Requer a concessão do auxílio por incapacidade temporária desde a DER, após perícia judicial."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic, c.procuracao(cli) }, detail: vara);
        c.ev("DESPACHO/DECISÃO - DESIGNADA PERÍCIA MÉDICA", "MAGISTRADO", gap: (5, 10));
        var perito = c.pf();
        var laudo = c.doc_s("LAUDO1", "Laudo pericial", Pn(
            $"Perit{perito.o}: Dr{(perito.f ? "a" : "")}. {Esc(perito.nome)}, clínic{perito.o} geral.",
            $"Diagnóstico informado: {doenca}. Exame físico: dor à palpação, sem limitação funcional significativa no "
            + "momento do exame.",
            "Quesito 1 (há incapacidade?): \"Não há incapacidade para a atividade habitual.\"",
            "Quesito 5 (a atividade exige esforço físico?): \"Prejudicado.\"",
            "Observação: os exames de imagem e atestados juntados não foram analisados, por não terem sido "
            + "apresentados no ato pericial."));
        int n_laudo = c.ev("JUNTADA DE LAUDO PERICIAL", "PERITO", new[] { laudo }, gap: (20, 45));
        c.ev("MANIFESTAÇÃO SOBRE O LAUDO", c.socio.nome.Up(), new[] { c.doc_p("PET1", "Impugnação ao laudo e quesitos "
                                                                               + "complementares", $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) FEDERAL DA {vara.Up()}", P(
                "O perito é clínico geral e não examinou os exames de imagem juntados com a inicial. Apresenta "
                + "quesitos complementares e requer nova perícia com ortopedista."),
            new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab }) }, gap: (5, 8));
        int ns = c.nxt;
        var sent = c.doc_j("SENT1", "Sentença", P(
            "Dispensado o relatório (art. 38 da Lei 9.099/95, c/c art. 1º da Lei 10.259/2001). Decido.",
            "O benefício exige a comprovação da incapacidade laborativa. O laudo pericial, elaborado por profissional "
            + "equidistante das partes, concluiu pela inexistência de incapacidade. As alegações da parte autora não "
            + "infirmam as conclusões periciais. Desnecessária nova perícia.",
            "Ante o exposto, JULGO IMPROCEDENTE o pedido. Sem custas e honorários (art. 55 da Lei 9.099/95). "
            + "Intimem-se."));
        c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - IMPROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (5, 15));
        c.intimacao($"AUTOR: {cli.nome.Up()}", 10);
        c.tarefa("Recurso inominado (Turma Recursal Federal)", "recurso_inominado", 10, "uteis",
            "art. 42 da Lei 9.099/95 c/c art. 1º da Lei 10.259/2001", vara, $"Turma Recursal da {u.sj}",
            "RECINOM");
        c.mail($"Sentença improcedente — recurso inominado ({cli.nome.Split(' ')[0]} x INSS)", OI,
            $"A sentença do caso d{cli.o} {cli.nome} foi de <b>improcedência</b> (evento {ns}), baseada só no "
            + $"laudo (evento {n_laudo}).",
            "O laudo é muito frágil: perito clínico geral, não examinou os exames de imagem, deixou quesito sem "
            + "resposta. Nós impugnamos e pedimos quesitos complementares e nova perícia com especialista — o juiz "
            + "ignorou e sentenciou.",
            "Redija o <b>recurso inominado</b> à Turma Recursal:" + c.dicas(
                "Preliminar de <b>nulidade por cerceamento de defesa</b>: quesitos complementares não respondidos e "
                + "pedido de nova perícia não apreciado.",
                "O juiz não está adstrito ao laudo (art. 479 do CPC): atestados, exames e a profissão braçal "
                + "demonstram a incapacidade.",
                "Pedidos: anulação da sentença com retorno para nova perícia ou, subsidiariamente, reforma para "
                + "conceder o benefício desde a DER.",
                "Parte autora com gratuidade — sem preparo."),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x INSS");
    }

    public static JsonObject Mandado(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var cargo = r.choice(new[] { "Analista Administrativo", "Técnico Judiciário", "Agente Fazendário", "Assistente Social" });
        var orgao_c = $"Secretaria de Estado da Administração {u.estado}";
        var edital = $"{r.randint(1, 12):D2}/{Today().Year - 1}";
        var vara = $"{c.ordm(1, 8)}ª Vara da Fazenda Pública da Comarca de {u.cidade}";
        var juiz = c.juiz();
        var nosso = c.nossos_advs();
        c.processo("estadual", "MANDADO DE SEGURANÇA CÍVEL", vara, new[] { "Concurso Público", "Pessoa com Deficiência" },
            1000.0, juiz, ("IMPETRANTE", "IMPETRADO"));
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", $"PRESIDENTE DA COMISSÃO DO CONCURSO — {orgao_c.Up()}", "", new[] { "Procuradoria-Geral do Estado" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var fatos = $"{cli.nome} foi aprovad{cli.o} em {r.randint(2, 9)}º lugar nas vagas reservadas a pessoas com "
                    + $"deficiência no concurso para {cargo} (Edital nº {edital}, {orgao_c}). Tem <b>visão monocular</b> "
                    + "(cegueira no olho esquerdo, CID H54.4), atestada por laudo oftalmológico. Na perícia da junta médica, "
                    + $"foi <b>excluíd{cli.o} da lista de PcD</b> sob o argumento de que a visão monocular \"não se "
                    + $"enquadra no Decreto 3.298/99\". O resultado foi publicado no Diário Oficial em {Tq(40)}.";
        var dicas = new[]
        {
            "<b>Lei 14.126/2021</b>: a visão monocular é classificada como deficiência sensorial, do tipo visual, para "
            + "todos os efeitos legais. E a <b>Súmula 377 do STJ</b> já garantia o direito de concorrer às vagas PcD.",
            "Direito líquido e certo com prova pré-constituída (laudo, edital, publicação do resultado).",
            "Prazo decadencial de 120 dias (art. 23 da Lei 12.016/09) — conte a partir da publicação.",
            "Liminar (art. 7º, III): risco de nomeação dos demais candidatos sem a cliente.",
        };
        if (v == "inicial")
        {
            c.anexo("Laudo oftalmológico", Pn($"Paciente {Esc(cli.nome)}: amaurose no olho esquerdo (CID H54.4), "
                                             + "irreversível. Acuidade visual do olho direito: 20/20."));
            c.anexo("Resultado da perícia (Diário Oficial)", Pn(
                $"Concurso {cargo} — Edital {edital}. Candidat{cli.o} {Esc(cli.nome)}: NÃO ENQUADRAD{cli.O} "
                + "como pessoa com deficiência. Fundamento: \"visão monocular não prevista no art. 4º do Decreto "
                + $"3.298/99\". Publicado em {Tq(40)}."));
            c.tarefa("Petição inicial — mandado de segurança com pedido liminar", "ms", 4, "interno",
                "O prazo decadencial do MS (120 dias) ainda está longe, mas a convocação para nomeação pode sair "
                + "a qualquer momento.", vara, code: "INIC");
            c.mail($"Mandado de segurança — concurso / visão monocular ({cli.nome.Split(' ')[0]})", OI,
                $"Recebemos um caso de concurso público. {fatos}",
                "Vamos impetrar <b>mandado de segurança</b> com pedido liminar para reincluir "
                + $"{cli.o} cliente na lista de PcD. Laudo e publicação estão em anexo. Pontos:" + c.dicas(dicas.Concat(new[]
                {
                    "Indique corretamente a <b>autoridade coatora</b> e a pessoa jurídica a que ela se vincula "
                    + "(art. 6º da Lei 12.016/09).",
                    "Valor da causa simbólico é aceito quando não há conteúdo econômico imediato.",
                }).ToArray()),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"{cli.nome} x Presidente da Comissão do Concurso");
        }
        var inic = c.doc_p("INIC1", "Mandado de segurança", $"EXCELENTÍSSIMO(A) SENHOR(A) JUIZ(A) DE DIREITO DA {vara.Up()}",
            P($"{Esc(cli.nome)} impetra MANDADO DE SEGURANÇA com pedido liminar contra ato do Presidente da "
              + $"Comissão do Concurso. {fatos}", "Requer liminar para reinclusão na lista de PcD e, ao final, "
                                                  + "a concessão da segurança."), new Adv { nome = c.socio.nome.Split(' ', 2)[1], oab = c.socio.oab },
            n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic }, detail: vara);
        int n_dec = c.ev("DECISÃO INTERLOCUTÓRIA - INDEFERIDA A LIMINAR", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            "Vistos. Em análise sumária, não verifico ilegalidade manifesta. A avaliação da condição de pessoa com "
            + "deficiência compete à junta médica oficial, cujos atos gozam de presunção de legitimidade, e a matéria "
            + "demanda maior aprofundamento.",
            "INDEFIRO a liminar. Notifique-se a autoridade coatora para prestar informações em 10 dias; dê-se ciência à "
            + "Procuradoria do Estado (art. 7º, I e II, da Lei 12.016/09). Após, ao Ministério Público.")) }, gap: (2, 4));
        c.intimacao($"IMPETRANTE: {cli.nome.Up()}", 15);
        c.tarefa("Agravo de instrumento contra o indeferimento da liminar", "agravo", 15, "uteis",
            "art. 7º, § 1º, da Lei 12.016/09 c/c arts. 1.003, § 5º, e 1.015, I, do CPC", vara, u.tj, "AGRAVO");
        c.mail($"Liminar negada no MS — agravo de instrumento ({cli.nome.Split(' ')[0]})", OI,
            $"No mandado de segurança d{cli.o} {cli.nome} (visão monocular), o juiz <b>indeferiu a liminar</b> "
            + $"(evento {n_dec}), dizendo que a junta médica tem presunção de legitimidade.",
            "Só que a junta decidiu contra a lei — depois da Lei 14.126/2021 não há margem de interpretação. E a "
            + "convocação para posse está prevista para as próximas semanas.",
            "Redija o <b>agravo de instrumento</b> com pedido de antecipação da tutela recursal:" + c.dicas(dicas.Take(2).Concat(new[]
            {
                "Cabimento expresso: art. 7º, § 1º, da Lei 12.016/09.",
                "Peças obrigatórias (art. 1.017 do CPC) e pedido de efeito ativo (art. 1.019, I).",
            }).ToArray()),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"{cli.nome} x Presidente da Comissão do Concurso");
    }

    public static JsonObject ExecFiscal(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var vara = $"Vara de Execuções Fiscais Municipais da Comarca de {u.cidade}";
        var juiz = c.juiz();
        var nosso = c.nossos_advs();
        int ano_aj = Today().Year - 3;
        int[] exercicios = { Today().Year - 11, Today().Year - 10, Today().Year - 9 };
        var valor = c.money(3800, 9600);
        c.processo("estadual", "EXECUÇÃO FISCAL", vara, new[] { "IPTU / Imposto Predial e Territorial Urbano", "Dívida Ativa" },
            valor, juiz, ("EXEQUENTE", "EXECUTADO"), ano: ano_aj);
        c.parte("a", u.mun.Up(), GenCnpj(r), new[] { "Procuradoria-Geral do Município" });
        c.parte("p", cli.nome, cli.cpf, nosso, true);
        var cda = $"{r.randint(1000, 9999)}/{ano_aj}";
        c.ev("DISTRIBUÍDO POR SORTEIO", "PROCURADORIA DO MUNICÍPIO", new[] { c.doc_s("INIC1", "Petição inicial e CDA", Pn(
            $"O {u.mun} promove EXECUÇÃO FISCAL contra {Esc(cli.nome)}, com base na Certidão de Dívida Ativa nº "
            + $"{cda}, referente ao <b>IPTU dos exercícios de {exercicios[0]}, {exercicios[1]} e {exercicios[2]}</b> "
            + $"(vencimentos em fevereiro de cada ano, com envio regular dos carnês), no valor atualizado de {Brl(valor)}.")) }, detail: vara);
        int n_desp = c.ev("DESPACHO - CITE-SE", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Despacho", P(
            "Cite-se o executado para, no prazo de 5 dias, pagar a dívida ou garantir a execução (art. 8º da Lei "
            + "6.830/80).")) }, gap: (20, 40));
        c.ev("CITAÇÃO POR CORREIO - AR NEGATIVO", "SISTEMA", detail: "Mudou-se", gap: (60, 120));
        c.ev("PETIÇÃO - PEDIDO DE NOVO ENDEREÇO", "PROCURADORIA DO MUNICÍPIO", gap: (420, 480));
        int n_cit = c.ev("JUNTADA DE AR - CITAÇÃO POSITIVA", "SISTEMA", gap: (40, 90));
        c.tarefa("Exceção de pré-executividade", "excecao", 4, "interno",
            "A exceção não tem prazo legal, mas precisamos protocolar antes de qualquer bloqueio de valores "
            + "(Sisbajud).", vara, code: "EXCECAO");
        c.mail($"Execução fiscal de IPTU — exceção de pré-executividade ({cli.nome.Split(' ')[0]})", OI,
            $"{cli.O} {cli.sr} {cli.nome} foi citad{cli.o} numa <b>execução fiscal</b> de IPTU "
            + $"(evento {n_cit}). Olhe as datas com atenção: são débitos de <b>{exercicios[0]} a {exercicios[2]}</b>, e "
            + $"a execução só foi ajuizada em §D1§ (evento 1); o despacho que ordenou a citação é do evento {n_desp}.",
            "Para mim, está tudo <b>prescrito</b>. Como é matéria de ordem pública e não depende de prova, vamos de "
            + "<b>exceção de pré-executividade</b>, sem precisar garantir o juízo:" + c.dicas(
                "Cabimento: <b>Súmula 393 do STJ</b>.",
                "Prescrição de 5 anos contados da constituição definitiva do crédito (art. 174 do CTN). No IPTU, o "
                + "envio do carnê constitui o crédito (Súmula 397 do STJ) e o prazo começa no dia seguinte ao "
                + "vencimento (STJ, Tema 980).",
                "A interrupção só ocorre com o despacho que ordena a citação (art. 174, parágrafo único, I, do CTN) "
                + "— que veio depois de mais de 5 anos para todos os exercícios. Monte uma tabela.",
                "Pedidos: extinção da execução (art. 156, V, do CTN) e honorários de sucumbência."),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "passivo", $"{u.mun} x {cli.nome}");
    }
}
