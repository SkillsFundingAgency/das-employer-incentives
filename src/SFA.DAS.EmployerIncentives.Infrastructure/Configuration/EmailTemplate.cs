
using System;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class EmailTemplate
    {
        public string Name { get; set; }
        public string TemplateId { get; set; }

        public string ReplyToAddress { get; set; }
        public string Subject { get; set; }
    }
}
