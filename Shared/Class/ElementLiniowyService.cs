using Microsoft.Extensions.Logging;

namespace GEORGE.Shared.Class
{
    public class ElementLiniowyService
    {
        private readonly ILogger<ElementLiniowyService> _logger;

        public ElementLiniowyService(ILogger<ElementLiniowyService> logger)
        {
            _logger = logger;
        }

        public void PrzetworzKandydatow(List<string> kandydaci)
        {
            _logger.LogDebug("🔷 ElementLiniowy: znaleziono {Count} wpisów z Przesunieciami", kandydaci.Count);

            foreach (var k in kandydaci)
            {
                _logger.LogTrace("Przetwarzam kandydata: {Kandydat}", k);
            }

            if (kandydaci.Count == 0)
            {
                _logger.LogWarning("Brak kandydatów do przetworzenia");
                return;
            }

            try
            {
                // ... Twoja logika ...
                _logger.LogInformation("Przetworzono {Count} kandydatów", kandydaci.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas przetwarzania kandydatów");
            }
        }
    }
}
