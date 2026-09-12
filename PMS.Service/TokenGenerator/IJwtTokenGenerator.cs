using System;
using System.Collections.Generic;
using System.Text;
using PMS.Core.Helpers;

namespace PMS.Service.TokenGenerator
{
    public interface IJwtTokenGenerator
    {
        (string token, DateTime expiresAt) GenerateToken(
            int userId,
            string email,
            UserRole role,
            string firstName,
            string lastName);
    }
}
