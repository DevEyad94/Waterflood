using BackEndWaterFloodApp.Domain.Constants;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Models;
using Microsoft.Extensions.Caching.Memory;
using ZSK.Services.ReferenceData.Dtos;
using ZSK.Services.ReferenceData.Interfaces;

namespace ZSK.Services.ReferenceData.Services;

public class ZskReferenceService : IZskReferenceService
{
    private const string ReferenceCacheKey = "zsk:reference-data";
    private const string RulesCacheKey = "zsk:monitoring-rules";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly IZskReferenceRepository _repository;
    private readonly IMemoryCache _cache;

    public ZskReferenceService(IZskReferenceRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ServiceResponse<ZskReferenceDataDto>> GetReferenceDataAsync()
    {
        var data = await _cache.GetOrCreateAsync(
            ReferenceCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _repository.GetReferenceDataAsync();
            }
        );

        return new ServiceResponse<ZskReferenceDataDto> { Data = data };
    }

    public async Task<ServiceResponse<List<ZskMonitoringRuleDto>>> GetMonitoringRulesAsync()
    {
        var rules = await _cache.GetOrCreateAsync(
            RulesCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _repository.GetMonitoringRulesAsync();
            }
        );

        return new ServiceResponse<List<ZskMonitoringRuleDto>> { Data = rules };
    }

    public async Task<bool> IsValidWellTypeCodeAsync(string code)
    {
        var data = await GetCachedReferenceDataAsync();
        return data.WellTypes.Any(w => w.Code == code);
    }

    public async Task<bool> IsValidWellStatusCodeAsync(string code)
    {
        var data = await GetCachedReferenceDataAsync();
        return data.WellStatuses.Any(w => w.Code == code);
    }

    public async Task<bool> IsValidRelationshipStatusCodeAsync(string code)
    {
        var data = await GetCachedReferenceDataAsync();
        return data.RelationshipStatuses.Any(r => r.Code == code);
    }

    public async Task<ZskEffectiveThresholds> GetEffectiveThresholdsAsync(AlertThreshold? thresholdOverride)
    {
        var rules = await GetCachedRulesAsync();
        var defaults = BuildDefaultsFromRules(rules);

        if (thresholdOverride is null)
            return defaults;

        defaults.MaxWaterCutPercent = thresholdOverride.MaxWaterCutPercent;
        defaults.MinOilProductionRate = thresholdOverride.MinOilProductionRate;
        defaults.MinInjectionRate = thresholdOverride.MinInjectionRate;
        defaults.MaxInjectionPressure = thresholdOverride.MaxInjectionPressure;
        defaults.ProductionDeclinePercent = thresholdOverride.ProductionDeclinePercent;
        return defaults;
    }

    private async Task<ZskReferenceDataDto> GetCachedReferenceDataAsync() =>
        await _cache.GetOrCreateAsync(
            ReferenceCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _repository.GetReferenceDataAsync();
            }
        ) ?? new ZskReferenceDataDto();

    private async Task<List<ZskMonitoringRuleDto>> GetCachedRulesAsync() =>
        await _cache.GetOrCreateAsync(
            RulesCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await _repository.GetMonitoringRulesAsync();
            }
        ) ?? new List<ZskMonitoringRuleDto>();

    private static ZskEffectiveThresholds BuildDefaultsFromRules(List<ZskMonitoringRuleDto> rules)
    {
        decimal GetRuleValue(string ruleCode) =>
            rules.FirstOrDefault(r => r.RuleCode == ruleCode)?.DefaultThresholdValue ?? 0m;

        return new ZskEffectiveThresholds
        {
            MaxWaterCutPercent = GetRuleValue(WaterfloodAlertRuleIdentifiers.HighWaterCut),
            MinOilProductionRate = GetRuleValue(WaterfloodAlertRuleIdentifiers.LowOilProduction),
            MinInjectionRate = GetRuleValue(WaterfloodAlertRuleIdentifiers.LowInjection),
            MaxInjectionPressure = GetRuleValue(WaterfloodAlertRuleIdentifiers.HighPressure),
            ProductionDeclinePercent = GetRuleValue(WaterfloodAlertRuleIdentifiers.ProductionDecline),
            Rules = rules,
        };
    }
}
