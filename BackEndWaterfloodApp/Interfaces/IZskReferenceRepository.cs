using ZSK.Services.ReferenceData.Dtos;

namespace ZSK.Services.ReferenceData.Interfaces;

public interface IZskReferenceRepository
{
    Task<ZskReferenceDataDto> GetReferenceDataAsync();
    Task<List<ZskMonitoringRuleDto>> GetMonitoringRulesAsync();
}
