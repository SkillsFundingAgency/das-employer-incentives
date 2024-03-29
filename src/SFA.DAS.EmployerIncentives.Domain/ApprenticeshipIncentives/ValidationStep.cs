﻿namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public static class ValidationStep
    {
        public const string HasBankDetails = "HasBankDetails";
        public const string IsInLearning = "IsInLearning";
        public const string HasLearningRecord = "HasLearningRecord";
        public const string HasNoDataLocks = "HasNoDataLocks";
        public const string HasIlrSubmission = "HasIlrSubmission";
        public const string HasDaysInLearning = "HasDaysInLearning";
        public const string PaymentsNotPaused = "PaymentsNotPaused";
        public const string HasSignedMinVersion = "HasSignedMinVersion";
        public const string LearnerMatchSuccessful = "LearnerMatchSuccessful";
        public const string EmployedAtStartOfApprenticeship = "EmployedAtStartOfApprenticeship";
        public const string EmployedBeforeSchemeStarted = "EmployedBeforeSchemeStarted";
        public const string BlockedForPayments = "BlockedForPayments";
        public const string EmployedAt365Days = "EmployedAt365Days";
    }
}