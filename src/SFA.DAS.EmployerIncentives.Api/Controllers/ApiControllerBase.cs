//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmployerIncentives.Application.Commands;
//using SFA.DAS.EmployerIncentives.Application.Queries;

//namespace SFA.DAS.EmployerIncentives.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Produces("application/json")]
//    public class ApiControllerBase<TController, TCommand, TQuery> : ControllerBase 
//        where TController: Controller where TCommand : ICommand where TQuery : IQuery
//    {
//        private readonly ILogger<TController> _logger;
//        private readonly ICommandHandler<TCommand> _command;
//        private readonly IQueryHandler<TQuery> _query;

//        public ApiControllerBase(ILogger<TController> logger,
//            ICommandHandler<TCommand> command, IQueryHandler<TQuery> query)
//        {
//            _logger = logger;
//            _command = command;
//            _query = query;
//        }

//    }
//}