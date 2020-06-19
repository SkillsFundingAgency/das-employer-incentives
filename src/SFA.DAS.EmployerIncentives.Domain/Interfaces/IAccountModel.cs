using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IAccountModel : IEntityModel<long>
    {
        ICollection<ILegalEntityModel> LegalEntityModels { get; set; }
    }
}
