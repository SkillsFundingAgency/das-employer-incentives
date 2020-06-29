using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public interface IQueryProvider
    {
        Task<TResponse> Execute<TResponse, TQuery>(TQuery query);
    }
}
