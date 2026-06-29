using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreWhatIf.Data;
using CoreWhatIf.Models;

namespace CoreWhatIf.Controllers;

[Authorize]
[Route("api")]
[ApiController]
public class ApiDataController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApiDataController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetEmail() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email")
        ?? "";

    // ── Simulaciones ─────────────────────────────────────────────────────────

    [HttpGet("simulations")]
    public async Task<IActionResult> GetSimulations()
    {
        var sims = await _db.SavedSimulations
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new { s.Id, s.Title, s.UserEmail, s.StateJson, s.CreatedAt, s.UpdatedAt })
            .ToListAsync();
        return Ok(sims);
    }

    [HttpPost("simulations")]
    public async Task<IActionResult> CreateSimulation([FromBody] SavePayload payload)
    {
        var sim = new SavedSimulation
        {
            UserEmail = GetEmail(),
            Title = payload.Title ?? "Sin título",
            StateJson = payload.StateJson ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.SavedSimulations.Add(sim);
        await _db.SaveChangesAsync();
        return Ok(new { sim.Id, sim.Title, sim.StateJson, sim.CreatedAt, sim.UpdatedAt });
    }

    [HttpPut("simulations/{id}")]
    public async Task<IActionResult> UpdateSimulation(int id, [FromBody] SavePayload payload)
    {
        var email = GetEmail();
        var sim = await _db.SavedSimulations.FirstOrDefaultAsync(s => s.Id == id && s.UserEmail == email);
        if (sim == null) return NotFound();

        if (payload.Title != null) sim.Title = payload.Title;
        if (payload.StateJson != null) sim.StateJson = payload.StateJson;
        sim.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { sim.Id, sim.Title, sim.StateJson, sim.CreatedAt, sim.UpdatedAt });
    }

    [HttpDelete("simulations/{id}")]
    public async Task<IActionResult> DeleteSimulation(int id)
    {
        var email = GetEmail();
        var sim = await _db.SavedSimulations.FirstOrDefaultAsync(s => s.Id == id && s.UserEmail == email);
        if (sim == null) return NotFound();

        _db.SavedSimulations.Remove(sim);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // ── Planes de migración ──────────────────────────────────────────────────

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _db.SavedPlans
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new { p.Id, p.Title, p.UserEmail, p.StateJson, p.CreatedAt, p.UpdatedAt })
            .ToListAsync();
        return Ok(plans);
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] SavePayload payload)
    {
        var plan = new SavedPlan
        {
            UserEmail = GetEmail(),
            Title = payload.Title ?? "Sin título",
            StateJson = payload.StateJson ?? "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.SavedPlans.Add(plan);
        await _db.SaveChangesAsync();
        return Ok(new { plan.Id, plan.Title, plan.StateJson, plan.CreatedAt, plan.UpdatedAt });
    }

    [HttpPut("plans/{id}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] SavePayload payload)
    {
        var email = GetEmail();
        var plan = await _db.SavedPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserEmail == email);
        if (plan == null) return NotFound();

        if (payload.Title != null) plan.Title = payload.Title;
        if (payload.StateJson != null) plan.StateJson = payload.StateJson;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { plan.Id, plan.Title, plan.StateJson, plan.CreatedAt, plan.UpdatedAt });
    }

    [HttpDelete("plans/{id}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var email = GetEmail();
        var plan = await _db.SavedPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserEmail == email);
        if (plan == null) return NotFound();

        _db.SavedPlans.Remove(plan);
        await _db.SaveChangesAsync();
        return Ok();
    }

    public class SavePayload
    {
        public string? Title { get; set; }
        public string? StateJson { get; set; }
    }
}
