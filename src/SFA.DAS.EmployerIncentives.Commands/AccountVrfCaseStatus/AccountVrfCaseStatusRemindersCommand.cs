using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.AccountVrfCaseStatus
{
    public class AccountVrfCaseStatusRemindersCommand : ICommand
    {
        public DateTime ApplicationCutOffDate { get; private set; }

        public AccountVrfCaseStatusRemindersCommand(DateTime applicationCutOffDate)
        {
            ApplicationCutOffDate = applicationCutOffDate;
        }
    }
}
