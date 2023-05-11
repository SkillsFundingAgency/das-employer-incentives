namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages
{
    public interface ISlackNotification
    {
        SlackMessage Message { get; }
    }
}
