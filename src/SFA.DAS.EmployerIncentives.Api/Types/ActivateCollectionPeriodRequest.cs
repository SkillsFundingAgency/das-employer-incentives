
namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class ActivateCollectionPeriodRequest
    {
        public byte CollectionPeriodNumber { get; set; }
        public short CollectionPeriodYear { get; set; }
        public bool Active { get; set; }
    }
}
