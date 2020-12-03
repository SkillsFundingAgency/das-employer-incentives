using System;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class FundingStream
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}