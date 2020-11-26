using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionPeriod
{
    public class ActivateCollectionPeriodCommand : ICommand
    {
        public byte CollectionPeriodNumber { get; private set; }
        public short CollectionPeriodYear { get; private set; }

        public ActivateCollectionPeriodCommand(byte collectionPeriodNumber, short collectionPeriodYear)
        {
            CollectionPeriodNumber = collectionPeriodNumber;
            CollectionPeriodYear = collectionPeriodYear;
        }
    }
}
