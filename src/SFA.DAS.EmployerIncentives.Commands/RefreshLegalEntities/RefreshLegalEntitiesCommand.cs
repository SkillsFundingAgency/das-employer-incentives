using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities
{
    public class RefreshLegalEntitiesCommand : ICommand
    {
        public IEnumerable<AccountLegalEntity> AccountLegalEntities { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }

        public RefreshLegalEntitiesCommand(IEnumerable<AccountLegalEntity> accountLegalEntities, int pageNumber, int pageSize, int totalPages)
        {
            AccountLegalEntities = accountLegalEntities;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = totalPages;
        }
    }
}
