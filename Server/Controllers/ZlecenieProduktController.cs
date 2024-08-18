using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Shared.Models;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZlecenieProduktController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ZlecenieProduktController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<string>>> ListAsync()
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

        [HttpGet("bez-linii")]
        [ProducesResponseType(typeof(IEnumerable<ZleceniaProdukcyjne>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> ListWithoutLinesAsync()
        {
            try
            {
                // Pobierz wszystkie ZleceniaProdukcyjne, które nie mają powiązania w ZleceniaNaLinii
                var zleceniaBezLinii = await context.ZleceniaProdukcyjne
                    .Where(zp => !context.ZleceniaNaLinii
                        .Any(znl => znl.RowIdZleceniaProdukcyjne == zp.RowId))
                    .ToListAsync();

                return Ok(zleceniaBezLinii);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

        [HttpGet("z-linia")]
        [ProducesResponseType(typeof(IEnumerable<ZleceniaProdukcyjne>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> ListWithLinesAsync()
        {
            try
            {
                // Pobierz wszystkie ZleceniaProdukcyjne, które mają powiązanie w ZleceniaNaLinii
                var zleceniaZLinia = await context.ZleceniaProdukcyjne
                    .Where(zp => context.ZleceniaNaLinii
                        .Any(znl => znl.RowIdZleceniaProdukcyjne == zp.RowId))
                    .ToListAsync();

                return Ok(zleceniaZLinia);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }

    }
}
