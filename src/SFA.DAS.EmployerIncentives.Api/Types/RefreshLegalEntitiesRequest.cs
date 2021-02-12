using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class RefreshLegalEntitiesRequest
    {
        public IEnumerable<AccountLegalEntity> AccountLegalEntities { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
