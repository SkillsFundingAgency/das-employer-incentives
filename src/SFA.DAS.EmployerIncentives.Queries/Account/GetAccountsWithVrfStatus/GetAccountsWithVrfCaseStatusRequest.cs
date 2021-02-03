
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus
{
    public class GetAccountsWithVrfCaseStatusRequest : IQuery
    {
        public string VrfStatus { get; }

        public GetAccountsWithVrfCaseStatusRequest(string vrfStatus)
        {
            VrfStatus = vrfStatus;
        }
    }
}
