using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.Map
{    
    public static class DomainExtensions
    {
        public static IEnumerable<LegalEntity> Map(this IEnumerable<LegalEntityModel> models)
        {
            return models.Select(q => q.Map());
        }

        public static LegalEntity Map(this LegalEntityModel model)
        {
            return LegalEntity.Create(model);
        }
    }
}
