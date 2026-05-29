using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreWhatIf.Models;

[Table("cloud_clients")]
public class CloudClient
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("client_code")]
    public int ClientCode { get; set; }

    [Column("arr_cloud")]
    public decimal ArrCloud { get; set; }

    [ForeignKey("ClientCode")]
    public Client? Client { get; set; }
}
