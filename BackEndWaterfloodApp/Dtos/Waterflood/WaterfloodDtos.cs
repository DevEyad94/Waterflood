namespace BackEndWaterFloodApp.Application.Dtos.Waterflood;

using BackEndWaterFloodApp.Attributes;

public class WaterfloodAlertDto
{
    public string RuleIdentifier { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string AlertStatus { get; set; } = string.Empty;
}

public class WaterfloodRecordDto
{
    public Guid Id { get; set; }
    public string WellName { get; set; } = string.Empty;
    public string WellTypeCode { get; set; } = string.Empty;
    public string WellTypeName { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? InjectionRate { get; set; }
    public decimal? OilProductionRate { get; set; }
    public decimal? WaterProductionRate { get; set; }
    public decimal? WaterCut { get; set; }
    public decimal? InjectionPressure { get; set; }
    public string WellStatusCode { get; set; } = string.Empty;
    public string WellStatusName { get; set; } = string.Empty;
    public string StatusColorCode { get; set; } = string.Empty;
    public DateTime MeasurementDate { get; set; }
    public bool RequiresAttention { get; set; }
    public List<WaterfloodAlertDto> Alerts { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

[WellMeasurementValidation]
public class CreateWaterfloodRecordDto
{
    public string WellName { get; set; } = string.Empty;
    public string WellTypeCode { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? InjectionRate { get; set; }
    public decimal? OilProductionRate { get; set; }
    public decimal? WaterProductionRate { get; set; }
    public decimal? WaterCut { get; set; }
    public decimal? InjectionPressure { get; set; }
    public string WellStatusCode { get; set; } = string.Empty;
    public DateTime MeasurementDate { get; set; }
}

[WellMeasurementValidation]
public class UpdateWaterfloodRecordDto : CreateWaterfloodRecordDto
{
    public Guid Id { get; set; }
}

public class WaterfloodFilterDto
{
    public string? FieldName { get; set; }
    public string? WellTypeCode { get; set; }
    public string? WellStatusCode { get; set; }
    public decimal? MinInjectionRate { get; set; }
    public decimal? MaxInjectionRate { get; set; }
    public decimal? MinOilProductionRate { get; set; }
    public decimal? MaxOilProductionRate { get; set; }
    public decimal? MinWaterCut { get; set; }
    public decimal? MaxWaterCut { get; set; }
    public decimal? MinInjectionPressure { get; set; }
    public decimal? MaxInjectionPressure { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public bool? RequiresAttentionOnly { get; set; }
}
