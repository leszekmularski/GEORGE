using GEORGE.Client.Pages.KonfiguratorOkien;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using netDxf;
using System.IO;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadDxfController : ControllerBase
    {
        [HttpPost("{nazwaPliku}")]
        public IActionResult Post(string nazwaPliku, [FromBody] string svgContent)
        {
            try
            {
                var dxf = SvgToDxfConverter.ConvertSvgToDxf(svgContent);

                using var ms = new MemoryStream();
                dxf.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                var safeFileName = SanitizeFileName(nazwaPliku);
                return File(ms.ToArray(), "application/dxf", $"{safeFileName}.dxf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd przy generowaniu DXF: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(fileName.Where(ch => !invalidChars.Contains(ch)));
        }
    }
}
