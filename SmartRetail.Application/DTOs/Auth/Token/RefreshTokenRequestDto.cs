using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetail.Application.DTOs.Auth.Token
{
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = null!;
    }
}
