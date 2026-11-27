using GEORGE.Shared.Models;
using System.Net.Http.Json;

namespace GEORGE.Client.Pages.Szyby
{
    public class PakietSzybowyService
    {
        private const string API = "api/pakiety-szybowe";

        private readonly HttpClient _http;
        private readonly List<PakietSzybowy> _cache = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _loaded;

        public PakietSzybowyService(HttpClient http) => _http = http;

        public event Action? OnChange;

        public async Task EnsureLoadedAsync()
        {
            if (_loaded) return;
            await _lock.WaitAsync();
            try
            {
                if (_loaded) return;
                var list = await _http.GetFromJsonAsync<List<PakietSzybowy>>(API);
                _cache.Clear();
                if (list != null) _cache.AddRange(list);
                _loaded = true;
                OnChange?.Invoke();
            }
            finally { _lock.Release(); }
        }

        public async Task ReloadAsync()
        {
            _loaded = false;
            await EnsureLoadedAsync();
        }

        public IReadOnlyList<PakietSzybowy> GetAll()
            => _cache.OrderBy(p => p.Nazwa).ToList();

        public IReadOnlyList<PakietSzybowy> GetAktywne()
            => _cache.Where(p => p.Aktywny).OrderBy(p => p.Nazwa).ToList();

        public IReadOnlyList<PakietSzybowy> GetAktywneTylkoSystemowe(Guid idSystem)
            => _cache.Where(p => p.Aktywny && p.SystemOkienId == idSystem).OrderBy(p => p.Nazwa).ToList();

        public PakietSzybowy? GetById(Guid id)
            => _cache.FirstOrDefault(p => p.Id == id);

        public async Task<PakietSzybowy> SaveAsync(PakietSzybowy pakiet)
        {
            if (pakiet == null) throw new ArgumentNullException(nameof(pakiet));

            var exists = _cache.Any(p => p.Id == pakiet.Id);
            HttpResponseMessage resp = exists
                ? await _http.PutAsJsonAsync($"{API}/{pakiet.Id}", pakiet)
                : await _http.PostAsJsonAsync(API, pakiet);

            resp.EnsureSuccessStatusCode();
            var saved = await resp.Content.ReadFromJsonAsync<PakietSzybowy>();
            if (saved != null)
            {
                var idx = _cache.FindIndex(p => p.Id == saved.Id);
                if (idx >= 0) _cache[idx] = saved;
                else _cache.Add(saved);
                OnChange?.Invoke();
            }
            return saved ?? pakiet;
        }

        public async Task DeleteAsync(Guid id)
        {
            var resp = await _http.DeleteAsync($"{API}/{id}");
            resp.EnsureSuccessStatusCode();
            _cache.RemoveAll(p => p.Id == id);
            OnChange?.Invoke();
        }

        public async Task ResetAsync()
        {
            var resp = await _http.PostAsync($"{API}/RESET", null);
            resp.EnsureSuccessStatusCode();
            await ReloadAsync();
        }

        public async Task ImportToServerAsync(string json, bool replaceAll = true)
        {
            var imported = System.Text.Json.JsonSerializer.Deserialize<List<PakietSzybowy>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (imported == null) return;

            if (replaceAll)
            {
                foreach (var p in _cache.ToList())
                    await DeleteAsync(p.Id);
            }

            foreach (var p in imported)
            {
                p.Id = Guid.NewGuid(); // unikamy kolizji Id przy imporcie
                foreach (var w in p.Warstwy) w.Id = Guid.NewGuid();
                await SaveAsync(p);
            }
        }

        public string ExportJson()
            => System.Text.Json.JsonSerializer.Serialize(_cache,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}