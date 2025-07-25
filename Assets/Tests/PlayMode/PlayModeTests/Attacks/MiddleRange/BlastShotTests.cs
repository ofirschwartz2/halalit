using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BlastShotTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;
        float actualLifetime = 0;
        float supposedLifetime = shot.GetComponent<BlastShot>().GetLifeTime();
        float supposedSpeed = shot.GetComponent<BlastShot>().GetSpeed();
        Vector2 supposedLastPosition = shot.transform.position + shot.transform.up * supposedLifetime * supposedSpeed;
        var acceptedDelta = 0.3f;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            actualLifetime += Time.deltaTime;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(!TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Finished On Edges", _currentSeed);
        AssertWrapper.AreEqual(supposedLifetime, actualLifetime, "Lifetime not as expected", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(supposedLastPosition.x, lastShotPosition.x, "Last position not as expected", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(supposedLastPosition.y, lastShotPosition.y, "Last position not as expected", _currentSeed, acceptedDelta);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(Tag.EXPLOSION.GetDescription());
        var blastShockWave = GameObject.FindGameObjectWithTag(Tag.EXPLOSION_SHOCK_WAVE.GetDescription());
        float blastSupposedLifetime = blast.GetComponent<Blast>().GetLifeTime();
        actualLifetime = 0;

        // WHEN
        do
        {
            actualLifetime += Time.deltaTime;
            yield return null;

            blast = GameObject.FindGameObjectWithTag(Tag.EXPLOSION.GetDescription());
            blastShockWave = GameObject.FindGameObjectWithTag(Tag.EXPLOSION_SHOCK_WAVE.GetDescription());
        } while (blast != null || blastShockWave != null);

        AssertWrapper.IsNull(blast, _currentSeed);
        AssertWrapper.IsNull(blastShockWave, _currentSeed);
        AssertWrapper.AreEqual(blastSupposedLifetime, actualLifetime, "Blast Lifetime not as expected", _currentSeed, acceptedDelta);

    }

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;
        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(2); // MUST BE SMALL TO HIT BEFORE MinLifeTime
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var initialTargetPosition = TestUtils.GetEnemyPosition();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(initialTargetPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        float timeUntilHit = 0;
        var lifeTime = shot.GetComponent<BlastShot>().GetLifeTime();

        // WHEN
        do
        {
            timeUntilHit += Time.deltaTime;
            yield return null;

        } while (shot != null);

        // THEN
        AssertWrapper.GreaterOrEqual(lifeTime, timeUntilHit, "Didn't Hit Target Fast As Expected", _currentSeed);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(Tag.EXPLOSION.GetDescription());
        GameObject blastShockWave;
        float blastExpectedLifetime = blast.GetComponent<Blast>().GetLifeTime();
        var actualLifetime = 0f;
        var acceptedDelta = 0.3f;
        // WHEN
        do
        {
            actualLifetime += Time.deltaTime;
            yield return null;

            blast = GameObject.FindGameObjectWithTag(Tag.EXPLOSION.GetDescription());
            blastShockWave = GameObject.FindGameObjectWithTag(Tag.EXPLOSION_SHOCK_WAVE.GetDescription());
        } while (blast != null || blastShockWave != null);

        AssertWrapper.IsNull(blast, _currentSeed);
        AssertWrapper.IsNull(blastShockWave, _currentSeed);
        AssertWrapper.AreEqual(blastExpectedLifetime, actualLifetime, "Blast Lifetime not as expected", _currentSeed, acceptedDelta);
        
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.01f;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.EXPLOSIVE.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(!TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Finish On Edges", _currentSeed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Direction",
            _currentSeed,
            acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}