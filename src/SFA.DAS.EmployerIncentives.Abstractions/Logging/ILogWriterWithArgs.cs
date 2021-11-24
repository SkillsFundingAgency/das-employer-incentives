namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface ILogWriterWithArgs
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public LogWithArgs Log { get; }
    }
}
