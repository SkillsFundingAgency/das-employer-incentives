using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> SendAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
    }
}
