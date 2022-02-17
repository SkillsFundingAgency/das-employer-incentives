using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationDataRepository
    {
        Task Add(IncentiveApplicationModel incentiveApplication);
        Task<IncentiveApplicationModel> Get(Guid incentiveApplicationId);
        Task Update(IncentiveApplicationModel incentiveApplication);
        Task<IEnumerable<IncentiveApplicationModel>> FindApplicationsWithoutEarningsCalculated();
        Task<IEnumerable<IncentiveApplicationModel>> FindApplicationsByAccountLegalEntityAndUln(long accountLegalEntity, long uln);
        Task<IEnumerable<IncentiveApplicationModel>> FindApplicationsByAccountLegalEntity(long accountLegalEntity);
    }
}
