using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> Handle(TQuery query);
    }
}
