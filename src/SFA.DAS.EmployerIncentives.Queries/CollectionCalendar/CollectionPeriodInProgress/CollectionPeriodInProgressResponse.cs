namespace SFA.DAS.EmployerIncentives.Queries.CollectionCalendar.CollectionPeriodInProgress
{
    public class CollectionPeriodInProgressResponse
    {
        public bool IsInProgress { get; }

        public CollectionPeriodInProgressResponse(bool isInProgress)
        {
            IsInProgress = isInProgress;
        }
    }
}
