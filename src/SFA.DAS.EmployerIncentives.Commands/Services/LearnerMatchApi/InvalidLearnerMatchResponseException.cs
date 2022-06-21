using System;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class InvalidLearnerMatchResponseException : Exception
    {
        public string ResponseJson { get; }

        public InvalidLearnerMatchResponseException(string responseJson)
        {
            ResponseJson = responseJson;
        }
    }
}
