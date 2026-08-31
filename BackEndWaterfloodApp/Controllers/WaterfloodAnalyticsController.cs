using BackEndWaterFloodApp.Application.Dtos.WaterfloodAnalytics;
using BackEndWaterFloodApp.Application.Interfaces;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndWaterFloodApp.Controllers;

[Route("api/waterflood-analytics")]
[Authorize]
public class WaterfloodAnalyticsController : BaseApiController
{
    private readonly IWaterfloodAnalyticsService _analyticsService;
    private readonly IMonitoringService _monitoringService;

    public WaterfloodAnalyticsController(
        IWaterfloodAnalyticsService analyticsService,
        IMonitoringService monitoringService
    )
    {
        _analyticsService = analyticsService;
        _monitoringService = monitoringService;
    }

    [HttpGet("kpi")]
    public async Task<ActionResult<ServiceResponse<WaterfloodKpiSummaryDto>>> GetKpi(
        [FromQuery] WaterfloodAnalyticsFilterDto filter
    )
    {
        var result = await _analyticsService.GetKpiSummaryAsync(filter);
        var alerts = await _monitoringService.GetAlertsAsync();
        if (result.Data != null)
            result.Data.WellsRequiringAttention = alerts.Data?.Count ?? 0;
        return Ok(result);
    }

    [HttpGet("trends")]
    public async Task<ActionResult<ServiceResponse<WaterfloodTrendsResponseDto>>> GetTrends(
        [FromQuery] WaterfloodAnalyticsFilterDto filter
    )
    {
        var result = await _analyticsService.GetTrendsAsync(filter);
        return Ok(result);
    }
}
