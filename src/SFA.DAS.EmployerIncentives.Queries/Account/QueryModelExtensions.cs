using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Queries.Account
{
    public static class QueryModelExtensions
    {
        public static IEnumerable<LegalEntityDto> ToLegalEntityDto(this IEnumerable<Domain.Accounts.Account> entities)
        {
            return entities.Select(entity => new LegalEntityDto
            {
                // todo
            });
        }
    }
}