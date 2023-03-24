using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IChangeOfCircumstancesDataRepository
    {
        Task Save(ChangeOfCircumstance changeOfCircumstance);
        Task<List<ChangeOfCircumstance>> GetList(Expression<Func<Models.ChangeOfCircumstance, bool>> predicate = null);
    }
}
