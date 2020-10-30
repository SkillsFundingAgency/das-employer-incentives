using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck
{
    public class EarningsResilienceCheckHandler : IQueryHandler<EarningsResilienceCheckRequest, EarningsResilienceCheckResponse>
    {
        public async Task<EarningsResilienceCheckResponse> Handle(EarningsResilienceCheckRequest query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
