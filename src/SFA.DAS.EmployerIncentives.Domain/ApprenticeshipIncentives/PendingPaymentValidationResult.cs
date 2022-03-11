using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class PendingPaymentValidationResult : Entity<Guid, PendingPaymentValidationResultModel>
    {
        public string Step => Model.Step;
        public CollectionPeriod CollectionPeriod => Model.CollectionPeriod;
        public bool Result { get; private set; }

        internal static PendingPaymentValidationResult New(
            Guid id,
            CollectionPeriod collectionPeriod,
            string step,
            bool validationResult,
            ValidationOverrideStep overrideResult = null)
        {
            return new PendingPaymentValidationResult(new PendingPaymentValidationResultModel
            {
                Id = id,
                CollectionPeriod = collectionPeriod,
                Step = step,
                ValidationResult = validationResult,
                OverrideResult = (overrideResult != null) && !overrideResult.HasExpired,
                CreatedDateUtc = DateTime.Today
            },
            true);
        }

        internal static PendingPaymentValidationResult Get(PendingPaymentValidationResultModel model)
        {
            return new PendingPaymentValidationResult(model);
        }

        private PendingPaymentValidationResult(PendingPaymentValidationResultModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
            Result = model.OverrideResult ? model.OverrideResult : model.ValidationResult;
        }
    }
}
