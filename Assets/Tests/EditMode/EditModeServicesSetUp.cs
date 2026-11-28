using NUnit.Framework;

// Runs once around every EditMode test (#307), as PlayModeServicesSetUp does for the PlayMode tests.
// It has no namespace on purpose, so it covers every test in this assembly. Its name differs from the PlayMode one,
// because this assembly references TestsPlayMode and two public classes of the same name in the global namespace would clash
[SetUpFixture]
public class EditModeServicesSetUp
{
    [OneTimeSetUp]
    public void UseFakeServices()
    {
        TestServices.StartRunOnFakes();
    }

    // The real services come back even when the check fails
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
