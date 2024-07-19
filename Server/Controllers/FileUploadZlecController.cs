using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using static GEORGE.Client.Pages.Zlecenia.Zlecenia_produkcyjne;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server;
using GEORGE.Shared.Models;

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

    [HttpPost("upload/{rowIdZlecenia}")]
    public async Task<IActionResult> UploadFile(string rowIdZlecenia, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Pliku nie wysłano");

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

        var plik = new PlikiZlecenProdukcyjnych
        {
            RowId = Guid.NewGuid().ToString(),
            RowIdZleceniaProdukcyjne = rowIdZlecenia,
            NazwaPliku = Path.GetFileName(filePath),
            TypPliku = file.ContentType,
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

        if (files == null || !files.Any())
            return NotFound();

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

}
