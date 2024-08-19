using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using static GEORGE.Client.Pages.KartyInstrukcyjne.Karty_Instrukcyjne;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public FileUploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        if (_environment.WebRootPath != null) Console.WriteLine($"_environment.WebRootPath {_environment.WebRootPath}");

        var filePath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

        if (!System.IO.File.Exists(filePath))
        {
            return BadRequest("Plik nie istnieje!!!!");
        }

            using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var response = new ResponseModel
        {
            name = Path.GetFileName(filePath),
            status = "Success",
            url = Path.Combine("uploads", Path.GetFileName(filePath)),
            thumbUrl = null // Optional: Add logic to generate thumbnail URL if needed
        };

        return Ok(response);
    }
}
