using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaProdukcyjneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaProdukcyjneController> _logger;

        public ZleceniaProdukcyjneController(ApplicationDbContext context, ILogger<ZleceniaProdukcyjneController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZleceniaProdukcyjneAsync()
        {
            return await _context.ZleceniaProdukcyjne.OrderByDescending(e => e.Id).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddZlecenieProdukcyjneAsync(ZleceniaProdukcyjne zlecenie)
        {
            try
            {
                _context.ZleceniaProdukcyjne.Add(zlecenie);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania zlecenia produkcyjnego.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateZlecenieProdukcyjneAsync(long id, ZleceniaProdukcyjne zlecenie)
        {
            if (id != zlecenie.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(zlecenie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ZlecenieProdukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono zlecenia o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji zlecenia produkcyjnego.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var zlecenie = await _context.ZleceniaProdukcyjne.FindAsync(id);
            if (zlecenie == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.ZleceniaProdukcyjne.Remove(zlecenie);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine(zlecenie.NumerZamowienia);

            return Ok("Zlecenie zostało pomyślnie usunięte.");
        }

        private bool ZlecenieProdukcyjneExists(long id)
        {
            return _context.ZleceniaProdukcyjne.Any(e => e.Id == id);
        }

        [HttpPost("zmien-date-produkcji")]
        public async Task<IActionResult> ZmienDateProdukcji(string rowId, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateProdukcji(rowId, nowaDataProdukcji);
            if (result)
            {
                return Ok("Uwagi zaktualizowane pomyślnie.");
            }
            else
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }
        }

        [HttpPost("zmien-date-rozpoczecia-produkcji")]
        public async Task<IActionResult> ZmienDateRozpoczeciaProdukcji(string rowId, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateRozpoczeciaProdukcji(rowId, nowaDataProdukcji);
            if (result)
            {
                return Ok("Uwagi zaktualizowane pomyślnie.");
            }
            else
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }
        }
        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne
    }

}
