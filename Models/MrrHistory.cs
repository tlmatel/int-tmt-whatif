using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

[Table("mrr_history")]
public class MrrHistory
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("customer_name")]
    public string CustomerName { get; set; } = "";

    [Column("jan_26")]
    public decimal Jan26 { get; set; }

    [Column("feb_26")]
    public decimal Feb26 { get; set; }

    [Column("mar_26")]
    public decimal Mar26 { get; set; }

    [Column("apr_26")]
    public decimal Apr26 { get; set; }
}
