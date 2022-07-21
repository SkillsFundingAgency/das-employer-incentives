using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings
{
    public abstract class EarningsCalculator : ValueObject
    {
        private readonly ApprenticeshipIncentiveModel _model;
        private readonly CollectionCalendar _collectionCalendar;
        private readonly Incentive _incentive;
        private readonly Action<IDomainEvent> _eventHandler;
        protected ApprenticeshipIncentiveModel Model => _model;        
        protected Incentive Incentive => _incentive;
        protected CollectionCalendar CollectionCalendar => _collectionCalendar;
        protected Action<IDomainEvent> EventHandler => _eventHandler;

        public virtual void ReCalculate()
        {
        }

        public virtual void Calculate()
        {
        }

        protected bool HasPaidEarnings()
        {
            return Model.PaymentModels.Any(p => p.PaidDate.HasValue);
        }

        protected bool HasPaidEarning(PendingPaymentModel pendingPaymentModel)
        {
            if (pendingPaymentModel.PaymentMadeDate == null)
            {
                return false;
            }

            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == pendingPaymentModel.Id);
            if (existingPayment == null || existingPayment.PaidDate == null)
            {
                return false;
            }

            return true;
        }

        protected bool PaymentHasChanged(PendingPaymentModel pendingPaymentModel, Payment incentivePayment)
        {
            return incentivePayment.Amount != pendingPaymentModel.Amount || pendingPaymentModel.CollectionPeriod != CollectionCalendar.GetPeriod(incentivePayment.PaymentDate)?.CollectionPeriod;
        }

        protected void Delete(PendingPaymentModel pendingPaymentModel)
        {
            if (Model.PendingPaymentModels.Remove(pendingPaymentModel))
            {
                EventHandler.Invoke(new PendingPaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, pendingPaymentModel));
            }
        }

        protected void Delete(PaymentModel paymentModel, Action OnDeleted = null)
        {
            if (paymentModel != null && paymentModel.PaidDate == null)
            {
                if (Model.PaymentModels.Remove(paymentModel))
                {
                    EventHandler.Invoke(new PaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, paymentModel));
                }

                OnDeleted.Invoke();
            }
        }

        protected void Delete(List<PendingPaymentModel> pendingPaymentModels)
        {
            foreach (var pendingPaymentModel in pendingPaymentModels)
            {
                Delete(pendingPaymentModel);
            }
        }

        protected void Delete(ClawbackPaymentModel clawbackPaymentModel)
        {
            if (Model.ClawbackPaymentModels.Remove(clawbackPaymentModel))
            {
                EventHandler.Invoke(new ClawbackDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, clawbackPaymentModel));
            }
        }
        protected void RemoveUnpaidEarnings()
        {
            RemoveUnpaidEarnings(Model.PendingPaymentModels);
        }

        protected void RemoveUnpaidEarnings(IEnumerable<PendingPaymentModel> pendingPaymentModels)
        {
            pendingPaymentModels
                .Where(x => x.PaymentMadeDate == null)
                .ToList()
                .ForEach(pp => Delete(pp));

            var pendingPaymentsToDelete = new List<PendingPaymentModel>();
            foreach (var paidPendingPayment in pendingPaymentModels.Where(x => x.PaymentMadeDate != null))
            {
                var payment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == paidPendingPayment.Id);

                Delete(payment, () => pendingPaymentsToDelete.Add(paidPendingPayment));
            }

            Delete(pendingPaymentsToDelete);            
        }


        protected void RemoveUnsentClawbacks(IEnumerable<ClawbackPaymentModel> clawbackModels)
        {
            clawbackModels.Where(x => x.DateClawbackSent == null).ToList()
                .ForEach(cb => {
                    if (Model.ClawbackPaymentModels.Remove(cb))
                    {
                        EventHandler.Invoke(new ClawbackDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, cb));
                    }
                });
        }
        protected void ClawbackAllPayments()
        {
            ClawbackPayments(Model.PendingPaymentModels);
        }

        protected void ClawbackPayments(IEnumerable<PendingPaymentModel> pendingPaymentModels)
        {
            ClawbackPayments(pendingPaymentModels, CollectionCalendar.GetActivePeriod().CollectionPeriod);
        }

        protected void ClawbackPayments(IEnumerable<PendingPaymentModel> pendingPaymentModels, CollectionPeriod collectionPeriod)
        {
            foreach (var pendingPaymentModel in pendingPaymentModels.Where(pp => pp.PaymentMadeDate != null))
            {
                ClawbackPayment(pendingPaymentModel);
            }
        }

        protected PendingPayment CreatePendingPayment(Payment incentivePayment)
        {
            var pendingPayment = PendingPayment.New(
                            Guid.NewGuid(),
                            Model.Account,
                            Model.Id,
                            incentivePayment.Amount,
                            incentivePayment.PaymentDate,
                            DateTime.UtcNow,
                            incentivePayment.EarningType
                            );

            pendingPayment.SetPaymentPeriod(CollectionCalendar);

            return pendingPayment;
        }

        protected void ClawbackPayment(PendingPaymentModel pendingPaymentModel)
        {
            pendingPaymentModel.ClawedBack = true;
            var payment = Model.PaymentModels.Single(p => p.PendingPaymentId == pendingPaymentModel.Id);

            if (!Model.ClawbackPaymentModels.Any(c => c.PendingPaymentId == pendingPaymentModel.Id))
            {
                var clawback = ApprenticeshipIncentives.ClawbackPayment.New(
                    Guid.NewGuid(),
                    Model.Account,
                    Model.Id,
                    pendingPaymentModel.Id,
                    -pendingPaymentModel.Amount,
                    DateTime.UtcNow,
                    payment.SubnominalCode,
                    payment.Id,
                    payment.VrfVendorId);

                clawback.SetPaymentPeriod(CollectionCalendar.GetActivePeriod().CollectionPeriod);

                Model.ClawbackPaymentModels.Add(clawback.GetModel());
            }
        }
        protected void AddNewPendingPayment(Payment incentivePayment)
        {
            Model.PendingPaymentModels.Add(CreatePendingPayment(incentivePayment).GetModel());
        }

        protected (bool Exists, PaymentModel Model) PaymentLookup(PendingPaymentModel pendingPaymentModel)
        {
            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == pendingPaymentModel.Id);

            if (existingPayment != null)
            {
                return (true, existingPayment);
            }
            else
            {
                return (false, null);
            }
        }

        protected (bool Exists, PendingPaymentModel Model) PendingPaymentLookup(Payment incenticePayment)
        {
            var existingPendingPayment = Model.PendingPaymentModels.SingleOrDefault(x => x.EarningType == incenticePayment.EarningType && !x.ClawedBack);

            if (existingPendingPayment != null)
            {
                return (true, existingPendingPayment);
            }
            else
            {
                return (false, null);
            }
        }

        protected bool PendingPaymentHasChanged(PendingPaymentModel existingPendingPayment, Payment incentivePayment)
        {
            return !PendingPayment.Get(existingPendingPayment).EquivalentTo(CreatePendingPayment(incentivePayment));
        }

        protected void DeletePayment(PaymentModel existingPayment)
        {
            if (Model.PaymentModels.Remove(existingPayment))
            {
                EventHandler.Invoke(new PaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, existingPayment));
            }
        }

        protected void DeletePendingPayment(PendingPaymentModel existingPendingPayment)
        {
            if (Model.PendingPaymentModels.Remove(existingPendingPayment))
            {
                EventHandler.Invoke(new PendingPaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, existingPendingPayment));
            }
        }

        protected EarningsCalculator(
            ApprenticeshipIncentiveModel model,
            Incentive incentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler)
        {
            _model = model;
            _incentive = incentive;
            _collectionCalendar = collectionCalendar;
            _eventHandler = eventHandler;
        }

        public static EarningsCalculator Create(
            ApprenticeshipIncentive apprenticeshipIncentive,
            CollectionCalendar collectionCalendar,
            Action<IDomainEvent> eventHandler,
            Learner learner = default)
        {
            var model = apprenticeshipIncentive.GetModel();
            var incentive = Incentive.Create(apprenticeshipIncentive);

            return apprenticeshipIncentive.Status switch
            {
                IncentiveStatus.Active => new ActiveEarningsCalculator(model, incentive, collectionCalendar, eventHandler),
                IncentiveStatus.Withdrawn => new WithdrawnEarningsCalculator(model, incentive, collectionCalendar, eventHandler),
                IncentiveStatus.Paused => new PausedEarningsCalculator(model, incentive, collectionCalendar, eventHandler),
                IncentiveStatus.Stopped => new StoppedEarningsCalculator(model, incentive, collectionCalendar, eventHandler, learner),
                _ => null
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _model;
            yield return _collectionCalendar;
            yield return _incentive;
        }
    }
}
