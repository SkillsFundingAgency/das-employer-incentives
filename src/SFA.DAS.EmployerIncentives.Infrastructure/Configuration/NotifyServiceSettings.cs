using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class NotifyServiceSettings
    {
        public string SystemId { get; set; }
        public string ApiBaseUrl { get; set; }
        public string ClientToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public string Tenant { get; set; }
        public List<EmailTemplate> EmailTemplates { get; set; }
    }
}
