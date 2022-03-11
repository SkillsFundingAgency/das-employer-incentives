using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IValidationOverrideAuditRepository
    {
        Task Add(ValidationOverrideStepAudit validationOverrideStepAudit);
        Task Delete(Guid id);
    }
}
