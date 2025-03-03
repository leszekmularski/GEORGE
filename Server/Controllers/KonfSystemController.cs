using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

[ApiController]
[Route("api/konfsystem")]
public class KonfSystemController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KonfSystemController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<KonfSystem>>> GetAll()
    {
        return await _context.KonfSystem.OrderBy(e => e.Nazwa).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<KonfSystem>> GetById(int id)
    {
        var konfsystem = await _context.KonfSystem.FindAsync(id);
        if (konfsystem == null) return NotFound();
        return konfsystem;
    }

    [HttpPost]
    public async Task<IActionResult> SaveKonfSystem([FromBody] KonfSystem konfSystem)
    {
        if (konfSystem == null)
        {
            return BadRequest("Dane są puste!");
        }

        _context.KonfSystem.Add(konfSystem);
        await _context.SaveChangesAsync();

        return Ok("Dane zapisane!");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, KonfSystem konf)
    {
        if (id != konf.Id) return BadRequest();
        _context.Entry(konf).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var konf = await _context.KonfSystem.FindAsync(id);
        if (konf == null) return NotFound();
        _context.KonfSystem.Remove(konf);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
