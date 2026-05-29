using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

[Table("clients")]
public class Client
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("client_code")]
    public int ClientCode { get; set; }

    [Column("business_name")]
    public string? BusinessName { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("cif")]
    public string? Cif { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [Column("activity")]
    public string? Activity { get; set; }

    [Column("partner")]
    public string? Partner { get; set; }

    [Column("erp")]
    public string? Erp { get; set; }

    [Column("users_erp")]
    public int UsersErp { get; set; }

    [Column("has_mobile_a")]
    public bool HasMobileA { get; set; }

    [Column("has_mobile_e")]
    public bool HasMobileE { get; set; }

    [Column("has_auna")]
    public bool HasAuna { get; set; }

    [Column("has_fegime")]
    public bool HasFegime { get; set; }

    [Column("has_obras")]
    public bool HasObras { get; set; }

    [Column("arr_actual")]
    public decimal ArrActual { get; set; }

    [Column("infra_cost")]
    public decimal InfraCost { get; set; }

    [Column("arr_core")]
    public decimal ArrCore { get; set; }

    [Column("delta_arr")]
    public decimal DeltaArr { get; set; }

    [Column("is_dam")]
    public bool IsDam { get; set; }

    [Column("monthly_rate")]
    public decimal MonthlyRate { get; set; } = 65.00m;

    [Column("dam_rate")]
    public decimal DamRate { get; set; } = 50.00m;

    public ICollection<SatelliteProduct> SatelliteProducts { get; set; } = new List<SatelliteProduct>();
    public CloudClient? CloudClient { get; set; }
}
