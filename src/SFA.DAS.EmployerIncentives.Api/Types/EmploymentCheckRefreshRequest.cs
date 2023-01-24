namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class EmploymentCheckRefreshRequest
    {
        public string CheckType { get; set; }
        public Application[] Applications { get; set; }
        public ServiceRequest ServiceRequest { get; set; }
    }
}
