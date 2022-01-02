using AutoMapper;
using SolidTradeServer.Data.Dtos.HistoricalPosition.Response;
using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Dtos.OngoingKnockout;
using SolidTradeServer.Data.Dtos.Portfolio.Response;
using SolidTradeServer.Data.Dtos.User.Response;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Entities;

namespace SolidTradeServer.MappingProfiles
{
    public class EntityToResponseDtoProfile : Profile
    {
        public EntityToResponseDtoProfile()
        {
            CreateMap<HistoricalPosition, HistoricalPositionResponseDto>();
            
            CreateMap<KnockoutPosition, KnockoutPositionResponseDto>();
            CreateMap<WarrantPosition, WarrantPositionResponseDto>();
            
            CreateMap<OngoingKnockoutPosition, OngoingKnockoutPositionResponseDto>();
            CreateMap<OngoingWarrantPosition, OngoingWarrantPosition>();
            
            CreateMap<Portfolio, PortfolioResponseDto>();
            CreateMap<User, UserResponseDto>();
        }
    }
}