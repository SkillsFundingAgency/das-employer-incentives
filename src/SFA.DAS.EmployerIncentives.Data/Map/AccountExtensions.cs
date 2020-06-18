using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Domain.Data;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Data.Map
{
    public static class AccountExtensions
    {
        public static Account Map(this IAccountModel accountProperties)
        {
            return new Account
            {
                Id = accountProperties.Id,
                AccountLegalEntityId = accountProperties.AccountLegalEntityId,
                LegalEntityId = accountProperties.LegalEntityModel.Id,
                LegalEntityName = accountProperties.LegalEntityModel.Name
            };
        }

        public static IAccountModel Map(this Account account)
        {
            return new AccountModel
            {
                Id = account.Id,                
                AccountLegalEntityId = account.AccountLegalEntityId,
                LegalEntityModel = new LegalEntityModel
                {
                    Id = account.LegalEntityId,
                    Name = account.LegalEntityName
                }
            };
        }
    }
}
