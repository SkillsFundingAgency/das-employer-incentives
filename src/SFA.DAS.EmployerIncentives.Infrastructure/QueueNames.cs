namespace SFA.DAS.EmployerIncentives.Infrastructure
{
    public static class QueueNames
    {
        public const string LegalEntityAdded = "SFA.DAS.EmployerIncentives.LegalEntityAdded";
        public const string RefreshLegalEntities = "SFA.DAS.EmployerIncentives.RefreshLegalEntities";
        public const string RefreshLegalEntity = "SFA.DAS.EmployerIncentives.RefreshLegalEntity";
        public const string RemovedLegalEntity = "SFA.DAS.EmployerIncentives.LegalEntityRemoved";

        public const string ApprenticeshipIncentivesCreate = "SFA.DAS.EmployerIncentives.CreateIncentive";
        public const string ApprenticeshipIncentivesCalculateEarnings = "SFA.DAS.EmployerIncentives.CalcEarnings";
        public const string CompleteEarningsCalculation = "SFA.DAS.EmployerIncentives.CompleteEarningsCalc";
        public const string AddEmployerVendorId = "SFA.DAS.EmployerIncentives.AddEmployerVendorId";
        public const string ApprenticeshipIncentivesWithdraw = "SFA.DAS.EmployerIncentives.Withdraw";
        public const string EmployerWithdrawal = "SFA.DAS.EmployerIncentives.EmployerWithdrawal";
        public const string ComplianceWithdrawal = "SFA.DAS.EmployerIncentives.ComplianceWithdrawal";
        public const string ValidationOverride = "SFA.DAS.EmployerIncentives.ValidationOverride";

        public const string UpdateVendorRegistrationCaseStatus = "SFA.DAS.EmployerIncentives.UpdateVrfStatus";
        public const string SendEmploymentCheckRequests = "SFA.DAS.EmployerIncentives.RequestEmploymentChecks";
        public const string UpdateEmploymentCheck = "SFA.DAS.EmployerIncentives.UpdateEmploymentCheck";
        public const string RefreshEmploymentCheckCommand = "SFA.DAS.EmployerIncentives.RefreshEmploymentCheck";
        public const string RecalculateEarningsCommand = "SFA.DAS.EmploymentIncentives.RecalculateEarnings";
    }
}
