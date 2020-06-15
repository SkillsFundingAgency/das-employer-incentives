using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class HandleAddedLegalEntityEvent
    {
        private readonly ICommandHandler<AddLegalEntityCommand> _handler;
        //private readonly ILogger<AddedLegalEntityEvent> _log;


        public HandleAddedLegalEntityEvent(
                  ICommandHandler<AddLegalEntityCommand> handler)
        {
            _handler = handler;
        }

        [FunctionName("Function1")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup =true)]TimerInfo myTimer)
        {
            await _handler.Handle(null);
        }
    }
}
