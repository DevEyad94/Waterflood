using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Extensions.Pagination;

namespace BackEndWaterFloodApp.Application.Interfaces;

public interface IWaterfloodRepository
{
    IQueryable<WaterfloodRecord> Query();
    Task<WaterfloodRecord?> GetByIdAsync(Guid id);
    Task<WaterfloodRecord> AddAsync(WaterfloodRecord record);
    void Update(WaterfloodRecord record);
    void Delete(WaterfloodRecord record);
    Task<bool> SaveChangesAsync();
    Task<PagedList<WaterfloodRecord>> GetPagedAsync(
        Pagination pagination,
        WaterfloodFilterDto? filter = null
    );
    Task<List<WaterfloodRecord>> GetAllFilteredAsync(WaterfloodFilterDto? filter = null);
    Task AddHistoryAsync(WaterfloodMeasurementHistory history);
    Task<List<WaterfloodMeasurementHistory>> GetHistoryByWellRecordIdAsync(Guid wellRecordId);
    Task<WaterfloodMeasurementHistory?> GetLatestHistoryByWellRecordIdAsync(Guid wellRecordId);
    Task<List<WaterfloodMeasurementHistory>> GetAllHistoryAsync();
    Task<Dictionary<Guid, WaterfloodMeasurementHistory>> GetLatestHistoryByWellIdsAsync(
        IEnumerable<Guid> wellRecordIds
    );
}

public interface IRelationshipRepository
{
    IQueryable<InjectorProducerRelationship> Query();
    Task<InjectorProducerRelationship?> GetByIdAsync(Guid id);
    Task<InjectorProducerRelationship> AddAsync(InjectorProducerRelationship relationship);
    void Update(InjectorProducerRelationship relationship);
    void Delete(InjectorProducerRelationship relationship);
    Task<bool> SaveChangesAsync();
    Task<List<InjectorProducerRelationship>> GetByInjectorIdAsync(Guid injectorWellId);
}

public interface IThresholdRepository
{
    Task<AlertThreshold> GetOrCreateDefaultAsync();
    Task<bool> SaveChangesAsync();
}

public interface IWaterfloodAnalyticsService
{
    Task<Models.ServiceResponse<WaterfloodKpiSummaryDto>> GetKpiSummaryAsync(
        WaterfloodAnalyticsFilterDto? filter = null
    );
    Task<Models.ServiceResponse<WaterfloodTrendsResponseDto>> GetTrendsAsync(
        WaterfloodAnalyticsFilterDto? filter = null
    );
}
