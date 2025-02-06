using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[Route("api/FileDownload")]
[ApiController]
public class FileDownloadController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FileDownloadController> _logger;

    public FileDownloadController(IHttpClientFactory httpClientFactory, ILogger<FileDownloadController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        string baseUrl = "https://github.com/leszekmularski/GEORGE/releases/latest/download/";
        string fileUrl = $"{baseUrl}{fileName}";

        using var httpClient = _httpClientFactory.CreateClient();

        try
        {
            _logger.LogInformation($"Pobieranie pliku: {fileUrl}");

            HttpResponseMessage response = await httpClient.GetAsync(fileUrl);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Błąd pobierania pliku: {fileUrl}, Status: {response.StatusCode}, Treść: {errorContent}");
                return StatusCode((int)response.StatusCode, $"Błąd pobierania pliku: {fileName}");
            }

            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation($"Plik {fileName} pobrany pomyślnie ({fileBytes.Length} bajtów).");

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Błąd HTTP podczas pobierania {fileUrl}: {ex.Message}");
            return StatusCode(500, $"Błąd HTTP: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError($"Błąd: Timeout podczas pobierania {fileUrl}");
            return StatusCode(504, "Timeout podczas pobierania pliku.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Niespodziewany błąd pobierania {fileUrl}: {ex.Message}");
            return StatusCode(500, $"Niespodziewany błąd: {ex.Message}");
        }
    }
}
