namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
     public interface IRequestLogWriter
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public RequestLog Log { get; }
    }
}
