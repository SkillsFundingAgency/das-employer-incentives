using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
                                                            HashedLegalEntityId = i.HashedLegalEntityId,
                                                            LegalEntityId = i.Id,
                                                            LegalEntityName = i.Name,
                                                            HasSignedIncentivesTerms = i.HasSignedAgreementTerms,
                                                            VrfCaseId = i.VrfCaseId,
                                                            VrfCaseStatus = i.VrfCaseStatus,
                                                            VrfVendorId = i.VrfVendorId,
                                                            VrfCaseStatusLastUpdatedDateTime = i.VrfCaseStatusLastUpdatedDateTime
                                                        }
            ));

            return accounts;
        }

        public static IEnumerable<AccountModel> Map(this IEnumerable<Models.Account> models)
        {
            var accounts = new List<AccountModel>();

            foreach (var model in models)
            {
                var account = accounts.SingleOrDefault(a => a.Id == model.Id);
                if (account == null)
                {
                    account = new AccountModel { Id = model.Id };
                    accounts.Add(account);
                }
                account.LegalEntityModels.Add(MapLegalEntity(model));
            }

            return accounts;
        }

        private static LegalEntityModel MapLegalEntity(Models.Account model)
        {
            return new LegalEntityModel
            {
                Id = model.LegalEntityId,
                AccountLegalEntityId = model.AccountLegalEntityId,
                HasSignedAgreementTerms = model.HasSignedIncentivesTerms,
                Name = model.LegalEntityName,
                HashedLegalEntityId = model.HashedLegalEntityId,
                VrfCaseId = model.VrfCaseId,
                VrfVendorId = model.VrfVendorId,
                VrfCaseStatus = model.VrfCaseStatus,
                VrfCaseStatusLastUpdatedDateTime = model.VrfCaseStatusLastUpdatedDateTime
            };
        }

        public static AccountModel MapSingle(this IList<Models.Account> accounts)
        {
            if (!accounts.Any() || (accounts.Count(s => s.Id == accounts.First().Id) != accounts.Count()))
            {
                return null;
            }

            var model = new AccountModel { Id = accounts.First().Id, LegalEntityModels = new Collection<LegalEntityModel>() };
            accounts.ToList().ForEach(i => model.LegalEntityModels.Add(MapLegalEntity(i)));

            return model;
        }

        public static IEnumerable<AccountDto> MapDto(this IEnumerable<Models.Account> models)
        {
            var accounts = new List<AccountDto>();

            foreach (var model in models)
            {
                var account = accounts.SingleOrDefault(a => a.AccountId == model.Id);
                if (account == null)
                {
                    account = new AccountDto { AccountId = model.Id, LegalEntities = new List<LegalEntityDto>() };
                    accounts.Add(account);
                }
                account.LegalEntities.Add(MapLegalEntityDto(model));
            }

            return accounts;
        }

        private static LegalEntityDto MapLegalEntityDto(Models.Account model)
        {
            return new LegalEntityDto
            {
                AccountId = model.Id,
                AccountLegalEntityId = model.AccountLegalEntityId,
                HasSignedIncentivesTerms = model.HasSignedIncentivesTerms,
                LegalEntityName = model.LegalEntityName,
                VrfVendorId = model.VrfVendorId
            };
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

        private static ICollection<Models.IncentiveApplicationApprenticeship> Map(this ICollection<ApprenticeshipModel> models, Guid applicationId)
        {
            return models.Select(x => new Models.IncentiveApplicationApprenticeship
            {
                Id = x.Id,
                IncentiveApplicationId = applicationId,
                ApprenticeshipId = x.ApprenticeshipId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                DateOfBirth = x.DateOfBirth,
                ApprenticeshipEmployerTypeOnApproval = x.ApprenticeshipEmployerTypeOnApproval,
                PlannedStartDate = x.PlannedStartDate,
                EarningsCalculated = x.EarningsCalculated,
                Uln = x.Uln,
                TotalIncentiveAmount = x.TotalIncentiveAmount,
                UKPRN = x.UKPRN
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
                ApprenticeshipModels = entity.Apprenticeships.Map()
            };
        }

        private static ICollection<ApprenticeshipModel> Map(this ICollection<Models.IncentiveApplicationApprenticeship> models)
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
                TotalIncentiveAmount = x.TotalIncentiveAmount,
                EarningsCalculated = x.EarningsCalculated,
                UKPRN = x.UKPRN
            }).ToList();
        }

        public static LegalEntityDto Map(this LegalEntityModel model, long accountId)
        {
            return new LegalEntityDto
            {
                AccountId = accountId,
                AccountLegalEntityId = model.AccountLegalEntityId,
                HasSignedIncentivesTerms = model.HasSignedAgreementTerms,
                LegalEntityId = model.Id,
                LegalEntityName = model.Name,
                VrfVendorId = model.VrfVendorId
            };
        }
    }
}
