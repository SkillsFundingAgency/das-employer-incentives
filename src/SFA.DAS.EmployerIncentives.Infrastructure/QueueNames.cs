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
        
        public const string UpdateVendorRegistrationCaseStatus = "SFA.DAS.EmployerIncentives.UpdateVendorRegistrationCaseStatus";
    }
}
