public class AppState
{
    public event Action? OnChange;

    private string? _someData;
    public string? SomeData
    {
        get => _someData;
        set
        {
            _someData = value;
            NotifyStateChanged();
        }
    }
    private void NotifyStateChanged() => OnChange?.Invoke();
}
