using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public interface IMultiEventPublisher
    {
        Task Publish<T>(IEnumerable<T> messages) where T : class;
    }
}
