using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using static GEORGE.Client.Pages.Zlecenia.Zlecenia_produkcyjne;

[ApiController]
[Route("api/[controller]")]
public class FileUploadZlecController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public FileUploadZlecController(IWebHostEnvironment environment, ApplicationDbContext context)
    {
        _environment = environment;
        _context = context;
    }

    [Authorize]
    [HttpPost("upload/{rowIdZlecenia}/{orygFileName}/{czyWidocznyDlaWszystkich}")]
    public async Task<IActionResult> UploadFile(string rowIdZlecenia, string orygFileName, bool czyWidocznyDlaWszystkich, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("❌ Pliku nie wysłano lub plik jest pusty.");

            var webRootPath = _environment.WebRootPath;
            Console.WriteLine($"WebRootPath: {webRootPath}");

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                return BadRequest("❌ Brak dostępu do katalogu - WebRootPath/uploads_zlecenia");
            }

            var uploadsFolder = Path.Combine(webRootPath, "uploads_zlecenia");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                Console.WriteLine("📂 Utworzono katalog uploads_zlecenia");
            }

            var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            var filePath = Path.Combine(uploadsFolder, newFileName);

            Console.WriteLine($"📂 Zapis: {DateTime.Now} Pełna ścieżka zapisu: {filePath}");

            // Zapisywanie pliku na dysku
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Dekodowanie i sanityzacja nazwy pliku
            orygFileName = WebUtility.UrlDecode(orygFileName ?? string.Empty);
            orygFileName = orygFileName.Replace("..", ".");

            var plik = new PlikiZlecenProdukcyjnych
            {
                RowId = Guid.NewGuid().ToString(),
                RowIdZleceniaProdukcyjne = rowIdZlecenia,
                NazwaPliku = newFileName,
                OryginalnaNazwaPliku = orygFileName,
                TypPliku = file.ContentType + "/" + GetFileExtension(orygFileName),
                DataZapisu = DateTime.Now,
                KtoZapisal =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst(ClaimTypes.Name)?.Value ??
                User.FindFirst("preferred_username")?.Value ??
                "Anonim",
                WidocznyDlaWszystkich = czyWidocznyDlaWszystkich,
                OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString()
            };

            _context.PlikiZlecenProdukcyjnych.Add(plik);
            await _context.SaveChangesAsync();

            var response = new ResponseModel
            {
                name = newFileName,
                status = "Success",
                url = Path.Combine("uploads_zlecenia", newFileName),
                thumbUrl = null
            };

            Console.WriteLine($"✅ Plik zapisany: {response.name}, URL: {response.url}");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"❌ Błąd dostępu do pliku: {ex}");
            return StatusCode(StatusCodes.Status403Forbidden, "Brak uprawnień do zapisu pliku.");
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"❌ Błąd IO: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Błąd przy zapisie pliku.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"❌ Nieoczekiwany błąd: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Wystąpił nieoczekiwany błąd podczas przesyłania pliku.");
        }
    }


    [HttpPost("upload/{rowIdZlecenia}/{orygFileName}/{staraNazwaPliku}/{id}")]
    public async Task<IActionResult> ReplaceUploadFile(string rowIdZlecenia, string orygFileName, string staraNazwaPliku, long id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Pliku nie wysłano");

        // Dodaj debugowanie
        var webRootPath = _environment.WebRootPath;
        Console.WriteLine($"WebRootPath: {webRootPath}");

        if (webRootPath == null)
        {
            return BadRequest("Pliku nie wysłano. Brak dostępu do katalogu - WebRootPath/uploads_zlecenia");
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads_zlecenia");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, staraNazwaPliku);

        if (System.IO.File.Exists(filePath))
        {
            Console.WriteLine("******************************* KASUJE ISTNIEJACY PLIK ************************");
            System.IO.File.Delete(filePath);
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var response = new ResponseModel
        {
            name = Path.GetFileName(filePath),
            status = "Success",
            url = Path.Combine("uploads_zlecenia", Path.GetFileName(filePath)),
            thumbUrl = null // Optional: Add logic to generate thumbnail URL if needed
        };

        orygFileName = WebUtility.UrlDecode(orygFileName ?? string.Empty);
        orygFileName = orygFileName.Replace("..", ".");

        await _context.ZmienNazwePliku(id, orygFileName);

        Console.WriteLine($"Plik zapisany: {response.name}, URL: {response.url}");

        return Ok(response);
    }

    [HttpGet("files/{rowIdZlecenia}")]
    public async Task<IActionResult> GetFiles(string rowIdZlecenia)
    {
        var files = await _context.PlikiZlecenProdukcyjnych
                                  .Where(f => f.RowIdZleceniaProdukcyjne == rowIdZlecenia)
                                  .ToListAsync();

        //if (files == null || !files.Any())
        //    return NotFound();

        return Ok(files);
    }

    [HttpGet("all-files-sat")]
    public async Task<IActionResult> GetAllFilesSat()
    {
        try
        {
            var files = await _context.PlikiZlecenProdukcyjnych
                .Where(f => f.TypPliku != null &&
                            EF.Functions.Like(f.TypPliku, "%SAT%"))
                .OrderBy(x => x.OryginalnaNazwaPliku)
                .ToListAsync();

            return Ok(files);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    [HttpGet("all-files-pdf")]
    public async Task<IActionResult> GetAllFilesPdf()
    {
        try
        {
            var files = await _context.PlikiZlecenProdukcyjnych
                .Where(f => f.TypPliku != null &&
                            EF.Functions.Like(f.TypPliku, "%PDF%"))
                .OrderBy(x => x.OryginalnaNazwaPliku)
                .ToListAsync();

            return Ok(files);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }


    [HttpPost("zmien-uwage")]
    public async Task<IActionResult> ZmienUwage(long id, [FromBody] string uwaga)
    {
        var result = await _context.ZmienUwage(id, uwaga);
        if (result)
        {
            return Ok("Uwagi zaktualizowane pomyślnie.");
        }
        else
        {
            return NotFound("Nie znaleziono pliku o podanym ID.");
        }
    }

    [HttpPost("zmien-widocznosc")]
    public async Task<IActionResult> ZmienWidocznosc(long id, [FromBody] bool widocznosc)
    {
        var result = await _context.ZmienWidocznosc(id, widocznosc);
        if (result)
        {
            return Ok("Uwagi zaktualizowane pomyślnie.");
        }
        else
        {
            return NotFound("Nie znaleziono pliku o podanym ID.");
        }
    }

    [HttpPost("zmien-liczbe-pobran")]
    public async Task<IActionResult> ZwiekszLicznikPobranPliku(long id)
    {
        var result = await _context.ZwiekszLicznikPobranPliku(id);
        if (result)
        {
            return Ok("Uwagi zaktualizowane pomyślnie.");
        }
        else
        {
            return NotFound("Nie znaleziono pliku o podanym ID.");
        }
    }


    public string GetFileExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex < 0)
        {
            return string.Empty;
        }

        return fileName.Substring(lastDotIndex + 1);
    }


    public class CopyFilesRequest
    {
        public List<long> Ids { get; set; } = new();
    }

    [HttpPost("copy-files/{targetRowIdZlecenia}/{user}")]
    public async Task<IActionResult> CopyFiles(string targetRowIdZlecenia, string user, [FromBody] CopyFilesRequest req)
    {
        if (req?.Ids == null || req.Ids.Count == 0)
            return BadRequest("Nie wybrano plików do skopiowania.");

        if (string.IsNullOrWhiteSpace(targetRowIdZlecenia))
            return BadRequest("Brak identyfikatora zlecenia docelowego.");

        // ASP.NET Core automatycznie DEKODUJE segment {user} z URL-a,
        // więc poniżej masz już "Jan Kowalski", a nie "Jan%20Kowalski".
        user = string.IsNullOrWhiteSpace(user) ? "Anonim" : user.Trim();

        // Zabezpieczenie przed długimi śmieciami w URL
        if (user.Length > 100)
            user = user.Substring(0, 100);

        var sourceFiles = await _context.PlikiZlecenProdukcyjnych
            .Where(f => req.Ids.Contains(f.Id))
            .ToListAsync();

        if (sourceFiles.Count == 0)
            return NotFound("Nie znaleziono plików źródłowych.");

        var skopiowane = new List<PlikiZlecenProdukcyjnych>();

        foreach (var src in sourceFiles)
        {
            var istnieje = await _context.PlikiZlecenProdukcyjnych.AnyAsync(f =>
                f.RowIdZleceniaProdukcyjne == targetRowIdZlecenia &&
                f.NazwaPliku == src.NazwaPliku);

            if (istnieje) continue;

            var nowy = new PlikiZlecenProdukcyjnych
            {
                RowId = Guid.NewGuid().ToString(),
                RowIdZleceniaProdukcyjne = targetRowIdZlecenia,
                NazwaPliku = src.NazwaPliku,
                OryginalnaNazwaPliku = src.OryginalnaNazwaPliku,
                TypPliku = src.TypPliku,
                DataZapisu = DateTime.Now,
                KtoZapisal = user,   // ← teraz z URL-a, nie z claims
                WidocznyDlaWszystkich = src.WidocznyDlaWszystkich,
                Uwagi = src.Uwagi,
                IloscPobranPliku = 0,
                OstatniaZmiana = $"Kopia z pliku Id={src.Id} ({DateTime.Now:yyyy-MM-dd HH:mm})"
            };

            _context.PlikiZlecenProdukcyjnych.Add(nowy);
            skopiowane.Add(nowy);
        }

        await _context.SaveChangesAsync();
        return Ok(skopiowane);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var Pracownicy = await _context.PlikiZlecenProdukcyjnych.SingleOrDefaultAsync(b => b.Id.Equals(id));

        if (Pracownicy == null)
        {
            return NotFound();
        }

        _context.PlikiZlecenProdukcyjnych.Remove(Pracownicy);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("delete-file/{fileName}")]
    public async Task<IActionResult> DeleteFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Nie podano nazwy pliku.");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads_zlecenia");
        var filePath = Path.Combine(uploadsFolder, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("Plik nie istnieje.");
        }

        try
        {
            System.IO.File.Delete(filePath);

            // dodatkowo usuń rekord z bazy, jeśli taki istnieje
            var plik = await _context.PlikiZlecenProdukcyjnych
                                     .FirstOrDefaultAsync(p => p.NazwaPliku == fileName);
            if (plik != null)
            {
                _context.PlikiZlecenProdukcyjnych.Remove(plik);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Błąd podczas usuwania pliku {fileName}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Błąd podczas usuwania pliku.");
        }
    }

    /// <summary>
    /// Usuwa TYLKO wpis z bazy danych. Plik fizyczny na dysku pozostaje.
    /// Używane, gdy ten sam plik jest współdzielony między zleceniami
    /// (np. po operacji „Kopiuj / Wklej").
    /// </summary>
    [HttpDelete("delete-record/{id:long}")]
    public async Task<IActionResult> DeleteRecordAsync(long id)
    {
        var plik = await _context.PlikiZlecenProdukcyjnych
                                 .FirstOrDefaultAsync(p => p.Id == id);

        if (plik is null)
            return NotFound($"Nie znaleziono wpisu o Id={id}.");

        _context.PlikiZlecenProdukcyjnych.Remove(plik);
        await _context.SaveChangesAsync();

        Console.WriteLine($"🗑️ Usunięto wpis Id={id} (NazwaPliku={plik.NazwaPliku}). Plik na dysku pozostał.");
        return NoContent();
    }

}
