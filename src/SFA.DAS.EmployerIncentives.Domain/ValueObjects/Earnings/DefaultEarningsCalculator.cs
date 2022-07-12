using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;
namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings
{
    public class DefaultEarningsCalculator : EarningsCalculator
    {
        public DefaultEarningsCalculator(
            ApprenticeshipIncentiveModel model,
            Incentive incentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler) : base(model, incentive, collectionCalendar, eventHandler)
        {
        }

        public override void Calculate()
        {
            if (Incentive.IsEligible)
            {
                foreach (var incentivePayment in Incentive.Payments)
                {
                    Calculate(incentivePayment);
                }

                EventHandler.Invoke(new EarningsCalculated
                {
                    ApprenticeshipIncentiveId = Model.Id,
                    AccountId = Model.Account.Id,
                    ApprenticeshipId = Model.Apprenticeship.Id,
                    ApplicationApprenticeshipId = Model.ApplicationApprenticeshipId
                });

                Model.RefreshedLearnerForEarnings = false;
            }
            else
            {
                RemoveUnpaidEarnings();
                ClawbackAllPayments();
            }
        }

        public override void ReCalculate()
        {
            var pendingPaymentsHaveBeenPaid = false;
            foreach (var pendingPaymentModel in Model.PendingPaymentModels)
            {
                if (HasPaidEarning(pendingPaymentModel))
                {
                    pendingPaymentsHaveBeenPaid = true;
                }
            }

            if (!pendingPaymentsHaveBeenPaid)
            {
                RemoveUnpaidEarnings();
                Calculate();
            }
        }

        protected void Calculate(Payment incentivePayment)
        {
            var pendingPayment = PendingPaymentLookup(incentivePayment);

            if (pendingPayment.Exists)
            {
                if (HasPaidEarning(pendingPayment.Model))
                {
                    if (PaymentHasChanged(pendingPayment.Model, incentivePayment))
                    {
                        ClawbackPayment(CollectionCalendar.GetActivePeriod().CollectionPeriod, pendingPayment.Model);
                        AddNewPendingPayment(incentivePayment);
                    }
                }
                else // payment not made
                {
                    var unpaidPayment = PaymentLookup(pendingPayment.Model);

                    if (unpaidPayment.Exists)
                    {
                        DeletePayment(unpaidPayment.Model);
                    }

                    if (PendingPaymentHasChanged(pendingPayment.Model, incentivePayment))
                    {
                        DeletePendingPayment(pendingPayment.Model);
                        AddNewPendingPayment(incentivePayment);
                    }
                }
            }
            else
            {
                AddNewPendingPayment(incentivePayment);
            }
        }
    }
}
