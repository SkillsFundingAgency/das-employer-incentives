using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class StartDateChanged : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public DateTime PreviousStartDate { get; set; }
        public CollectionPeriod PreviousPeriod { get; set; }
        public DateTime NewStartDate { get; set; }
        public ApprenticeshipIncentiveModel Model { get; }

        public StartDateChanged(
            Guid apprenticeshipIncentiveId,
            DateTime previousStartDate,
            CollectionPeriod previousPeriod,
            DateTime newStartDate,
            ApprenticeshipIncentiveModel model)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PreviousStartDate = previousStartDate;
            PreviousPeriod = previousPeriod;
            NewStartDate = newStartDate;
            Model = model;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive start date changed for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} " +
                    $"Start date changed from {PreviousStartDate} to {NewStartDate}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
