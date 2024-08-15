using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server.Controllers;
using GEORGE.Shared.Models;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZlecenieWewProduktController : Controller
    {
        private readonly ApplicationDbContext context;

        public ZlecenieWewProduktController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ZleceniaProdukcyjneWew>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ZleceniaProdukcyjneWew>>> ListAsync()
        {

            try
            {

                var produkt = await context.ZleceniaProdukcyjneWew
                .Where(p => p.NazwaProduktu != null)
                .Select(p => p.NazwaProduktu)
                .Distinct()
                .ToListAsync();

                return Ok(produkt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }

        }
    }
}
