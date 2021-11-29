using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class EmploymentCheck : Entity<Guid, EmploymentCheckModel>
    {
        public EmploymentCheckType CheckType => Model.CheckType;
        public DateTime MinimumDate => Model.MinimumDate;
        public DateTime MaximumDate => Model.MaximumDate;
        public Guid? CorrelationId => Model.CorrelationId;

        internal static EmploymentCheck New(Guid id,
            Guid apprenticeshipIncentiveId,
            EmploymentCheckType checkType, 
            DateTime minimumDate, 
            DateTime maximumDate)
        {
            return new EmploymentCheck(new EmploymentCheckModel
            {
                Id = id,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                CheckType = checkType,
                MinimumDate = minimumDate,
                MaximumDate = maximumDate
            },
                true);
        }

        public void SetCorrelationId(Guid correlationId)
        {
            Model.CorrelationId = correlationId;
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
