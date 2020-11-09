using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationDataRepository : IIncentiveApplicationDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public IncentiveApplicationDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<List<IncentiveApplicationModel>> FindApplicationsWithoutApprenticeshipIncentives()
        {
            var queryResult = (from result in (_dbContext.Applications.Include(x => x.Apprenticeships)
                               .Where(x => x.Status == Enums.IncentiveApplicationStatus.Submitted
                               && x.Apprenticeships.Any(y => !y.EarningsCalculated)))
                        let item = ApplicationToIncentiveApplicatonModel(result)
                        select item).ToList();

            return await Task.FromResult(queryResult);
        }

        private static IncentiveApplicationModel ApplicationToIncentiveApplicatonModel(Models.IncentiveApplication application)
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
                                            TotalIncentiveAmount = apprenticeship.TotalIncentiveAmount,
                                            Uln = apprenticeship.Uln
                                        }
                                        select apprenticeshipModel).ToList();
            return new Collection<ApprenticeshipModel>(apprenticeshipModels);
        }
    }
}
