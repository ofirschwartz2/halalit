using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CoinThiefTests
{
    private const float CARRY_DISTANCE = 1.2f;
    private const float CARRY_DISTANCE_DELTA = 0.3f;
    private const float STEAL_TIMEOUT = 10f;
    private const float WAIT_MARGIN = 1f;
    private static readonly Vector2 THIEF_OFFSET_FROM_HALALIT = new(2, 0);
    private static readonly Vector2 COIN_IN_REACH_OFFSET_FROM_HALALIT = new(5, 0);
    private static readonly Vector2 THIEF_FAR_OFFSET_FROM_HALALIT = new(-4, 0);
    private static readonly Vector2 COIN_OUT_OF_REACH_OFFSET_FROM_HALALIT = new(6, 0);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator StealsCoinInReach()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var coin = TestUtils.DropValuable(ValuableName.GOLD, halalitPosition + COIN_IN_REACH_OFFSET_FROM_HALALIT);

        // WHEN
        yield return WaitUntilCarried(coin, coinThief);

        // THEN
        AssertWrapper.IsTrue(IsCarriedBy(coin, coinThief), "Coin Thief Did Not Steal The Coin In Its Reach", _currentSeed);
        AssertWrapper.AreEqual(CARRY_DISTANCE, Vector2.Distance(coin.transform.position, coinThief.transform.position), "Coin Is Not Carried At The Carry Distance", _currentSeed, CARRY_DISTANCE_DELTA);
    }

    [UnityTest]
    public IEnumerator IgnoresCoinOutOfReach()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + THIEF_FAR_OFFSET_FROM_HALALIT, _currentSeed);
        var coin = TestUtils.DropValuable(ValuableName.GOLD, halalitPosition + COIN_OUT_OF_REACH_OFFSET_FROM_HALALIT);
        var waitingInterval = coinThief.GetComponent<WaitAttackLootWait>().GetWaitingInterval();

        // WHEN
        yield return new WaitForSeconds(waitingInterval + WAIT_MARGIN / 2);

        // THEN
        AssertWrapper.IsNotNull(coin, _currentSeed, "Coin Out Of Reach Was Destroyed");
        AssertWrapper.IsTrue(coin.transform.parent == null, "Coin Thief Took A Coin Out Of Its Reach", _currentSeed);
    }

    [UnityTest]
    public IEnumerator EatsCoinAfterDelay()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var originalScore = TestUtils.GetScore();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var coin = TestUtils.DropValuable(ValuableName.GOLD, halalitPosition + COIN_IN_REACH_OFFSET_FROM_HALALIT);
        var eatDelay = coinThief.GetComponent<WaitAttackStealAttack>().GetEatDelay();

        yield return WaitUntilCarried(coin, coinThief);
        AssertWrapper.IsTrue(IsCarriedBy(coin, coinThief), "Coin Thief Did Not Steal The Coin In Its Reach", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(eatDelay + WAIT_MARGIN);

        // THEN
        AssertWrapper.IsTrue(coin == null, "Coin Thief Did Not Eat The Coin After Its Eat Delay", _currentSeed);
        AssertWrapper.AreEqual(originalScore, TestUtils.GetScore(), "Score Changed When The Coin Thief Ate The Coin", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DropsCoinWhenHit()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var coin = TestUtils.DropValuable(ValuableName.GOLD, halalitPosition + COIN_IN_REACH_OFFSET_FROM_HALALIT);
        var waitingInterval = coinThief.GetComponent<WaitAttackLootWait>().GetWaitingInterval();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var asteroid = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);

        yield return WaitUntilCarried(coin, coinThief);
        AssertWrapper.IsTrue(IsCarriedBy(coin, coinThief), "Coin Thief Did Not Steal The Coin In Its Reach", _currentSeed);

        // WHEN
        TestUtils.SetGameObjectPosition(asteroid, coinThief.transform.position);
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(coin, _currentSeed, "Coin Was Destroyed When The Coin Thief Was Hit");
        AssertWrapper.IsTrue(coin.transform.parent == null, "Coin Thief Did Not Drop The Coin When Hit", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(waitingInterval / 2);

        // THEN
        AssertWrapper.IsNotNull(coin, _currentSeed, "Dropped Coin Was Destroyed");
        AssertWrapper.IsTrue(coin.transform.parent == null, "Coin Thief Took The Coin Back Before Its Wait Was Over", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShipTakesCoinFromTail()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var originalScore = TestUtils.GetScore();
        var goldValue = TestUtils.GetValuableValues().Find(valuableValue => valuableValue.Key == ValuableName.GOLD).Value;
        Vector2 halalitPosition = halalit.transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var coin = TestUtils.DropValuable(ValuableName.GOLD, halalitPosition + COIN_IN_REACH_OFFSET_FROM_HALALIT);

        yield return WaitUntilCarried(coin, coinThief);
        AssertWrapper.IsTrue(IsCarriedBy(coin, coinThief), "Coin Thief Did Not Steal The Coin In Its Reach", _currentSeed);

        // WHEN
        TestUtils.SetGameObjectPosition(halalit, coin.transform.position);
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsTrue(coin == null, "Halalit Did Not Take The Coin From The Coin Thief's Tail", _currentSeed);
        AssertWrapper.AreEqual(originalScore + goldValue, TestUtils.GetScore(), "Score Did Not Rise By The Gold Value", _currentSeed);
    }

    private IEnumerator WaitUntilCarried(GameObject coin, GameObject coinThief)
    {
        var timeout = Time.time + STEAL_TIMEOUT;
        while (Time.time < timeout && coin != null && coinThief != null && !IsCarriedBy(coin, coinThief))
        {
            yield return null;
        }
    }

    private static bool IsCarriedBy(GameObject coin, GameObject coinThief)
    {
        return coin != null && coinThief != null && coin.transform.parent == coinThief.transform;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
