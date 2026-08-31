using BackEndWaterFloodApp.Data;
using Microsoft.EntityFrameworkCore;
using ZSK.Services.ReferenceData.Dtos;
using ZSK.Services.ReferenceData.Interfaces;

namespace ZSK.Infrastructure.Repositories;

public class ZskReferenceRepository : IZskReferenceRepository
{
    private readonly WaterfloodDbContext _context;

    public ZskReferenceRepository(WaterfloodDbContext context)
    {
        _context = context;
    }

    public async Task<ZskReferenceDataDto> GetReferenceDataAsync()
    {
        var wellTypes = await _context.ZskRefWellTypes
            .Select(w => new ZskWellTypeDto
            {
                Code = w.Code,
                Name = w.Name,
                Description = w.Description,
            })
            .ToListAsync();

        var wellStatuses = await _context.ZskRefWellStatuses
            .Select(w => new ZskWellStatusDto
            {
                Code = w.Code,
                Name = w.Name,
                Description = w.Description,
                ColorCode = w.ColorCode,
            })
            .ToListAsync();

        var relationshipStatuses = await _context.ZskRefRelationshipStatuses
            .Select(r => new ZskRelationshipStatusDto
            {
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
            })
            .ToListAsync();

        return new ZskReferenceDataDto
        {
            WellTypes = wellTypes,
            WellStatuses = wellStatuses,
            RelationshipStatuses = relationshipStatuses,
        };
    }

    public async Task<List<ZskMonitoringRuleDto>> GetMonitoringRulesAsync() =>
        await _context.ZskRefMonitoringRules
            .Select(r => new ZskMonitoringRuleDto
            {
                RuleCode = r.RuleCode,
                Name = r.Name,
                Description = r.Description,
                TargetWellType = r.TargetWellType,
                DefaultThresholdValue = r.DefaultThresholdValue,
                Severity = r.Severity,
            })
            .ToListAsync();
}
