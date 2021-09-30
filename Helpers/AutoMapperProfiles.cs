using AutoMapper;
using platterr_api.Dtos;
using platterr_api.Entities;

namespace platterr_api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Platter, PlatterDto>();
            CreateMap<PlatterFormat, PlatterFormatDto>();
            CreateMap<PlatterDto, Platter>();
            CreateMap<PlatterFormatDto, PlatterFormat>();

            CreateMap<Order, OrderDto>();
            CreateMap<OrderDto, Order>();
            CreateMap<PlatterRequest, PlatterRequestDto>();
            CreateMap<PlatterRequestDto, PlatterRequest>();
        }
    }
}