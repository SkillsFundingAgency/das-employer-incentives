using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Spike.Api.Client.Types;
using SFA.DAS.EmployerIncentives.Spike.API.SampleDomain;

namespace SFA.DAS.EmployerIncentives.Spike.API.ErrorHandler
{
    public static class ErrorHandlerExtensions
    {
        public static IApplicationBuilder UseApiGlobalExceptionHandler(this IApplicationBuilder app)
        {
            async Task Handler(HttpContext context)
            {
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    if (contextFeature.Error is DomainException modelException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.Headers[HttpHeaderNames.SubStatusCode] = ((int)HttpSubStatusCode.DomainException).ToString();
                        await context.Response.WriteAsync(WriteErrorResponse(modelException));
                    }
                    else
                    {
                        //Do nothing but log it (will result in a 500 error)
                    }
                }
            }

            app.UseExceptionHandler(appError =>
            {
                appError.Run(Handler);
            });
            return app;
        }

        public static string WriteErrorResponse(DomainException domainException)
        {
            var response = new ErrorResponse(MapToApiErrors(domainException.DomainErrors));
            return JsonConvert.SerializeObject(response);
        }

        private static List<ErrorDetail> MapToApiErrors(IEnumerable<DomainError> source)
        {
            return source.Select(sourceItem => new ErrorDetail(sourceItem.PropertyName, sourceItem.ErrorMessage)).ToList();
        }
    }
}
