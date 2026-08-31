using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Domain.Constants;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Models;

namespace BackEndWaterFloodApp.Services.WaterfloodAnalytics;

public class WaterfloodAnalyticsService : IWaterfloodAnalyticsService
{
    private readonly IWaterfloodRepository _repository;

    public WaterfloodAnalyticsService(IWaterfloodRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResponse<WaterfloodKpiSummaryDto>> GetKpiSummaryAsync(
        WaterfloodAnalyticsFilterDto? filter = null
    )
    {
        var records = await _repository.GetAllFilteredAsync(filter);
        var producers = records
            .Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Producer)
            .ToList();
        var injectors = records
            .Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Injector)
            .ToList();

        var totalInjection = injectors.Sum(r => r.InjectionRate ?? 0);
        var totalOil = producers.Sum(r => r.OilProductionRate ?? 0);

        var kpi = new WaterfloodKpiSummaryDto
        {
            TotalInjectionRate = totalInjection,
            TotalOilProductionRate = totalOil,
            TotalWaterProductionRate = producers.Sum(r => r.WaterProductionRate ?? 0),
            AverageWaterCut = producers.Any()
                ? Math.Round(producers.Average(r => r.WaterCut ?? 0), 2)
                : 0,
            ActiveInjectorCount = injectors.Count(r =>
                r.WellStatusCode == WaterfloodWellStatusCodes.Active
            ),
            ActiveProducerCount = producers.Count(r =>
                r.WellStatusCode == WaterfloodWellStatusCodes.Active
            ),
            InjectionEfficiencyPercent =
                totalInjection > 0 ? Math.Round(totalOil / totalInjection * 100m, 2) : 0,
        };

        return new ServiceResponse<WaterfloodKpiSummaryDto> { Data = kpi };
    }

    public async Task<ServiceResponse<WaterfloodTrendsResponseDto>> GetTrendsAsync(
        WaterfloodAnalyticsFilterDto? filter = null
    )
    {
        var records = await _repository.GetAllFilteredAsync(filter);
        var history = FilterHistory(await _repository.GetAllHistoryAsync(), filter);

        var combinedPoints = history
            .Select(h => new TrendSource(
                h.MeasurementDate,
                h.WellTypeCode,
                h.InjectionRate,
                h.OilProductionRate,
                h.WaterCut,
                h.InjectionPressure,
                h.WellStatusCode
            ))
            .Concat(
                records.Select(r => new TrendSource(
                    r.MeasurementDate,
                    r.WellTypeCode,
                    r.InjectionRate,
                    r.OilProductionRate,
                    r.WaterCut,
                    r.InjectionPressure,
                    r.WellStatusCode
                ))
            )
            .ToList();

        var trends = combinedPoints
            .GroupBy(r => r.MeasurementDate.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new WaterfloodTrendDataPointDto
            {
                Period = g.Key,
                TotalInjectionRate = g.Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Injector)
                    .Sum(r => r.InjectionRate ?? 0),
                TotalOilProductionRate = g.Where(r =>
                        r.WellTypeCode == WaterfloodWellTypeCodes.Producer
                    )
                    .Sum(r => r.OilProductionRate ?? 0),
                AverageWaterCut = g.Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Producer)
                    .Any()
                    ? Math.Round(
                        g.Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Producer)
                            .Average(r => r.WaterCut ?? 0),
                        2
                    )
                    : 0,
                AverageInjectionPressure = g.Where(r =>
                        r.WellTypeCode == WaterfloodWellTypeCodes.Injector
                    )
                    .Any()
                    ? Math.Round(
                        g.Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Injector)
                            .Average(r => r.InjectionPressure ?? 0),
                        2
                    )
                    : 0,
            })
            .ToList();

        var statusLookup = records
            .GroupBy(r => r.WellStatusCode)
            .ToDictionary(g => g.Key, g => g.First().WellStatus);

        var statusDistribution = records
            .GroupBy(r => new
            {
                r.WellStatusCode,
                Name = r.WellStatus.Name,
                ColorCode = r.WellStatus.ColorCode,
            })
            .Select(g => new WaterfloodStatusDistributionDto
            {
                WellStatusCode = g.Key.WellStatusCode,
                WellStatusName = g.Key.Name,
                ColorCode = g.Key.ColorCode,
                Count = g.Count(),
            })
            .ToList();

        return new ServiceResponse<WaterfloodTrendsResponseDto>
        {
            Data = new WaterfloodTrendsResponseDto
            {
                Trends = trends,
                StatusDistribution = statusDistribution,
                InjectionByWell = records
                    .Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Injector)
                    .OrderByDescending(r => r.InjectionRate)
                    .Select(r => new WaterfloodWellRateDto
                    {
                        WellName = r.WellName,
                        WellTypeCode = r.WellTypeCode,
                        Rate = r.InjectionRate ?? 0,
                    })
                    .ToList(),
                OilProductionByWell = records
                    .Where(r => r.WellTypeCode == WaterfloodWellTypeCodes.Producer)
                    .OrderByDescending(r => r.OilProductionRate)
                    .Select(r => new WaterfloodWellRateDto
                    {
                        WellName = r.WellName,
                        WellTypeCode = r.WellTypeCode,
                        Rate = r.OilProductionRate ?? 0,
                    })
                    .ToList(),
            },
        };
    }

    private static List<WaterfloodMeasurementHistory> FilterHistory(
        List<WaterfloodMeasurementHistory> history,
        WaterfloodAnalyticsFilterDto? filter
    )
    {
        if (filter == null)
            return history;

        IEnumerable<WaterfloodMeasurementHistory> query = history;

        if (!string.IsNullOrWhiteSpace(filter.FieldName))
            query = query.Where(r => r.FieldName == filter.FieldName);

        if (!string.IsNullOrWhiteSpace(filter.WellTypeCode))
            query = query.Where(r => r.WellTypeCode == filter.WellTypeCode);

        if (!string.IsNullOrWhiteSpace(filter.WellStatusCode))
            query = query.Where(r => r.WellStatusCode == filter.WellStatusCode);

        if (filter.FromDate.HasValue)
            query = query.Where(r => r.MeasurementDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(r => r.MeasurementDate <= filter.ToDate.Value);

        if (filter.MinInjectionRate.HasValue)
            query = query.Where(r => r.InjectionRate >= filter.MinInjectionRate.Value);

        if (filter.MaxInjectionRate.HasValue)
            query = query.Where(r => r.InjectionRate <= filter.MaxInjectionRate.Value);

        if (filter.MinOilProductionRate.HasValue)
            query = query.Where(r => r.OilProductionRate >= filter.MinOilProductionRate.Value);

        if (filter.MaxOilProductionRate.HasValue)
            query = query.Where(r => r.OilProductionRate <= filter.MaxOilProductionRate.Value);

        if (filter.MinWaterCut.HasValue)
            query = query.Where(r => r.WaterCut >= filter.MinWaterCut.Value);

        if (filter.MaxWaterCut.HasValue)
            query = query.Where(r => r.WaterCut <= filter.MaxWaterCut.Value);

        if (filter.MinInjectionPressure.HasValue)
            query = query.Where(r => r.InjectionPressure >= filter.MinInjectionPressure.Value);

        if (filter.MaxInjectionPressure.HasValue)
            query = query.Where(r => r.InjectionPressure <= filter.MaxInjectionPressure.Value);

        return query.ToList();
    }

    private sealed record TrendSource(
        DateTime MeasurementDate,
        string WellTypeCode,
        decimal? InjectionRate,
        decimal? OilProductionRate,
        decimal? WaterCut,
        decimal? InjectionPressure,
        string WellStatusCode
    );
}
