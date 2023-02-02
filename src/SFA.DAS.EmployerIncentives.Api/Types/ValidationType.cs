namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public enum ValidationType
    {
        NotSet = 0,
        HasDaysInLearning = 1,
        IsInLearning = 2,
        HasNoDataLocks = 3,
        EmployedBeforeSchemeStarted = 4,
        EmployedAtStartOfApprenticeship = 5,
        EmployedAt365Days = 6
    }
}
