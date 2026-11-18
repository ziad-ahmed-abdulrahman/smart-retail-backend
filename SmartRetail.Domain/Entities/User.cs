using Microsoft.AspNetCore.Identity;
using SmartRetail.Domain.Entities;

namespace SmartRetail.Domain.Entities
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Address? Address { get; set; }
        public bool IsActive { get; set; } = false;
        public string? OneTimeCode { get; set; }
        public DateTime? OneTimeCodeExpiry { get; set; }
        public string? CodeOperation { get; set; }
        public DateTime? InactivityStartDate { get; set; }
        public List<RefreshToken>? RefreshTokens { get; set; } = new();

        // Navigation property: An owner can have multiple stores
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
