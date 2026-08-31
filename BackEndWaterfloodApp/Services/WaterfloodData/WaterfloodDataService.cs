using System.Security.Claims;
using System.Text;
using AutoMapper;
using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Application.Validators;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Extensions.Pagination;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.Monitoring;
using CsvHelper;
using CsvHelper.Configuration;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using ZSK.Services.ReferenceData.Interfaces;

namespace BackEndWaterFloodApp.Services.WaterfloodData;

public interface IWaterfloodDataService
{
    Task<ServiceResponse<WaterfloodRecordDto>> GetByIdAsync(Guid id);
    Task<ServiceResponse<PagedList<WaterfloodRecordDto>>> GetPagedAsync(
        Pagination pagination,
        WaterfloodFilterDto? filter = null
    );
    Task<ServiceResponse<WaterfloodRecordDto>> CreateAsync(CreateWaterfloodRecordDto dto);
    Task<ServiceResponse<WaterfloodRecordDto>> UpdateAsync(UpdateWaterfloodRecordDto dto);
    Task<ServiceResponse<bool>> DeleteAsync(Guid id);
    Task<byte[]> ExportAsync(WaterfloodFilterDto? filter, string format);
    Task<ServiceResponse<List<WaterfloodHistoryPointDto>>> GetHistoryAsync(Guid id);
}

public class WaterfloodDataService : IWaterfloodDataService
{
    private readonly IWaterfloodRepository _repository;
    private readonly IMonitoringService _monitoringService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IValidator<CreateWaterfloodRecordDto> _createValidator;
    private readonly IValidator<UpdateWaterfloodRecordDto> _updateValidator;

    public WaterfloodDataService(
        IWaterfloodRepository repository,
        IMonitoringService monitoringService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IValidator<CreateWaterfloodRecordDto> createValidator,
        IValidator<UpdateWaterfloodRecordDto> updateValidator
    )
    {
        _repository = repository;
        _monitoringService = monitoringService;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ServiceResponse<WaterfloodRecordDto>> GetByIdAsync(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        if (record == null)
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = "Waterflood record not found.",
            };

        return new ServiceResponse<WaterfloodRecordDto> { Data = await MapWithAlertsAsync(record) };
    }

    public async Task<ServiceResponse<PagedList<WaterfloodRecordDto>>> GetPagedAsync(
        Pagination pagination,
        WaterfloodFilterDto? filter = null
    )
    {
        var paged = await _repository.GetPagedAsync(pagination, filter);
        var dtos = new List<WaterfloodRecordDto>();

        foreach (var record in paged)
        {
            var dto = _mapper.Map<WaterfloodRecordDto>(record);
            await _monitoringService.ApplyAlertInfoAsync(dto, record);
            if (filter?.RequiresAttentionOnly != true || dto.RequiresAttention)
                dtos.Add(dto);
        }

        if (filter?.RequiresAttentionOnly == true)
        {
            return new ServiceResponse<PagedList<WaterfloodRecordDto>>
            {
                Data = new PagedList<WaterfloodRecordDto>(
                    dtos,
                    dtos.Count,
                    pagination.PageNumber,
                    pagination.PageSize
                ),
            };
        }

        return new ServiceResponse<PagedList<WaterfloodRecordDto>>
        {
            Data = new PagedList<WaterfloodRecordDto>(
                dtos,
                paged.TotalCount,
                paged.CurrentPage,
                paged.PageSize
            ),
        };
    }

    public async Task<ServiceResponse<WaterfloodRecordDto>> CreateAsync(CreateWaterfloodRecordDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
            };

        var record = _mapper.Map<WaterfloodRecord>(dto);
        record.CreatedBy = GetCurrentUserId();
        record.MeasurementDate = DateTime.SpecifyKind(dto.MeasurementDate, DateTimeKind.Utc);

