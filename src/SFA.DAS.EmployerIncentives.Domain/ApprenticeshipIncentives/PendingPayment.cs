using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class PendingPayment : Entity<Guid, PendingPaymentModel>
    {
        public Account Account => Model.Account;
        public DateTime DueDate => Model.DueDate;
        public decimal Amount => Model.Amount;
        public byte? PeriodNumber => Model.PeriodNumber;
        public short? PaymentYear => Model.PaymentYear;
        public DateTime? PaymentMadeDate => Model.PaymentMadeDate;
        public EarningType EarningType => Model.EarningType;

        public bool IsValidated => Model.PendingPaymentValidationResultModels.Count > 0 && Model.PendingPaymentValidationResultModels.All(r => r.Result);

        public IReadOnlyCollection<PendingPaymentValidationResult> PendingPaymentValidationResults => Model.PendingPaymentValidationResultModels.Map().ToList().AsReadOnly();

        internal static PendingPayment New(
            Guid id,
            Account account,
            Guid apprenticeshipIncentiveId,
            decimal amount,
            DateTime dueDate,
            DateTime calculatedDate,
            EarningType earningType)
        {
            return new PendingPayment(new PendingPaymentModel
            {
                Id = id,
                Account = account,
                ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                Amount = amount,
                CalculatedDate = calculatedDate,
                DueDate = dueDate,
                EarningType = earningType
            },
                true);
        }

        public void SetPaymentPeriod(CollectionCalendar collectionCalendar)
        {
            var period = collectionCalendar.GetPeriod(DueDate);
            Model.PeriodNumber = period.PeriodNumber;
            Model.PaymentYear = period.CalendarYear;
        }

        public void SetPaymentMadeDate(DateTime paymentDate)
        {
            Model.PaymentMadeDate = paymentDate;
		}

        public void AddValidationResult(PendingPaymentValidationResult validationResult)
        {
            var existing = Model
                .PendingPaymentValidationResultModels
                .SingleOrDefault(v => v.Step.Equals(validationResult.Step) &&
                                      v.CollectionPeriod.CalendarMonth == validationResult.CollectionPeriod.CalendarMonth &&
                                      v.CollectionPeriod.CalendarYear == validationResult.CollectionPeriod.CalendarYear);

            if (existing != null)
            {
                Model.PendingPaymentValidationResultModels.Remove(existing);
            }

            Model.PendingPaymentValidationResultModels.Add(validationResult.GetModel());
        }

        internal static PendingPayment Get(PendingPaymentModel model)
        {
            return new PendingPayment(model);
        }

        private PendingPayment(PendingPaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }
    }
}
