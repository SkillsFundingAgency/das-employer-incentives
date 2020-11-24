using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public class Learner : AggregateRoot<Guid, LearnerModel>, ILogWriter
    {
        public long Ukprn => Model.Ukprn;
        public long UniqueLearnerNumber => Model.UniqueLearnerNumber;
        public long ApprenticeshipId => Model.ApprenticeshipId;
        public Guid ApprenticeshipIncentiveId => Model.ApprenticeshipIncentiveId;

        public SubmissionData SubmissionData => Model.SubmissionData;
        public bool SubmissionFound => Model.SubmissionData != null;

        internal static Learner New(
            Guid id,
            Guid apprenticeshipIncentiveId,
            long apprenticeshipId,
            long ukprn,
            long uniqueLearnerNumber,
            DateTime createdDate
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
                    CreatedDate = createdDate
                }, true);
        }

        internal static Learner Get(LearnerModel model)
        {
            return new Learner(model);
        }

        public void SetSubmissionData(SubmissionData submissionData)
        {
            Model.SubmissionData = submissionData;
        }

        private Learner(LearnerModel model, bool isNew = false) : base(model.Id, model, isNew)
        {
        }

        public Log Log
        {
            get
            {
                var message = $"Learner data IsNew : {IsNew} : ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and ApprenticeshipId {ApprenticeshipId} with Ukprn {Ukprn} and UniqueLearnerNumber {UniqueLearnerNumber}. ";

                if (SubmissionFound)
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
