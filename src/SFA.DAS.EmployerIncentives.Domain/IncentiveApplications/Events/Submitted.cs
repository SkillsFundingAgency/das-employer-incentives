using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class Submitted : IDomainEvent, ILogWriter<Submitted>
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApplicationId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }

        public void Write(ILogger<Submitted> logger)
        {
            logger.LogInformation($"Application Submitted event with AccountId {AccountId} and IncentiveClaimApplicationId {IncentiveClaimApplicationId} on {SubmittedAt} by {SubmittedBy}");
        }
    }
}
