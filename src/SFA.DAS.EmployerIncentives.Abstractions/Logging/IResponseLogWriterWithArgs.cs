namespace SFA.DAS.EmployerIncentives.Abstractions.Logging
{
    public interface IResponseLogWriterWithArgs
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public ResponseLogWithArgs Log { get; }
    }
}
