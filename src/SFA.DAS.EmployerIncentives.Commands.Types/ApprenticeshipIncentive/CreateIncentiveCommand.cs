﻿using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class CreateIncentiveCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; }
        public long AccountLegalEntityId { get; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }
        public Guid IncentiveApplicationApprenticeshipId { get; }
        public long ApprenticeshipId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime DateOfBirth { get; }
        public long Uln { get; }
        public DateTime PlannedStartDate { get; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerTypeOnApproval { get; }
        public long? UKPRN { get; }
        public DateTime SubmittedDate { get; }
        public string SubmittedByEmail { get; }
        public string CourseName { get; }
        public DateTime EmploymentStartDate { get; }
        public Phase Phase { get; }

        public CreateIncentiveCommand(
            long accountId,
            long accountLegalEntityId, Guid incentiveApplicationApprenticeshipId, long apprenticeshipId,
            string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate,
            ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval, long? ukprn, DateTime submittedDate, 
            string submittedByEmail, string courseName, DateTime employmentStartDate, Phase phase)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
            ApprenticeshipId = apprenticeshipId;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Uln = uln;
            PlannedStartDate = plannedStartDate;
            ApprenticeshipEmployerTypeOnApproval = apprenticeshipEmployerTypeOnApproval;
            UKPRN = ukprn;
            SubmittedDate = submittedDate;
            SubmittedByEmail = submittedByEmail;
            CourseName = courseName;
            EmploymentStartDate = employmentStartDate;
            Phase = phase;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message =
                    $"ApprenticeshipIncentive CreateApprenticeshipIncentiveCommand for AccountId {AccountId}, " +
                    $"AccountLegalEntityId {AccountLegalEntityId} " +
                    $"and IncentiveApplicationApprenticeshipId {IncentiveApplicationApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
