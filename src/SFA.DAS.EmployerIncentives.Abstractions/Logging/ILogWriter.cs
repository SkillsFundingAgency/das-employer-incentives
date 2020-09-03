using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface ILogWriter<in T>
    {
        public void Write(ILogger<T> logger);
    }
}
