using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication
{
    public class SubmitIncentiveApplicationCommand : ICommand
    {
        public Guid IncentiveApplicationId { get; private set; }
        public long AccountId { get; private set; }
        public DateTime DateSubmitted { get; private set; }
        public string SubmittedByEmail { get; private set; }

        public SubmitIncentiveApplicationCommand(
            Guid incentiveApplicationId,
            long accountId,
            DateTime dateSubmitted,
            string submittedByEmail)
        {
            IncentiveApplicationId = incentiveApplicationId;
            AccountId = accountId;
            DateSubmitted = dateSubmitted;
            SubmittedByEmail = submittedByEmail;
        }
    }
}
