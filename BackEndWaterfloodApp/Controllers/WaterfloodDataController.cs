using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;
using BackEndWaterFloodApp.Constants;
using BackEndWaterFloodApp.Extensions;
using BackEndWaterFloodApp.Extensions.Pagination;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.WaterfloodData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndWaterFloodApp.Controllers;

[Route("api/waterflood-data")]
public class WaterfloodDataController : BaseApiController
{
    private readonly IWaterfloodDataService _waterfloodDataService;

    public WaterfloodDataController(IWaterfloodDataService waterfloodDataService)
    {
        _waterfloodDataService = waterfloodDataService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ServiceResponse<PagedList<WaterfloodRecordDto>>>> GetAll(
        [FromQuery] Pagination pagination,
        [FromQuery] WaterfloodFilterDto filter
    )
    {
        var result = await _waterfloodDataService.GetPagedAsync(pagination, filter);
        Response.AddPagination(
            result.Data!.CurrentPage,
            result.Data.PageSize,
            result.Data.TotalCount,
            result.Data.TotalPages
        );
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ServiceResponse<WaterfloodRecordDto>>> GetById(Guid id)
    {
        var result = await _waterfloodDataService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("{id:guid}/history")]
    [Authorize]
    public async Task<ActionResult<ServiceResponse<List<WaterfloodHistoryPointDto>>>> GetHistory(
        Guid id
    )
    {
        var result = await _waterfloodDataService.GetHistoryAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<ActionResult<ServiceResponse<WaterfloodRecordDto>>> Create(
        CreateWaterfloodRecordDto dto
    )
    {
        var result = await _waterfloodDataService.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = Policies.AdminOperatorPolicy)]
    public async Task<ActionResult<ServiceResponse<WaterfloodRecordDto>>> Update(
        UpdateWaterfloodRecordDto dto
    )
    {
        var result = await _waterfloodDataService.UpdateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<ActionResult<ServiceResponse<bool>>> Delete(Guid id)
    {
        var result = await _waterfloodDataService.DeleteAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("export")]
    [Authorize]
    public async Task<IActionResult> Export(
        [FromQuery] WaterfloodFilterDto filter,
        [FromQuery] string format = "csv"
    )
    {
        var data = await _waterfloodDataService.ExportAsync(filter, format);
        var contentType = format.Equals("excel", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "text/csv";
        var extension = format.Equals("excel", StringComparison.OrdinalIgnoreCase) ? "xlsx" : "csv";
        return File(data, contentType, $"waterflood-export.{extension}");
    }
}
