using AutoMapper;
using BackEndWaterFloodApp.Application.Dtos.Relationships;
using BackEndWaterFloodApp.Application.Dtos.Thresholds;
using BackEndWaterFloodApp.Application.Dtos.Waterflood;
using BackEndWaterFloodApp.Domain.Entities;
using BackEndWaterFloodApp.Dtos.User;
using BackEndWaterFloodApp.Models;

namespace BackEndWaterFloodApp.AutoMapper;

public class WaterfloodMappingProfile : Profile
{
    public WaterfloodMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User2RegisterDto, User>();
        CreateMap<UserDto2Put, User>();
        CreateMap<UserRolePost, UserRole>();

        CreateMap<User, UserDto>()
            .ForMember(des => des.Roles, src => src.MapFrom(e => e.UserRoles));

        CreateMap<UserRole, RoleDto>()
            .ForMember(e => e.RoleName, opt => opt.MapFrom(ser => ser.zRole.Name));

        CreateMap<User2RegisterDto, User>();
        CreateMap<UserRolePost, UserRole>();
        CreateMap<UserDto2Put, User>();
        CreateMap<User, UserDto2Put>();

        CreateMap<WaterfloodRecord, WaterfloodRecordDto>()
            .ForMember(d => d.WellTypeName, o => o.MapFrom(s => s.WellType.Name))
            .ForMember(d => d.WellStatusName, o => o.MapFrom(s => s.WellStatus.Name))
            .ForMember(d => d.StatusColorCode, o => o.MapFrom(s => s.WellStatus.ColorCode));

        CreateMap<CreateWaterfloodRecordDto, WaterfloodRecord>();
        CreateMap<UpdateWaterfloodRecordDto, WaterfloodRecord>();

        CreateMap<CreateWaterfloodRelationshipDto, InjectorProducerRelationship>();
        CreateMap<UpdateWaterfloodRelationshipDto, InjectorProducerRelationship>();

        CreateMap<AlertThreshold, AlertThresholdDto>();
        CreateMap<UpdateAlertThresholdDto, AlertThreshold>();
    }
}
