using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class RevertPaymentsRequest
    {
        public List<Guid> Payments { get; set; }
        public ServiceRequest ServiceRequest { get; set;  }
    }
}