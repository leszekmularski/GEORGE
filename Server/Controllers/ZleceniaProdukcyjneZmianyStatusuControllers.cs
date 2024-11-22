using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<ActionResult<List<ZleceniaProdukcyjneZmianyStatusu>>> GetZleceniaProdukcyjneStatusAsync(string rowIdZlecenia)
        {
            var result = await _context.ZleceniaProdukcyjneZmianyStatusu
                                       .Where(x => x.RowIdZlecenia == rowIdZlecenia)
                                       .OrderByDescending(e => e.DataZapisu)
                                       .ToListAsync();

            //Console.WriteLine($"RowIdZlecenia: {rowIdZlecenia}, Znalezione rekordy: {result.Count}");
            return result;
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
