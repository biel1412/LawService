namespace LawService;

/// <summary>Temporizador de disparo único (reinicia a cada Start), como o QTimer singleShot.</summary>
public class Once
{
    readonly int _ms;
    readonly Func<Task> _action;
    CancellationTokenSource _cts;

    public Once(int ms, Func<Task> action)
    {
        _ms = ms;
        _action = action;
    }

    public bool Active { get; private set; }

    public void Start()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        Active = true;
        _ = Run(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        Active = false;
    }

    async Task Run(CancellationToken t)
    {
        try { await Task.Delay(_ms, t); }
        catch { return; }
        if (t.IsCancellationRequested) return;
        Active = false;
        await _action();
    }
}