        await _repository.AddAsync(record);
        if (!await _repository.SaveChangesAsync())
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = "Failed to create waterflood record.",
            };

        var saved = await _repository.GetByIdAsync(record.Id);
        return new ServiceResponse<WaterfloodRecordDto>
        {
            Data = await MapWithAlertsAsync(saved!),
        };
    }

    public async Task<ServiceResponse<WaterfloodRecordDto>> UpdateAsync(UpdateWaterfloodRecordDto dto)
    {
        var record = await _repository.GetByIdAsync(dto.Id);
        if (record == null)
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = "Waterflood record not found.",
            };

        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
            };

        await _repository.AddHistoryAsync(
            new WaterfloodMeasurementHistory
            {
                WellRecordId = record.Id,
                WellName = record.WellName,
                WellTypeCode = record.WellTypeCode,
                FieldName = record.FieldName,
                InjectionRate = record.InjectionRate,
                OilProductionRate = record.OilProductionRate,
                WaterProductionRate = record.WaterProductionRate,
                WaterCut = record.WaterCut,
                InjectionPressure = record.InjectionPressure,
                WellStatusCode = record.WellStatusCode,
                MeasurementDate = record.MeasurementDate,
                CreatedBy = GetCurrentUserId(),
            }
        );

        _mapper.Map(dto, record);
        record.UpdatedBy = GetCurrentUserId();
        record.UpdatedAt = DateTime.UtcNow;
        record.MeasurementDate = DateTime.SpecifyKind(dto.MeasurementDate, DateTimeKind.Utc);
        _repository.Update(record);

        if (!await _repository.SaveChangesAsync())
            return new ServiceResponse<WaterfloodRecordDto>
            {
                Success = false,
                Message = "Failed to update waterflood record.",
            };

        var updated = await _repository.GetByIdAsync(dto.Id);
        return new ServiceResponse<WaterfloodRecordDto>
        {
            Data = await MapWithAlertsAsync(updated!),
        };
    }

    public async Task<ServiceResponse<bool>> DeleteAsync(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        if (record == null)
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = "Waterflood record not found.",
            };

        _repository.Delete(record);
        var saved = await _repository.SaveChangesAsync();
        return new ServiceResponse<bool> { Data = saved, Success = saved };
    }

    public async Task<ServiceResponse<List<WaterfloodHistoryPointDto>>> GetHistoryAsync(Guid id)
    {
        var record = await _repository.GetByIdAsync(id);
        if (record == null)
            return new ServiceResponse<List<WaterfloodHistoryPointDto>>
            {
                Success = false,
                Message = "Waterflood record not found.",
            };

        var history = await _repository.GetHistoryByWellRecordIdAsync(id);
        var points = history
            .Select(h => new WaterfloodHistoryPointDto
            {
                MeasurementDate = h.MeasurementDate,
                InjectionRate = h.InjectionRate,
                OilProductionRate = h.OilProductionRate,
                WaterProductionRate = h.WaterProductionRate,
                WaterCut = h.WaterCut,
                InjectionPressure = h.InjectionPressure,
                WellStatusCode = h.WellStatusCode,
            })
            .ToList();

        points.Add(
            new WaterfloodHistoryPointDto
            {
                MeasurementDate = record.MeasurementDate,
                InjectionRate = record.InjectionRate,
                OilProductionRate = record.OilProductionRate,
                WaterProductionRate = record.WaterProductionRate,
                WaterCut = record.WaterCut,
                InjectionPressure = record.InjectionPressure,
                WellStatusCode = record.WellStatusCode,
            }
        );

        return new ServiceResponse<List<WaterfloodHistoryPointDto>>
        {
            Data = points.OrderBy(p => p.MeasurementDate).ToList(),
        };
    }

    public async Task<byte[]> ExportAsync(WaterfloodFilterDto? filter, string format)
    {
        var records = await _repository.GetAllFilteredAsync(filter);
        var thresholds = await _monitoringService.GetEffectiveThresholdsAsync();

        return format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? ExportExcel(records, thresholds)
            : ExportCsv(records, thresholds);
    }

    private async Task<WaterfloodRecordDto> MapWithAlertsAsync(WaterfloodRecord record)
    {
        var dto = _mapper.Map<WaterfloodRecordDto>(record);
        await _monitoringService.ApplyAlertInfoAsync(dto, record);
        return dto;
    }

    private string? GetCurrentUserId() =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.UserData)?.Value;

    private static byte[] ExportCsv(
        List<WaterfloodRecord> records,
        ZskEffectiveThresholds thresholds
    )
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(
            writer,
            new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        );

        csv.WriteRecords(
            records.Select(r =>
            {
                var (requiresAttention, _) = WaterfloodAlertEvaluator.Evaluate(r, thresholds);
                return new
                {
                    r.WellName,
                    r.WellTypeCode,
                    WellTypeName = r.WellType?.Name,
                    r.FieldName,
                    r.Latitude,
                    r.Longitude,
                    r.InjectionRate,
                    r.OilProductionRate,
                    r.WaterProductionRate,
                    r.WaterCut,
                    r.InjectionPressure,
                    r.WellStatusCode,
                    WellStatusName = r.WellStatus?.Name,
                    r.MeasurementDate,
                    RequiresAttention = requiresAttention,
                };
            })
        );

        writer.Flush();
        return memoryStream.ToArray();
    }

    private static byte[] ExportExcel(
        List<WaterfloodRecord> records,
        ZskEffectiveThresholds thresholds
    )
    {
        ExcelPackage.License.SetNonCommercialPersonal("Waterflood");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Waterflood Data");

        var headers = new[]
        {
            "Well Name",
            "Well Type Code",
            "Well Type",
            "Waterflood Field",
            "Latitude",
            "Longitude",
            "Injection Rate",
            "Oil Production",
            "Water Production",
            "Water Cut",
            "Injection Pressure",
            "Status Code",
            "Status",
            "Measurement Date",
            "Requires Attention",
        };

        for (var i = 0; i < headers.Length; i++)
            sheet.Cells[1, i + 1].Value = headers[i];

        for (var row = 0; row < records.Count; row++)
        {
            var r = records[row];
            var (requiresAttention, _) = WaterfloodAlertEvaluator.Evaluate(r, thresholds);
            sheet.Cells[row + 2, 1].Value = r.WellName;
            sheet.Cells[row + 2, 2].Value = r.WellTypeCode;
            sheet.Cells[row + 2, 3].Value = r.WellType?.Name;
            sheet.Cells[row + 2, 4].Value = r.FieldName;
            sheet.Cells[row + 2, 5].Value = r.Latitude;
            sheet.Cells[row + 2, 6].Value = r.Longitude;
            sheet.Cells[row + 2, 7].Value = r.InjectionRate;
            sheet.Cells[row + 2, 8].Value = r.OilProductionRate;
            sheet.Cells[row + 2, 9].Value = r.WaterProductionRate;
            sheet.Cells[row + 2, 10].Value = r.WaterCut;
            sheet.Cells[row + 2, 11].Value = r.InjectionPressure;
            sheet.Cells[row + 2, 12].Value = r.WellStatusCode;
            sheet.Cells[row + 2, 13].Value = r.WellStatus?.Name;
            sheet.Cells[row + 2, 14].Value = r.MeasurementDate.ToString("yyyy-MM-dd");
            sheet.Cells[row + 2, 15].Value = requiresAttention;
        }

        return package.GetAsByteArray();
    }
}
