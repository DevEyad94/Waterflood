using BackEndWaterFloodApp.Controllers;
using BackEndWaterFloodApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZSK.Services.ReferenceData.Dtos;
using ZSK.Services.ReferenceData.Interfaces;

namespace ZSK.Controllers;

[Authorize]
[Route("api/zsk")]
public class ZskReferenceController : BaseApiController
{
    private readonly IZskReferenceService _zskReferenceService;

    public ZskReferenceController(IZskReferenceService zskReferenceService)
    {
        _zskReferenceService = zskReferenceService;
    }

    [HttpGet("reference-data")]
    public async Task<ActionResult<ServiceResponse<ZskReferenceDataDto>>> GetReferenceData()
    {
        var result = await _zskReferenceService.GetReferenceDataAsync();
        return Ok(result);
    }

    [HttpGet("rules")]
    public async Task<ActionResult<ServiceResponse<List<ZskMonitoringRuleDto>>>> GetMonitoringRules()
    {
        var result = await _zskReferenceService.GetMonitoringRulesAsync();
        return Ok(result);
    }
}
