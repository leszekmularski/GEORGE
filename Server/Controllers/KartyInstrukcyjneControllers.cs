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
    public class KartyInstrukcyjneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KartyInstrukcyjneController> _logger;

        public KartyInstrukcyjneController(ApplicationDbContext context, ILogger<KartyInstrukcyjneController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<KartyInstrukcyjne>>> GetZleceniaProdukcyjneAsync()
        {
            return await _context.KartyInstrukcyjne.OrderBy(e => e.NumerKarty).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddKartyInstrukcyjneAsync(KartyInstrukcyjne karta)
        {
            try
            {
                _context.KartyInstrukcyjne.Add(karta);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Karty Instrukcyjne.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateZlecenieProdukcyjneAsync(long id, KartyInstrukcyjne karta)
        {
            if (id != karta.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(karta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!KartyInstrukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono Karty Instrukcyjne o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji KartyInstrukcyjne .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool KartyInstrukcyjneExists(long id)
        {
            return _context.KartyInstrukcyjne.Any(e => e.Id == id);
        }

        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKartyInstrukcyjneAsync(long id)
        {
            var karta = await _context.KartyInstrukcyjne.FindAsync(id);
            if (karta == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.KartyInstrukcyjne.Remove(karta);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine("Usunołem kartę numer" + karta.NumerKarty);

            return Ok("Karta została pomyślnie usunięta.");
        }
    }
}
