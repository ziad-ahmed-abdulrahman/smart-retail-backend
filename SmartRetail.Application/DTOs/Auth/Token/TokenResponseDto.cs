using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetail.Application.DTOs.Auth.Token
{
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiryDate { get; set; }

    }
}
