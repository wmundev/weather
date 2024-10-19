using AutoMapper;
using weather_domain.DatabaseEntities;
using weather_domain.Entities;

namespace weather_backend.Mapper
{
    public class GlobalMapperProfile : Profile
    {
        public GlobalMapperProfile()
        {
            CreateMap<City, DynamoDbCity>();
        }
    }
}
