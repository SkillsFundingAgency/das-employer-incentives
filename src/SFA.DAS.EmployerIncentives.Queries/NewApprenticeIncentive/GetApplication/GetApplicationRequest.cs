using System;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication
{
    public class GetApplicationRequest : IQuery
    {
        public Guid ApplicationId { get; }

        public GetApplicationRequest(Guid applicationId)
        {
            ApplicationId = applicationId;
        }
    }
}
