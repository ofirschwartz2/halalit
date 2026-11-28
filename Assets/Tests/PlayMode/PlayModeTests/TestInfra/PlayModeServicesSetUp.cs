using NUnit.Framework;

// Runs once around every PlayMode test (#307). It has no namespace on purpose: a set-up fixture outside any namespace covers
// every test in its assembly, and one inside a namespace covers only that namespace
[SetUpFixture]
public class PlayModeServicesSetUp
{
    [OneTimeSetUp]
    public void UseFakeServices()
    {
        TestServices.UseFakes();
    }

    // The real services come back even when the check fails. Otherwise Play in the editor could run on the fakes if domain reload is ever turned off
    [OneTimeTearDown]
    public void CheckAndUseRealServices()
    {
        try
        {
            AssertWrapper.IsFalse(Authentication.AreServicesStarted(), "A test started Unity Services, so it may have signed in or saved for real");
        }
        finally
        {
            TestServices.UseRealServices();
        }
    }
}
