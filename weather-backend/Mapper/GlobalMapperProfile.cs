using AutoMapper;
using weather_backend.Dto;
using weather_backend.Models;

namespace weather_backend.Mapper;

public class GlobalMapperProfile : Profile
{
    public GlobalMapperProfile()
    {
        CreateMap<City, DynamoDbCity>();
    }
}