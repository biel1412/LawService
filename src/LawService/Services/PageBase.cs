namespace LawService;

/// <summary>Base das abas: acesso à janela principal, ao JavaScript e ao redesenho.</summary>
public abstract class PageBase : ComponentBase, IPage
{
    [CascadingParameter] public App Win { get; set; }
    [Inject] public Js Js { get; set; }

    protected abstract string PageKey { get; }

    protected override void OnInitialized()
    {
        Win.Register(PageKey, this);
        Init();
    }

    /// <summary>Montagem inicial (equivale ao construtor da versão desktop).</summary>
    protected virtual void Init() { }

    public void Refresh() => InvokeAsync(StateHasChanged);

    /// <summary>Valor de um campo (eventos oninput/onchange).</summary>
    protected static string V(ChangeEventArgs e) => e.Value?.ToString() ?? "";

    public virtual void FocusSearch() { }
    public virtual void CtrlN() { }
    public virtual void CtrlS() { }
    public virtual void Flush() { }
    public virtual void OnShown() { }

    /// <summary>Executa algo depois do próximo desenho da tela (como o Post do desktop).</summary>
    protected void Post(Func<Task> f)
    {
        _after.Add(f);
        StateHasChanged();
    }

    readonly List<Func<Task>> _after = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_after.Count == 0) return;
        var l = _after.ToList();
        _after.Clear();
        foreach (var f in l) await f();
    }
}
