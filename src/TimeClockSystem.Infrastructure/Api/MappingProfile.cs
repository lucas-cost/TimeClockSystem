using AutoMapper;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Infrastructure.Api.DTOs;

namespace TimeClockSystem.Infrastructure.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TimeClockRecord, ApiTimeClockRecordDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString().ToLower()))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => Convert.ToBase64String(File.ReadAllBytes(src.PhotoPath))))
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => "DEVICE_001"));
        }
    }
}
