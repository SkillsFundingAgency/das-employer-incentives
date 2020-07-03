using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> Send<TQuery, TResult>(TQuery query) where TQuery : IQuery;
    }
}
