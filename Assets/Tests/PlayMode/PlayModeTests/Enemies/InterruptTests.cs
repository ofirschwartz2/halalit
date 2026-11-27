using Assets.Enums;
using Assets.Models;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
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
    private static readonly string[] INTERRUPTIBLE_ENEMY_PREFAB_PATHS =
    {
        TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH,
        TestUtils.SHOOTING_LASER_RANGE_ENEMY_PREFAB_PATH,
        TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH,
        TestUtils.FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH,
        TestUtils.FOLLOWING_ENEMY_RADIUS_PREFAB_PATH
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

        // WHEN
        yield return ShootPlayerShotAndWaitForHit(enemy);

        // THEN
        AssertWrapper.IsTrue(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, "A player shot during the aim did not stun the laser asterisk", _currentSeed);
        AssertWrapper.IsTrue(interrupt.IsBraced(), "The interrupted laser asterisk is not braced", _currentSeed);

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
        AssertHitDuringAimDidNotInterrupt(stateMachine, interrupt, "A player shot during the braced aim");
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

    [UnityTest]
    public IEnumerator EnemyShotDoesNotInterrupt()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        // Stands in for the enemy that fired: Interrupt only looks for the ShotOwner mark, not at who set it.
        var shooter = new GameObject("InterruptTestShooter");
        var interrupt = TestUtils.GetInterrupt(enemy);
        var stateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        var health = enemy.GetComponent<Health>();
        yield return null;
        SetHighHealth();
        yield return WaitForInterruptWindow(stateMachine, interrupt);

        // WHEN
        yield return ShootEnemyShotAndWaitForHit(enemy, shooter.transform);

        // THEN
        AssertWrapper.Less(health.GetHealth(), ENEMY_HEALTH, "Enemy shot did not hurt the laser asterisk (Needs Ofir in Unity: Enemy Shot in the shooting enemies' harmers, D11)", _currentSeed);
        AssertHitDuringAimDidNotInterrupt(stateMachine, interrupt, "An enemy shot with an owner");
        AssertWrapper.IsFalse(interrupt.IsBraced(), "An enemy shot with an owner braced the laser asterisk", _currentSeed);

        // WHEN
        yield return WaitForNextInterruptWindow(stateMachine, interrupt);
        var healthBeforeShot = health.GetHealth();
        yield return ShootEnemyShotAndWaitForHit(enemy, null);

        // THEN
        AssertWrapper.Less(health.GetHealth(), healthBeforeShot, "Enemy shot with no owner did not hurt the laser asterisk (Needs Ofir in Unity: Enemy Shot in the shooting enemies' harmers, D11)", _currentSeed);
        AssertHitDuringAimDidNotInterrupt(stateMachine, interrupt, "An enemy shot with no owner");
        AssertWrapper.IsFalse(interrupt.IsBraced(), "An enemy shot with no owner braced the laser asterisk", _currentSeed);
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

    // High enough that no hit kills an enemy or starts Wounded Retreat, and that the Halalit survives the enemies' attacks.
    private void SetHighHealth()
    {
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        TestUtils.GetHalalit().GetComponent<Health>().SetHealth(HALALIT_HEALTH);
    }

    // A hit in the window that must not interrupt: the enemy is not stunned, and it is still aiming in its window, so the hit landed there.
    private void AssertHitDuringAimDidNotInterrupt(MoveAimAttackStateMachine stateMachine, Interrupt interrupt, string hitName)
    {
        AssertWrapper.IsFalse(stateMachine.GetState() == MoveAimAttackEnemyState.STUNNED, $"{hitName} interrupted the laser asterisk", _currentSeed);
        AssertWrapper.IsTrue(stateMachine.IsInInterruptWindow(interrupt.GetWindupTime()), $"{hitName} landed after the aim ended, so the test checks nothing", _currentSeed);
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

    // Lets the current window close, then waits for the next one.
    private IEnumerator WaitForNextInterruptWindow(IInterruptibleEnemyBehaviour interruptible, Interrupt interrupt)
    {
        yield return WaitForCondition(() => !interruptible.IsInInterruptWindow(interrupt.GetWindupTime()));
        yield return WaitForInterruptWindow(interruptible, interrupt);
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
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
