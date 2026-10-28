namespace LawService;

public record Rect(double X, double Y, double W, double H);

/// <summary>Chamadas ao JavaScript do aplicativo (wwwroot/js/app.js).</summary>
public class Js
{
    readonly IJSRuntime _js;

    public Js(IJSRuntime js) => _js = js;

    public IJSRuntime Runtime => _js;

    public ValueTask Init(object dotnetRef) => _js.InvokeVoidAsync("LS.init", dotnetRef);
    public ValueTask SetDirty(bool v) => _js.InvokeVoidAsync("LS.setDirty", v);

    public ValueTask<string> KvGet(string k) => _js.InvokeAsync<string>("LS.kvGet", k);
    public ValueTask KvSet(string k, string v) => _js.InvokeVoidAsync("LS.kvSet", k, v);
    public ValueTask KvDel(string k) => _js.InvokeVoidAsync("LS.kvDel", k);
    public ValueTask Reload() => _js.InvokeVoidAsync("LS.reload");
    public ValueTask OpenImage(string name) => _js.InvokeVoidAsync("LS.openImage", name);

    public ValueTask ImgPut(string name, byte[] bytes) => _js.InvokeVoidAsync("LS.imgPut", name, bytes);
    public ValueTask<string[]> ImgNames() => _js.InvokeAsync<string[]>("LS.imgNames");
    public ValueTask<byte[]> ImgBytes(string name) => _js.InvokeAsync<byte[]>("LS.imgBytes", name);
    public ValueTask<string> ResolveImgs(string html) => _js.InvokeAsync<string>("LS.resolveImgs", html);

    public ValueTask AttachEditor(string id, ElementReference frame, object dotnetRef) => _js.InvokeVoidAsync("LS.attachEditor", id, frame, dotnetRef);
    public ValueTask Detach(string id) => _js.InvokeVoidAsync("LS.detach", id);
    public ValueTask EdLoad(string id, int gen, string html, object opts) => _js.InvokeVoidAsync("LS.edLoad", id, gen, html, opts);
    public ValueTask EdCmd(string id, string name, object arg) => _js.InvokeVoidAsync("LS.edCmd", id, name, arg);
    public ValueTask EdFocus(string id) => _js.InvokeVoidAsync("LS.edFocus", id);
    public ValueTask EdPaste(string id, bool plain) => _js.InvokeVoidAsync("LS.edPaste", id, plain);
    public ValueTask EdInsertFiles(string id, ElementReference input) => _js.InvokeVoidAsync("LS.edInsertFiles", id, input);
    public ValueTask ViewSet(string id, ElementReference frame, object dotnetRef, string html) => _js.InvokeVoidAsync("LS.viewSet", id, frame, dotnetRef, html);

    public ValueTask PrintHtml(string html, double ml, double mt, double mr, double mb) => _js.InvokeVoidAsync("LS.printHtml", html, ml, mt, mr, mb);
    public ValueTask Download(string name, string mime, string text) => _js.InvokeVoidAsync("LS.download", name, mime, text);
    public ValueTask DownloadBytes(string name, string mime, byte[] data) => _js.InvokeVoidAsync("LS.download", name, mime, data);
    public ValueTask<bool> CopyText(string t) => _js.InvokeAsync<bool>("LS.copyText", t);
    public ValueTask OpenUrl(string u) => _js.InvokeVoidAsync("LS.openUrl", u);

    public ValueTask Focus(ElementReference el, bool select = false) => _js.InvokeVoidAsync("LS.focus", el, select);
    public ValueTask ScrollIntoView(ElementReference el) => _js.InvokeVoidAsync("LS.scrollIntoView", el);
    public ValueTask ScrollTo(ElementReference el, bool end) => _js.InvokeVoidAsync("LS.scrollTo", el, end);
    public ValueTask<Rect> RectOf(ElementReference el) => _js.InvokeAsync<Rect>("LS.rect", el);
    public ValueTask FitMenu(ElementReference el) => _js.InvokeVoidAsync("LS.fitMenu", el);
    public ValueTask Click(ElementReference el) => _js.InvokeVoidAsync("LS.click", el);
}
