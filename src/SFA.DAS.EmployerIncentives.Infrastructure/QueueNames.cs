namespace SFA.DAS.EmployerIncentives.Infrastructure
{
    public static class QueueNames
    {
        public const string LegalEntityAdded = "SFA.DAS.EmployerIncentives.LegalEntityAdded";
        public const string RefreshLegalEntities = "SFA.DAS.EmployerIncentives.RefreshLegalEntities";
        public const string RefreshLegalEntity = "SFA.DAS.EmployerIncentives.RefreshLegalEntity";
        public const string RemovedLegalEntity = "SFA.DAS.EmployerIncentives.LegalEntityRemoved";

        public const string ApprenticeshipIncentivesCreate = "SFA.DAS.EmployerIncentives.ApprentiveshipIncentive.CreateIncentive";
        public const string ApprenticeshipIncentivesCalculateEarnings = "SFA.DAS.EmployerIncentives.ApprentiveshipIncentive.CalculateEarnings";
        public const string CompleteEarningsCalculation = "SFA.DAS.EmployerIncentives.ApprentiveshipIncentive.CompleteEarningsCalculation";
    }
}
