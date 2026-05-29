using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Auth0.AspNetCore.Authentication;
using CoreWhatIf.Data;
using CoreWhatIf.Models;
using CoreWhatIf.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
var dbConfig = builder.Configuration.GetSection("Database");
var connectionStringBuilder = new NpgsqlConnectionStringBuilder
{
    Host = dbConfig["Host"],
    Port = int.Parse(dbConfig["Port"] ?? "5432"),
    Database = dbConfig["Name"],
    Username = dbConfig["User"],
    Password = dbConfig["Password"],
    SslMode = Enum.Parse<SslMode>(dbConfig["SslMode"] ?? "Disable")
};

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionStringBuilder.ConnectionString));

// ── Auth0 ────────────────────────────────────────────────────────────────────
builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"]!;
        options.ClientId = builder.Configuration["Auth0:ClientId"]!;
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"]!;
        options.Scope = "openid profile email";
    });

// ── HubRD Access Service ─────────────────────────────────────────────────────
builder.Services.Configure<HubRdAccessSettings>(
    builder.Configuration.GetSection("HubRdAccess"));

var hubRdConfig = builder.Configuration.GetSection("HubRdAccess");

builder.Services.AddHttpClient<HubRdAccessService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(hubRdConfig["TimeoutSeconds"] ?? "10"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

// ── OnTokenValidated: verificación de acceso en el login ─────────────────────
builder.Services.Configure<OpenIdConnectOptions>(
    Auth0Constants.AuthenticationScheme, options =>
    {
        options.Events ??= new OpenIdConnectEvents();
        var existingOnTokenValidated = options.Events.OnTokenValidated;

        options.Events.OnTokenValidated = async context =>
        {
            if (existingOnTokenValidated != null)
                await existingOnTokenValidated(context);

            if (context.Principal?.Identity is not ClaimsIdentity identity)
                return;

            var email = identity.FindFirst(ClaimTypes.Email)?.Value
                        ?? identity.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email)) return;

            var accessService = context.HttpContext.RequestServices
                .GetRequiredService<HubRdAccessService>();
            var accessResult = await accessService.CheckAccessAsync(email);

            identity.AddClaim(new Claim("hubrd_checked_at",
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            var debugJson = System.Text.Json.JsonSerializer.Serialize(accessResult);
            identity.AddClaim(new Claim("hubrd_debug", debugJson));

            if (accessResult is { HasAccess: true })
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, accessResult.GetEffectiveRole()));
            }
            else
            {
                identity.AddClaim(new Claim("hubrd_access", "denied"));
                identity.AddClaim(new Claim("hubrd_reason",
                    accessResult?.Reason ?? "api_error"));
            }
        };
    });

// ── Re-validación periódica (cookie) ─────────────────────────────────────────
var revalidateMinutes = int.Parse(hubRdConfig["RevalidateIntervalMinutes"] ?? "30");

builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Welcome";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events.OnValidatePrincipal = async context =>
        {
            if (context.Principal?.Identity is not ClaimsIdentity identity)
                return;

            var checkedAtStr = identity.FindFirst("hubrd_checked_at")?.Value;
            if (string.IsNullOrEmpty(checkedAtStr)) return;

            var checkedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(checkedAtStr));
            if (DateTimeOffset.UtcNow - checkedAt < TimeSpan.FromMinutes(revalidateMinutes))
                return;

            var email = identity.FindFirst(ClaimTypes.Email)?.Value
                        ?? identity.FindFirst("email")?.Value;
            if (string.IsNullOrEmpty(email)) return;

            var accessService = context.HttpContext.RequestServices
                .GetRequiredService<HubRdAccessService>();
            var accessResult = await accessService.CheckAccessAsync(email);

            var claimsToRemove = identity.Claims
                .Where(c => c.Type is "hubrd_access" or "hubrd_reason"
                            or "hubrd_checked_at" or "hubrd_debug"
                            || c.Type == ClaimTypes.Role)
                .ToList();
            foreach (var c in claimsToRemove) identity.RemoveClaim(c);

            identity.AddClaim(new Claim("hubrd_checked_at",
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            var debugJson = System.Text.Json.JsonSerializer.Serialize(accessResult);
            identity.AddClaim(new Claim("hubrd_debug", debugJson));

            if (accessResult is { HasAccess: true })
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, accessResult.GetEffectiveRole()));
            }
            else
            {
                identity.AddClaim(new Claim("hubrd_access", "denied"));
                identity.AddClaim(new Claim("hubrd_reason",
                    accessResult?.Reason ?? "api_error"));
            }

            context.ShouldRenew = true;
        };
    });

var app = builder.Build();

// ── DB seed ──────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var applyMigrations = bool.Parse(dbConfig["ApplyMigrationsOnStartup"] ?? "true");

        if (applyMigrations)
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones aplicadas.");
        }

        var enableSeed = bool.Parse(dbConfig["EnableSeed"] ?? "false");
        await DbSeeder.SeedDataAsync(context, enableSeed);

        logger.LogInformation("Inicialización de base de datos completada.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al inicializar la base de datos.");
        throw;
    }
}

// ── Pipeline ─────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Middleware de bloqueo HubRD ──────────────────────────────────────────────
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true
        && user.HasClaim("hubrd_access", "denied"))
    {
        var path = context.Request.Path.Value ?? "";
        var allowedPaths = new[] { "/Account/AccessDenied", "/Account/Logout", "/Account/Welcome" };

        if (!allowedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Redirect("/Account/AccessDenied");
            return;
        }
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
