﻿using SFA.DAS.EmployerIncentives.Abstractions.Domain;
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
        public bool SuccessfulLearnerMatch => Model.SuccessfulLearnerMatch;
        public bool HasPossibleChangeOfCircumstances { get; private set; }
        public IReadOnlyCollection<LearningPeriod> LearningPeriods => Model.LearningPeriods.OrderBy(l => l.StartDate).ToList().AsReadOnly();
        public DateTime? LastRefreshed => Model.RefreshDate;

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
                HasPossibleChangeOfCircumstances = submissionData.HasChangeOfCircumstances(Model.SubmissionData);
                Model.SubmissionData = submissionData;
            }

            Model.RefreshDate = DateTime.UtcNow;
        }

        public void SetLearningPeriods(IEnumerable<LearningPeriod> learningPeriods)
        {
            if (!LearningPeriodsChanged(LearningPeriods, learningPeriods)) return;

            Model.LearningPeriods.Clear();

            foreach (var learningPeriod in learningPeriods)
            {   
                Model.LearningPeriods.Add(learningPeriod);
            }

            HasPossibleChangeOfCircumstances = true;
        }

        private static bool LearningPeriodsChanged(IEnumerable<LearningPeriod> periods1, IEnumerable<LearningPeriod> periods2)
        {
            return !periods1.OrderBy(p => p.StartDate).SequenceEqual(periods2.OrderBy(p => p.StartDate));
        }

        public int GetDaysInLearning(CollectionPeriod collectionPeriod)
        {
            var daysInLearningForCollectionPeriod = Model.DaysInLearnings.FirstOrDefault(d => d.CollectionPeriod == collectionPeriod);

            return daysInLearningForCollectionPeriod != null ? daysInLearningForCollectionPeriod.NumberOfDays : 0;
        }

        public void ClearDaysInLearning()
        {
            Model.DaysInLearnings.Clear();
        }
        
        public void SetDaysInLearning(CollectionCalendarPeriod collectionCalendarPeriod)
        {
            var censusDate = collectionCalendarPeriod.CensusDate;

            int days = 0;
            foreach (var learningPeriod in Model.LearningPeriods)
            {
                if (learningPeriod.EndDate.Date < censusDate)
                {
                    days += learningPeriod.EndDate.Date.Subtract(learningPeriod.StartDate.Date).Days + 1;
                }
                else
                {
                    days += censusDate.Subtract(learningPeriod.StartDate.Date).Days + 1;
                }
            }

            var daysInLearning = new DaysInLearning(collectionCalendarPeriod.CollectionPeriod, days);
            var existing = Model.DaysInLearnings.SingleOrDefault(d => d.CollectionPeriod == collectionCalendarPeriod.CollectionPeriod);
            if (existing != null)
            {
                Model.DaysInLearnings.Remove(existing);
            }
            
            Model.DaysInLearnings.Add(daysInLearning);
        }

        public void SetSuccessfulLearnerMatch(bool succeeded) => Model.SuccessfulLearnerMatch = succeeded;

        public bool HasFoundSubmission => Model.SubmissionData.SubmissionFound;

        public bool HasStartDate => Model.SubmissionData.SubmissionFound && SubmissionData.LearningData.StartDate.HasValue;
        
        public DateTime? StartDate => Model.SubmissionData.LearningData.StartDate;
        public LearningStoppedStatus StoppedStatus => Model.SubmissionData.LearningData.StoppedStatus;

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
