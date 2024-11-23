using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Shared.Models;
using System.Net;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZleceniaProdukcyjneZmianyStatusuController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaProdukcyjneZmianyStatusuController> _logger;

        public ZleceniaProdukcyjneZmianyStatusuController(ApplicationDbContext context, ILogger<ZleceniaProdukcyjneZmianyStatusuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{rowIdZlecenia}")]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZleceniaProdukcyjneStatusAsync(string rowIdZlecenia)
        {
            if (string.IsNullOrWhiteSpace(rowIdZlecenia))
            {
                return BadRequest("RowIdZlecenia nie może być pusty.");
            }

            try
            {
                var result = await _context.ZleceniaProdukcyjneZmianyStatusu
                                           .Where(x => x.RowIdZlecenia == rowIdZlecenia)
                                           .OrderByDescending(e => e.DataZapisu)
                                           .ToListAsync();

                //if (!result.Any())
                //{
                //    return NotFound($"Brak wyników dla rowIdZlecenia: {rowIdZlecenia}.");
                //}

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił błąd podczas pobierania statusów zlecenia.");
                return StatusCode(500, "Wystąpił błąd serwera.");
            }
        }




        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var zlecenie = await _context.ZleceniaProdukcyjneZmianyStatusu.FindAsync(id);
            if (zlecenie == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.ZleceniaProdukcyjneZmianyStatusu.Remove(zlecenie);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine(zlecenie.NumerZamowienia);

            return Ok("Zlecenie zostało pomyślnie usunięte.");
        }

    }

}
