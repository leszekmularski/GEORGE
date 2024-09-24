using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Shared.Models;
using System.Net;
using GEORGE.Shared.ViewModels;

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

        [HttpGet("getzlecclickzew/{rowId}")]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZlecClickZew(string rowId)
        {
            try
            {
                // Używamy przekazanego parametru rowId do porównania
                var zleceniaProdukcyjne = await context.ZleceniaProdukcyjne
                    .Where(p => p.RowId == rowId) // Poprawiamy porównanie
                    .ToListAsync();

                // Sprawdzamy, czy lista jest pusta
                if (zleceniaProdukcyjne == null || !zleceniaProdukcyjne.Any())
                {
                    // Zwracamy NotFound z wiadomością
                    return NotFound("Nie znaleziono żadnych produktów dla podanego RowId.");
                }

                // Zwracamy dane w odpowiedzi Ok
                return Ok(zleceniaProdukcyjne);
            }
            catch (Exception e)
            {
                // Obsługa błędów - logujemy i zwracamy status 500
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Wystąpił błąd serwera.");
            }
        }

        [HttpGet("getzlecclickwew/{rowId}")]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZlecClickWew(string rowId)
        {
            try
            {
                // Używamy przekazanego parametru rowId do porównania
                var zleceniaProdukcyjne = await context.ZleceniaProdukcyjneWew
                    .Where(p => p.RowId == rowId) // Poprawiamy porównanie
                    .ToListAsync();

                // Sprawdzamy, czy lista jest pusta
                if (zleceniaProdukcyjne == null || !zleceniaProdukcyjne.Any())
                {
                    // Zwracamy NotFound z wiadomością
                    return NotFound("Nie znaleziono żadnych produktów dla podanego RowId.");
                }

                // Zwracamy dane w odpowiedzi Ok
                return Ok(zleceniaProdukcyjne);
            }
            catch (Exception e)
            {
                // Obsługa błędów - logujemy i zwracamy status 500
                Console.WriteLine(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Wystąpił błąd serwera.");
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
