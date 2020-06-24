using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands
{
    public interface IValidator<T>
    {
        Task<ValidationResult> Validate(T item);
    }
}
