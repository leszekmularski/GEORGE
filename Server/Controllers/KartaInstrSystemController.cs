using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server.Controllers;
using GEORGE.Shared.Models;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KartaInstrSystemController : Controller
    {
        private readonly ApplicationDbContext context;

        public KartaInstrSystemController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<KartyInstrukcyjne>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<KartyInstrukcyjne>>> ListAsync()
        {

            try
            {

                var produkt = await context.KartyInstrukcyjne
                .Where(p => p.KodProduktu != null)
                .Select(p => p.KodProduktu)
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
