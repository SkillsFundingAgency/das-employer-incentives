
namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateCollectionPeriodRequest
    {
        public byte PeriodNumber { get; set; }
        public string AcademicYear { get; set; }
        public bool Active { get; set; }
    }
}
