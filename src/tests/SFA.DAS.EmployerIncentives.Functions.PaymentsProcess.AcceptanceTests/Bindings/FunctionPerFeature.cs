using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class FunctionPerFeature
    {
        private const int Port = 7002;

        [BeforeFeature("FunctionsPerFeature")]
        public static void StartFunctionsHost(FeatureContext featureContext)
        {
            //   FunctionsController.StartFunctionsHost(Port);


        }


        //[AfterScenario("usingFunctionPerFeature", "usingFunctionPerFeatureWithAdditionalConfiguration")]
        //public static void WriteOutput(FeatureContext featureContext)
        //{
        //    FunctionsController functionsController = FunctionsBindings.GetFunctionsController(featureContext);
        //    functionsController.GetFunctionsOutput().WriteAllToConsoleAndClear();
        //}

        //[AfterFeature("usingFunctionPerFeature", "usingFunctionPerFeatureWithAdditionalConfiguration")]
        //public static void StopFunction(FeatureContext featureContext)
        //{
        //    FunctionsController functionsController = FunctionsBindings.GetFunctionsController(featureContext);
        //    functionsController.TeardownFunctions();
        //}
    }
}
