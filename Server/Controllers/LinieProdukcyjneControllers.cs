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
    public class LinieProdukcyjneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LinieProdukcyjneController> _logger;

        public LinieProdukcyjneController(ApplicationDbContext context, ILogger<LinieProdukcyjneController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<LinieProdukcyjne>>> GetLinieProdukcyjneAsync()
        {
            return await _context.LinieProdukcyjne.OrderBy(e => e.IdLiniiProdukcyjnej).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddLinieProdukcyjneAsync(LinieProdukcyjne linia)
        {
            try
            {
                _context.LinieProdukcyjne.Add(linia);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Linie.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLinieProdukcyjneAsync(long id, LinieProdukcyjne lp)
        {
            if (id != lp.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(lp).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!LinieProdukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono Linii o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji LinieProdukcyjne .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool LinieProdukcyjneExists(long id)
        {
            return _context.LinieProdukcyjne.Any(e => e.Id == id);
        }

        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLinieProdukcyjneAsync(long id)
        {
            var lp = await _context.LinieProdukcyjne.FindAsync(id);
            if (lp == null)
            {
                return NotFound("Nie znaleziono linii o podanym ID.");
            }

            _context.LinieProdukcyjne.Remove(lp);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine("Usunołem linię #numer" + lp.IdLiniiProdukcyjnej);

            return Ok("linia usunięta.");
        }
    }
}
