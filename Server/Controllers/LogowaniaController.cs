using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogowaniaController : Controller
    {
        private readonly ApplicationDbContext context;

        public LogowaniaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Logowania>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<Logowania>>> ListAsync()
        {

            try
            {
                var logowania = await context.Logowania
               .OrderByDescending(l => l.Id)
               .Take(90)
               .ToListAsync();

                return Ok(logowania);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }

        }

        [HttpGet("{uzytkownik}")]
        public async Task<ActionResult<Logowania>> GetAsync(string uzytkownik)
        {
            var logowania = await context.Logowania
                        .Where(l => l.Uzytkownik == uzytkownik)
                        .OrderByDescending(l => l.Id)
                        .Take(15)
                        .ToListAsync();

            return Ok(logowania);
        }

        [HttpPost]
        public async Task<ActionResult<Logowania>> CreateAsync(Logowania logowania)
        {
            if (logowania.Id != 0)
            {
                return BadRequest();
            }

            context.Logowania.Add(logowania);

            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { logowania.Id }, logowania);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Logowania>> Get(int id)
        {
            var item = await context.Logowania.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

    }
}
