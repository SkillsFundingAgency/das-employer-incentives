using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands
{
    public interface IValidator<in T> where T: ICommand
    {
        Task<ValidationResult> Validate(T item);
    }
}
