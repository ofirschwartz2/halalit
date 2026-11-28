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

    // No check that Unity Services never started, unlike PlayModeServicesSetUp. Unity does not reset code when it leaves Play mode,
    // so after Ofir plays in the editor the services are still running in edit mode, and the check would fail when no test is at fault
    [OneTimeTearDown]
    public void UseRealServices()
    {
        TestServices.UseRealServices();
    }
}
