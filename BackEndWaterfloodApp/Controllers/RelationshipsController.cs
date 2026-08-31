using BackEndWaterFloodApp.Application.Dtos.Relationships;
using BackEndWaterFloodApp.Constants;
using BackEndWaterFloodApp.Models;
using BackEndWaterFloodApp.Services.Relationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndWaterFloodApp.Controllers;

[Route("api/relationships")]
[Authorize]
public class RelationshipsController : BaseApiController
{
    private readonly IRelationshipService _relationshipService;

    public RelationshipsController(IRelationshipService relationshipService)
    {
        _relationshipService = relationshipService;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<WaterfloodRelationshipDto>>>> GetAll()
    {
        var result = await _relationshipService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<WaterfloodRelationshipDto>>> GetById(Guid id)
    {
        var result = await _relationshipService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("injector/{injectorWellId:guid}")]
    public async Task<ActionResult<ServiceResponse<WaterfloodInjectorDetailDto>>> GetInjectorDetail(
        Guid injectorWellId
    )
    {
        var result = await _relationshipService.GetInjectorDetailAsync(injectorWellId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<ActionResult<ServiceResponse<WaterfloodRelationshipDto>>> Create(
        CreateWaterfloodRelationshipDto dto
    )
    {
        var result = await _relationshipService.CreateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<ActionResult<ServiceResponse<WaterfloodRelationshipDto>>> Update(
        UpdateWaterfloodRelationshipDto dto
    )
    {
        var result = await _relationshipService.UpdateAsync(dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminPolicy)]
    public async Task<ActionResult<ServiceResponse<bool>>> Delete(Guid id)
    {
        var result = await _relationshipService.DeleteAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}
