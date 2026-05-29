using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreWhatIf.Data;
using CoreWhatIf.Models;

namespace CoreWhatIf.Controllers;

[Authorize]
public class WhatIfController : Controller
{
    private readonly ApplicationDbContext _db;

    public WhatIfController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Dashboard()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Simulador()
    {
        var clients = await _db.Clients.Include(c => c.SatelliteProducts).ToListAsync();
        var satelliteNames = new[] { "MobileA", "MobileE", "Obras", "Fegime", "Auna" };

        var clientData = clients
            .OrderByDescending(c => c.UsersErp)
            .Select(c => new
            {
                code = c.ClientCode,
                name = c.BusinessName ?? c.Name ?? "",
                city = c.City ?? "",
                partner = c.Partner ?? "",
                usersErp = c.UsersErp,
                isDam = c.IsDam,
                arrActual = c.ArrActual,
                infraCost = c.InfraCost,
                satellites = satelliteNames.ToDictionary(
                    p => p,
                    p => c.SatelliteProducts.Where(s => s.ProductName == p).Sum(s => s.UsersProduct)
                )
            })
            .ToList();

        var satSummary = satelliteNames.Select(p => new
        {
            product = p,
            totalUsers = clients.SelectMany(c => c.SatelliteProducts).Where(s => s.ProductName == p).Sum(s => s.UsersProduct),
            totalClients = clients.Count(c => c.SatelliteProducts.Any(s => s.ProductName == p))
        }).ToList();

        var clientsWithInfra = clients.Where(c => c.InfraCost > 0 && c.UsersErp > 0).ToList();
        var infraRatePerUser = clientsWithInfra.Count > 0
            ? clientsWithInfra.Select(c => c.InfraCost / c.UsersErp).OrderBy(x => x).ElementAt(clientsWithInfra.Count / 2)
            : 450m;

        var model = new
        {
            clients = clientData,
            satellites = satSummary,
            totalClients = clients.Count,
            stdClients = clients.Count(c => !c.IsDam),
            damClients = clients.Count(c => c.IsDam),
            stdUsers = clients.Where(c => !c.IsDam).Sum(c => c.UsersErp),
            damUsers = clients.Where(c => c.IsDam).Sum(c => c.UsersErp),
            totalArrActual = clients.Sum(c => c.ArrActual),
            totalInfraCost = clients.Sum(c => c.InfraCost),
            infraMedianPerUser = Math.Round(infraRatePerUser, 0),
            clientsWithInfra = clientsWithInfra.Count
        };

        ViewBag.DataJson = System.Text.Json.JsonSerializer.Serialize(model);
        return View();
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> PlanMigracion()
    {
        var clients = await _db.Clients.Include(c => c.SatelliteProducts).ToListAsync();
        var satelliteNames = new[] { "MobileA", "MobileE", "Obras", "Fegime", "Auna" };

        var clientData = clients.Select(c =>
        {
            var satCount = c.SatelliteProducts.Count(s => satelliteNames.Contains(s.ProductName) && s.UsersProduct > 0);
            var satProducts = satelliteNames
                .Where(p => c.SatelliteProducts.Any(s => s.ProductName == p && s.UsersProduct > 0))
                .ToList();

            return new
            {
                code = c.ClientCode,
                name = c.BusinessName ?? c.Name ?? "",
                city = c.City ?? "",
                partner = c.Partner ?? "",
                usersErp = c.UsersErp,
                isDam = c.IsDam,
                arrActual = c.ArrActual,
                infraCost = c.InfraCost,
                satCount,
                satProducts,
                satellites = satelliteNames.ToDictionary(
                    p => p,
                    p => c.SatelliteProducts.Where(s => s.ProductName == p).Sum(s => s.UsersProduct)
                )
            };
        }).ToList();

        var model = new
        {
            clients = clientData,
            totalClients = clients.Count,
            stdClients = clients.Count(c => !c.IsDam),
            damClients = clients.Count(c => c.IsDam),
            stdUsers = clients.Where(c => !c.IsDam).Sum(c => c.UsersErp),
            damUsers = clients.Where(c => c.IsDam).Sum(c => c.UsersErp),
            totalArrActual = clients.Sum(c => c.ArrActual)
        };

        ViewBag.DataJson = System.Text.Json.JsonSerializer.Serialize(model);
        return View();
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ComparadorPlanes()
    {
        var clients = await _db.Clients.Include(c => c.SatelliteProducts).ToListAsync();
        var satelliteNames = new[] { "MobileA", "MobileE", "Obras", "Fegime", "Auna" };

        var clientData = clients.Select(c =>
        {
            var satCount = c.SatelliteProducts.Count(s => satelliteNames.Contains(s.ProductName) && s.UsersProduct > 0);
            return new
            {
                code = c.ClientCode,
                usersErp = c.UsersErp,
                isDam = c.IsDam,
                arrActual = c.ArrActual,
                satCount
            };
        }).ToList();

        ViewBag.DataJson = System.Text.Json.JsonSerializer.Serialize(new { clients = clientData });
        return View();
    }

}
