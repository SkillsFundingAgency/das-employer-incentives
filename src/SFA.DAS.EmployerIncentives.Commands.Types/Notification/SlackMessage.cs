namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification
{
    public class SlackMessage
    {
        public string Content { get; }

        public SlackMessage(string content)
        {
            Content = content;
        }        
    }
}
