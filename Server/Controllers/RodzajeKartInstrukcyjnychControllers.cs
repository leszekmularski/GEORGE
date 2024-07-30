using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RodzajeKartInstrukcyjnychController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RodzajeKartInstrukcyjnychController> _logger;

        public RodzajeKartInstrukcyjnychController(ApplicationDbContext context, ILogger<RodzajeKartInstrukcyjnychController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<RodzajeKartInstrukcyjnych>>> GetRodzKartInstrukcyjnychAsync()
        {
            return await _context.RodzajeKartInstrukcyjnych.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddRodzKartInstrukcyjnychAsync(RodzajeKartInstrukcyjnych rodz_kart_instr)
        {
            try
            {
                _context.RodzajeKartInstrukcyjnych.Add(rodz_kart_instr);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Rodzaju Karty Instrukcyjne.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRodzKartInstrukcyjnychAsync(long id, RodzajeKartInstrukcyjnych rodz_kart_instr)
        {
            if (id != rodz_kart_instr.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(rodz_kart_instr).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RodzKartyInstrukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono Karty Instrukcyjne o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji RodzajeKartInstrukcyjnych.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRodzKartInstrukcyjnychAsync(long id)
        {
            var rodz_kart_instr = await _context.RodzajeKartInstrukcyjnych.FindAsync(id);
            if (rodz_kart_instr == null)
            {
                return NotFound("Nie znaleziono Karty Instrukcyjne o podanym ID.");
            }

            _context.RodzajeKartInstrukcyjnych.Remove(rodz_kart_instr);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania RodzajeKartInstrukcyjnych.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        private bool RodzKartyInstrukcyjneExists(long id)
        {
            return _context.RodzajeKartInstrukcyjnych.Any(e => e.Id == id);
        }

    }
}
