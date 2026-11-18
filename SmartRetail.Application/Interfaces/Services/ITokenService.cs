using SmartRetail.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetail.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<JwtSecurityToken> CreateToken(User user);
    }
}
