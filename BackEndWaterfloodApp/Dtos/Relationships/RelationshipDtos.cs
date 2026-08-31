namespace BackEndWaterFloodApp.Application.Dtos.Relationships;

public class WaterfloodRelationshipDto
{
    public Guid Id { get; set; }
    public Guid InjectorWellId { get; set; }
    public string InjectorWellName { get; set; } = string.Empty;
    public Guid ProducerWellId { get; set; }
    public string ProducerWellName { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string RelationshipStatusCode { get; set; } = string.Empty;
    public string RelationshipStatusName { get; set; } = string.Empty;
}

public class CreateWaterfloodRelationshipDto
{
    public Guid InjectorWellId { get; set; }
    public Guid ProducerWellId { get; set; }
    public decimal Distance { get; set; }
    public string RelationshipStatusCode { get; set; } = "ACT";
}

public class UpdateWaterfloodRelationshipDto : CreateWaterfloodRelationshipDto
{
    public Guid Id { get; set; }
}

public class WaterfloodInjectorDetailDto
{
    public Waterflood.WaterfloodRecordDto Injector { get; set; } = null!;
    public List<WaterfloodRelationshipDto> Relationships { get; set; } = new();
    public List<Waterflood.WaterfloodRecordDto> LinkedProducers { get; set; } = new();
    public List<WaterfloodAnalytics.WaterfloodHistoryPointDto> InjectorTrend { get; set; } = new();
    public List<WaterfloodProducerTrendDto> ProducerTrends { get; set; } = new();
}

public class WaterfloodProducerTrendDto
{
    public Guid WellId { get; set; }
    public string WellName { get; set; } = string.Empty;
    public List<WaterfloodAnalytics.WaterfloodHistoryPointDto> Points { get; set; } = new();
}
