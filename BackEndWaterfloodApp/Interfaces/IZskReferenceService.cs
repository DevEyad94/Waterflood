using BackEndWaterFloodApp.Models;
using ZSK.Services.ReferenceData.Dtos;

namespace ZSK.Services.ReferenceData.Interfaces;

public interface IZskReferenceService
{
    Task<ServiceResponse<ZskReferenceDataDto>> GetReferenceDataAsync();
    Task<ServiceResponse<List<ZskMonitoringRuleDto>>> GetMonitoringRulesAsync();
    Task<bool> IsValidWellTypeCodeAsync(string code);
    Task<bool> IsValidWellStatusCodeAsync(string code);
    Task<bool> IsValidRelationshipStatusCodeAsync(string code);
    Task<ZskEffectiveThresholds> GetEffectiveThresholdsAsync(BackEndWaterFloodApp.Domain.Entities.AlertThreshold? thresholdOverride);
}

public class ZskEffectiveThresholds
{
    public decimal MaxWaterCutPercent { get; set; }
    public decimal MinOilProductionRate { get; set; }
    public decimal MinInjectionRate { get; set; }
    public decimal MaxInjectionPressure { get; set; }
    public decimal ProductionDeclinePercent { get; set; }
    public List<ZskMonitoringRuleDto> Rules { get; set; } = new();
}
