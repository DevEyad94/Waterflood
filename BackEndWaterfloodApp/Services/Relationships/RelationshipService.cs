using AutoMapper;
using BackEndWaterFloodApp.Application.Dtos.Relationships;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Domain.Constants;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Helpers;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.WaterfloodData;
using Microsoft.EntityFrameworkCore;

namespace BackEndWaterFloodApp.Services.Relationships;

public interface IRelationshipService
{
    Task<ServiceResponse<List<WaterfloodRelationshipDto>>> GetAllAsync();
    Task<ServiceResponse<WaterfloodRelationshipDto>> GetByIdAsync(Guid id);
    Task<ServiceResponse<WaterfloodRelationshipDto>> CreateAsync(CreateWaterfloodRelationshipDto dto);
    Task<ServiceResponse<WaterfloodRelationshipDto>> UpdateAsync(UpdateWaterfloodRelationshipDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(Guid id);
    Task<ServiceResponse<WaterfloodInjectorDetailDto>> GetInjectorDetailAsync(Guid injectorWellId);
}

public class RelationshipService : IRelationshipService
{
    private readonly IRelationshipRepository _relationshipRepository;
    private readonly IWaterfloodRepository _waterfloodRepository;
    private readonly IWaterfloodDataService _waterfloodDataService;
    private readonly IMapper _mapper;

    public RelationshipService(
        IRelationshipRepository relationshipRepository,
        IWaterfloodRepository waterfloodRepository,
        IWaterfloodDataService waterfloodDataService,
        IMapper mapper
    )
    {
        _relationshipRepository = relationshipRepository;
        _waterfloodRepository = waterfloodRepository;
        _waterfloodDataService = waterfloodDataService;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<List<WaterfloodRelationshipDto>>> GetAllAsync()
    {
        var relationships = await _relationshipRepository.Query().ToListAsync();
        return new ServiceResponse<List<WaterfloodRelationshipDto>>
        {
            Data = relationships.Select(MapToDto).ToList(),
        };
    }

    public async Task<ServiceResponse<WaterfloodRelationshipDto>> GetByIdAsync(Guid id)
    {
        var relationship = await _relationshipRepository.GetByIdAsync(id);
        if (relationship == null)
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = "Waterflood relationship not found.",
            };

        return new ServiceResponse<WaterfloodRelationshipDto> { Data = MapToDto(relationship) };
    }

    public async Task<ServiceResponse<WaterfloodRelationshipDto>> CreateAsync(
        CreateWaterfloodRelationshipDto dto
    )
    {
        var validation = await ValidateRelationshipAsync(dto.InjectorWellId, dto.ProducerWellId);
        if (validation != null)
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = validation,
            };

        var relationship = _mapper.Map<InjectorProducerRelationship>(dto);
        relationship.Distance = await ResolveDistanceAsync(
            dto.Distance,
            dto.InjectorWellId,
            dto.ProducerWellId
        );

        await _relationshipRepository.AddAsync(relationship);

        if (!await _relationshipRepository.SaveChangesAsync())
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = "Failed to create waterflood relationship.",
            };

