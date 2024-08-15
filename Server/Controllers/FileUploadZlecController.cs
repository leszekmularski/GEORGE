using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using static GEORGE.Client.Pages.Zlecenia.Zlecenia_produkcyjne;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server;
using GEORGE.Shared.Models;
using System.Net;

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

    [HttpPost("upload/{rowIdZlecenia}/{orygFileName}")]
    public async Task<IActionResult> UploadFile(string rowIdZlecenia, string orygFileName, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Pliku nie wysłano");

        // Dodaj debugowanie
        var webRootPath = _environment.WebRootPath;
        Console.WriteLine($"WebRootPath: {webRootPath}");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads_zlecenia");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        orygFileName = WebUtility.UrlDecode(orygFileName);

        orygFileName = orygFileName.Replace("..", "."); 

        var plik = new PlikiZlecenProdukcyjnych
        {
            RowId = Guid.NewGuid().ToString(),
            RowIdZleceniaProdukcyjne = rowIdZlecenia,
            NazwaPliku = Path.GetFileName(filePath),
            OryginalnaNazwaPliku = orygFileName,
            TypPliku = file.ContentType + "/" + GetFileExtension(orygFileName),
            DataZapisu = DateTime.Now,
            KtoZapisal = User.Identity.Name, // Zakładając, że masz uwierzytelnianie użytkowników
            OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString()
        };

        _context.PlikiZlecenProdukcyjnych.Add(plik);
        await _context.SaveChangesAsync();

        var response = new ResponseModel
        {
            name = Path.GetFileName(filePath),
            status = "Success",
            url = Path.Combine("uploads_zlecenia", Path.GetFileName(filePath)),
            thumbUrl = null // Optional: Add logic to generate thumbnail URL if needed
        };

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
