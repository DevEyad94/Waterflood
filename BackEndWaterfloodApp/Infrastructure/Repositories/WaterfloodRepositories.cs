using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Data;
using BackEndWaterFloodApp.Domain.Constants;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Extensions.Pagination;
using Microsoft.EntityFrameworkCore;

namespace BackEndWaterFloodApp.Infrastructure.Repositories;

public class WaterfloodRepository : IWaterfloodRepository
{
    private readonly WaterfloodDbContext _context;

    public WaterfloodRepository(WaterfloodDbContext context)
    {
        _context = context;
    }

    public IQueryable<WaterfloodRecord> Query() =>
        _context.WaterfloodRecords
            .Include(r => r.WellType)
            .Include(r => r.WellStatus)
            .AsQueryable();

    public Task<WaterfloodRecord?> GetByIdAsync(Guid id) =>
        Query().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<WaterfloodRecord> AddAsync(WaterfloodRecord record)
    {
        await _context.WaterfloodRecords.AddAsync(record);
        return record;
    }

    public void Update(WaterfloodRecord record) => _context.WaterfloodRecords.Update(record);

    public void Delete(WaterfloodRecord record) => _context.WaterfloodRecords.Remove(record);

    public Task<bool> SaveChangesAsync() =>
        _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);

    public async Task<PagedList<WaterfloodRecord>> GetPagedAsync(
        Pagination pagination,
        WaterfloodFilterDto? filter = null
    )
    {
        var query = ApplyFilter(Query(), filter);
        query = ApplySorting(query, pagination);
        return await Task.FromResult(
            PagedList<WaterfloodRecord>.ToPagedList(
                query,
                pagination.PageNumber,
                pagination.PageSize
            )
        );
    }

    public Task<List<WaterfloodRecord>> GetAllFilteredAsync(WaterfloodFilterDto? filter = null) =>
        ApplyFilter(Query(), filter).ToListAsync();

    public async Task AddHistoryAsync(WaterfloodMeasurementHistory history)
    {
        await _context.WaterfloodMeasurementHistories.AddAsync(history);
    }

    public Task<List<WaterfloodMeasurementHistory>> GetHistoryByWellRecordIdAsync(Guid wellRecordId) =>
        _context
            .WaterfloodMeasurementHistories.Where(h => h.WellRecordId == wellRecordId)
            .OrderBy(h => h.MeasurementDate)
            .ToListAsync();

    public Task<WaterfloodMeasurementHistory?> GetLatestHistoryByWellRecordIdAsync(Guid wellRecordId) =>
        _context
            .WaterfloodMeasurementHistories.Where(h => h.WellRecordId == wellRecordId)
            .OrderByDescending(h => h.MeasurementDate)
            .FirstOrDefaultAsync();

    public Task<List<WaterfloodMeasurementHistory>> GetAllHistoryAsync() =>
        _context.WaterfloodMeasurementHistories.OrderBy(h => h.MeasurementDate).ToListAsync();

    public async Task<Dictionary<Guid, WaterfloodMeasurementHistory>> GetLatestHistoryByWellIdsAsync(
        IEnumerable<Guid> wellRecordIds
    )
    {
        var ids = wellRecordIds.Distinct().ToList();
        var histories = await _context
            .WaterfloodMeasurementHistories.Where(h => ids.Contains(h.WellRecordId))
            .ToListAsync();

        return histories
            .GroupBy(h => h.WellRecordId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(h => h.MeasurementDate).First()
            );
    }

    private static IQueryable<WaterfloodRecord> ApplyFilter(
        IQueryable<WaterfloodRecord> query,
        WaterfloodFilterDto? filter
    )
    {
        if (filter == null)
            return query;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(r =>
                r.WellName.ToLower().Contains(search) || r.FieldName.ToLower().Contains(search)
            );
        }

        if (!string.IsNullOrWhiteSpace(filter.FieldName))
            query = query.Where(r => r.FieldName == filter.FieldName);

        if (!string.IsNullOrWhiteSpace(filter.WellTypeCode))
            query = query.Where(r => r.WellTypeCode == filter.WellTypeCode);

        if (!string.IsNullOrWhiteSpace(filter.WellStatusCode))
            query = query.Where(r => r.WellStatusCode == filter.WellStatusCode);

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

        if (filter.FromDate.HasValue)
            query = query.Where(r => r.MeasurementDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(r => r.MeasurementDate <= filter.ToDate.Value);

        return query;
    }

    private static IQueryable<WaterfloodRecord> ApplySorting(
        IQueryable<WaterfloodRecord> query,
        Pagination pagination
    )
    {
        var isDesc = pagination.SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return pagination.SortColumn?.ToLower() switch
        {
            "wellname" => isDesc
                ? query.OrderByDescending(r => r.WellName)
                : query.OrderBy(r => r.WellName),
            "fieldname" => isDesc
                ? query.OrderByDescending(r => r.FieldName)
                : query.OrderBy(r => r.FieldName),
            "welltypecode" => isDesc
                ? query.OrderByDescending(r => r.WellTypeCode)
                : query.OrderBy(r => r.WellTypeCode),
            "wellstatuscode" => isDesc
                ? query.OrderByDescending(r => r.WellStatusCode)
                : query.OrderBy(r => r.WellStatusCode),
            "injectionrate" => isDesc
                ? query.OrderByDescending(r => r.InjectionRate)
                : query.OrderBy(r => r.InjectionRate),
            "oilproductionrate" => isDesc
                ? query.OrderByDescending(r => r.OilProductionRate)
                : query.OrderBy(r => r.OilProductionRate),
            _ => isDesc
                ? query.OrderByDescending(r => r.MeasurementDate)
                : query.OrderBy(r => r.MeasurementDate),
        };
    }
}

