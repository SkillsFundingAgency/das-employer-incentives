using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public interface IValidator<in T> where T: ICommand
    {
        Task<ValidationResult> Validate(T item);
    }
}
