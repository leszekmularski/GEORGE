using Microsoft.AspNetCore.Mvc;
using GEORGE.Shared.ViewModels;
using System.Runtime;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JSettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            var settings = _configuration.GetSection("FileSettings").Get<JSettings>();
            if (settings == null)
            {
                return NotFound();
            }
            return Ok(settings);
        }
    }

}
