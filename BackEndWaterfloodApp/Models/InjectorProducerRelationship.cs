using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndWaterFloodApp.Models;
using ZSK.Services.ReferenceData.Entities;

namespace BackEndWaterFloodApp.Domain.Entities;

public class InjectorProducerRelationship : RefModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid InjectorWellId { get; set; }

    [ForeignKey(nameof(InjectorWellId))]
    public WaterfloodRecord InjectorWell { get; set; } = null!;

    public Guid ProducerWellId { get; set; }

    [ForeignKey(nameof(ProducerWellId))]
    public WaterfloodRecord ProducerWell { get; set; } = null!;

    [Column(TypeName = "decimal(8,2)")]
    public decimal Distance { get; set; }

    [Required]
    [MaxLength(4)]
    public string RelationshipStatusCode { get; set; } = string.Empty;

    [ForeignKey(nameof(RelationshipStatusCode))]
    public ZskRefRelationshipStatus RelationshipStatus { get; set; } = null!;
}
