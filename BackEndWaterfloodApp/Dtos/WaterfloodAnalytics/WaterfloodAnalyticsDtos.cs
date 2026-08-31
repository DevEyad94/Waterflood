using BackEndWaterFloodApp.Application.Dtos.Waterflood;

namespace BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;

public class WaterfloodKpiSummaryDto
{
    public decimal TotalInjectionRate { get; set; }
    public decimal TotalOilProductionRate { get; set; }
    public decimal TotalWaterProductionRate { get; set; }
    public decimal AverageWaterCut { get; set; }
    public int ActiveInjectorCount { get; set; }
    public int ActiveProducerCount { get; set; }
    public int WellsRequiringAttention { get; set; }
    public decimal InjectionEfficiencyPercent { get; set; }
}

public class WaterfloodTrendDataPointDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalInjectionRate { get; set; }
    public decimal TotalOilProductionRate { get; set; }
    public decimal AverageWaterCut { get; set; }
    public decimal AverageInjectionPressure { get; set; }
}

public class WaterfloodStatusDistributionDto
{
    public string WellStatusCode { get; set; } = string.Empty;
    public string WellStatusName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WaterfloodWellRateDto
{
    public string WellName { get; set; } = string.Empty;
    public string WellTypeCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

public class WaterfloodTrendsResponseDto
{
    public List<WaterfloodTrendDataPointDto> Trends { get; set; } = new();
    public List<WaterfloodStatusDistributionDto> StatusDistribution { get; set; } = new();
    public List<WaterfloodWellRateDto> InjectionByWell { get; set; } = new();
    public List<WaterfloodWellRateDto> OilProductionByWell { get; set; } = new();
}

public class WaterfloodHistoryPointDto
{
    public DateTime MeasurementDate { get; set; }
    public decimal? InjectionRate { get; set; }
    public decimal? OilProductionRate { get; set; }
    public decimal? WaterProductionRate { get; set; }
    public decimal? WaterCut { get; set; }
    public decimal? InjectionPressure { get; set; }
    public string WellStatusCode { get; set; } = string.Empty;
}

public class WaterfloodAnalyticsFilterDto : WaterfloodFilterDto { }
