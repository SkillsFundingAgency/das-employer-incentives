using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks
{
    public interface IHook
    {
    }
    public interface IHook<T> : IHook
    {
        Action<T> OnReceived { get; set; }
        Action<T> OnProcessed { get; set; }
        Action<Exception, T> OnErrored { get; set; }
    }
}
