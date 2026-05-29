using Microsoft.EntityFrameworkCore;
using CoreWhatIf.Models;

namespace CoreWhatIf.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DummyRecord> DummyRecords { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<SatelliteProduct> SatelliteProducts { get; set; }
    public DbSet<CloudClient> CloudClients { get; set; }
    public DbSet<MrrHistory> MrrHistories { get; set; }
    public DbSet<RoadmapFeature> RoadmapFeatures { get; set; }
    public DbSet<SavedSimulation> SavedSimulations { get; set; }
    public DbSet<SavedPlan> SavedPlans { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(e =>
        {
            e.HasIndex(c => c.ClientCode).IsUnique();
            e.HasIndex(c => c.Partner);
            e.HasIndex(c => c.IsDam);
        });

        builder.Entity<SatelliteProduct>(e =>
        {
            e.HasIndex(s => s.ClientCode);
            e.HasIndex(s => s.ProductName);
            e.HasOne(s => s.Client)
                .WithMany(c => c.SatelliteProducts)
                .HasForeignKey(s => s.ClientCode)
                .HasPrincipalKey(c => c.ClientCode);
        });

        builder.Entity<CloudClient>(e =>
        {
            e.HasOne(cc => cc.Client)
                .WithOne(c => c.CloudClient)
                .HasForeignKey<CloudClient>(cc => cc.ClientCode)
                .HasPrincipalKey<Client>(c => c.ClientCode);
        });

        builder.Entity<SavedSimulation>(e =>
        {
            e.HasIndex(s => s.UserEmail);
        });

        builder.Entity<SavedPlan>(e =>
        {
            e.HasIndex(p => p.UserEmail);
        });
    }
}
