using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CoinTossTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.COIN_TOSS;
    private const float TARGET_DISTANCE = 5f;
    private const float COIN_RANGE = 8f;
    private const float LANDING_DELTA = 1.5f;
    private const int ENEMY_HEALTH = 100;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator ShotCostsOneSilver()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<CoinToss>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var silverValue = GetSilverValue();
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        AssertWrapper.AreEqual(silverValue * 2, TestUtils.GetScore(), "Halalit Did Not Collect Two Silvers", _currentSeed);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(TestUtils.GetShot(), _currentSeed);
        AssertWrapper.AreEqual(silverValue, TestUtils.GetScore(), "Score Did Not Drop By One Silver Value", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CoinLandsAsSilverAtRangeEnd()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<CoinToss>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        AssertWrapper.AreEqual(0, TestUtils.GetValuables().Length, "A Valuable Is Left Before The Shot", _currentSeed);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        // The shot starts at the weapon, so its range is measured from there.
        Vector2 firePosition = weaponAttack.transform.position;
        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        yield return WaitUntilShotIsGone();

        // THEN
        var valuables = TestUtils.GetValuables();
        AssertWrapper.AreEqual(1, valuables.Length, "Coin Did Not Land As Exactly One Valuable", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetValuableName(valuables[0]) == ValuableName.SILVER, "Coin Did Not Land As A Silver", _currentSeed);
        AssertWrapper.AreEqual(COIN_RANGE, Vector2.Distance(firePosition, valuables[0].transform.position), "Coin Did Not Land At The End Of Its Range", _currentSeed, LANDING_DELTA);
    }

    [UnityTest]
    public IEnumerator CoinHitsEnemyAndLandsThere()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<CoinToss>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_5);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var halalit = TestUtils.GetHalalit();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        Vector2 halalitPosition = halalit.transform.position;
        TestUtils.SetEnemyPosition(halalitPosition + touchOnJoystick.normalized * TARGET_DISTANCE);
        yield return null;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_5.Power, TestUtils.GetEnemyHealth(), "Enemy Did Not Lose The Coin's Power", _currentSeed);
        var valuables = TestUtils.GetValuables();
        AssertWrapper.AreEqual(1, valuables.Length, "Coin Did Not Land As Exactly One Valuable", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetValuableName(valuables[0]) == ValuableName.SILVER, "Coin Did Not Land As A Silver", _currentSeed);
        AssertWrapper.AreEqual(TestUtils.GetEnemyPosition(), valuables[0].transform.position, "Coin Did Not Land At The Enemy", _currentSeed, LANDING_DELTA);
    }

    [UnityTest]
    public IEnumerator PebbleUnderOneSilver()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<CoinToss>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_5);
        var pebblePower = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<CoinToss>().GetPebblePower();
        AssertWrapper.AreEqual(0, TestUtils.GetScore(), "Score Is Not 0 Before The Shot", _currentSeed);

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var halalit = TestUtils.GetHalalit();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        Vector2 halalitPosition = halalit.transform.position;
        TestUtils.SetEnemyPosition(halalitPosition + touchOnJoystick.normalized * TARGET_DISTANCE);
        yield return null;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - pebblePower, TestUtils.GetEnemyHealth(), "Enemy Did Not Lose The Pebble's Power", _currentSeed);
        AssertWrapper.AreEqual(0, TestUtils.GetScore(), "Score Changed After A Pebble", _currentSeed);
        AssertWrapper.AreEqual(0, TestUtils.GetValuables().Length, "A Pebble Dropped A Valuable", _currentSeed);
    }

    private int GetSilverValue()
    {
        return TestUtils.GetValuableValues().Find(valuableValue => valuableValue.Key == ValuableName.SILVER).Value;
    }

    private IEnumerator WaitUntilShotIsGone()
    {
        var shot = TestUtils.GetShot();
        while (shot != null)
        {
            yield return null;
            shot = TestUtils.GetShot();
        }

        yield return new WaitForFixedUpdate();
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
