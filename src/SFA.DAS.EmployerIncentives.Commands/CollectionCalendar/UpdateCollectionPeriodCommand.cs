using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionPeriod
{
    public class UpdateCollectionPeriodCommand : ICommand
    {
        public byte PeriodNumber { get; private set; }
        public string AcademicYear { get; private set; }
        public bool Active { get; private set; }

        public UpdateCollectionPeriodCommand(byte periodNumber, string academicYear, bool active)
        {
            PeriodNumber = periodNumber;
            AcademicYear = academicYear;
            Active = active;
        }
    }
}
