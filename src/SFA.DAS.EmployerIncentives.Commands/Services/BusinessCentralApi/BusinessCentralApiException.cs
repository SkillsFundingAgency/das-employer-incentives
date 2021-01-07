using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    [Serializable]
    public class BusinessCentralApiException : ApplicationException
    {
        public HttpStatusCode? HttpStatusCode { get; }

        public BusinessCentralApiException(HttpStatusCode httpStatusCode, HttpContent httpContent) : base(BuildMessage(httpStatusCode, httpContent))
        {
            HttpStatusCode = httpStatusCode;
        }

        protected BusinessCentralApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            HttpStatusCode = (HttpStatusCode?)info.GetValue("HttpStatusCode", typeof(HttpStatusCode));
        }

        private static string BuildMessage(HttpStatusCode httpStatusCode, HttpContent httpContent)
        {
            var dataSent = httpContent.ReadAsStringAsync().Result;
            if (httpStatusCode >= System.Net.HttpStatusCode.InternalServerError)
            {
                return $"Business Central API is unavailable and returned an internal code of {httpStatusCode}. Data sent: {dataSent}";
            }
            return $"Business Central API returned a server code of {httpStatusCode}. Data sent: {dataSent}";
        }
    }
}
