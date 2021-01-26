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
            var period = collectionCalendar.GetPeriod(DueDate);
            Model.PeriodNumber = period.PeriodNumber;
            Model.PaymentYear = period.AcademicYear;
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
                                      v.CollectionPeriod.AcademicYear == validationResult.CollectionPeriod.AcademicYear);

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

        public bool IsValidated(short collectionYear, byte collectionPeriod)
        {
            return Model.PendingPaymentValidationResultModels.Count() > 0
                   && AllPendingPaymentsForPeriodAreValid(collectionYear, collectionPeriod);
        }

        private bool AllPendingPaymentsForPeriodAreValid(short collectionYear, byte collectionPeriod)
        {
            return Model.PendingPaymentValidationResultModels
                .Where(v =>
                    v.CollectionPeriod.AcademicYear == collectionYear &&
                    v.CollectionPeriod.PeriodNumber == collectionPeriod)
                .All(r => r.Result);
        }

        public bool RequiresNewPayment(PendingPayment pendingPayment)
        {
            return Amount != pendingPayment.Amount || PeriodNumber != pendingPayment.PeriodNumber || PaymentYear != pendingPayment.PaymentYear;
        }

        public override bool Equals(object obj)
        {
            var pendingPayment = obj as PendingPayment;
            if (obj == null)
            {
                return false;
            }

            return Amount == pendingPayment.Amount &&
                   PeriodNumber == pendingPayment.PeriodNumber &&
                   PaymentYear == pendingPayment.PaymentYear &&
                   DueDate == pendingPayment.DueDate &&
                   PaymentMadeDate == pendingPayment.PaymentMadeDate &&
                   EarningType == pendingPayment.EarningType &&
                   ClawedBack == pendingPayment.ClawedBack;
        }
    }
}
