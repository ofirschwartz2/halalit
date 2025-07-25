using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class GrenadeTests
{

    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.GRENADE;
    private const Tag ATTACK_TAG = Tag.EXPLOSIVE;
    private const Tag BLAST_TAG = Tag.EXPLOSION;
    private const Tag SHOCK_WAVE_TAG = Tag.EXPLOSION_SHOCK_WAVE;

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
        TestUtils.SetTestMode();

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetTestMode();

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;
        float actualLifetime = 0;
        float supposedLifetime = shot.GetComponent<Grenade>().GetLifeTime();
        var acceptedDelta = 0.3f;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            actualLifetime += Time.deltaTime;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(!TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Finished On Edges", seed);
        AssertWrapper.AreEqual(supposedLifetime, actualLifetime, "Lifetime not as expected", seed, acceptedDelta);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
        var blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
        float blastSupposedLifetime = blast.GetComponent<Blast>().GetLifeTime();
        actualLifetime = 0;

        // WHEN
        do
        {
            actualLifetime += Time.deltaTime;
            yield return null;

            blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
            blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
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

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(2); // MUST BE SMALL TO HIT BEFORE MinLifeTime
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var initialTargetPosition = TestUtils.GetEnemyPosition();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(initialTargetPosition, weaponAttack.GetAttackJoystickEdge());
        var acceptedDelta = 0.2f;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        float timeUntilExplosion = 0;
        var lifeTime = shot.GetComponent<Grenade>().GetLifeTime();
        var initialDirection = Utils.GetDirectionVector(weaponMovement.transform.position, shot.transform.position);
        Vector2 finalGrenadePosition;
        // WHEN
        do
        {
            timeUntilExplosion += Time.deltaTime;
            finalGrenadePosition = shot.transform.position;
            yield return null;

        } while (shot != null);

        // THEN
        AssertWrapper.GreaterOrEqual(timeUntilExplosion, lifeTime, "Exploded Before it should", _currentSeed, acceptedDelta);
        var finalDirection = Utils.GetDirectionVector(weaponMovement.transform.position, finalGrenadePosition);
        AssertWrapper.AreNotEqual(Utils.Vector2ToDegrees(initialDirection), Utils.Vector2ToDegrees(finalDirection), "Did Not Change Direction", _currentSeed);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
        var blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
        float blastSupposedLifetime = blast.GetComponent<Blast>().GetLifeTime();
        var actualLifetime = 0f;
        acceptedDelta = 0.3f;
        // WHEN
        do
        {
            actualLifetime += Time.deltaTime;
            yield return null;

            blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
            blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
        } while (blast != null || blastShockWave != null);

        var finalTargetPosition = TestUtils.GetEnemyPosition();

        AssertWrapper.IsNull(blast, _currentSeed);
        AssertWrapper.IsNull(blastShockWave, _currentSeed);
        AssertWrapper.AreEqual(blastSupposedLifetime, actualLifetime, "Blast Lifetime not as expected", _currentSeed, acceptedDelta);
        AssertWrapper.AreNotEqual(initialTargetPosition.magnitude, finalTargetPosition.magnitude, "Did Not Knockback", _currentSeed);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN

        TestUtils.SetUpShot(ATTACK_NAME);
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
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
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
    }
}