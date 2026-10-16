using GEORGE.Shared.Models;

namespace GEORGE.Client.Services
{
    public class FileClipboardService
    {
        public List<PlikiZlecenProdukcyjnych> Files { get; private set; } = new();
        public string? SourceRowId { get; private set; }

        public bool HasData => Files.Count > 0;
        public int Count => Files.Count;

        /// <summary>Wywoływane przy każdej zmianie zawartości schowka.</summary>
        public event Action? OnChange;

        public void Set(IEnumerable<PlikiZlecenProdukcyjnych> files, string? sourceRowId)
        {
            Files = files.ToList();
            SourceRowId = sourceRowId;
            NotifyChanged();
        }

        public void Clear()
        {
            Files.Clear();
            SourceRowId = null;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            // eventy w Blazor WASM są synchroniczne – OK
            OnChange?.Invoke();
        }
    }
}