using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class CommandHandlerWithValidator<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IValidator<T> _validator;

        public CommandHandlerWithValidator(
            ICommandHandler<T> handler,
            IValidator<T> validator)
        {
            _handler = handler;
            _validator = validator;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.Validate(command);

            if (validationResult == null)
            {
                validationResult = new ValidationResult();
                validationResult.AddError(nameof(_validator), "Validator is invalid");
                throw new Exceptions.InvalidRequestException(validationResult.ValidationDictionary);
            }
                
            if(!validationResult.IsValid())
            {
                throw new Exceptions.InvalidRequestException(validationResult.ValidationDictionary);
            }
 
            await _handler.Handle(command, cancellationToken);
           
        }
    }
}
