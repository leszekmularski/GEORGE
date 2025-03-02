using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost]
    public async Task<IActionResult> SaveKonfSystem([FromBody] KonfSystem konfSystem)
    {
        if (konfSystem == null)
        {
            return BadRequest("Dane są puste!");
        }

        //_context.KonfSystem.Add(konfSystem);
        await _context.SaveChangesAsync();

        return Ok("Dane zapisane!");
    }
}
