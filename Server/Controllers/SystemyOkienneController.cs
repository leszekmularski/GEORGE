using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/systemy-okienne")]
[ApiController]
public class SystemyOkienneController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SystemyOkienneController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<SystemyOkienne>>> GetAll()
    {
        return await _context.SystemyOkienne.OrderBy(e=>e.Nazwa_Systemu).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SystemyOkienne>> GetById(int id)
    {
        var system = await _context.SystemyOkienne.FindAsync(id);
        if (system == null) return NotFound();
        return system;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SystemyOkienne system)
    {
        _context.SystemyOkienne.Add(system);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = system.Id }, system);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, SystemyOkienne system)
    {
        if (id != system.Id) return BadRequest();
        _context.Entry(system).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var system = await _context.SystemyOkienne.FindAsync(id);
        if (system == null) return NotFound();
        _context.SystemyOkienne.Remove(system);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
