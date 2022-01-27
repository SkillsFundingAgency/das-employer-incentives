using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class ValidationOverride : Entity<Guid, ValidationOverrideModel>
    {
        internal static ValidationOverride New(
            Guid id,
            Guid apprenticeshipIncentiveId,
            string step,
            DateTime expiryDate
            )
        {
            return new ValidationOverride(new ValidationOverrideModel
            {
                Id = id,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                Step = step,
                ExpiryDate = expiryDate,
            },
                true);
        }

        internal static ValidationOverride Get(ValidationOverrideModel model)
        {
            return new ValidationOverride(model);
        }
        private ValidationOverride(ValidationOverrideModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }

    }
}
