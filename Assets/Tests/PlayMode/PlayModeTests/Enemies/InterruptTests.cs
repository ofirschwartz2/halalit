using Assets.Enums;
using Assets.Models;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class InterruptTests
{
    private const int ENEMY_HEALTH = 100;
    private const int HALALIT_HEALTH = 1000;
    private const float SHOT_DISTANCE_FROM_ENEMY = 1.5f;
    private const float MAX_WAIT_FOR_HIT = 5f;
    private const float MAX_WAIT_FOR_STATE = 10f;
    private const float STUN_END_MARGIN = 0.5f;
    private const float WAIT_END_MARGIN = 0.5f;
    // Farther from the Halalit than the Radius Following's waiting radius (10 in its prefab), and inside the world (half width about 21.6).
    private static readonly Vector2 OUT_OF_RADIUS_OFFSET_FROM_HALALIT = new(12, 0);
    private static readonly string[] INTERRUPTIBLE_ENEMY_PREFAB_PATHS =
    {
        TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH,
        TestUtils.SHOOTING_LASER_RANGE_ENEMY_PREFAB_PATH,
        TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH,
        TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH,
        TestUtils.FOLLOWING_ENEMY_RADIUS_PREFAB_PATH
    };
    // The only enemy prefabs that may carry Interrupt: the three shooters and their base, the two Followings,
    // and the Mimic and Coin Thief, which are made from the Followings and whose waits never open the window.
    private static readonly string[] ENEMY_PREFAB_NAMES_WITH_INTERRUPT =
    {
        "MoveAimAttackEnemy",
        "ShootingInRangeEnemy",
        "ShootingLaserRangeEnemy",
        "ShootingLaserAsteriskEnemy",
        "FollowingEnemyInterval",
        "FollowingEnemyRadius",
        "Mimic",
        "CoinThief"
    };
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator ShootersAndFollowingsHaveInterrupt()
    {
        // GIVEN
        yield return null;

        // WHEN
        var prefabs = INTERRUPTIBLE_ENEMY_PREFAB_PATHS.Select(prefabPath => Resources.Load<GameObject>(prefabPath)).ToList();

        // THEN
        for (int i = 0; i < prefabs.Count; i++)
        {
            AssertWrapper.IsNotNull(prefabs[i], _currentSeed, $"No enemy prefab at Resources/{INTERRUPTIBLE_ENEMY_PREFAB_PATHS[i]}");
            // Unity's != also sees the stand-in object that GetComponent returns in the editor for a missing component.
            AssertWrapper.IsTrue(prefabs[i].GetComponent<Interrupt>() != null, $"{prefabs[i].name} has no Interrupt (Needs Ofir in Unity, #238)", _currentSeed);
        }
    }

    // The "not here" half of the Unity steps: Interrupt on the WaitAtackEnemy base, the Shield Ram or the Mine Layer fails this test.
    // It loads only the prefabs that exist, so it passes before the Shield Ram and Mine Layer prefabs are made.
    [UnityTest]
    public IEnumerator OnlyTheCardsEnemiesHaveInterrupt()
    {
        // GIVEN
        yield return null;

        // WHEN
        var prefabs = Resources.LoadAll<GameObject>(TestUtils.ENEMIES_PREFABS_PATH);

        // THEN
        foreach (var prefab in prefabs)
        {
            // Unity's == also sees the stand-in object that GetComponent returns in the editor for a missing component.
            if (prefab.GetComponent<Interrupt>() == null)
            {
                continue;
            }

            AssertWrapper.IsTrue(ENEMY_PREFAB_NAMES_WITH_INTERRUPT.Contains(prefab.name), $"{prefab.name} has Interrupt, which card #238 does not include: remove it (Needs Ofir in Unity, #238)", _currentSeed);
        }
    }

    [UnityTest]
    public IEnumerator PlayerShotDuringAimCancelsLaserAsterisk()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        // The aim lines appear on the first aiming step, which can come one step after the window opens.
        yield return new WaitForFixedUpdate();
        AssertWrapper.Greater(GetAimLineCount(enemy), 0, "Test setup: the laser asterisk shows no aim lines", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, "A player shot during the aim did not stun the laser asterisk", _currentSeed);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The interrupted laser asterisk is not braced", _currentSeed);
        AssertWrapper.AreEqual(0, GetAimLineCount(enemy), "The interrupted laser asterisk kept its aim lines", _currentSeed);

        // WHEN
        var stunCheckEndTime = Time.time + interrupt.GetStunDuration() + STUN_END_MARGIN;
        while (stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED && Time.time < stunCheckEndTime)
        {
            AssertWrapper.AreEqual(0, TestUtils.GetEnemyLaserBeams().Count, "The interrupted laser asterisk fired during its stun", _currentSeed);
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.MOVING, "The laser asterisk did not start a fresh move phase after its stun", _currentSeed);
        AssertWrapper.AreEqual(0, TestUtils.GetEnemyLaserBeams().Count, "The interrupted laser asterisk fired", _currentSeed);
    }

    [UnityTest]
    public IEnumerator BracedAimIsNotInterrupted()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        var health = enemy.GetComponent<Health>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        yield return ShootPlayerShotAndWaitForHit(enemy);
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, "The first shot did not interrupt the laser asterisk, so its braced aim cannot be checked", _currentSeed);
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The laser asterisk is not braced in the aim after the interrupt", _currentSeed);
        var healthBeforeShot = health.GetHealth();

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.Less(health.GetHealth(), healthBeforeShot, "The shot during the braced aim did not hurt the laser asterisk", _currentSeed);
        AssertHitDuringWindupDidNotInterrupt(stateMachine, interrupt, "A player shot during the braced aim");
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The laser asterisk stopped being braced before its attack", _currentSeed);

        // WHEN
        yield return WaitForCondition(() => TestUtils.GetEnemyLaserBeams().Count > 0);

        // THEN
        AssertWrapper.Greater(TestUtils.GetEnemyLaserBeams().Count, 0, "The braced laser asterisk did not fire", _currentSeed);

        // WHEN
        yield return WaitForCondition(() => stateMachine.GetState() == MoveAimAttackEnemyState.MOVING);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.MOVING, "The braced laser asterisk's attack never finished", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "The laser asterisk stayed braced after its attack finished", _currentSeed);
    }

    [UnityTest]
    public IEnumerator HitWhileMovingDoesNotInterrupt()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_RANGE_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        yield return null;
        SetHighHealth();
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.MOVING, "The laser range enemy did not start by moving", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.Less(enemy.GetComponent<Health>().GetHealth(), ENEMY_HEALTH, "The player shot did not hurt the laser range enemy", _currentSeed);
        AssertWrapper.IsFalse(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, "A player shot while moving interrupted the laser range enemy", _currentSeed);
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.MOVING, "The shot landed after the move phase ended, so the test checks nothing", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "A player shot while moving braced the laser range enemy", _currentSeed);
    }

    // On a Following, because enemy shots hurt it. They do not hurt the shooting enemies until debt D11 is fixed, so there
    // the shots would raise no hit event and the test would check D11, not Interrupt. Both shots land in one wind-up.
    [UnityTest]
    public IEnumerator EnemyShotDoesNotInterrupt()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadEnemy(TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        // Stands in for the enemy that fired: Interrupt only looks for the ShotOwner mark, not at who set it.
        var shooter = new GameObject("InterruptTestShooter");
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        var health = enemy.GetComponent<Health>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);

        // WHEN
        yield return ShootEnemyShotAndWaitForHit(enemy, shooter.transform);

        // THEN
        AssertWrapper.Less(health.GetHealth(), ENEMY_HEALTH, "Enemy shot did not hurt the Following", _currentSeed);
        AssertHitDuringWindupDidNotInterrupt(stateMachine, interrupt, "An enemy shot with an owner");
        AssertWrapper.IsFalse(interrupt.IsBraced(), "An enemy shot with an owner braced the Following", _currentSeed);

        // WHEN
        var healthBeforeShot = health.GetHealth();
        yield return ShootEnemyShotAndWaitForHit(enemy, null);

        // THEN
        AssertWrapper.Less(health.GetHealth(), healthBeforeShot, "Enemy shot with no owner did not hurt the Following", _currentSeed);
        AssertHitDuringWindupDidNotInterrupt(stateMachine, interrupt, "An enemy shot with no owner");
        AssertWrapper.IsFalse(interrupt.IsBraced(), "An enemy shot with no owner braced the Following", _currentSeed);
    }

    // Interrupt checks the Enemy Shot tag before the ShotOwner mark, and every enemy attack has the tag, so the shots above never
    // reach the mark. This checks the mark alone: a Ball Shot, tagged Shot like every player shot, marked as fired by an enemy.
    [UnityTest]
    public IEnumerator ShotWithEnemyOwnerDoesNotInterrupt()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var shooter = new GameObject("InterruptTestShooter");
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);

        // WHEN
        var shot = TestUtils.LoadBallShot(GetShotStartPosition(enemy), Vector2.right);
        ShotOwner.Mark(shot, shooter.transform);
        yield return WaitForShotToLand(shot);

        // THEN
        AssertWrapper.Less(enemy.GetComponent<Health>().GetHealth(), ENEMY_HEALTH, "The Ball Shot did not hurt the laser asterisk", _currentSeed);
        AssertHitDuringWindupDidNotInterrupt(stateMachine, interrupt, "A Ball Shot marked as fired by an enemy");
        AssertWrapper.IsFalse(interrupt.IsBraced(), "A Ball Shot marked as fired by an enemy braced the laser asterisk", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PlayerShotInterruptsFollowingWindup()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, "A player shot at the end of the spin did not stun the Following", _currentSeed);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The interrupted Following is not braced", _currentSeed);

        // WHEN
        var stunCheckEndTime = Time.time + interrupt.GetStunDuration() + STUN_END_MARGIN;
        while (stateMachine.GetState() == WaitAttackEnemyState.STUNNED && Time.time < stunCheckEndTime)
        {
            yield return new WaitForFixedUpdate();
            AssertWrapper.IsFalse(stateMachine.GetState() == WaitAttackEnemyState.ATTACKING, "The interrupted Following charged during its stun", _currentSeed);
        }

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "The Following did not spin again after its stun", _currentSeed);
        AssertWrapper.IsFalse(stateMachine.IsInInterruptWindow(interrupt.GetWindupTime()), "The Following's spin after the stun is not a fresh one", _currentSeed);
    }

    // The brace must end when the charge finishes, as a shooter's ends with its attack (BracedAimIsNotInterrupted),
    // or the Following could never be interrupted again.
    [UnityTest]
    public IEnumerator FollowingBraceEndsAfterItsCharge()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        yield return ShootPlayerShotAndWaitForHit(enemy);
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, "The shot did not interrupt the Following, so its brace cannot be checked", _currentSeed);

        // WHEN
        yield return WaitForCondition(() => stateMachine.GetState() == WaitAttackEnemyState.ATTACKING);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.ATTACKING, "The braced Following never charged", _currentSeed);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The Following stopped being braced before its charge", _currentSeed);

        // WHEN
        yield return WaitForCondition(() => stateMachine.GetState() == WaitAttackEnemyState.WAITING);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "The braced Following's charge never finished", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "The Following stayed braced after its charge finished", _currentSeed);
    }

    // Health raises Wounded before the hit event, so a hit that starts Wounded Retreat pauses the Following before Interrupt asks:
    // the retreat wins. The paused machine keeps its WAITING state, so only its enabled check keeps the window shut,
    // for that hit and for any hit during the retreat.
    [UnityTest]
    public IEnumerator RetreatingFollowingIsNotInterrupted()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadEnemy(TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        var health = enemy.GetComponent<Health>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        var woundedHealth = woundedRetreat.GetWoundedHealth();
        TestUtils.SetEnemiesHealth(woundedHealth + TestUtils.DEFAULT_POWER_1);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.AreEqual(woundedHealth, health.GetHealth(), "The Ball Shot did not hurt the Following exactly once", _currentSeed);
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "The wounding shot did not start the retreat", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "The shot that started the retreat also interrupted the Following", _currentSeed);
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "The Following left its wait: it was stunned, or the shot landed after its wind-up", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.Less(health.GetHealth(), woundedHealth, "The shot during the retreat did not hurt the Following", _currentSeed);
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "The retreat ended before the shot during it landed", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "A shot during the retreat interrupted the Following", _currentSeed);
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "A shot during the retreat stunned the Following", _currentSeed);
    }

    // The Radius Following charges only once the Halalit is within its radius, and nobody can know when that will be. So its wind-up
    // runs from the last second of its wait until it charges: with the Halalit out of range, it stays open after the wait is over.
    [UnityTest]
    public IEnumerator RadiusFollowingIsInterruptibleUntilItCharges()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var enemy = TestUtils.LoadEnemy(TestUtils.FOLLOWING_ENEMY_RADIUS_PREFAB_PATH, halalitPosition + OUT_OF_RADIUS_OFFSET_FROM_HALALIT, _currentSeed);
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);
        // The window opens windupTime before the wait is over.
        yield return new WaitForSeconds(interrupt.GetWindupTime() + WAIT_END_MARGIN);
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "Test setup: the Radius Following charged, so the Halalit was within its radius", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, "A player shot after the Radius Following's wait, before its charge, did not stun it", _currentSeed);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The interrupted Radius Following is not braced", _currentSeed);
    }

    // The Coin Thief's wait keeps the default wind-up, which is never on. It is copied from FollowingEnemyRadius (#164), so it may
    // carry Interrupt. Past its waiting interval, with no coin to steal, it waits like a Radius Following out of range, which is open.
    [UnityTest]
    public IEnumerator CoinThiefIsNeverInterrupted()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadCoinThief(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = GetOrAddInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        var waitingInterval = enemy.GetComponent<WaitAttackLootWait>().GetWaitingInterval();
        yield return null;
        SetHighHealth();
        yield return new WaitForSeconds(waitingInterval + WAIT_END_MARGIN);
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "Test setup: the Coin Thief stopped waiting", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.Less(enemy.GetComponent<Health>().GetHealth(), ENEMY_HEALTH, "The Ball Shot did not hurt the Coin Thief", _currentSeed);
        AssertWrapper.IsFalse(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, "A player shot interrupted the Coin Thief", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "A player shot braced the Coin Thief", _currentSeed);
    }

    // The Mimic's wait keeps the default wind-up, which is never on. It is a variant of FollowingEnemyInterval (#214), so it
    // inherits Interrupt. Hidden, it is waiting; the shot hurts it, which wakes it, and must not interrupt it.
    [UnityTest]
    public IEnumerator MimicIsNeverInterrupted()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = TestUtils.LoadMimic(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var interrupt = GetOrAddInterrupt(enemy);
        var stateMachine = enemy.GetComponent<WaitAttackStateMachine>();
        yield return null;
        SetHighHealth();
        AssertWrapper.IsTrue(stateMachine.GetState() == WaitAttackEnemyState.WAITING, "Test setup: the hidden Mimic is not waiting", _currentSeed);

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.Less(enemy.GetComponent<Health>().GetHealth(), ENEMY_HEALTH, "The Ball Shot did not hurt the Mimic", _currentSeed);
        AssertWrapper.IsFalse(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, "A player shot interrupted the Mimic", _currentSeed);
        AssertWrapper.IsFalse(interrupt.IsBraced(), "A player shot braced the Mimic", _currentSeed);
    }

    // High enough that no hit kills an enemy or starts Wounded Retreat, and that the Halalit survives the enemies' attacks.
    private void SetHighHealth()
    {
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        TestUtils.GetHalalit().GetComponent<Health>().SetHealth(HALALIT_HEALTH);
    }

    // A hit in the window that must not interrupt: the enemy is not stunned, and it is still winding up in its window, so the hit landed there.
    private void AssertHitDuringWindupDidNotInterrupt(MoveAimAttackStateMachine stateMachine, Interrupt interrupt, string hitName)
    {
        AssertWrapper.IsFalse(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, $"{hitName} interrupted the enemy", _currentSeed);
        AssertHitLandedInWindow(stateMachine, interrupt, hitName);
    }

    private void AssertHitDuringWindupDidNotInterrupt(WaitAttackStateMachine stateMachine, Interrupt interrupt, string hitName)
    {
        AssertWrapper.IsFalse(stateMachine.GetState() == WaitAttackEnemyState.STUNNED, $"{hitName} interrupted the enemy", _currentSeed);
        AssertHitLandedInWindow(stateMachine, interrupt, hitName);
    }

    private void AssertHitLandedInWindow(IInterruptibleEnemyBehaviour interruptible, Interrupt interrupt, string hitName)
    {
        AssertWrapper.IsTrue(interruptible.IsInInterruptWindow(interrupt.GetWindupTime()), $"{hitName} landed after the wind-up ended, so the test checks nothing", _currentSeed);
    }

    // The aim lines an enemy shows are its children with a Fader: no other part of an enemy has one.
    private int GetAimLineCount(GameObject enemy)
    {
        return enemy.GetComponentsInChildren<Fader>().Length;
    }

    // The Coin Thief and the Mimic are made from the Following prefabs, so they may carry Interrupt or not.
    // Gives the enemy one if it has none, so the test checks its wait, not how its prefab was made.
    private Interrupt GetOrAddInterrupt(GameObject enemy)
    {
        var interrupt = enemy.GetComponent<Interrupt>();
        if (interrupt == null)
        {
            interrupt = enemy.AddComponent<Interrupt>();
        }
        return interrupt;
    }

    // Fires a Ball Shot at the enemy from SHOT_DISTANCE_FROM_ENEMY to its left and waits until it lands.
    private IEnumerator ShootPlayerShotAndWaitForHit(GameObject enemy)
    {
        var shot = TestUtils.LoadBallShot(GetShotStartPosition(enemy), Vector2.right);
        yield return WaitForShotToLand(shot);
    }

    // Fires an enemy Ball Shot the same way, marked as fired by owner (null: no owner).
    private IEnumerator ShootEnemyShotAndWaitForHit(GameObject enemy, Transform owner)
    {
        var shot = TestUtils.LoadEnemyShot(GetShotStartPosition(enemy), Vector2.right, owner);
        yield return WaitForShotToLand(shot);
    }

    private Vector2 GetShotStartPosition(GameObject enemy)
    {
        return (Vector2)enemy.transform.position + Vector2.left * SHOT_DISTANCE_FROM_ENEMY;
    }

    private IEnumerator WaitForShotToLand(GameObject shot)
    {
        var waitStartTime = Time.time;
        while (shot != null && Time.time < waitStartTime + MAX_WAIT_FOR_HIT)
        {
            yield return new WaitForFixedUpdate();
        }

        // Unity's == sees a destroyed shot as null.
        AssertWrapper.IsTrue(shot == null, "The shot never landed", _currentSeed);
    }

    private IEnumerator WaitForInterruptWindow(IInterruptibleEnemyBehaviour interruptible, Interrupt interrupt)
    {
        yield return WaitForCondition(() => interruptible.IsInInterruptWindow(interrupt.GetWindupTime()));
        AssertWrapper.IsTrue(interruptible.IsInInterruptWindow(interrupt.GetWindupTime()), "The enemy never wound up an attack", _currentSeed);
    }

    // Waits one physics step at a time until the condition holds, for at most MAX_WAIT_FOR_STATE seconds. The caller asserts it.
    private IEnumerator WaitForCondition(System.Func<bool> condition)
    {
        var waitStartTime = Time.time;
        while (!condition() && Time.time < waitStartTime + MAX_WAIT_FOR_STATE)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    [TearDown]
    public void TearDown()
    {
        EnemyTarget.ClearAllGrudges();
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
