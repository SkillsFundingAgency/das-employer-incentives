using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class EmploymentCheck : Entity<Guid, EmploymentCheckModel>
    {
        public Guid ApprenticeshipIncentiveId => Model.ApprenticeshipIncentiveId;
        public EmploymentCheckType CheckType => Model.CheckType;
        public DateTime MinimumDate => Model.MinimumDate;
        public DateTime MaximumDate => Model.MaximumDate;
        public Guid CorrelationId => Model.CorrelationId;
        public bool? Result => Model.Result;
        public DateTime CreatedDateTime => Model.CreatedDateTime;
        public DateTime? ResultDateTime => Model.ResultDateTime;

        internal static EmploymentCheck New(
            Guid id,
            Guid apprenticeshipIncentiveId,
            EmploymentCheckType checkType,
            DateTime minimumDate,
            DateTime maximumDate,
            Guid correlationId,
            bool? result,
            DateTime createdDateTime,
            DateTime? resultDateTime
        )
        {
            return new EmploymentCheck(new EmploymentCheckModel 
            {
                Id = id,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                CheckType = checkType,
                MinimumDate = minimumDate,
                MaximumDate = maximumDate,
                CorrelationId = correlationId,
                Result = result,
                CreatedDateTime = createdDateTime,
                ResultDateTime = resultDateTime
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
