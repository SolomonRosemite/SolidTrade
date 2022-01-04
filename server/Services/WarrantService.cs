using System;
using System.Threading.Tasks;
using OneOf;
using SolidTradeServer.Data.Dtos.Warrant.Request;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Services
{
    public class WarrantService
    {
        private Task<OneOf<WarrantPosition, ErrorResponse>> GetWarrant(GetWarrantRequestDto data)
        {
            // Todo: Implement Get Warrant handler.
            throw new NotImplementedException();
        }
    }
}