using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class Learner : AggregateRoot<Guid, LearnerModel>, ILogWriter
    {
        public long Ukprn => Model.Ukprn;
        public long UniqueLearnerNumber => Model.UniqueLearnerNumber;
        public long ApprenticeshipId => Model.ApprenticeshipId;
        public Guid ApprenticeshipIncentiveId => Model.ApprenticeshipIncentiveId;

        public SubmissionData SubmissionData => Model.SubmissionData;

        internal static Learner New(
            Guid id,
            Guid apprenticeshipIncentiveId,
            long apprenticeshipId,
            long ukprn,
            long uniqueLearnerNumber
            )
        {
            return new Learner(
                new LearnerModel
                {
                    Id = id,
                    ApprenticeshipIncentiveId = apprenticeshipIncentiveId,
                    ApprenticeshipId = apprenticeshipId,
                    Ukprn = ukprn,
                    UniqueLearnerNumber = uniqueLearnerNumber,
                    SubmissionData = new SubmissionData()
                }, true);
        }

        internal static Learner Get(LearnerModel model)
        {
            return new Learner(model);
        }

        public void SetSubmissionData(SubmissionData submissionData)
        {
            if (submissionData == null)
            {
                Model.SubmissionData = new SubmissionData();
            }
            else
            {
                Model.SubmissionData = submissionData;
            }
        }

        public void SetLearningPeriods(IEnumerable<LearningPeriod> learningPeriods)
        {
            Model.LearningPeriods.Clear();

            foreach (var learningPeriod in learningPeriods)
            {   
                Model.LearningPeriods.Add(learningPeriod);
            }
        }

        public int GetDaysInLearning(CollectionPeriod collectionPeriod)
        {
            var daysInLearningForCollectionPeriod = Model.DaysInLearnings.FirstOrDefault(d => d.CollectionYear == collectionPeriod.CalendarYear && d.CollectionPeriodNumber == collectionPeriod.PeriodNumber);

            return daysInLearningForCollectionPeriod != null ? daysInLearningForCollectionPeriod.NumberOfDays : 0;
        }

        public void SetDaysInLearning(CollectionPeriod collectionPeriod)
        {
            var censusDate = collectionPeriod.CensusDate;

            int days = 0;
            foreach (var learningPeriod in Model.LearningPeriods)
            {
                if(!learningPeriod.EndDate.HasValue)
                {
                    days += censusDate.Subtract(learningPeriod.StartDate.Date).Days + 1;
                }
                else
                {
                    if (learningPeriod.EndDate.Value.Date < censusDate)
                    {
                        days += learningPeriod.EndDate.Value.Date.Subtract(learningPeriod.StartDate.Date).Days + 1;
                    }
                    else
                    {
                        days += censusDate.Subtract(learningPeriod.StartDate.Date).Days + 1;
                    }
                }
            }

            var daysInLearning = new DaysInLearning(collectionPeriod.PeriodNumber, collectionPeriod.CalendarYear, days);
            var existing = Model.DaysInLearnings.SingleOrDefault(d => d.CollectionPeriodNumber == collectionPeriod.PeriodNumber && d.CollectionYear == collectionPeriod.CalendarYear);
            if (existing != null)
            {
                Model.DaysInLearnings.Remove(existing);
            }
            
            Model.DaysInLearnings.Add(daysInLearning);
        }

        private Learner(LearnerModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }

        public Log Log
        {
            get
            {
                var message = $"Learner data IsNew : {IsNew} : ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and ApprenticeshipId {ApprenticeshipId} with Ukprn {Ukprn} and UniqueLearnerNumber {UniqueLearnerNumber}. ";

                if (SubmissionData.SubmissionFound)
                {
                    message += ((ILogWriter)SubmissionData).Log.OnProcessed;
                }
                else
                {
                    message += "Submission data not found.";
                }

                return new Log
                {
                    OnProcessing = () => message,
                    OnProcessed = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
