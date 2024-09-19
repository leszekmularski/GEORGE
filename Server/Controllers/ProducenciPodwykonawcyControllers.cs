using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducenciPodwykonawcyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProducenciPodwykonawcyController> _logger;

        public ProducenciPodwykonawcyController(ApplicationDbContext context, ILogger<ProducenciPodwykonawcyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProducenciPodwykonawcy>>> GetProducenciAsync()
        {
            return await _context.ProducenciPodwykonawcy.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddProducencihAsync(ProducenciPodwykonawcy producenci)
        {
            try
            {
                _context.ProducenciPodwykonawcy.Add(producenci);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Producenta.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProducenciAsync(long id, ProducenciPodwykonawcy producenci)
        {
            if (id != producenci.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(producenci).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProducentExists(id))
                {
                    return NotFound("Nie znaleziono Producenta o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ProducenciPodwykonawcy.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProcentaAsync(long id)
        {
            var producenci = await _context.ProducenciPodwykonawcy.FindAsync(id);
            if (producenci == null)
            {
                return NotFound("Nie znaleziono Producenta o podanym ID.");
            }

            _context.ProducenciPodwykonawcy.Remove(producenci);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania ProducenciPodwykonawcy.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        private bool ProducentExists(long id)
        {
            return _context.ProducenciPodwykonawcy.Any(e => e.Id == id);
        }

    }
}
