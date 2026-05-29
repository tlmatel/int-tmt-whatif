using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

[Table("satellite_products")]
public class SatelliteProduct
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("client_code")]
    public int ClientCode { get; set; }

    [Required]
    [Column("product_name")]
    public string ProductName { get; set; } = "";

    [Column("product_variant")]
    public string? ProductVariant { get; set; }

    [Column("province")]
    public int Province { get; set; }

    [Column("is_blocked")]
    public bool IsBlocked { get; set; }

    [Column("contract_date")]
    public DateOnly? ContractDate { get; set; }

    [Column("partner_code")]
    public int PartnerCode { get; set; }

    [Column("partner")]
    public string? Partner { get; set; }

    [Column("users_product")]
    public int UsersProduct { get; set; }

    [ForeignKey("ClientCode")]
    public Client? Client { get; set; }
}
