using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static GEORGE.Client.Pages.Zlecenia.Zlecenia_produkcyjne;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

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

            Console.WriteLine($"📂 Pełna ścieżka zapisu: {filePath}");

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

}
