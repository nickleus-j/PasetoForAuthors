using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OpineHere.Data.entity;

[Table("PopularityApproval")]
public class PopularityApproval
{
    [Key]
    public int Id { get; set; }
    [DefaultValue(true)]
    public bool IsApproved { get; set; }
    [DefaultValue(true)]
    [DisplayName("In Use")]
    public bool inUse { get; set; }
    [StringLength(40)]
    [Required]
    [DisplayName("PostId")]
    public Guid PostId{ get; set; }
    [StringLength(40)]
    [Required]
    public string UserId { get; set; }
}