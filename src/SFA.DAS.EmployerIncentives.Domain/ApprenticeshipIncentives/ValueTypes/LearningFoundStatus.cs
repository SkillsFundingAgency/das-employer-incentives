namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningFoundStatus
    {
        public bool LearningFound { get; }

        public string NotFoundReason { get; }

        public LearningFoundStatus(string learningNotFoundReason = null)
        {
            NotFoundReason = learningNotFoundReason;
            LearningFound = learningNotFoundReason == null;
        }

        public LearningFoundStatus(bool learningFound)
        {
            LearningFound = learningFound;
        }
    }
}
