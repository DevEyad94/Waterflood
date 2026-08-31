using BackEndWaterFloodApp.Application.Dtos.Thresholds;
using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Constants;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndWaterFloodApp.Controllers;

[Route("api/monitoring")]
[Authorize]
public class MonitoringController : BaseApiController
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<ServiceResponse<List<WaterfloodRecordDto>>>> GetAlerts()
    {
        var result = await _monitoringService.GetAlertsAsync();
        return Ok(result);
    }

    [HttpGet("thresholds")]
    public async Task<ActionResult<ServiceResponse<AlertThresholdDto>>> GetThresholds()
    {
        var result = await _monitoringService.GetThresholdsAsync();
        return Ok(result);
    }

    [HttpPut("thresholds")]
    [Authorize(Policy = Policies.AdminEngineerPolicy)]
    public async Task<ActionResult<ServiceResponse<AlertThresholdDto>>> UpdateThresholds(
        UpdateAlertThresholdDto dto
    )
    {
        var result = await _monitoringService.UpdateThresholdsAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
