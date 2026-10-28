# LawService (versão web)

Aplicativo de estudos para Direito que roda direto no navegador. Foi feito em **C# com Blazor WebAssembly (.NET 8)** e tem as mesmas telas e funções da versão para computador:

- **Cadernos**: cadernos e páginas em árvore, com editor de texto rico (títulos, listas, tarefas, tabelas, imagens, cores, marca-texto e blocos de destaque), busca e arrastar para reorganizar.
- **Questões**: banco de questões com modo de resolução e de edição, aproveitamento e comentários.
- **Flashcards**: baralhos com revisão espaçada e cartões que viram.
- **Estágio simulado**: casos gerados automaticamente, com e-mails do escritório, processo eletrônico simulado (SIPE), redação da peça e entrega ao chefe.
- **TCC**: partes do trabalho com metas de palavras, citações (NBR 10520), referências automáticas (NBR 6023) e exportação com capa e folha de rosto.
- **Tutoria OAB** e **Tutoria de TCC**: páginas de solicitação de tutoria.

## Publicar no GitHub Pages (grátis)

1. Crie um repositório vazio no GitHub, por exemplo `LawService`, e envie esta pasta para ele (veja os comandos abaixo).
2. No repositório, abra **Settings → Pages** e, em **Build and deployment → Source**, escolha **GitHub Actions**.
3. Abra a aba **Actions**. O workflow **Publicar no GitHub Pages** compila e publica o site em 2 a 4 minutos. Se ele já tiver rodado antes do passo 2 e falhado, clique em **Re-run jobs**.
4. O site fica em `https://SEU-USUARIO.github.io/LawService/`.

Comandos para enviar a pasta (troque `SEU-USUARIO`):

```bash
git init
git add .
git commit -m "LawService web"
git branch -M main
git remote add origin https://github.com/SEU-USUARIO/LawService.git
git push -u origin main
```

Cada novo `git push` publica a versão atualizada automaticamente.

## Rodar no seu computador

Requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project src/LawService
```

Depois, abra **http://localhost:5080** no navegador. No VS Code, também é possível usar **Terminal → Executar Tarefa → rodar (dotnet watch)**: a página recarrega sozinha a cada alteração no código.

## Onde ficam os dados

Tudo fica salvo **no próprio navegador** (IndexedDB) e nada é enviado para servidores. Por isso, cada navegador e cada computador tem os seus próprios dados.

No botão **Meus dados**, no menu lateral, você encontra:

- **Exportar backup (.zip)**: baixa o `estudos.json` e as imagens.
- **Importar backup ou estudos.json**: aceita o backup do site, o `estudos.json` da versão para Windows/Python ou a pasta `dados_estudos` compactada em `.zip`.
- **Restaurar cópia automática**: o site guarda uma cópia por dia (as 15 mais recentes).

O formato dos dados é o mesmo da versão para computador.

## Atalhos

| Atalho | Ação |
|---|---|
| Alt + 1 … 7 | Trocar de aba |
| Alt + N | Novo item na aba atual |
| Ctrl + F | Buscar |
| Ctrl + S | Salvar |

Os navegadores reservam Ctrl+N e Ctrl+1…7 para abrir janelas e trocar de guia, por isso esses atalhos usam Alt no site.

## Diferenças em relação à versão para computador

- **PDF**: o site abre a janela de impressão do navegador. Escolha **Salvar como PDF**.
- **Word (.doc)**: a exportação do TCC e da peça do estágio baixa um arquivo `.doc`, que abre no Word, no LibreOffice e no Google Docs.
- **Dados**: ficam no navegador, e não numa pasta ao lado do programa (veja “Onde ficam os dados”).

## Links das tutorias

Os links dos formulários podem ser informados no próprio site, pelo ícone de engrenagem nas páginas de tutoria. Também podem ser fixados no código para todos os alunos:

- `src/LawService/Pages/TutoriaPage.razor`: `TUTORIA_URL`
- `src/LawService/Pages/TutoriaTccPage.razor`: `TUTORIA_TCC_URL`
- `src/LawService/Estagio/Engine.cs`: `CORRECAO_URL` (correção profissional da peça do estágio)

## Estrutura

```
LawService.sln
src/LawService/
├── App.razor              janela principal: menu lateral, abas, diálogos, menus e backup
├── Pages/                 Cadernos, Questões, Flashcards, Estágio, TCC e Tutorias
├── Components/            editor de texto, visualizador, ícones, paleta de cores…
├── Core/                  dados (Store), tema, utilitários e regras ABNT
├── Estagio/               gerador de casos do estágio simulado
├── Services/              ponte com o navegador (JavaScript) e modelos de interface
└── wwwroot/               index.html, estilos, app.js, editor.html e ícones (Material Design Icons)
```

Ícones: [Material Design Icons](https://pictogrammers.com/library/mdi/) (licença Apache 2.0, incluída em `wwwroot/lib/mdi`).
