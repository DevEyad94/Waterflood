using AutoMapper;
using BackEndWaterFloodApp.Application.Dtos.Thresholds;
using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Models;
using ZSK.Services.ReferenceData.Interfaces;

namespace BackEndWaterFloodApp.Services.Monitoring;

public interface IMonitoringService
{
    Task<ServiceResponse<List<WaterfloodRecordDto>>> GetAlertsAsync();
    Task<ServiceResponse<AlertThresholdDto>> GetThresholdsAsync();
    Task<ServiceResponse<AlertThresholdDto>> UpdateThresholdsAsync(UpdateAlertThresholdDto dto);
    Task<ZskEffectiveThresholds> GetEffectiveThresholdsAsync();
    Task ApplyAlertInfoAsync(WaterfloodRecordDto dto, WaterfloodRecord record);
    Task<bool> RequiresAttentionAsync(WaterfloodRecord record);
}

public class MonitoringService : IMonitoringService
{
    private readonly IWaterfloodRepository _waterfloodRepository;
    private readonly IThresholdRepository _thresholdRepository;
    private readonly IZskReferenceService _zskReferenceService;
    private readonly IMapper _mapper;

    public MonitoringService(
        IWaterfloodRepository waterfloodRepository,
        IThresholdRepository thresholdRepository,
        IZskReferenceService zskReferenceService,
        IMapper mapper
    )
    {
        _waterfloodRepository = waterfloodRepository;
        _thresholdRepository = thresholdRepository;
        _zskReferenceService = zskReferenceService;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<List<WaterfloodRecordDto>>> GetAlertsAsync()
    {
        var thresholds = await GetEffectiveThresholdsAsync();
        var records = await _waterfloodRepository.GetAllFilteredAsync();
        var previousByWell = await _waterfloodRepository.GetLatestHistoryByWellIdsAsync(
            records.Select(r => r.Id)
        );
        var alerts = new List<WaterfloodRecordDto>();

        foreach (var record in records)
        {
            var dto = _mapper.Map<WaterfloodRecordDto>(record);
            previousByWell.TryGetValue(record.Id, out var previous);
            ApplyAlertInfo(dto, record, thresholds, previous?.OilProductionRate);
            if (dto.RequiresAttention)
                alerts.Add(dto);
        }

        return new ServiceResponse<List<WaterfloodRecordDto>> { Data = alerts };
    }

    public async Task<ServiceResponse<AlertThresholdDto>> GetThresholdsAsync()
    {
        var threshold = await _thresholdRepository.GetOrCreateDefaultAsync();
        return new ServiceResponse<AlertThresholdDto>
        {
            Data = _mapper.Map<AlertThresholdDto>(threshold),
        };
    }

    public async Task<ServiceResponse<AlertThresholdDto>> UpdateThresholdsAsync(
        UpdateAlertThresholdDto dto
    )
    {
        var threshold = await _thresholdRepository.GetOrCreateDefaultAsync();
        _mapper.Map(dto, threshold);
        threshold.UpdatedAt = DateTime.UtcNow;

        if (!await _thresholdRepository.SaveChangesAsync())
            return new ServiceResponse<AlertThresholdDto>
            {
                Success = false,
                Message = "Failed to update thresholds.",
            };

        return new ServiceResponse<AlertThresholdDto>
        {
            Data = _mapper.Map<AlertThresholdDto>(threshold),
        };
    }

    public async Task<ZskEffectiveThresholds> GetEffectiveThresholdsAsync()
    {
        var threshold = await _thresholdRepository.GetOrCreateDefaultAsync();
        return await _zskReferenceService.GetEffectiveThresholdsAsync(threshold);
    }

    public async Task ApplyAlertInfoAsync(WaterfloodRecordDto dto, WaterfloodRecord record)
    {
        var thresholds = await GetEffectiveThresholdsAsync();
        var previous = await _waterfloodRepository.GetLatestHistoryByWellRecordIdAsync(record.Id);
        ApplyAlertInfo(dto, record, thresholds, previous?.OilProductionRate);
    }

    public async Task<bool> RequiresAttentionAsync(WaterfloodRecord record)
    {
        var thresholds = await GetEffectiveThresholdsAsync();
        var previous = await _waterfloodRepository.GetLatestHistoryByWellRecordIdAsync(record.Id);
        var (requiresAttention, _) = WaterfloodAlertEvaluator.Evaluate(
            record,
            thresholds,
            previous?.OilProductionRate
        );
        return requiresAttention;
    }

    private static void ApplyAlertInfo(
        WaterfloodRecordDto dto,
        WaterfloodRecord record,
        ZskEffectiveThresholds thresholds,
        decimal? previousOilProductionRate
    )
    {
        var (requiresAttention, alertList) = WaterfloodAlertEvaluator.Evaluate(
            record,
            thresholds,
            previousOilProductionRate
        );
        dto.RequiresAttention = requiresAttention;
        dto.Alerts = alertList;
    }
}
