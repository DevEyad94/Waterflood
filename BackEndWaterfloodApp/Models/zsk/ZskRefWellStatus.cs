using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZSK.Services.ReferenceData.Entities;

[Table("ZSK_Ref_WellStatus")]
public class ZskRefWellStatus
{
    [Key]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ColorCode { get; set; } = string.Empty;
}
