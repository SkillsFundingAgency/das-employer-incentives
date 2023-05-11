using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class PublishedCommand
    {
        public ICommand Command { get; private set; }
        public bool IsReceived { get; set; }
        public bool IsPublished { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsDelayed { get; set; }
        public bool IsDomainCommand { get; set; }
        public bool IsPublishedWithNoListener { get; set; }
        public bool IsErrored { get; set; }
        public Exception LastError { get; set; }

        public PublishedCommand(ICommand command)
        {
            Command = command;
        }
    }
}
