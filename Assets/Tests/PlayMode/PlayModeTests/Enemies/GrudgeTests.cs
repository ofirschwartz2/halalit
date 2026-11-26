using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GrudgeTests
{
    private const int ENEMY_HEALTH = 7;
    private const float GRUDGE_DURATION = 1f;
    private const float GRUDGE_END_MARGIN = 0.5f;
    private const float SHOT_DISTANCE_FROM_ENEMY = 1.5f;
    private const float MAX_WAIT_FOR_HIT = 5f;
    private static readonly Vector2 SHOOTER_OFFSET_FROM_ENEMY = new(0, 4);
    private static readonly Vector2 THIRD_ENEMY_OFFSET_FROM_ENEMY = new(0, -4);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator EveryEnemyPrefabHasGrudge()
    {
        // GIVEN
        yield return null;

        // WHEN
        var prefabs = Resources.LoadAll<GameObject>(TestUtils.ENEMIES_PREFABS_PATH);

        // THEN
        foreach (var prefab in prefabs)
        {
            if (prefab.GetComponent<EnemySharedBehavior>() == null)
            {
                continue;
            }

            AssertWrapper.IsNotNull(prefab.GetComponent<Grudge>(), _currentSeed, $"{prefab.name} has no Grudge (Needs Ofir in Unity, #216)");
        }
    }

    [UnityTest]
    public IEnumerator GrudgeTargetsOnlyItsHolder()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var holder = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var target = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + SHOOTER_OFFSET_FROM_ENEMY);
        var bystander = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + THIRD_ENEMY_OFFSET_FROM_ENEMY);
        yield return null;

        // WHEN
        EnemyTarget.SetGrudge(holder.transform, target.transform, Utils.GetEndOfLifeTime(GRUDGE_DURATION));

        // THEN
        AssertWrapper.AreEqual(target.transform.position, EnemyTarget.GetPosition(holder.transform), "Grudge holder does not aim at the enemy it holds a grudge on", _currentSeed);
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(bystander.transform), "An enemy without a grudge does not aim at the Halalit", _currentSeed);
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(), "The shared enemy target is not the Halalit", _currentSeed);
    }

    [UnityTest]
    public IEnumerator GrudgeEndsAfterItsTime()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var holder = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var target = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + SHOOTER_OFFSET_FROM_ENEMY);
        yield return null;
        EnemyTarget.SetGrudge(holder.transform, target.transform, Utils.GetEndOfLifeTime(GRUDGE_DURATION));
        AssertWrapper.IsTrue(EnemyTarget.HasGrudge(holder.transform), "Grudge was not set", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(GRUDGE_DURATION + GRUDGE_END_MARGIN);

        // THEN
        AssertWrapper.IsFalse(EnemyTarget.HasGrudge(holder.transform), "Grudge did not end after its time", _currentSeed);
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(holder.transform), "Enemy did not go back to aiming at the Halalit after its grudge ended", _currentSeed);
    }

    [UnityTest]
    public IEnumerator GrudgeEndsWhenTargetIsDestroyed()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var holder = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var target = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + SHOOTER_OFFSET_FROM_ENEMY);
        yield return null;
        EnemyTarget.SetGrudge(holder.transform, target.transform, Utils.GetEndOfLifeTime(GRUDGE_DURATION));

        // WHEN
        Object.Destroy(target);
        yield return null;

        // THEN
        AssertWrapper.IsFalse(EnemyTarget.HasGrudge(holder.transform), "Grudge did not end when its target was destroyed", _currentSeed);
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(holder.transform), "Enemy did not go back to aiming at the Halalit after its grudge target died", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NewerGrudgeReplacesOld()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var holder = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var firstTarget = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + SHOOTER_OFFSET_FROM_ENEMY);
        var secondTarget = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + THIRD_ENEMY_OFFSET_FROM_ENEMY);
        yield return null;
        EnemyTarget.SetGrudge(holder.transform, firstTarget.transform, Utils.GetEndOfLifeTime(GRUDGE_DURATION));

        // WHEN
        EnemyTarget.SetGrudge(holder.transform, secondTarget.transform, Utils.GetEndOfLifeTime(GRUDGE_DURATION));

        // THEN
        AssertWrapper.AreEqual(secondTarget.transform.position, EnemyTarget.GetPosition(holder.transform), "The newer grudge did not replace the old one", _currentSeed);
    }

    [UnityTest]
    public IEnumerator EnemyShotStartsGrudgeOnItsShooter()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var victim = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var shooter = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + SHOOTER_OFFSET_FROM_ENEMY);
        var grudge = TestUtils.GetGrudge(victim);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        var shot = TestUtils.LoadEnemyShot(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + Vector2.left * SHOT_DISTANCE_FROM_ENEMY, Vector2.right, shooter.transform);
        yield return WaitForShotToLand(shot);

        // THEN
        AssertWrapper.Greater(ENEMY_HEALTH, victim.GetComponent<Health>().GetHealth(), "Enemy shot did not hurt the enemy (Needs Ofir in Unity: Enemy Shot in its harmers)", _currentSeed);
        AssertWrapper.IsTrue(EnemyTarget.HasGrudge(victim.transform), "Enemy shot did not start a grudge", _currentSeed);
        AssertWrapper.AreEqual(shooter.transform.position, EnemyTarget.GetPosition(victim.transform), "Hurt enemy does not aim at the enemy that shot it", _currentSeed);
        AssertWrapper.Greater(grudge.GetDuration(), 0, "Grudge duration is not positive", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShotWithoutOwnerStartsNoGrudge()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var victim = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetGrudge(victim);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        var shot = TestUtils.LoadEnemyShot(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + Vector2.left * SHOT_DISTANCE_FROM_ENEMY, Vector2.right, null);
        yield return WaitForShotToLand(shot);

        // THEN
        AssertWrapper.Greater(ENEMY_HEALTH, victim.GetComponent<Health>().GetHealth(), "Enemy shot did not hurt the enemy (Needs Ofir in Unity: Enemy Shot in its harmers)", _currentSeed);
        AssertWrapper.IsFalse(EnemyTarget.HasGrudge(victim.transform), "A shot with no owner started a grudge", _currentSeed);
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(victim.transform), "Enemy hurt by a shot with no owner stopped aiming at the Halalit", _currentSeed);
    }

    // The plain enemy from the scene's ObjectLoader (a Lame Enemy, its movement stopped), placed at position.
    private GameObject LoadPlainEnemy(Vector2 position)
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetGameObjectPosition(enemy, position);
        return enemy;
    }

    private IEnumerator WaitForShotToLand(GameObject shot)
    {
        var waitStartTime = Time.time;
        while (shot != null && Time.time < waitStartTime + MAX_WAIT_FOR_HIT)
        {
            yield return new WaitForFixedUpdate();
        }

        AssertWrapper.IsNull(shot, _currentSeed, "Enemy shot never landed");
    }

    [TearDown]
    public void TearDown()
    {
        EnemyTarget.ClearAllGrudges();
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
