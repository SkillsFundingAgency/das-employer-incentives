namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public class GetLegalEntitiesRequest : IQuery<GetLegalEntitiesResponse>
    {
        public long AccountId { get; }

        public GetLegalEntitiesRequest(long accountId)
        {
            AccountId = accountId;
        }
    }
}
