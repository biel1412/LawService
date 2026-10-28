using System.Text.Json.Nodes;
using static LawService.Est;
using static LawService.U;

namespace LawService;

// ════════════════════════════════════════════════════════════════ ESTÁGIO — casos adicionais (parte 1)
public static partial class Casos
{
    public static JsonObject Divorcio(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var mar = c.pf("M");
        var mul = c.pf("F");
        var filho = c.pf();
        int idade = r.randint(5, 13);
        int anos = r.randint(9, 16);
        var casa = c.money(380000, 690000);
        var carro = c.money(48000, 92000);
        var vara = $"{c.ordm(1, 6)}ª Vara de Família da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        var adv_x = c.adv();
        var (autor, re_) = v == "inicial" ? (mul, mar) : (mar, mul);
        c.processo("estadual", "DIVÓRCIO LITIGIOSO", vara, new[] { "Dissolução", "Guarda", "Alimentos", "Partilha" },
            Math.Round(casa / 2 + carro / 2, 2), segredo: true);
        c.parte("a", autor.nome, autor.cpf, v == "inicial" ? nosso : new List<string> { $"{adv_x.nome} ({adv_x.oab})" },
            v == "inicial");
        c.parte("p", re_.nome, re_.cpf, v == "inicial" ? new List<string> { $"{adv_x.nome} ({adv_x.oab})" } : nosso,
            v != "inicial");
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var bse = $"{mul.nome} e {mar.nome} são casados há {anos} anos pelo regime da <b>comunhão parcial de bens</b> "
                  + $"e têm um{(filho.f ? "a" : "")} filh{filho.o} de {idade} anos ({filho.init}). Estão separados "
                  + $"de fato desde {Tq(120)}; a criança mora com a mãe.";
        if (v == "inicial")
        {
            var sal = c.money(6800, 12500);
            c.anexo("Relato do atendimento", Pn(
                bse, $"Bens: (i) apartamento em {u.cidade}, comprado em {Tq(365 * 6)}, durante o casamento, avaliado em "
                     + $"{Brl(casa)}, financiado em nome do casal; (ii) veículo comprado em {Tq(365 * 2)}, em nome do "
                     + $"marido, avaliado em {Brl(carro)}.",
                $"O marido é {Esc(mar.prof)} e recebe cerca de {Brl(sal)} líquidos. Ele se recusa a assinar o divórcio "
                + "consensual e não paga nada pela criança desde a separação. A cliente quer o divórcio, a guarda "
                + "compartilhada com residência materna, alimentos para o filho e a partilha de 50% dos bens."));
            c.anexo("Documentos", Pn("Certidão de casamento; certidão de nascimento da criança; matrícula do imóvel; "
                                     + "documento do veículo; comprovantes de despesas escolares e de saúde da criança."));
            c.tarefa("Petição inicial — divórcio litigioso c/c guarda, alimentos e partilha", "inicial", 5, "interno",
                "Não há prazo processual, mas a criança está sem pensão.", vara, code: "INIC");
            c.mail($"Nova cliente — divórcio litigioso ({mul.nome.Split(' ')[0]})", OI,
                $"Atendi a {mul.sr} {mul.nome}. {bse} O marido não aceita o divórcio consensual.",
                "Os documentos e o relato estão em anexo. Redija a <b>petição inicial</b>. Pontos:" + c.dicas(
                    "Desde a EC 66/2010, o divórcio não depende de prazo nem de discutir culpa (art. 226, § 6º, da CF). "
                    + "Peça a <b>decretação liminar do divórcio</b> (tutela de evidência), já que é direito potestativo.",
                    "Guarda <b>compartilhada</b> como regra (art. 1.584, § 2º, do CC), com residência na casa materna e "
                    + "regime de convivência proposto.",
                    "Alimentos para a criança (art. 1.694 do CC), com pedido de provisórios. Renda do pai: cerca de "
                    + $"{Brl(sal)} líquidos.",
                    "Partilha: na comunhão parcial, comunicam-se os bens adquiridos onerosamente na constância do "
                    + "casamento (art. 1.658 do CC) — apartamento (com o saldo do financiamento) e veículo.",
                    "Segredo de justiça (art. 189, II, do CPC)."),
                c.prazo_frase(), c.assinatura());
            return c.build(mul.nome, "ativo", $"Divórcio — {mul.nome} x {mar.nome}");
        }
        var ender = EndJuiz(c);
        var inic = c.doc_p("INIC1", "Petição inicial", ender, P(
            $"{Esc(mar.nome)} propõe AÇÃO DE DIVÓRCIO LITIGIOSO C/C GUARDA E PARTILHA em face de {Esc(mul.nome)}. "
            + bse,
            $"Pede: (i) o divórcio; (ii) a <b>guarda unilateral</b> da criança, alegando que a mãe trabalha à noite; (iii) a "
            + $"partilha igualitária do apartamento em que a ré mora ({Brl(casa)}) e do veículo que ela utiliza "
            + $"({Brl(carro)})."), adv_x, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_x.nome.Up(), new[] { inic }, detail: $"{vara} — SEGREDO DE JUSTIÇA");
        c.ev("DESPACHO", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Despacho", P(
            "Designo audiência de mediação (art. 695 do CPC). Cite-se a ré, com mandado desacompanhado de cópia da "
            + "inicial (art. 695, § 1º). Não havendo acordo, o prazo para contestação correrá da audiência (art. 697 c/c "
            + "art. 335, I, do CPC).")) }, gap: (3, 8));
        c.citacao($"RÉ: {mul.nome.Up()}");
        int n_aud = c.ev("AUDIÊNCIA DE MEDIAÇÃO REALIZADA - SEM ACORDO", "MEDIADOR", new[] { c.doc_s("TERMOAUD1", "Termo de sessão de mediação", Pn(
            "As partes concordaram com o divórcio, mas não houve acordo quanto à guarda e à partilha. Aberto à ré o "
            + "prazo de contestação.")) }, gap: (20, 35));
        c.prazo = new JsonObject { ["ev"] = n_aud, ["dias"] = 15, ["contagem"] = "uteis" };
        c.tarefa("Contestação (com reconvenção)", "contestacao", 15, "uteis", "art. 335, I, c/c art. 697 do CPC", vara,
            code: "CONT");
        c.mail($"Divórcio — defesa da {mul.nome.Split(' ')[0]} (guarda e partilha)", OI,
            $"Assumimos a defesa da {mul.sr} {mul.nome} na ação de divórcio movida pelo marido. {bse} A mediação "
            + $"não deu certo (evento {n_aud}) e o prazo de contestação está correndo.",
            "A versão da cliente:" + c.dicas(
                "Ela <b>não se opõe ao divórcio</b>.",
                "Trabalha como enfermeira em plantões de 12x36, alguns noturnos; nesses dias a avó materna cuida da "
                + "criança. Sempre foi a principal cuidadora. Aceita guarda <b>compartilhada</b> com residência dela.",
                $"O apartamento foi comprado em parte com {Brl(casa * .35)} da venda de um imóvel que ela "
                + "<b>herdou do pai</b> antes do casamento (temos a escritura e o extrato da transferência).",
                "O marido nunca pagou pensão desde a separação."),
            "Redija a <b>contestação</b>, e use a <b>reconvenção</b> (art. 343 do CPC) para pedir alimentos para a "
            + "criança. Pontos:" + c.dicas(
                "Guarda compartilhada é a regra (art. 1.584, § 2º, do CC); trabalho em plantão não indica inaptidão.",
                "Sub-rogação de bem particular: o valor oriundo da herança fica fora da partilha (art. 1.659, I e II, "
                + "do CC) — partilha apenas da parte comum.",
                "O veículo também é comum: ela pede partilha ou compensação.",
                "Provas: escritura, extratos, testemunhas, estudo psicossocial."),
            c.prazo_frase(), c.assinatura());
        return c.build(mul.nome, "passivo", $"Divórcio — {mar.nome} x {mul.nome}");
    }

    public static JsonObject Inventario(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var falecido = c.pf("M");
        var viuva = c.pf("F");
        var filhos = Enumerable.Range(0, r.randint(2, 3)).Select(_ => c.pf()).ToList();
        foreach (var f in filhos) f.nome = f.nome.Split(' ')[0] + " " + falecido.nome.Split(' ')[^1];
        var imovel = c.money(420000, 880000);
        var saldo = c.money(18000, 76000);
        var carro = c.money(35000, 70000);
        var vara = $"{c.ordm(1, 4)}ª Vara de Sucessões da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        c.processo("estadual", "INVENTÁRIO", vara, new[] { "Inventário e Partilha" }, Math.Round(imovel + saldo + carro, 2));
        c.parte("a", viuva.nome, viuva.cpf, nosso, true);
        c.parte("p", $"ESPÓLIO DE {falecido.nome.Up()}", "", new List<string>());
        var nomes_f = string.Join(", ", filhos.Select(f => f.nome));
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var bse = $"{falecido.nome} faleceu em {Tq(v == "inicial" ? 35 : 110)}, sem testamento, deixando a esposa "
                  + $"{viuva.nome} (casados pela comunhão parcial de bens) e {filhos.Count} filhos maiores: {nomes_f}.";
        var bens = $"Bens: (i) imóvel residencial em {u.cidade}, comprado durante o casamento, avaliado em "
                   + $"{Brl(imovel)}; (ii) saldo em conta corrente de {Brl(saldo)}; (iii) veículo avaliado em {Brl(carro)}. "
                   + "Não há dívidas conhecidas.";
        if (v == "inicial")
        {
            c.anexo("Relato do atendimento", Pn(bse, bens, "Há desentendimento entre os filhos sobre a venda do imóvel, "
                                                          + "por isso não será possível fazer inventário extrajudicial em "
                                                          + "cartório."));
            c.anexo("Documentos", Pn("Certidão de óbito; certidão de casamento; certidões de nascimento dos filhos; "
                                     + "matrícula do imóvel; extrato bancário; documento do veículo."));
            c.tarefa("Petição de abertura de inventário", "inicial", 5, "interno",
                "O CPC fixa 2 meses da abertura da sucessão para instaurar o inventário (art. 611) — e a multa do "
                + "ITCMD costuma incidir pelo atraso.", vara, code: "INIC");
            c.mail($"Inventário — família {falecido.nome.Split(' ')[^1]}", OI,
                $"A {viuva.sr} {viuva.nome} nos contratou para o inventário do marido. {bse}", bens,
                "Como os herdeiros não estão de acordo, o inventário será <b>judicial</b>. Redija a <b>petição de "
                + "abertura</b>:" + c.dicas(
                    "Legitimidade para requerer (art. 615 do CPC) e pedido de nomeação da viúva como "
                    + "<b>inventariante</b> (art. 617, I).",
                    "Indicação do falecido, data do óbito, herdeiros e bens, com a documentação (art. 615, parágrafo "
                    + "único).",
                    "Atenção: na comunhão parcial, a viúva é <b>meeira</b> dos bens comuns e, em regra, não concorre como "
                    + "herdeira sobre eles (art. 1.829, I, do CC) — explique na petição.",
                    "Valor da causa = valor do monte; pedido de expedição de alvará se houver despesas urgentes."),
                c.prazo_frase(), c.assinatura());
            return c.build(viuva.nome, "ativo", $"Inventário de {falecido.nome}");
        }
        var inic = c.doc_p("INIC1", "Petição de abertura de inventário", EndJuiz(c), P(
            $"{Esc(viuva.nome)} requer a abertura do inventário. {bse} {bens}",
            "Requer a sua nomeação como inventariante (art. 617, I, do CPC)."), AdvNosso(c), n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic, c.procuracao(viuva) }, detail: vara);
        int n_dec = c.ev("DECISÃO - NOMEADO INVENTARIANTE", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            $"Nomeio inventariante {Esc(viuva.nome)}, que deverá prestar compromisso em 5 dias (art. 617, parágrafo "
            + "único, do CPC) e, nos 20 dias seguintes, apresentar as primeiras declarações (art. 620).")) }, gap: (5, 12));
        int n_comp = c.ev("TERMO DE COMPROMISSO DE INVENTARIANTE ASSINADO", "SERVIDOR", new[] { c.doc_s(
            "TERMO1", "Termo de compromisso", Pn($"{Esc(viuva.nome)} prestou compromisso de bem e fielmente "
                                                + "desempenhar o cargo de inventariante.")) }, gap: (3, 5));
        c.prazo = new JsonObject { ["ev"] = n_comp, ["dias"] = 20, ["contagem"] = "uteis" };
        c.tarefa("Primeiras declarações", "primeiras_declaracoes", 20, "uteis", "art. 620 do CPC", vara, code: "PRIMDECL");
        c.mail($"Inventário — primeiras declarações ({falecido.nome.Split(' ')[^1]})", OI,
            $"No inventário do {falecido.sr} {falecido.nome}, a cliente foi nomeada inventariante (evento "
            + $"{n_dec}) e assinou o compromisso (evento {n_comp}). Agora vêm as <b>primeiras declarações</b>.",
            bse + " " + bens,
            "Monte a peça seguindo o art. 620 do CPC:" + c.dicas(
                "Qualificação do autor da herança, data e local do óbito e se deixou testamento (inciso I).",
                "Qualificação dos herdeiros e do cônjuge, com o regime de bens (II e III).",
                "Relação completa dos bens, com descrição e valor (IV) — imóvel com matrícula, saldo bancário e "
                + "veículo com RENAVAM.",
                "Separe a <b>meação</b> da viúva (50% dos bens comuns) da <b>herança</b> (os outros 50%, divididos "
                + "entre os filhos — art. 1.829, I, do CC).",
                "Requeira a citação dos herdeiros, a intimação da Fazenda para o ITCMD e a avaliação, se necessária."),
            c.prazo_frase(), c.assinatura());
        return c.build(viuva.nome, "ativo", $"Inventário de {falecido.nome}");
    }

    public static JsonObject Usucapiao(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var dono = c.pf("M");
        int anos = r.randint(17, 24);
        int area = r.randint(180, 420);
        var lote = $"lote {r.randint(1, 30)} da quadra {r.choice("ABCDEFGH")}";
        var bairro = r.choice(new[] { "Vila Esperança", "Jardim Santa Rita", "Parque das Árvores", "Vila São Jorge" });
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        var valor = c.money(160000, 320000);
        c.processo("estadual", "USUCAPIÃO", vara, new[] { "Usucapião Extraordinária" }, valor);
        c.parte("a", cli.nome, cli.cpf, nosso, true);
        c.parte("p", $"{dono.nome} (titular registral) e eventuais interessados", "", new[] { "Curador especial" });
        Func<int, string> Tq = v == "inicial" ? Ctx.B : Ctx.A;
        var bse = $"{cli.nome} mora há <b>{anos} anos</b> num terreno de {area} m² ({lote}) no bairro {bairro}, em "
                  + $"{u.cidade}, registrado em nome de {dono.nome}, que nunca apareceu. {Cap(cli.ele)} "
                  + $"construiu a casa, paga o IPTU em seu nome há mais de 12 anos e todos os vizinhos {cli.o} conhecem "
                  + $"como {(cli.f ? "dona" : "dono")}.";
        if (v == "inicial")
        {
            c.anexo("Relato do atendimento", Pn(bse, $"Entrou no imóvel em {Tq(365 * anos)}, quando o terreno estava "
                                                     + "abandonado. Nunca houve oposição. Contas de luz e água em seu nome "
                                                     + "desde então."));
            c.anexo("Documentos", Pn("Carnês de IPTU; contas de consumo antigas; fotos da construção; planta e memorial "
                                     + "descritivo assinados por engenheiro, com ART; certidão da matrícula; declarações de "
                                     + "três vizinhos."));
            c.tarefa("Petição inicial — usucapião extraordinária", "inicial", 6, "interno", "Sem prazo processual.", vara,
                code: "INIC");
            c.mail($"Usucapião — {cli.nome.Split(' ')[0]}", OI, bse,
                "Vamos entrar com <b>ação de usucapião extraordinária</b> (também seria possível a via extrajudicial, "
                + "mas o titular registral não foi localizado para anuir). Pontos:" + c.dicas(
                    "Requisitos do art. 1.238 do CC: posse mansa, pacífica, ininterrupta e com ânimo de dono por 15 anos "
                    + "— ou 10 anos, se houver moradia habitual ou obras de caráter produtivo (parágrafo único).",
                    "Junte planta e memorial descritivo e peça a citação do titular registral e dos confinantes "
                    + "(art. 246, § 3º, do CPC) e edital para interessados incertos (art. 259, I).",
                    "Requeira a intimação das Fazendas (União, Estado e Município) para manifestarem interesse.",
                    "Testemunhas: os vizinhos que assinaram as declarações."),
                c.prazo_frase(), c.assinatura());
            return c.build(cli.nome, "ativo", $"Usucapião — {cli.nome}");
        }
        var inic = c.doc_p("INIC1", "Petição inicial", EndJuiz(c), P(
            $"{Esc(cli.nome)} propõe AÇÃO DE USUCAPIÃO EXTRAORDINÁRIA. " + bse,
            "Requer a declaração do domínio, com fundamento no art. 1.238 do Código Civil."), AdvNosso(c), n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", c.socio.nome.Up(), new[] { inic }, detail: vara);
        c.ev("JUNTADA DE EDITAL PUBLICADO", "SERVIDOR", gap: (30, 60));
        c.ev("CONTESTAÇÃO POR NEGATIVA GERAL - CURADOR ESPECIAL", "CURADOR ESPECIAL", gap: (30, 50));
        var test = c.pf();
        c.ev("AUDIÊNCIA DE INSTRUÇÃO REALIZADA", "MAGISTRADO", new[] { c.doc_s("TERMOAUD1", "Termo de audiência", Pn(
            "Depoimento pessoal: a parte autora disse que entrou no terreno porque \"a mãe do dono, que morava ao lado, "
            + $"deixou\" e que, após a morte dela, há cerca de {anos - 4} anos, \"passou a cuidar de tudo como seu\".",
            $"Testemunha {Esc(test.nome)}: conhece a parte autora há mais de 15 anos; ela construiu a casa, ampliou-a e "
            + "sempre foi vista como dona; nunca ninguém reclamou o imóvel.")) }, gap: (60, 120));
        int ns = c.nxt;
        var sent = c.doc_j("SENT1", "Sentença", P(
            $"{Esc(cli.nome)} pretende a usucapião extraordinária do imóvel descrito na inicial. É o relatório. Decido.",
            "Do próprio depoimento da parte autora extrai-se que o ingresso no imóvel decorreu de permissão da mãe do "
            + "proprietário. Atos de mera permissão ou tolerância não induzem posse (art. 1.208 do CC). Ausente o "
            + "<i>animus domini</i> desde o início, não se configura a posse <i>ad usucapionem</i>.",
            "Ante o exposto, JULGO IMPROCEDENTE o pedido. Custas pela parte autora. Intimem-se."));
        c.ev("SENTENÇA COM RESOLUÇÃO DE MÉRITO - IMPROCEDÊNCIA", "MAGISTRADO", new[] { sent }, gap: (15, 40));
        c.intimacao($"AUTOR: {cli.nome.Up()}", 15);
        c.tarefa("Apelação", "apelacao", 15, "uteis", "art. 1.003, § 5º, do CPC", vara, u.tj, "APELACAO");
        c.mail($"Usucapião julgada improcedente — apelação ({cli.nome.Split(' ')[0]})", OI,
            $"A usucapião d{cli.o} {cli.nome} foi julgada <b>improcedente</b> (evento {ns}). O juiz entendeu que, "
            + "como a entrada no terreno foi com permissão da mãe do dono, a posse seria mera tolerância (art. 1.208 do "
            + "CC) e nunca teria havido ânimo de dono.",
            $"Ele ignorou o resto do depoimento: depois que a mãe do dono morreu, há mais de {anos - 4} anos, "
            + $"{cli.o} cliente passou a agir como dono — construiu, ampliou, pagou IPTU, e ninguém se opôs.",
            "Redija a <b>apelação</b>:" + c.dicas(
                "<b>Interversão do caráter da posse</b>: a detenção/tolerância inicial pode se converter em posse com "
                + "ânimo de dono por atos externos inequívocos de oposição ao proprietário (Enunciado 237 da III Jornada "
                + "de Direito Civil).",
                "Contado da interversão, o prazo supera os 10 anos do art. 1.238, parágrafo único (moradia habitual).",
                "Prova: testemunha, carnês de IPTU, contas de consumo e construção da casa.",
                "Preparo, tempestividade e pedido de reforma para declarar o domínio."),
            c.prazo_frase(), c.assinatura());
        return c.build(cli.nome, "ativo", $"Usucapião — {cli.nome}");
    }

    public static JsonObject Monitoria(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var credor = r.choice(new[] { "Marcenaria Ipê Móveis Planejados Ltda.", "Gráfica Ponto Final Ltda.",
            "Climatiza Ar-Condicionado Ltda." });
        var devedor = c.pf();
        var valor = c.money(6400, 24800);
        var emissao = v != "inicial" ? Ctx.A(365 * 2 + 60) : Ctx.B(365 * 2 + 60);
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        var adv_x = c.adv();
        var servico = credor.Split(' ')[0] switch
        {
            "Marcenaria" => "móveis planejados para a cozinha",
            "Gráfica" => "material gráfico para uma loja",
            _ => "instalação de três aparelhos de ar-condicionado",
        };
        c.processo("estadual", "MONITÓRIA", vara, new[] { "Cheque", "Pagamento" }, valor);
        var cnpj = GenCnpj(r);
        if (v == "inicial")
        {
            c.parte("a", credor, cnpj, nosso, true);
            c.parte("p", devedor.nome, devedor.cpf, new List<string>());
            c.anexo("Cheque e comprovante de devolução", Pn(
                $"Cheque nº {r.randint(100000, 999999)}, emitido por {Esc(devedor.nome)} em {emissao}, no valor de "
                + $"{Brl(valor)}, nominal a {credor}. Devolvido por insuficiência de fundos (alínea 11) e reapresentado "
                + "(alínea 12)."));
            c.anexo("Orçamento aprovado e termo de entrega", Pn($"Serviço: {servico}. Termo de entrega assinado pel{devedor.o} "
                                                                + "cliente, sem ressalvas."));
            c.tarefa("Petição inicial — ação monitória", "inicial", 5, "interno",
                "O cheque já perdeu a força executiva; o prazo da monitória é de 5 anos (Súmula 503 do STJ).",
                vara, code: "INIC");
            c.mail($"Cobrança de cheque prescrito — {credor.Split(' ')[0]} {credor.Split(' ')[1]}", OI,
                $"Nossa cliente {credor} recebeu de {devedor.nome} um cheque de {Brl(valor)}, emitido em {emissao}, "
                + $"pelo serviço de {servico}. O cheque voltou sem fundos e nunca foi pago.",
                "O prazo da execução do cheque (6 meses após o prazo de apresentação) já passou, então vamos de "
                + "<b>ação monitória</b>. Pontos:" + c.dicas(
                    "Cabimento: prova escrita sem eficácia de título executivo (art. 700 do CPC; Súmula 299 do STJ).",
                    "Não é preciso provar a causa da dívida (Súmula 531 do STJ), mas mencione o serviço prestado.",
                    "Prazo de 5 anos contado do dia seguinte à data de emissão (Súmula 503 do STJ) — estamos dentro.",
                    "Memória de cálculo com correção e juros (art. 700, § 2º, I) e pedido de expedição do mandado de "
                    + "pagamento em 15 dias, com honorários de 5% (art. 701)."),
                c.prazo_frase(), c.assinatura());
            return c.build(credor, "ativo", $"{credor} x {devedor.nome}");
        }
        c.parte("a", credor, cnpj, new[] { $"{adv_x.nome} ({adv_x.oab})" });
        c.parte("p", devedor.nome, devedor.cpf, nosso, true);
        var inic = c.doc_p("INIC1", "Petição inicial", EndJuiz(c), P(
                $"{credor} propõe AÇÃO MONITÓRIA em face de {Esc(devedor.nome)}, com base em cheque emitido em {emissao}, "
                + $"no valor de {Brl(valor)}, devolvido sem provisão de fundos. Requer a expedição de mandado de pagamento."),
            adv_x, n: 1);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_x.nome.Up(), new[] { inic }, detail: vara);
        c.ev("DECISÃO - DEFERIDA A EXPEDIÇÃO DE MANDADO MONITÓRIO", "MAGISTRADO", new[] { c.doc_j("DESPADEC1", "Decisão", P(
            "Evidente o direito da parte autora, defiro a expedição de mandado de pagamento, concedendo ao réu o prazo de "
            + "15 dias para cumprimento e pagamento de honorários de 5% do valor da causa (art. 701 do CPC), ou oposição de "
            + "embargos (art. 702).")) }, gap: (3, 8));
        c.ev("MANDADO DE CITAÇÃO EXPEDIDO", "SERVIDOR", gap: (1, 2));
        int n_cit = c.ev("JUNTADA DE MANDADO DE CITAÇÃO CUMPRIDO", "OFICIAL DE JUSTIÇA", gap: (5, 15));
        c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 15, ["contagem"] = "uteis" };
        c.tarefa("Embargos à monitória", "embargos_monitoria", 15, "uteis", "art. 702 do CPC", vara, code: "EMBMON");
        c.mail($"Embargos à monitória ({devedor.nome.Split(' ')[0]})", OI,
            $"{devedor.O} {devedor.sr} {devedor.nome} foi citad{devedor.o} numa <b>ação monitória</b> movida "
            + $"por {credor} (evento {n_cit}), cobrando um cheque de {Brl(valor)}.",
            $"A história é outra: o cheque foi dado como <b>sinal</b> para {servico}, que <b>nunca foi concluído</b>. "
            + "A empresa entregou só parte do serviço, com defeitos, e parou de responder (temos as conversas de "
            + "WhatsApp, fotos e um laudo de um profissional que avaliou o que foi feito em cerca de 30% do contratado).",
            "Redija os <b>embargos à monitória</b>:" + c.dicas(
                "Como o cheque não circulou, entre as partes originais é possível discutir a <b>causa da dívida</b>.",
                "<b>Exceção de contrato não cumprido</b> (art. 476 do CC) e inadimplemento da embargada.",
                "Se alegar que cobram mais do que o devido, indique o valor que entende correto com demonstrativo "
                + "(art. 702, § 2º) — senão a alegação de excesso não será examinada.",
                "Pedidos: rejeição do pedido monitório (ou redução ao valor proporcional ao serviço feito) e provas: "
                + "documental, testemunhal e pericial."),
            c.prazo_frase(), c.assinatura());
        return c.build(devedor.nome, "passivo", $"{credor} x {devedor.nome}");
    }

    public static JsonObject EmbExec(Ctx c, string v)
    {
        var r = c.r;
        var u = c.u;
        var cli = c.pf();
        var empresa = $"{cli.nome.Split(' ')[^1]} {r.choice(new[] { "Comércio de Alimentos", "Serviços Automotivos", "Confecções" })} Ltda.";
        var banco = r.choice(new[] { "Banco Meridiano S.A.", "Banco Horizonte Digital S.A.", "Banco Sulino S.A." });
        var emprestimo = c.money(48000, 120000);
        var cobrado = Math.Round(emprestimo * r.uniform(1.85, 2.4), 2);
        var vara = $"{c.ordm(1, 18)}ª Vara Cível da Comarca de {u.cidade}";
        var nosso = c.nossos_advs();
        var adv_x = c.adv();
        c.processo("estadual", "EXECUÇÃO DE TÍTULO EXTRAJUDICIAL", vara, new[] { "Cédula de Crédito Bancário", "Contratos Bancários" },
            cobrado, lab: ("EXEQUENTE", "EXECUTADO"));
        c.parte("a", banco, GenCnpj(r), new[] { $"{adv_x.nome} ({adv_x.oab})" });
        c.parte("p", empresa, GenCnpj(r), nosso, true);
        c.parte("p", cli.nome + " (avalista)", cli.cpf, nosso, true);
        c.ev("DISTRIBUÍDO POR SORTEIO", adv_x.nome.Up(), new[] { c.doc_p("INIC1", "Petição inicial", EndJuiz(c), P(
                $"{banco} promove EXECUÇÃO DE TÍTULO EXTRAJUDICIAL contra {empresa} e seu avalista {Esc(cli.nome)}, com base "
                + $"em Cédula de Crédito Bancário de {Brl(emprestimo)}, inadimplida desde a 7ª parcela. Valor atualizado conforme "
                + $"planilha: {Brl(cobrado)}."), adv_x, n: 1),
            c.doc_s("CALC2", "Planilha de cálculo", Pn(
                "Encargos aplicados após o vencimento: <b>comissão de permanência</b> + juros remuneratórios de 3,9% a.m. + "
                + "juros moratórios de 1% a.m. + multa de 2%, com capitalização mensal.")),
            c.doc_s("CCB3", "Cédula de Crédito Bancário", Pn(
                $"Valor: {Brl(emprestimo)}. Taxa de juros: 2,1% a.m. Encargos de inadimplência: comissão de permanência à "
                + "taxa de mercado, juros de mora de 1% a.m. e multa de 2%. A cédula não prevê expressamente a capitalização "
                + "mensal dos juros.")) }, detail: vara);
        c.ev("DESPACHO - CITE-SE PARA PAGAMENTO EM 3 DIAS", "MAGISTRADO", gap: (3, 8));
        int n_cit = c.ev("JUNTADA DE MANDADO DE CITAÇÃO CUMPRIDO", "OFICIAL DE JUSTIÇA", gap: (10, 20));
        c.prazo = new JsonObject { ["ev"] = n_cit, ["dias"] = 15, ["contagem"] = "uteis" };
        c.tarefa("Embargos à execução", "embargos_execucao", 15, "uteis", "arts. 914 e 915 do CPC", vara, code: "EMBEXEC");
        c.mail($"Execução bancária — embargos à execução ({empresa.Split(' ')[0]})", OI,
            $"A {empresa} e o sócio {cli.nome} (avalista) foram citados numa execução movida por {banco} "
            + $"(evento {n_cit}). O empréstimo foi de {Brl(emprestimo)} e o banco está cobrando <b>{Brl(cobrado)}</b>.",
            "Abra a planilha (CALC2) e a cédula (CCB3) no evento 1: o banco somou comissão de permanência com juros "
            + "remuneratórios, moratórios e multa, e ainda capitalizou mensalmente juros que a cédula não prevê.",
            "Redija os <b>embargos à execução</b>:" + c.dicas(
                "Prazo de 15 dias úteis da juntada do mandado (art. 915 c/c art. 231, II, do CPC). Embargos dispensam "
                + "penhora (art. 914), mas o efeito suspensivo exige garantia (art. 919, § 1º).",
                "<b>Súmula 472 do STJ</b>: a comissão de permanência não pode ser cumulada com juros remuneratórios, "
                + "moratórios ou multa.",
                "Capitalização de juros só é válida se expressamente pactuada (Súmulas 539 e 541 do STJ).",
                "Excesso de execução (art. 917, III): você <b>tem</b> que indicar o valor correto com demonstrativo "
                + "(art. 917, § 3º), sob pena de rejeição liminar. Faça uma estimativa e peça perícia contábil.",
                "Embargos são distribuídos por dependência e autuados em apartado (art. 914, § 1º)."),
            c.prazo_frase(), c.assinatura());
        return c.build(empresa, "passivo", $"{banco} x {empresa}");
    }
}
