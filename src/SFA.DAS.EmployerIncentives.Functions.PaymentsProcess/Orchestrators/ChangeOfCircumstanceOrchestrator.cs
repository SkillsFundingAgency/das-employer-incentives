using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class ChangeOfCircumstanceOrchestrator
    {
        private ILogger<ChangeOfCircumstanceOrchestrator> _logger;

        public ChangeOfCircumstanceOrchestrator(ILogger<ChangeOfCircumstanceOrchestrator> logger)
        {
            _logger = logger;
        }


    }
}
