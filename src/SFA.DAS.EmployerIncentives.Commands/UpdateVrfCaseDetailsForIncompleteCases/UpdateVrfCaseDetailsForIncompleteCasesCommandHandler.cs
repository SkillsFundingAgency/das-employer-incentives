using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForIncompleteCases
{
    public class UpdateVrfCaseDetailsForIncompleteCasesCommandHandler : ICommandHandler<UpdateVrfCaseDetailsForIncompleteCasesCommand>
    {
        public UpdateVrfCaseDetailsForIncompleteCasesCommandHandler()
        {
        }

        public async Task Handle(UpdateVrfCaseDetailsForIncompleteCasesCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

