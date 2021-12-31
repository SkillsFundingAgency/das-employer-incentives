using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi
{
    [Serializable]
    public class EmploymentCheckApiException : ApplicationException
    {
        public HttpStatusCode? HttpStatusCode { get; }

        public EmploymentCheckApiException(HttpStatusCode httpStatusCode) : base(BuildMessage(httpStatusCode))
        {
            HttpStatusCode = httpStatusCode;
        }

        protected EmploymentCheckApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            HttpStatusCode = (HttpStatusCode?)info.GetValue("HttpStatusCode", typeof(HttpStatusCode));
        }

        private static string BuildMessage(HttpStatusCode httpStatusCode)
        {
            if (httpStatusCode >= System.Net.HttpStatusCode.InternalServerError)
            {
                return $"Employment Check API is unavailable and returned an internal code of {httpStatusCode}.";
            }
            return $"Employment Check API returned a server code of {httpStatusCode}.";
        }
    }
}
