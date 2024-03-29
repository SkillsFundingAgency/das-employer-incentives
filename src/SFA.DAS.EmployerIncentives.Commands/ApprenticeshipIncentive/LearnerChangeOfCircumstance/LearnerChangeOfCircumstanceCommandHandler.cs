﻿using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommandHandler : ICommandHandler<LearnerChangeOfCircumstanceCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly ICollectionCalendarService _collectionCalendarService;
        private readonly IDateTimeService _dateTimeService;

        public LearnerChangeOfCircumstanceCommandHandler(
            IApprenticeshipIncentiveDomainRepository domainRepository, 
            ILearnerDomainRepository learnerDomainRepository,
            ICollectionCalendarService collectionCalendarService,
            IDateTimeService dateTimeService)
        {
            _domainRepository = domainRepository;
            _learnerDomainRepository = learnerDomainRepository;
            _collectionCalendarService = collectionCalendarService;
            _dateTimeService = dateTimeService;
        }

        public async Task Handle(LearnerChangeOfCircumstanceCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);            

            if (!incentive.HasPossibleChangeOfCircumstances)
            {
                return;
            }

            var learner = await _learnerDomainRepository.GetOrCreate(incentive);
            var collectionCalendar = await _collectionCalendarService.Get();

            incentive.SetBreaksInLearning(learner.LearningPeriods.ToList(), collectionCalendar, _dateTimeService);

            if (learner.HasFoundSubmission)
            {
				if (learner.HasStartDate)
                {
                    incentive.SetStartDateChangeOfCircumstance(learner.StartDate.Value, collectionCalendar, _dateTimeService);
                }

                incentive.SetLearningStoppedChangeOfCircumstance(learner, collectionCalendar);
            }

            incentive.SetHasPossibleChangeOfCircumstances(false);

            await _domainRepository.Save(incentive);
        }
    }
}
