using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiGlobalExceptionHandler(this IApplicationBuilder app)
        {
            static async Task Handler(HttpContext context)
            {
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    if (contextFeature.Error is InvalidRequestException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync(contextFeature.Error.Message);
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
    }
}
