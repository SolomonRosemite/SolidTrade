using AutoMapper;
using SolidTradeServer.Data.Dtos.HistoricalPosition.Response;
using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Dtos.OngoingKnockout.Response;
using SolidTradeServer.Data.Dtos.OngoingWarrant.Response;
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
            CreateMap<OngoingWarrantPosition, OngoingWarrantPositionResponseDto>();
            
            CreateMap<Portfolio, PortfolioResponseDto>();
            CreateMap<User, UserResponseDto>();
        }
    }
}