using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationDataRepository : IIncentiveApplicationDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public IncentiveApplicationDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(IncentiveApplicationModel incentiveApplication)
        {
            await _dbContext.AddAsync(incentiveApplication.Map());
        }

        public async Task<IncentiveApplicationModel> Get(Guid incentiveApplicationId)
        {
            var incentiveApplication = await _dbContext.Applications
                .Include(x => x.Apprenticeships)
                .FirstOrDefaultAsync(a => a.Id == incentiveApplicationId);
            if (incentiveApplication != null)
            {
                return incentiveApplication.Map();
            }
            return null;
        }

        public async Task Update(IncentiveApplicationModel incentiveApplication)
        {
            var model = incentiveApplication.Map();
            var existingApplication = await _dbContext.Applications.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existingApplication != null)
            {
                _dbContext.Entry(existingApplication).CurrentValues.SetValues(model);
                _dbContext.RemoveRange(existingApplication.Apprenticeships);
                _dbContext.AddRange(model.Apprenticeships);
            }
        }

        public async Task<List<IncentiveApplicationModel>> FindApplicationsWithoutEarningsCalculated()
        {
            var queryResult = (from result in (_dbContext.Applications.Include(x => x.Apprenticeships)
                               .Where(x => x.Status == Enums.IncentiveApplicationStatus.Submitted
                               && x.Apprenticeships.Any(y => !y.EarningsCalculated)))
                        let item = ApplicationToIncentiveApplicationModel(result)
                        select item).ToList();

            return await Task.FromResult(queryResult);
        }        

        public async Task<IEnumerable<IncentiveApplicationModel>> FindApplicationsByAccountLegalEntityAndUln(long accountLegalEntity, long uln)
        {
            var queryResult = (from result in (_dbContext.Applications.Include(x => x.Apprenticeships)
                                           .Where(x => x.AccountLegalEntityId == accountLegalEntity
                                           && x.Apprenticeships.Any(y => y.ULN  == uln)))
                               let item = ApplicationToIncentiveApplicationModel(result)
                               select item);

            return await Task.FromResult(queryResult);
        }

        public async Task<IEnumerable<IncentiveApplicationModel>> FindApplicationsByAccountLegalEntity(long accountLegalEntity)
        {
            var queryResult = (from result in (_dbContext.Applications.Include(x => x.Apprenticeships)
                    .Where(x => x.AccountLegalEntityId == accountLegalEntity))
                let item = ApplicationToIncentiveApplicationModel(result)
                select item);

            return await Task.FromResult(queryResult);
        }

        private static IncentiveApplicationModel ApplicationToIncentiveApplicationModel(Models.IncentiveApplication application)
        {
            return new IncentiveApplicationModel
            {
                AccountId = application.AccountId,
                AccountLegalEntityId = application.AccountLegalEntityId,
                ApprenticeshipModels = ApprenticeshipsToApprenticeshipModels(application.Apprenticeships),
                DateCreated = application.DateCreated,
                DateSubmitted = application.DateSubmitted,
                Id = application.Id,
                Status = application.Status,
                SubmittedByEmail = application.SubmittedByEmail,
                SubmittedByName = application.SubmittedByName
            };
        }

        private static ICollection<ApprenticeshipModel> ApprenticeshipsToApprenticeshipModels(ICollection<Models.IncentiveApplicationApprenticeship> apprenticeships)
        {
            var apprenticeshipModels = (from apprenticeship in apprenticeships
                                        let apprenticeshipModel = new ApprenticeshipModel
                                        {
                                            ApprenticeshipEmployerTypeOnApproval = apprenticeship.ApprenticeshipEmployerTypeOnApproval,
                                            ApprenticeshipId = apprenticeship.ApprenticeshipId,
                                            DateOfBirth = apprenticeship.DateOfBirth,
                                            EarningsCalculated = apprenticeship.EarningsCalculated,
                                            FirstName = apprenticeship.FirstName,
                                            Id = apprenticeship.Id,
                                            LastName = apprenticeship.LastName,
                                            PlannedStartDate = apprenticeship.PlannedStartDate,
                                            ULN = apprenticeship.ULN,
                                            UKPRN = apprenticeship.UKPRN,
                                            WithdrawnByEmployer = apprenticeship.WithdrawnByEmployer,
                                            WithdrawnByCompliance = apprenticeship.WithdrawnByCompliance,
                                            CourseName = apprenticeship.CourseName,
                                            EmploymentStartDate = apprenticeship.EmploymentStartDate,
                                            StartDatesAreEligible = apprenticeship.StartDatesAreEligible,
                                            Phase = apprenticeship.Phase
                                        }
                                        select apprenticeshipModel).ToList();
            return new Collection<ApprenticeshipModel>(apprenticeshipModels);
        }
    }
}
