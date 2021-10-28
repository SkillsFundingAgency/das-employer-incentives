using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class ValidateIncompleteEarningsCalculationCommand : DomainCommand
    {
        public IEnumerable<EarningsCalculationValidation> EarningsCalculationValidations { get; private set; }
        public ValidateIncompleteEarningsCalculationCommand(IEnumerable<EarningsCalculationValidation> earningsCalculationValidation)
        {
            EarningsCalculationValidations = earningsCalculationValidation;
        }
    }
}