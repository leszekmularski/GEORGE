using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BazaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BazaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("CheckDb")]
        public async Task<IActionResult> CheckDb()
        {
            try
            {
                var dbName = _context.Database.GetDbConnection().Database;
                var sql = $"DBCC CHECKDB([{dbName}]) WITH NO_INFOMSGS, ALL_ERRORMSGS";

                // uruchomienie komendy
                await _context.Database.ExecuteSqlRawAsync(sql);

                return Ok($"DBCC CHECKDB wykonane dla bazy: {dbName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd podczas sprawdzania bazy: {ex.Message}");
            }
        }
    }


}
