public class DialogService
{
    public Task ShowDialogAsync(string message)
    {
        // Implementacja pokazująca okno dialogowe
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
