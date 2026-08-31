namespace ZSK.Services.ReferenceData.Dtos;

public class ZskWellTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ZskWellStatusDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
}

public class ZskRelationshipStatusDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ZskMonitoringRuleDto
{
    public string RuleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetWellType { get; set; } = string.Empty;
    public decimal DefaultThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class ZskReferenceDataDto
{
    public List<ZskWellTypeDto> WellTypes { get; set; } = new();
    public List<ZskWellStatusDto> WellStatuses { get; set; } = new();
    public List<ZskRelationshipStatusDto> RelationshipStatuses { get; set; } = new();
}
