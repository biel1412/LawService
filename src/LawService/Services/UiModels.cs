using Microsoft.AspNetCore.Components.Rendering;

namespace LawService;

/// <summary>Interface comum das abas.</summary>
public interface IPage
{
    void FocusSearch();
    void CtrlN();
    void CtrlS() { }
    void Flush() { }
    void OnShown() { }
}

/// <summary>Item de menu (menu de contexto ou menu de botão).</summary>
public class MItem
{
    public string Icon, Text, IconColor;
    public Func<Task> Action;
    public bool Enabled = true, Separator;
    public List<MItem> Children;
    public RenderFragment IconFragment;
    /// <summary>Conteúdo livre (paletas de cor).</summary>
    public RenderFragment<Action> Content;

    public static MItem Sep() => new() { Separator = true };

    public static MItem Of(string icon, string text, Action a, string color = null, bool enabled = true) => new()
    {
        Icon = icon, Text = text, IconColor = color, Enabled = enabled, Action = a == null ? null : () => { a(); return Task.CompletedTask; },
    };

    public static MItem Async(string icon, string text, Func<Task> a, string color = null, bool enabled = true) => new()
    {
        Icon = icon, Text = text, IconColor = color, Enabled = enabled, Action = a,
    };

    public static MItem Sub(string icon, string text, List<MItem> children) => new() { Icon = icon, Text = text, Children = children };
}

public class MenuState
{
    public List<MItem> Items;
    public double X, Y;
    public int Key;
}

/// <summary>Uma caixa de diálogo aberta.</summary>
public class DialogState
{
    public string Title, Text, Icon = "help-circle-outline", Tone, Ok = "Confirmar", Cancel = "Cancelar", Placeholder = "";
    public string Value;
    public bool Multiline, Danger, IsColor;
    public RenderFragment Body;
    public int Width = 460;
    public TaskCompletionSource<string> Tcs = new();
    public ElementReference InputRef;
    public bool Focused;
    public bool Wide;
}

/// <summary>Paleta de cores para menus e botões.</summary>
public class ColorSpec
{
    public IList<string> Colors;
    public int Cols = 10;
    public string Title = "", NoneLabel = "Sem cor", Shape = "circle", RecentKey, Current;
    public bool AllowNone;
    public Action<string> Picked;
}
