using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationQueryRepository
    {
        Task<IncentiveApplicationDto> Get(Expression<Func<IncentiveApplicationDto, bool>> predicate);
        Task<List<IncentiveApplicationDto>> GetList(Expression<Func<IncentiveApplicationDto, bool>> predicate = null);
    }
}
