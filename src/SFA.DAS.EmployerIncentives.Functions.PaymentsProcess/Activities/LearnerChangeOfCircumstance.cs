using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
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

        [FunctionName(nameof(LearnerChangeOfCircumstanceActivity))]
        public async Task Update([ActivityTrigger] LearnerChangeOfCircumstanceInput input)
        {
            await _commandDispatcher.Send(new LearnerChangeOfCircumstanceCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
