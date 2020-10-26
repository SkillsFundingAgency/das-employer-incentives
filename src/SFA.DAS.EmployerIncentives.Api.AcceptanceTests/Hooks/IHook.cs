using System;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks
{
    public interface IHook
    {
    }
    public interface IHook<T> : IHook
    {
        Action<T> OnReceived { get; set; }
        Action<T> OnProcessed { get; set; }
        /// <summary>
        /// return true to suppress the raising of the exception
        /// </summary>
        Func<Exception, T, bool> OnErrored { get; set; }
    }
}
