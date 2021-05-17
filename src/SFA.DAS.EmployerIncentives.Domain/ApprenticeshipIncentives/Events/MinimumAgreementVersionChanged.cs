using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class MinimumAgreementVersionChanged : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public int? PreviousAgreementVersion { get; set; }
        public int NewAgreementVersion { get; set; }
        public ApprenticeshipIncentiveModel Model { get; }

        public MinimumAgreementVersionChanged(
            Guid apprenticeshipIncentiveId,
            int? previousAgreementVersion,
            int newAgreementVersion,
            ApprenticeshipIncentiveModel model)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PreviousAgreementVersion = previousAgreementVersion;
            NewAgreementVersion = newAgreementVersion;
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive minimum agreement required version for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} " +
                    $"MinimumAgreementVersion changed from {PreviousAgreementVersion} to {NewAgreementVersion}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
