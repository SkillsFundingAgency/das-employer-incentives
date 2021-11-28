using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class EmploymentCheck : Entity<Guid, EmploymentCheckModel>
    { 
        internal static EmploymentCheck New(
            Guid id,
            Guid apprenticeshipIncentiveId,
            EmploymentCheckType checkType,
            DateTime minimumDate,
            DateTime maximumDate,
            Guid correlationId)
        {
            return new EmploymentCheck(new EmploymentCheckModel
            {
                Id = id,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                CheckType = checkType,
                MinimumDate = minimumDate,
                MaximumDate = maximumDate,
                CorrelationId = correlationId,
                CreatedDateTime = DateTime.Now
            },            
            true);
        }

        internal static EmploymentCheck Get(EmploymentCheckModel model)
        {
            return new EmploymentCheck(model);
        }

        private EmploymentCheck(EmploymentCheckModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
