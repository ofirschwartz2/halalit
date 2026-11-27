using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Tarnish (#231): a free valuable turns Diamond to Gold to Silver to nothing, one step every few seconds.
public class TarnishTests
{
    private const string VALUABLES_PREFABS_PATH = "Prefabs/Valuables";
    private const string DIAMOND_PREFAB_NAME = "DiamondValuable";
    private const string GOLD_PREFAB_NAME = "GoldValuable";
    private const string SILVER_PREFAB_NAME = "SliverValuable";
    private const string VALUABLE_BASE_PREFAB_NAME = "ValuableBase";
    private const float WAIT_MARGIN = 1f;
    // The test counts physics steps from the drop; the valuable's clock may start one step later.
    private const float BLINK_SAMPLE_MARGIN = 0.1f;
    private const float POSITION_DELTA = 0.1f;
    private const float VELOCITY_DELTA = 0.05f;
    // The Diamond must still move clearly faster than VELOCITY_DELTA when it tarnishes, or a Gold at rest would pass too.
    private const float MIN_SPEED_AT_TARNISH = 4 * VELOCITY_DELTA;
    private static readonly Vector2 COIN_OFFSET_FROM_HALALIT = new(5, 0);
    private static readonly Vector2 MOVING_COIN_OFFSET_FROM_HALALIT = new(3, 0);
    // With the valuables' linear drag (0.3) this is about 0.32 after 8 seconds, and the coin stays inside the world (about 11 units).
    private static readonly Vector2 MOVING_COIN_VELOCITY = new(3.5f, 0);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator PrefabChainIsDiamondGoldSilver()
    {
        // GIVEN
        yield return null;

        // WHEN
        var diamondTarnish = TestUtils.GetTarnish(LoadValuablePrefab(DIAMOND_PREFAB_NAME));
        var goldTarnish = TestUtils.GetTarnish(LoadValuablePrefab(GOLD_PREFAB_NAME));
        var silverTarnish = TestUtils.GetTarnish(LoadValuablePrefab(SILVER_PREFAB_NAME));
        var valuableBase = LoadValuablePrefab(VALUABLE_BASE_PREFAB_NAME);

        // THEN
        AssertWrapper.IsNotNull(diamondTarnish.GetNextStepPrefab(), _currentSeed, "Diamond Has No Next Tarnish Step");
        AssertWrapper.AreEqual(GOLD_PREFAB_NAME, diamondTarnish.GetNextStepPrefab().name, "Diamond Does Not Tarnish Into Gold", _currentSeed);
        AssertWrapper.IsNotNull(goldTarnish.GetNextStepPrefab(), _currentSeed, "Gold Has No Next Tarnish Step");
        AssertWrapper.AreEqual(SILVER_PREFAB_NAME, goldTarnish.GetNextStepPrefab().name, "Gold Does Not Tarnish Into Silver", _currentSeed);
        AssertWrapper.IsNull(silverTarnish.GetNextStepPrefab(), _currentSeed, "Silver Should Vanish, Not Tarnish Into Another Valuable");
        AssertWrapper.IsTrue(valuableBase.GetComponent<ValuableTarnish>() == null, "ValuableBase Should Not Tarnish", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DiamondTurnsIntoGoldAfterStepDuration()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + COIN_OFFSET_FROM_HALALIT;
        var valuablesBeforeDrop = TestUtils.GetValuables();
        var diamond = TestUtils.DropValuable(ValuableName.DIAMOND, dropPosition);
        var stepDuration = TestUtils.GetTarnish(diamond).GetStepDuration();

        // WHEN
        yield return new WaitForSeconds(stepDuration - WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(diamond != null && diamond.activeSelf, "Diamond Tarnished Before Its Step Duration", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetValuableName(diamond) == ValuableName.DIAMOND, "Diamond Changed Before Its Step Duration", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(2 * WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(diamond == null, "Diamond Did Not Tarnish After Its Step Duration", _currentSeed);
        var newValuables = GetNewValuables(valuablesBeforeDrop);
        AssertWrapper.AreEqual(1, newValuables.Length, "Tarnished Diamond Did Not Leave Exactly One Valuable", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetValuableName(newValuables[0]) == ValuableName.GOLD, "Tarnished Diamond Did Not Turn Into Gold", _currentSeed);
        AssertWrapper.AreEqual(dropPosition, (Vector2)newValuables[0].transform.position, "Gold Did Not Appear Where The Diamond Was", _currentSeed, POSITION_DELTA);
        AssertWrapper.AreEqual(Vector2.zero, newValuables[0].GetComponent<Rigidbody2D>().velocity, "Gold Of A Still Diamond Is Moving", _currentSeed, VELOCITY_DELTA);
    }

    [UnityTest]
    public IEnumerator TarnishKeepsVelocity()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + MOVING_COIN_OFFSET_FROM_HALALIT;
        var valuablesBeforeDrop = TestUtils.GetValuables();
        var diamond = TestUtils.DropValuable(ValuableName.DIAMOND, dropPosition);
        var stepDuration = TestUtils.GetTarnish(diamond).GetStepDuration();
        var diamondRigidBody = diamond.GetComponent<Rigidbody2D>();
        diamondRigidBody.velocity = MOVING_COIN_VELOCITY;
        var lastDiamondVelocity = diamondRigidBody.velocity;
        var watchedTime = 0f;

        // WHEN
        while (diamond != null && diamond.activeSelf && watchedTime < stepDuration + WAIT_MARGIN)
        {
            lastDiamondVelocity = diamondRigidBody.velocity;
            yield return new WaitForFixedUpdate();
            watchedTime += Time.fixedDeltaTime;
        }

        // THEN
        AssertWrapper.IsTrue(diamond == null || !diamond.activeSelf, "Moving Diamond Did Not Tarnish After Its Step Duration", _currentSeed);
        AssertWrapper.Greater(lastDiamondVelocity.magnitude, MIN_SPEED_AT_TARNISH, "Diamond Was Too Slow When It Tarnished To Check The Velocity", _currentSeed);
        var newValuables = GetNewValuables(valuablesBeforeDrop);
        AssertWrapper.AreEqual(1, newValuables.Length, "Tarnished Diamond Did Not Leave Exactly One Valuable", _currentSeed);
        AssertWrapper.AreEqual(lastDiamondVelocity, newValuables[0].GetComponent<Rigidbody2D>().velocity, "Gold Did Not Keep The Diamond's Velocity", _currentSeed, VELOCITY_DELTA);
    }

    [UnityTest]
    public IEnumerator SilverVanishesWithoutNextStep()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + COIN_OFFSET_FROM_HALALIT;
        var valuablesBeforeDrop = TestUtils.GetValuables();
        var silver = TestUtils.DropValuable(ValuableName.SILVER, dropPosition);
        var stepDuration = TestUtils.GetTarnish(silver).GetStepDuration();

        // WHEN
        yield return new WaitForSeconds(stepDuration + WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(silver == null, "Silver Did Not Vanish After Its Step Duration", _currentSeed);
        AssertWrapper.AreEqual(0, GetNewValuables(valuablesBeforeDrop).Length, "Vanished Silver Left A Valuable Behind", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DiamondChainIsGoneAfterAllSteps()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + COIN_OFFSET_FROM_HALALIT;
        var valuablesBeforeDrop = TestUtils.GetValuables();
        TestUtils.DropValuable(ValuableName.DIAMOND, dropPosition);
        var chainDuration =
            TestUtils.GetTarnish(LoadValuablePrefab(DIAMOND_PREFAB_NAME)).GetStepDuration() +
            TestUtils.GetTarnish(LoadValuablePrefab(GOLD_PREFAB_NAME)).GetStepDuration() +
            TestUtils.GetTarnish(LoadValuablePrefab(SILVER_PREFAB_NAME)).GetStepDuration();

        // WHEN
        yield return new WaitForSeconds(chainDuration + WAIT_MARGIN);

        // THEN
        AssertWrapper.AreEqual(0, GetNewValuables(valuablesBeforeDrop).Length, "Diamond Chain Left A Valuable After All Its Steps", _currentSeed);
    }

    [UnityTest]
    public IEnumerator BlinksOnlyNearTheEnd()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + COIN_OFFSET_FROM_HALALIT;
        var diamond = TestUtils.DropValuable(ValuableName.DIAMOND, dropPosition);
        var tarnish = TestUtils.GetTarnish(diamond);
        var stepDuration = tarnish.GetStepDuration();
        var blinkStart = stepDuration - tarnish.GetBlinkDuration();
        var spriteRenderer = diamond.GetComponent<SpriteRenderer>();
        var watchedTime = 0f;
        var wasHiddenBeforeBlink = false;
        var wasHiddenDuringBlink = false;

        // WHEN
        while (diamond != null && diamond.activeSelf && watchedTime < stepDuration - BLINK_SAMPLE_MARGIN)
        {
            yield return new WaitForFixedUpdate();
            watchedTime += Time.fixedDeltaTime;

            if (diamond == null || !diamond.activeSelf || spriteRenderer.enabled)
            {
                continue;
            }

            if (watchedTime < blinkStart - BLINK_SAMPLE_MARGIN)
            {
                wasHiddenBeforeBlink = true;
            }
            else if (watchedTime > blinkStart)
            {
                wasHiddenDuringBlink = true;
            }
        }

        // THEN
        AssertWrapper.IsFalse(wasHiddenBeforeBlink, "Diamond Blinked Before Its Last Seconds", _currentSeed);
        AssertWrapper.IsTrue(wasHiddenDuringBlink, "Diamond Did Not Blink In Its Last Seconds", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ClockPausesWhileGrabbed()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 dropPosition = (Vector2)TestUtils.GetHalalit().transform.position + COIN_OFFSET_FROM_HALALIT;
        var diamond = TestUtils.DropValuable(ValuableName.DIAMOND, dropPosition);
        var stepDuration = TestUtils.GetTarnish(diamond).GetStepDuration();
        var itemMovement = diamond.GetComponent<ItemMovement>();
        var holder = new GameObject("TarnishTestHolder");
        holder.transform.position = dropPosition;

        // WHEN
        itemMovement.Grabbed(holder.transform);
        yield return new WaitForSeconds(stepDuration + 2 * WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(diamond != null && diamond.activeSelf, "Carried Diamond Tarnished", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetValuableName(diamond) == ValuableName.DIAMOND, "Carried Diamond Changed", _currentSeed);

        // WHEN
        itemMovement.UnGrabbed();
        yield return new WaitForSeconds(stepDuration - WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(diamond != null && diamond.activeSelf, "Released Diamond Tarnished Before Its Step Duration", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(2 * WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(diamond == null, "Released Diamond Did Not Tarnish After Its Step Duration", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CollectedDiamondScoresOnce()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var originalScore = TestUtils.GetScore();
        var diamondValue = TestUtils.GetValuableValues().Find(valuableValue => valuableValue.Key == ValuableName.DIAMOND).Value;
        var stepDuration = TestUtils.GetTarnish(LoadValuablePrefab(DIAMOND_PREFAB_NAME)).GetStepDuration();
        var valuablesBeforeDrop = TestUtils.GetValuables();

        // WHEN
        yield return TestUtils.CollectValuable(ValuableName.DIAMOND);

        // THEN
        AssertWrapper.AreEqual(originalScore + diamondValue, TestUtils.GetScore(), "Collecting A Diamond Did Not Add Its Value", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(stepDuration + WAIT_MARGIN);

        // THEN
        AssertWrapper.AreEqual(originalScore + diamondValue, TestUtils.GetScore(), "Score Changed After The Diamond Was Collected", _currentSeed);
        AssertWrapper.AreEqual(0, GetNewValuables(valuablesBeforeDrop).Length, "A Collected Diamond Left A Valuable Behind", _currentSeed);
    }

    private static GameObject LoadValuablePrefab(string prefabName)
    {
        var valuablePrefab = Resources.LoadAll<GameObject>(VALUABLES_PREFABS_PATH).FirstOrDefault(prefab => prefab.name == prefabName);
        if (valuablePrefab == null)
        {
            throw new System.Exception($"{prefabName} prefab not found at Resources/{VALUABLES_PREFABS_PATH}");
        }
        return valuablePrefab;
    }

    private static GameObject[] GetNewValuables(GameObject[] valuablesBefore)
    {
        return TestUtils.GetValuables().Where(valuable => !valuablesBefore.Contains(valuable)).ToArray();
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
