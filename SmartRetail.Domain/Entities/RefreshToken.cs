using System.ComponentModel.DataAnnotations.Schema;

namespace SmartRetail.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime ExpiresOn { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokeOn { get; set; }
        public bool IsActive => RevokeOn == null && !IsExpired;

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
