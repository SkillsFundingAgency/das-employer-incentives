using System;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    public class CalculatePaymentsSteps
    {
        [Given(@"a legal entity does not have a valid vendor Id")]
        public void GivenALegalEntityDoesNotHaveAValidVendorId()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"pending payments for the legal entity are validated")]
        public void WhenPendingPaymentsForTheLegalEntityAreValidated()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the validation fails")]
        public void ThenTheValidationFails()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"validation results are recorded")]
        public void ThenValidationResultsAreRecorded()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"pending payment is marked as not payable")]
        public void ThenPendingPaymentIsMarkedAsNotPayable()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
