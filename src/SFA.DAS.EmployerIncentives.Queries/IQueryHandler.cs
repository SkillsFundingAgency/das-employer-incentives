using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public interface IQueryHandler<in TQuery>
    {
        Task<TResponse> Execute<TResponse>(TQuery query);
    }
}
