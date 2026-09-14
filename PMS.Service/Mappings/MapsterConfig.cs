using Mapster;
using PMS.Core.DTOs;
using PMS.Core.DTOs.Auth;
using PMS.Core.DTOs.Students;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
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
