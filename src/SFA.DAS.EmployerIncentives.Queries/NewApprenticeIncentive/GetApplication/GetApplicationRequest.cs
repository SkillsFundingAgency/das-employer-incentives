using System;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationRequest : IQuery
    {
        public long AccountId { get; }
        public Guid ApplicationId { get; }

        public GetApplicationRequest(long accountId, Guid applicationId)
        {
            AccountId = accountId;
            ApplicationId = applicationId;
        }
    }
}
