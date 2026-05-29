using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CoreWhatIf.Models;

namespace CoreWhatIf.Data;

public static class DbSeeder
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task SeedDataAsync(
        ApplicationDbContext context,
        bool enableSeed)
    {
        if (!enableSeed) return;

        await SeedRoadmapFeaturesAsync(context);
        await SeedFromJsonAsync(context);
    }

    private static async Task SeedRoadmapFeaturesAsync(ApplicationDbContext context)
    {
        if (await context.RoadmapFeatures.AnyAsync()) return;

        context.RoadmapFeatures.AddRange(
            new RoadmapFeature { FeatureName = "Integración eDam / GO!Catalog", PlannedVersion = "CORE 1", PlannedDate = new DateOnly(2027, 3, 1), IsReady = true, EffortWeeks = 0, Notes = "Incluida en funcionalidad base" },
            new RoadmapFeature { FeatureName = "SII / Verifactu / Batuz", PlannedVersion = "CORE 1", PlannedDate = new DateOnly(2027, 3, 1), IsReady = true, EffortWeeks = 0, Notes = "Facturación electrónica incluida" },
            new RoadmapFeature { FeatureName = "Integración CLC / PTL", PlannedVersion = "CORE 1", PlannedDate = new DateOnly(2027, 3, 1), IsReady = true, EffortWeeks = 0, Notes = "Incluida en Core 1" },
            new RoadmapFeature { FeatureName = "Integración Fegime", SatelliteProduct = "Fegime", PlannedVersion = "CORE 1", PlannedDate = new DateOnly(2027, 3, 1), IsReady = false, EffortWeeks = 8, Notes = "Bancos de datos Fegime - pendiente integración final" },
            new RoadmapFeature { FeatureName = "Integración Auna", SatelliteProduct = "Auna", PlannedVersion = "CORE 1", PlannedDate = new DateOnly(2027, 3, 1), IsReady = false, EffortWeeks = 6, Notes = "Bancos de datos Auna - pendiente integración final" },
            new RoadmapFeature { FeatureName = "Movilidad de Almacén (MobileA)", SatelliteProduct = "MobileA", PlannedVersion = "CORE 2", PlannedDate = new DateOnly(2028, 3, 1), IsReady = false, EffortWeeks = 24, Notes = "Requiere app PWA + APIs específicas" },
            new RoadmapFeature { FeatureName = "Movilidad de Entregas (MobileE)", SatelliteProduct = "MobileE", PlannedVersion = "CORE 2", PlannedDate = new DateOnly(2028, 3, 1), IsReady = false, EffortWeeks = 20, Notes = "Requiere app PWA + geolocalización" },
            new RoadmapFeature { FeatureName = "Gestión de Obras", SatelliteProduct = "Obras", PlannedVersion = "CORE 2", PlannedDate = new DateOnly(2028, 3, 1), IsReady = false, EffortWeeks = 16, Notes = "Módulo de taller y obras" },
            new RoadmapFeature { FeatureName = "Integración SGA / IntraDAM", PlannedVersion = "CORE 2", PlannedDate = new DateOnly(2028, 3, 1), IsReady = false, EffortWeeks = 12, Notes = "Integración almacén avanzada" },
            new RoadmapFeature { FeatureName = "Integración GSGESTIÓN", PlannedVersion = "CORE 2", PlannedDate = new DateOnly(2028, 3, 1), IsReady = false, EffortWeeks = 8, Notes = "Sistema de gestión externo" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedFromJsonAsync(ApplicationDbContext context)
    {
        var seedFile = Path.Combine(AppContext.BaseDirectory, "Data", "seed-data.json");
        if (!File.Exists(seedFile))
            seedFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "seed-data.json");
        if (!File.Exists(seedFile)) return;

        var json = await File.ReadAllTextAsync(seedFile);
        var data = JsonSerializer.Deserialize<SeedData>(json, JsonOpts);
        if (data == null) return;

        if (data.Clients != null && !await context.Clients.AnyAsync())
        {
            foreach (var c in data.Clients)
            {
                context.Clients.Add(new Client
                {
                    ClientCode = c.ClientCode,
                    BusinessName = c.BusinessName,
                    Name = c.Name,
                    Cif = c.Cif,
                    City = c.City,
                    PostalCode = c.PostalCode,
                    Activity = c.Activity,
                    Partner = c.Partner,
                    Erp = c.Erp,
                    UsersErp = c.UsersErp,
                    HasMobileA = c.HasMobileA,
                    HasMobileE = c.HasMobileE,
                    HasAuna = c.HasAuna,
                    HasFegime = c.HasFegime,
                    HasObras = c.HasObras,
                    ArrActual = c.ArrActual,
                    InfraCost = c.InfraCost,
                    ArrCore = c.ArrCore,
                    DeltaArr = c.DeltaArr,
                    IsDam = c.IsDam,
                    MonthlyRate = c.MonthlyRate,
                    DamRate = c.DamRate,
                });
            }
            await context.SaveChangesAsync();
        }

        if (data.Satellites != null && !await context.SatelliteProducts.AnyAsync())
        {
            var validCodes = await context.Clients.Select(c => c.ClientCode).ToHashSetAsync();

            foreach (var s in data.Satellites)
            {
                if (!validCodes.Contains(s.ClientCode)) continue;

                DateOnly? contractDate = null;
                if (!string.IsNullOrEmpty(s.ContractDate))
                    contractDate = DateOnly.Parse(s.ContractDate);

                context.SatelliteProducts.Add(new SatelliteProduct
                {
                    ClientCode = s.ClientCode,
                    ProductName = s.ProductName,
                    ProductVariant = s.ProductVariant,
                    Province = s.Province,
                    IsBlocked = s.IsBlocked,
                    ContractDate = contractDate,
                    PartnerCode = s.PartnerCode,
                    Partner = s.Partner,
                    UsersProduct = s.UsersProduct,
                });
            }
            await context.SaveChangesAsync();
        }

        if (data.Cloud != null && !await context.CloudClients.AnyAsync())
        {
            var validCodes = await context.Clients.Select(c => c.ClientCode).ToHashSetAsync();

            foreach (var cc in data.Cloud)
            {
                if (!validCodes.Contains(cc.ClientCode)) continue;

                context.CloudClients.Add(new CloudClient
                {
                    ClientCode = cc.ClientCode,
                    ArrCloud = cc.ArrCloud,
                });
            }
            await context.SaveChangesAsync();
        }

        if (data.Mrr != null && !await context.MrrHistories.AnyAsync())
        {
            foreach (var m in data.Mrr)
            {
                context.MrrHistories.Add(new MrrHistory
                {
                    CustomerName = m.CustomerName,
                    Jan26 = m.Jan26,
                    Feb26 = m.Feb26,
                    Mar26 = m.Mar26,
                    Apr26 = m.Apr26,
                });
            }
            await context.SaveChangesAsync();
        }
    }
}

#region Seed DTOs

file class SeedData
{
    public List<SeedClient>? Clients { get; set; }
    public List<SeedSatellite>? Satellites { get; set; }
    public List<SeedCloud>? Cloud { get; set; }
    public List<SeedMrr>? Mrr { get; set; }
}

file class SeedClient
{
    public int ClientCode { get; set; }
    public string? BusinessName { get; set; }
    public string? Name { get; set; }
    public string? Cif { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Activity { get; set; }
    public string? Partner { get; set; }
    public string? Erp { get; set; }
    public int UsersErp { get; set; }
    public bool HasMobileA { get; set; }
    public bool HasMobileE { get; set; }
    public bool HasAuna { get; set; }
    public bool HasFegime { get; set; }
    public bool HasObras { get; set; }
    public decimal ArrActual { get; set; }
    public decimal InfraCost { get; set; }
    public decimal ArrCore { get; set; }
    public decimal DeltaArr { get; set; }
    public bool IsDam { get; set; }
    public decimal MonthlyRate { get; set; }
    public decimal DamRate { get; set; }
}

file class SeedSatellite
{
    public int ClientCode { get; set; }
    public string ProductName { get; set; } = "";
    public string? ProductVariant { get; set; }
    public int Province { get; set; }
    public bool IsBlocked { get; set; }
    public string? ContractDate { get; set; }
    public int PartnerCode { get; set; }
    public string? Partner { get; set; }
    public int UsersProduct { get; set; }
}

file class SeedCloud
{
    public int ClientCode { get; set; }
    public decimal ArrCloud { get; set; }
}

file class SeedMrr
{
    public string CustomerName { get; set; } = "";
    public decimal Jan26 { get; set; }
    public decimal Feb26 { get; set; }
    public decimal Mar26 { get; set; }
    public decimal Apr26 { get; set; }
}

#endregion
