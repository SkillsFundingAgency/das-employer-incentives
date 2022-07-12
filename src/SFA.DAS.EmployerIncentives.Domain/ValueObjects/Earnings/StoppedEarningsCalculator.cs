using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings
{
    public class StoppedEarningsCalculator : DefaultEarningsCalculator
    {
        private readonly Learner _learner;

        public StoppedEarningsCalculator(
            ApprenticeshipIncentiveModel model,
            Incentive incentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler,
            Learner learner) : base(model, incentive, collectionCalendar, eventHandler)
        {
            if (model.Status != IncentiveStatus.Stopped)
            {
                throw new ArgumentException("invalid apprenticeship incentive status");
            }

            if( (learner != null) && (learner .ApprenticeshipIncentiveId != model.Id))
            {
                throw new ArgumentException("Invalid learner record");
            }
            _learner = learner;
        }

        public override void Calculate()
        {
            if (!IsChangeOfCircumstance())
            {
                return;
            }

            if (Model.PreviousStatus == IncentiveStatus.Stopped)
            {
                foreach (var incentivePayment in Incentive.Payments.OrderBy(p => p.PaymentDate))
                {
                    if (incentivePayment.PaymentDate <= _learner.StoppedStatus.DateStopped.Value)
                    {
                        var pendingPayment = PendingPaymentLookup(incentivePayment);
                        if (pendingPayment.Exists)
                        {
                            if (!HasPaidEarning(pendingPayment.Model))
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

            RemoveUnpaidEarnings(Model.PendingPaymentModels.Where(x => x.DueDate > _learner.StoppedStatus.DateStopped.Value));
            ClawbackPayments(Model.PendingPaymentModels.Where(x => x.DueDate > _learner.StoppedStatus.DateStopped.Value));
        }
        public override void ReCalculate()
        {
            // do nothing
        }

        private bool IsChangeOfCircumstance()
        {
            return _learner != null;
        }
    }
}
