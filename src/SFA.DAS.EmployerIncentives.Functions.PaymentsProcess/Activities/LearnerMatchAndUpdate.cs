using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities
{
    public class LearnerMatchAndUpdate
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public LearnerMatchAndUpdate(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;        
        }

        [FunctionName(nameof(LearnerMatchAndUpdate))]
        public async Task Create([ActivityTrigger] LearnerMatchInput input)
        {
            await _commandDispatcher.Send(new RefreshLearnerCommand(input.ApprenticeshipIncentiveId));
        }
    }
}
