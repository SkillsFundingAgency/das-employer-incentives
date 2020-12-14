using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionPeriod
{
    public class UpdateCollectionPeriodCommand : ICommand
    {
        public byte CollectionPeriodNumber { get; private set; }
        public short CollectionPeriodYear { get; private set; }
        public bool Active { get; private set; }

        public UpdateCollectionPeriodCommand(byte collectionPeriodNumber, short collectionPeriodYear, bool active)
        {
            CollectionPeriodNumber = collectionPeriodNumber;
            CollectionPeriodYear = collectionPeriodYear;
            Active = active;
        }
    }
}