        var created = await _relationshipRepository.GetByIdAsync(relationship.Id);
        return new ServiceResponse<WaterfloodRelationshipDto> { Data = MapToDto(created!) };
    }

    public async Task<ServiceResponse<WaterfloodRelationshipDto>> UpdateAsync(
        UpdateWaterfloodRelationshipDto dto
    )
    {
        var relationship = await _relationshipRepository.GetByIdAsync(dto.Id);
        if (relationship == null)
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = "Waterflood relationship not found.",
            };

        var validation = await ValidateRelationshipAsync(
            dto.InjectorWellId,
            dto.ProducerWellId,
            dto.Id
        );
        if (validation != null)
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = validation,
            };

        _mapper.Map(dto, relationship);
        relationship.Distance = await ResolveDistanceAsync(
            dto.Distance,
            dto.InjectorWellId,
            dto.ProducerWellId
        );
        _relationshipRepository.Update(relationship);

        if (!await _relationshipRepository.SaveChangesAsync())
            return new ServiceResponse<WaterfloodRelationshipDto>
            {
                Success = false,
                Message = "Failed to update waterflood relationship.",
            };

        var updated = await _relationshipRepository.GetByIdAsync(dto.Id);
        return new ServiceResponse<WaterfloodRelationshipDto> { Data = MapToDto(updated!) };
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(Guid id)
    {
        var relationship = await _relationshipRepository.GetByIdAsync(id);
        if (relationship == null)
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = "Waterflood relationship not found.",
            };

        _relationshipRepository.Delete(relationship);
        var saved = await _relationshipRepository.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = saved, Success = saved };
    }

    public async Task<ServiceResponse<WaterfloodInjectorDetailDto>> GetInjectorDetailAsync(
        Guid injectorWellId
    )
    {
        var injector = await _waterfloodRepository.GetByIdAsync(injectorWellId);
        if (injector == null || injector.WellTypeCode != WaterfloodWellTypeCodes.Injector)
            return new ServiceResponse<WaterfloodInjectorDetailDto>
            {
                Success = false,
                Message = "Waterflood injector well not found.",
            };

        var injectorResponse = await _waterfloodDataService.GetByIdAsync(injectorWellId);
        var relationships = await _relationshipRepository.GetByInjectorIdAsync(injectorWellId);
        var linkedProducers = new List<Application.Dtos.Waterflood.WaterfloodRecordDto>();
        var producerTrends = new List<WaterfloodProducerTrendDto>();

        foreach (var rel in relationships)
        {
            var producerResponse = await _waterfloodDataService.GetByIdAsync(rel.ProducerWellId);
            if (producerResponse.Data != null)
                linkedProducers.Add(producerResponse.Data);

            var producerHistory = await _waterfloodDataService.GetHistoryAsync(rel.ProducerWellId);
            producerTrends.Add(
                new WaterfloodProducerTrendDto
                {
                    WellId = rel.ProducerWellId,
                    WellName = rel.ProducerWell?.WellName ?? producerResponse.Data?.WellName ?? string.Empty,
                    Points = producerHistory.Data ?? new(),
                }
            );
        }

        var injectorHistory = await _waterfloodDataService.GetHistoryAsync(injectorWellId);

        return new ServiceResponse<WaterfloodInjectorDetailDto>
        {
            Data = new WaterfloodInjectorDetailDto
            {
                Injector = injectorResponse.Data!,
                Relationships = relationships.Select(MapToDto).ToList(),
                LinkedProducers = linkedProducers,
                InjectorTrend = injectorHistory.Data ?? new(),
                ProducerTrends = producerTrends,
            },
        };
    }

    private async Task<decimal> ResolveDistanceAsync(
        decimal providedDistance,
        Guid injectorWellId,
        Guid producerWellId
    )
    {
        if (providedDistance > 0)
            return providedDistance;

        var injector = await _waterfloodRepository.GetByIdAsync(injectorWellId);
        var producer = await _waterfloodRepository.GetByIdAsync(producerWellId);
        if (injector == null || producer == null)
            return providedDistance;

        return (decimal)Math.Round(
            GeoDistanceHelper.HaversineDistanceKm(
                injector.Latitude,
                injector.Longitude,
                producer.Latitude,
                producer.Longitude
            ),
            2
        );
    }

    private async Task<string?> ValidateRelationshipAsync(
        Guid injectorWellId,
        Guid producerWellId,
        Guid? excludeId = null
    )
    {
        if (injectorWellId == producerWellId)
            return "Injector and producer wells must be different.";

        var injector = await _waterfloodRepository.GetByIdAsync(injectorWellId);
        var producer = await _waterfloodRepository.GetByIdAsync(producerWellId);

        if (injector == null || injector.WellTypeCode != WaterfloodWellTypeCodes.Injector)
            return "Invalid waterflood injector well.";

        if (producer == null || producer.WellTypeCode != WaterfloodWellTypeCodes.Producer)
            return "Invalid waterflood producer well.";

        var exists = _relationshipRepository
            .Query()
            .Any(r =>
                r.InjectorWellId == injectorWellId
                && r.ProducerWellId == producerWellId
                && (!excludeId.HasValue || r.Id != excludeId.Value)
            );

        return exists ? "This waterflood injector-producer relationship already exists." : null;
    }

    private static WaterfloodRelationshipDto MapToDto(InjectorProducerRelationship relationship) =>
        new()
        {
            Id = relationship.Id,
            InjectorWellId = relationship.InjectorWellId,
            InjectorWellName = relationship.InjectorWell?.WellName ?? string.Empty,
            ProducerWellId = relationship.ProducerWellId,
            ProducerWellName = relationship.ProducerWell?.WellName ?? string.Empty,
            Distance = relationship.Distance,
            RelationshipStatusCode = relationship.RelationshipStatusCode,
            RelationshipStatusName = relationship.RelationshipStatus?.Name ?? string.Empty,
        };
}
