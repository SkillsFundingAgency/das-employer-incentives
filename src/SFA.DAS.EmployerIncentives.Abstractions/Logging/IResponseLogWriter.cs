namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface IResponseLogWriter
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public ResponseLog Log { get; }
    }
}
