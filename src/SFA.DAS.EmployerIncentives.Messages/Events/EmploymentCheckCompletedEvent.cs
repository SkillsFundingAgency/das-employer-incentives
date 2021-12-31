using System;

namespace SFA.DAS.EmployerIncentives.Messages.Events
{
    public class EmploymentCheckCompletedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Result { get; set; }
        public DateTime DateChecked { get; set; }
    }
}
