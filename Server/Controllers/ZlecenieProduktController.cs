using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server.Controllers;
using GEORGE.Shared.Models;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZlecenieProduktController : Controller
    {
        private readonly ApplicationDbContext context;

        public ZlecenieProduktController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ZleceniaProdukcyjne>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> ListAsync()
        {

            try
            {

                var produkt = await context.ZleceniaProdukcyjne
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
