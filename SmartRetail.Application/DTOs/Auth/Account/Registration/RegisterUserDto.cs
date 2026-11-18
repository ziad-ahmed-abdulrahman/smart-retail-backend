using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetail.Application.DTOs.Auth.Account.Registration
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [PasswordPropertyText]
        public string Password { get; set; } = null!;

        public string? FullName { get; set; }

        public string? StoreName { get; set; }
    }
}
