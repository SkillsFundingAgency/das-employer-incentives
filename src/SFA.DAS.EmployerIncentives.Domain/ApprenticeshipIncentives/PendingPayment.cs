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
        public AcademicPeriod AcademicPeriod => Model.AcademicPeriod;
        public DateTime? PaymentMadeDate => Model.PaymentMadeDate;
        public EarningType EarningType => Model.EarningType;
        public bool ClawedBack => Model.ClawedBack;

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
            Model.AcademicPeriod = collectionCalendar.GetAcademicPeriod(DueDate);
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
                                      v.AcademicPeriod.AcademicYear == validationResult.AcademicPeriod.AcademicYear &&
                                      v.AcademicPeriod.AcademicYear == validationResult.AcademicPeriod.AcademicYear);

            if (existing != null)
            {
                Model.PendingPaymentValidationResultModels.Remove(existing);
            }

            Model.PendingPaymentValidationResultModels.Add(validationResult.GetModel());
        }

        public void ClawBack()
        {
            Model.ClawedBack = true;
        }

        internal static PendingPayment Get(PendingPaymentModel model)
        {
            return new PendingPayment(model);
        }

        private PendingPayment(PendingPaymentModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }

        public bool IsValidated(AcademicPeriod academicPeriod)
        {
            return Model.PendingPaymentValidationResultModels.Count() > 0
                   && AllPendingPaymentsForPeriodAreValid(academicPeriod);
        }

        private bool AllPendingPaymentsForPeriodAreValid(AcademicPeriod academicPeriod)
        {
            return Model.PendingPaymentValidationResultModels
                .Where(v =>
                    v.AcademicPeriod == academicPeriod)
                .All(r => r.Result);
        }

        public bool RequiresNewPaymentAfterBreakInLearning(IEnumerable<BreakInLearning> breakInLearnings)
        {
            if (breakInLearnings.Any())
            {
                var previousBreakInLearning = breakInLearnings.OrderBy(b => b.StartDate).FirstOrDefault(b => b.StartDate >= DueDate && b.EndDate.HasValue);
                if (previousBreakInLearning != null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool RequiresNewPayment(PendingPayment pendingPayment)
        {
            return Amount != pendingPayment.Amount || AcademicPeriod != pendingPayment.AcademicPeriod;
        }

        public override bool Equals(object obj)
        {
            var pendingPayment = obj as PendingPayment;
            if (pendingPayment == null)
            {
                return false;
            }

            return Amount == pendingPayment.Amount &&
                   AcademicPeriod == pendingPayment.AcademicPeriod &&
                   DueDate == pendingPayment.DueDate &&
                   PaymentMadeDate == pendingPayment.PaymentMadeDate &&
                   EarningType == pendingPayment.EarningType &&
                   ClawedBack == pendingPayment.ClawedBack;
        }
    }
}
