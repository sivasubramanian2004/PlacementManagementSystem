using System;
using System.Collections.Generic;
using System.Text;
using Mapster;
using PMS.Core.DTOs;
using PMS.Core.DTOs.Auth;
using PMS.Data.Entities;
namespace PMS.Core.Mappings
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<User, UserBasicDto>
                .NewConfig()
                .Map(dest => dest.UserId, src => src.Id);
        }
    }
}
