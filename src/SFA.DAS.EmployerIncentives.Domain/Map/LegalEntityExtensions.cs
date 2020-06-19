using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.Map
{    
    public static class LegalEntityExtensions
    {
        public static IEnumerable<LegalEntity> Map(this IEnumerable<ILegalEntityModel> models)
        {
            return models.Select(q => q.Map());
        }

        public static LegalEntity Map(this ILegalEntityModel model)
        {
            return LegalEntity.Create(model);
        }
    }
}
