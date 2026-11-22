using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DropsBankTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator DropTableNamesOnlyStockedItems()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;

        var itemsBank = TestUtils.GetItemsBank();
        var dropsBank = TestUtils.GetDropsBank();

        // WHEN
        var defaultDropsList = dropsBank.GetDefaultDropsList();

        // THEN
        foreach (var dropper in defaultDropsList)
        {
            foreach (var drop in dropper.Value)
            {
                AssertWrapper.IsTrue(itemsBank.IsAvailable(drop.ItemName), $"{dropper.Key} drops {drop.ItemName}, which ItemsBank does not stock", _currentSeed);
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
