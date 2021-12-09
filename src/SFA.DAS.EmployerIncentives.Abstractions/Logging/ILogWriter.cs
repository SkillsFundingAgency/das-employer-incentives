namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface ILogWriter
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Log Log { get; }
    }
}
