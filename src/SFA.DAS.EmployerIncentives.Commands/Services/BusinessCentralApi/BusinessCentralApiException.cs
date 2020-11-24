using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralApiException : ApplicationException
    {
        public HttpStatusCode HttpStatusCode { get; }

        public BusinessCentralApiException(string message, HttpStatusCode httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}
