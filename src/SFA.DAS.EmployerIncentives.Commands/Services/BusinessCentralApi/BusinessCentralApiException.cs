using System;
using System.Net;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    [Serializable]
    public class BusinessCentralApiException : ApplicationException
    {
        public HttpStatusCode? HttpStatusCode { get; }

        public BusinessCentralApiException(string message, HttpStatusCode httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        protected BusinessCentralApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.HttpStatusCode = (HttpStatusCode?)info.GetValue("HttpStatusCode", typeof(HttpStatusCode));
        }
    }
}
