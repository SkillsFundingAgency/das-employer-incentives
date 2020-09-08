using System;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Data.Map
{
    public static class DataExtensions
    {
        public static IEnumerable<Models.Account> Map(this AccountModel model)
        {
            var accounts = new List<Models.Account>();
            model.LegalEntityModels.ToList().ForEach(i =>
                                                        accounts.Add(new Models.Account
                                                        {
                                                            Id = model.Id,
                                                            AccountLegalEntityId = i.AccountLegalEntityId,
                                                            LegalEntityId = i.Id,
                                                            LegalEntityName = i.Name,
                                                            HasSignedIncentivesTerms = i.HasSignedAgreementTerms
                                                        }
            ));

            return accounts;
        }

        public static AccountModel MapSingle(this IEnumerable<Models.Account> accounts)
        {
            if (!accounts.Any() || (accounts.Count(s => s.Id == accounts.First().Id) != accounts.Count()))
            {
                return null;
            }

            var model = new AccountModel { Id = accounts.First().Id, LegalEntityModels = new Collection<LegalEntityModel>() };
            accounts.ToList().ForEach(i => model.LegalEntityModels.Add(new LegalEntityModel { Id = i.LegalEntityId, AccountLegalEntityId = i.AccountLegalEntityId, Name = i.LegalEntityName, HasSignedAgreementTerms = i.HasSignedIncentivesTerms }));

            return model;
        }

        internal static Models.IncentiveApplication Map(this IncentiveApplicationModel model)
        {
            return new Models.IncentiveApplication
            {
                Id = model.Id,
                Status = model.Status,
                DateSubmitted = model.DateSubmitted,
                SubmittedByEmail = model.SubmittedByEmail,
                SubmittedByName = model.SubmittedByName,
                DateCreated = model.DateCreated,
                AccountId = model.AccountId,
                AccountLegalEntityId = model.AccountLegalEntityId,
                Apprenticeships = model.ApprenticeshipModels.Map(model.Id)
            };
        }

        private static ICollection<IncentiveApplicationApprenticeship> Map(this ICollection<ApprenticeshipModel> models, Guid applicationId)
        {
            return models.Select(x => new IncentiveApplicationApprenticeship
            {
                Id = x.Id, 
                IncentiveApplicationId = applicationId, 
                ApprenticeshipId = x.ApprenticeshipId,
                FirstName = x.FirstName, 
                LastName = x.LastName,
                DateOfBirth = x.DateOfBirth,
                ApprenticeshipEmployerTypeOnApproval = x.ApprenticeshipEmployerTypeOnApproval,
                PlannedStartDate = x.PlannedStartDate,
                Uln = x.Uln,
                TotalIncentiveAmount = x.TotalIncentiveAmount
            }).ToList();
        }

        internal static IncentiveApplicationModel Map(this Models.IncentiveApplication entity)
        {
            return new IncentiveApplicationModel
            {
                Id = entity.Id,
                Status = entity.Status,
                DateSubmitted = entity.DateSubmitted,
                SubmittedByEmail = entity.SubmittedByEmail,
                SubmittedByName = entity.SubmittedByName,
                DateCreated = entity.DateCreated,
                AccountId = entity.AccountId,
                AccountLegalEntityId = entity.AccountLegalEntityId,
                ApprenticeshipModels = entity.Apprenticeships.Map(entity.Id)
            };
        }

        private static ICollection<ApprenticeshipModel> Map(this ICollection<Models.IncentiveApplicationApprenticeship> models, Guid applicationId)
        {
            return models.Select(x => new ApprenticeshipModel
            {
                Id = x.Id,
                ApprenticeshipId = x.ApprenticeshipId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                DateOfBirth = x.DateOfBirth,
                ApprenticeshipEmployerTypeOnApproval = x.ApprenticeshipEmployerTypeOnApproval,
                PlannedStartDate = x.PlannedStartDate,
                Uln = x.Uln,
                TotalIncentiveAmount = x.TotalIncentiveAmount
            }).ToList();
        }
    }
}
