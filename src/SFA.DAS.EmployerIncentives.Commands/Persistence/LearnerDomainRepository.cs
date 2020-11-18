using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class LearnerDomainRepository : ILearnerDomainRepository
    {
        private readonly ILearnerDataRepository _learnerDataRepository;
        private readonly ILearnerFactory _learnerFactory;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public LearnerDomainRepository(
            ILearnerDataRepository learnerDataRepository,
            ILearnerFactory learnerFactory,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _learnerDataRepository = learnerDataRepository;
            _learnerFactory = learnerFactory;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Learner> Find(Guid id)
        {
            var learner = await _learnerDataRepository.Get(id);
            if (learner != null)
            {
                return _learnerFactory.GetExisting(learner);
            }

            return null;
        }

        public async Task<Learner> Get(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            Learner learner;

            var existing = await _learnerDataRepository.GetByApprenticeshipIncentiveId(incentive.Id);
            if (existing != null)
            {
                learner = _learnerFactory.GetExisting(existing);
            }
            else
            {
                learner = _learnerFactory.CreateNew(
                    Guid.NewGuid(),
                    incentive.Id,
                    incentive.Apprenticeship.Id,
                    incentive.Apprenticeship.Provider.Ukprn,
                    incentive.Apprenticeship.UniqueLearnerNumber,
                    DateTime.UtcNow
                );
            }

            var nextPayment = incentive.PendingPayments.Where(pp => pp.PaymentMadeDate == null)
                .OrderBy(pp => pp.DueDate).FirstOrDefault();

            if (nextPayment?.PaymentYear != null && nextPayment.PeriodNumber.HasValue)
            {
                learner.SetNextPendingPayment(new NextPendingPayment(
                    nextPayment.PaymentYear.Value, nextPayment.PeriodNumber.Value, nextPayment.DueDate));
            }

            return learner;
        }

        public async Task Save(Learner aggregate)
        {
            if (aggregate.IsNew)
            {
                await _learnerDataRepository.Add(aggregate.GetModel());
            }
            else
            {
                await _learnerDataRepository.Update(aggregate.GetModel());
            }

            foreach (dynamic domainEvent in aggregate.FlushEvents())
            {
                await _domainEventDispatcher.Send(domainEvent);
            }
        }

    }
}
