namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface IRequestLogWriterWithArgs
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public RequestLogWithArgs Log { get; }
    }
}
