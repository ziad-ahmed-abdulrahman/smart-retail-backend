using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SmartRetail.Application.DTOs.Auth.Account.ChangePassword
{
    public class ChangePasswordDto
    {
        [Required]
        [PasswordPropertyText]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [PasswordPropertyText]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = null!;
    }
}
