namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class PublishedEvent
    {
        public object Event { get; private set; }
        public bool IsReceived { get; set; }
        public bool IsProcessed { get; set; }
        public PublishedEvent(object @event)
        {
            Event = @event;
        }
    }
}
