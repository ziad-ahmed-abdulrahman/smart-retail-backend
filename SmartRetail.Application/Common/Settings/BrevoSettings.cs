namespace SmartRetail.Application.Common.Settings
{
    public class BrevoSettings
    {
        public string Host { get; set; } = "smtp-relay.brevo.com";
        public int Port { get; set; } = 587;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "tilawat.3758@gmail.com";
        public string DisplayName { get; set; } = "MUDAR";
    }
}
