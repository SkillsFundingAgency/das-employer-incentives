﻿namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public class OrchestratorStartResponse
    {
        public string Id { get; set; }
        public string StatusQueryGetUri { get; set; }
        public string SendEventPostUri { get; set; }
        public string TerminatePostUri { get; set; }
        public string PurgeHistoryDeleteUri { get; set; }
    }
}
