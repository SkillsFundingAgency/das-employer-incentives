using Microsoft.AspNetCore.Http;
using SFA.DAS.UnitOfWork.Managers;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.UnitOfWork
{
    public class UnitOfWorkManagerMiddleware : IMiddleware
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public UnitOfWorkManagerMiddleware(IUnitOfWorkManager unitOfWorkManager)
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await _unitOfWorkManager.BeginAsync();

            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await _unitOfWorkManager.EndAsync(ex);

                throw;
            }

            await _unitOfWorkManager.EndAsync();
        }
    }
}
