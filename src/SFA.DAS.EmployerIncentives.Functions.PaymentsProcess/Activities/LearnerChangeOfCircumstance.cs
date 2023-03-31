using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class LearnerChangeOfCircumstanceActivity
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public LearnerChangeOfCircumstanceActivity(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [Function(nameof(LearnerChangeOfCircumstanceActivity))]
        public async Task Update([ActivityTrigger] LearnerChangeOfCircumstanceInput input)
        {
            await _commandDispatcher.Send(new LearnerChangeOfCircumstanceCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
