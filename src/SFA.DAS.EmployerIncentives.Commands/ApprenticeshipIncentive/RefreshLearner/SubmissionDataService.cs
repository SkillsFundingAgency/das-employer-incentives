using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    internal class SubmissionDataService
    {
        internal SubmissionData GetSubmissionData(LearnerSubmissionDto learnerData, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive, Domain.ValueObjects.CollectionCalendar collectionCalendar)
        {
            var submissionData = new SubmissionData();

            if (learnerData != null)
            {
                submissionData.SetSubmissionDate(learnerData.IlrSubmissionDate);

                var learningFoundStatus = learnerData.LearningFound(incentive);
                submissionData.SetLearningData(new LearningData(learningFoundStatus.LearningFound,
                    learningFoundStatus.NotFoundReason));

                if (learningFoundStatus.LearningFound)
                {
                    submissionData.LearningData.SetStartDate(learnerData.LearningStartDate(incentive));
                    submissionData.LearningData.SetHasDataLock(learnerData.HasProviderDataLocks(incentive));
                    submissionData.LearningData.SetIsInLearning(learnerData.IsInLearning(incentive));
                    submissionData.LearningData.SetIsStopped(learnerData.IsStopped(incentive, collectionCalendar));
                }

                submissionData.SetRawJson(learnerData.RawJson);
            }

            return submissionData;
        }
    }
}
