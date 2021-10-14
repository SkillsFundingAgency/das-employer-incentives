using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationQueryRepository
    {
        Task<IncentiveApplicationDto> Get(List<IncentivePaymentProfile> incentivePaymentProfiles, Expression<Func<IncentiveApplicationDto, bool>> predicate);
        Task<List<IncentiveApplicationDto>> GetList(List<IncentivePaymentProfile> incentivePaymentProfiles, Expression<Func<IncentiveApplicationDto, bool>> predicate = null);
    }
}
