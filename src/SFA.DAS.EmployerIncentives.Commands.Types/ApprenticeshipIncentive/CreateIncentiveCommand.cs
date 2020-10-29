using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class CreateIncentiveCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public string LockId => $"{nameof(Account)}_{AccountId}";
        public List<IncentiveApprenticeship> Apprenticeships { get; set; }

        public CreateIncentiveCommand(
            long accountId,
            long accountLegalEntityId,
            List<IncentiveApprenticeship> apprenticeships)
        {
            Apprenticeships = apprenticeships;
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive CreateIncentiveCommand for AccountId {AccountId} and AccountLegalEntityId {AccountLegalEntityId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
        public class IncentiveApprenticeship
        {
            public IncentiveApprenticeship(Guid incentiveApplicationApprenticeshipId, long apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval, DateTime plannedStartDate)
            {
                IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
                ApprenticeshipId = apprenticeshipId;
                FirstName = firstName;
                LastName = lastName;
                DateOfBirth = dateOfBirth;
                Uln = uln;
                ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval;
                PlannedStartDate = plannedStartDate;
            }
            public Guid IncentiveApplicationApprenticeshipId { get; }
            public long ApprenticeshipId { get; }
            public string FirstName { get; }
            public string LastName { get; }
            public DateTime DateOfBirth { get; }
            public long Uln { get; }
            public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval { get; }
            public DateTime PlannedStartDate { get; }
        }
    }
}
