using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class Submitted : IDomainEvent, ILogWriter
    {
        public long AccountId { get; set; }
        public Guid IncentiveApplicationId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }
        public long AccountLegalEntityId { get; set; }
        public ICollection<ApprenticeshipModel> Apprenticeships { get; set; }

        public Log Log
        {
            get
            {
                var message = $"Application Submitted event with AccountId {AccountId} and IncentiveApplicationId {IncentiveApplicationId} on {SubmittedAt} by {SubmittedBy}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
