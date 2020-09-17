using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public interface ICommandService
    {
        Task Dispatch<T>(T command) where T : ICommand;
    }
}
