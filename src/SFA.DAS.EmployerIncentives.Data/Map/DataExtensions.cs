using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Data.Map
{
    public static class DataExtensions
    {
        public static IEnumerable<AccountTable> Map(this AccountModel model)
        {
            var accounts = new List<AccountTable>();
            model.LegalEntityModels.ToList().ForEach(i =>
                                                        accounts.Add(new AccountTable
                                                        {
                                                            Id = model.Id,
                                                            AccountLegalEntityId = i.AccountLegalEntityId,
                                                            LegalEntityId = i.Id,
                                                            LegalEntityName = i.Name
                                                        }
            ));

            return accounts;
        }

         public static AccountModel MapSingle(this IEnumerable<AccountTable> accounts)
        {   
            if (!accounts.Any() || (accounts.Count(s => s.Id == accounts.First().Id) != accounts.Count()))
            {
                return null;
            }

            var model = new AccountModel { Id = accounts.First().Id, LegalEntityModels = new Collection<LegalEntityModel>() };
            accounts.ToList().ForEach(i => model.LegalEntityModels.Add(new LegalEntityModel { Id = i.LegalEntityId, AccountLegalEntityId = i.AccountLegalEntityId, Name = i.LegalEntityName }));

            return model;
        }
    }
}
