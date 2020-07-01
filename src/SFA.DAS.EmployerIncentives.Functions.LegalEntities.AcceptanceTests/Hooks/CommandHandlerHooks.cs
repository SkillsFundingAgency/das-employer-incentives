using SFA.DAS.EmployerIncentives.Application.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks
{
    public class CommandHandlerHooks
    {
        public Action<ICommand> OnHandlerStart { get; set; }
        public Action<ICommand> OnHandlerEnd { get; set; }
        public Action<Exception, ICommand> OnHandlerErrored { get; set; }
    }
}
