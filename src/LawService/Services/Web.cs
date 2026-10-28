using System.Text.RegularExpressions;
using static LawService.Th;

namespace LawService;

/// <summary>Montagem de documentos HTML (visualização, impressão e exportação para Word).</summary>
public static class Web
{
    public static string BodyOf(string html)
    {
        var m = Regex.Match(html ?? "", "<body[^>]*>(.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var b = m.Success ? m.Groups[1].Value : (html ?? "");
        return Regex.Replace(b, @"<!--(Start|End)Fragment-->", "");
    }

    public static string Page(string body, string css = "", string font = SERIF, double pt = 12, string color = "#1a1a1a")
    {
        if (Regex.IsMatch(body ?? "", "<html", RegexOptions.IgnoreCase)) body = BodyOf(body);
        return "<!doctype html><html><head><meta charset=\"utf-8\"><style>"
               + $"body{{font-family:'{font}',serif;font-size:{U.Fmt(pt)}pt;color:{color};margin:0;}} img{{max-width:100%;height:auto}} "
               + "p{margin:0} table{border-collapse:collapse} ::-webkit-scrollbar{width:10px;height:10px} "
               + "::-webkit-scrollbar-thumb{background:#d6d0d3;border-radius:4px;border:2px solid #fff}" + css
               + "</style></head><body>" + body + "</body></html>";
    }

    /// <summary>Documento do Word (.doc em HTML — abre no Word, LibreOffice e Google Docs).</summary>
    public static string WordDoc(string html, string font = SERIF)
    {
        var body = BodyOf(html);
        body = body.Replace("page-break-before:always", "page-break-before:always; mso-break-type:page-break");
        return "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' "
               + "xmlns='http://www.w3.org/TR/REC-html40'><head><meta charset='utf-8'><title></title>"
               + "<!--[if gte mso 9]><xml><w:WordDocument><w:View>Print</w:View><w:Zoom>100</w:Zoom></w:WordDocument></xml><![endif]-->"
               + "<style>@page Section1 { size:21cm 29.7cm; margin:3cm 2cm 2cm 3cm; } div.Section1 { page:Section1; } "
               + $"body {{ font-family:'{font}'; font-size:12pt; }} p {{ margin:0; }}</style></head>"
               + "<body><div class='Section1'>" + body + "</div></body></html>";
    }
}
