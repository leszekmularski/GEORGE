using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GEORGE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string BaseUrl = "https://github.com/leszekmularski/GEORGE/releases/latest/download/";

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var fileUrl = BaseUrl + fileName;
            var response = await client.GetAsync(fileUrl);
            Console.WriteLine(BaseUrl);  
            Console.WriteLine($"----------------------------------------->  Plik: {fileName}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                return File(content, "application/octet-stream", fileName);
            }
            else
            {
                return NotFound($"Plik {fileName} nie został znaleziony.");
            }
        }
    }
}
