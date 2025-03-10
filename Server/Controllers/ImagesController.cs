using Microsoft.AspNetCore.Mvc;
using GEORGE.Shared.ViewModels;
using System.Runtime;

namespace GEORGE.Server.Controllers 
{ 
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ImagesController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            string filePath = Path.Combine(_env.WebRootPath, "tekstury", imageName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Plik {imageName} nie istnieje.");
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
            return File(imageBytes, "image/jpeg"); // Możesz zmienić na "image/png" jeśli używasz PNG
        }
    }
}