public class RelationshipRepository : IRelationshipRepository
{
    private readonly WaterfloodDbContext _context;

    public RelationshipRepository(WaterfloodDbContext context)
    {
        _context = context;
    }

    public IQueryable<InjectorProducerRelationship> Query() =>
        _context.InjectorProducerRelationships
            .Include(r => r.InjectorWell)
            .ThenInclude(w => w.WellType)
            .Include(r => r.ProducerWell)
            .ThenInclude(w => w.WellType)
            .Include(r => r.RelationshipStatus)
            .AsQueryable();

    public Task<InjectorProducerRelationship?> GetByIdAsync(Guid id) =>
        Query().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<InjectorProducerRelationship> AddAsync(
        InjectorProducerRelationship relationship
    )
    {
        await _context.InjectorProducerRelationships.AddAsync(relationship);
        return relationship;
    }

    public void Update(InjectorProducerRelationship relationship) =>
        _context.InjectorProducerRelationships.Update(relationship);

    public void Delete(InjectorProducerRelationship relationship) =>
        _context.InjectorProducerRelationships.Remove(relationship);

    public Task<bool> SaveChangesAsync() =>
        _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);

    public Task<List<InjectorProducerRelationship>> GetByInjectorIdAsync(Guid injectorWellId) =>
        Query().Where(r => r.InjectorWellId == injectorWellId).ToListAsync();
}

public class ThresholdRepository : IThresholdRepository
{
    private readonly WaterfloodDbContext _context;

    public ThresholdRepository(WaterfloodDbContext context)
    {
        _context = context;
    }

    public async Task<AlertThreshold> GetOrCreateDefaultAsync()
    {
        var threshold = await _context.AlertThresholds.FirstOrDefaultAsync();
        if (threshold != null)
            return threshold;

        threshold = new AlertThreshold();
        await _context.AlertThresholds.AddAsync(threshold);
        await _context.SaveChangesAsync();
        return threshold;
    }

    public Task<bool> SaveChangesAsync() =>
        _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
}
