using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class GrenadeTests
{

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    private const AttackName ATTACK_NAME = AttackName.GRENADE;
    private const Tag ATTACK_TAG = Tag.EXPLOSIVE;
    private const Tag BLAST_TAG = Tag.EXPLOSION;
    private const Tag SHOCK_WAVE_TAG = Tag.EXPLOSION_SHOCK_WAVE;

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_ENEMY_NAME);
                break;
            default:
                SceneManager.LoadScene(SCENE_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        
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

        AssertWrapper.IsNull(blast, seed);
        AssertWrapper.IsNull(blastShockWave, seed);
        AssertWrapper.AreEqual(blastSupposedLifetime, actualLifetime, "Blast Lifetime not as expected", seed, acceptedDelta);

    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetUpShot(ATTACK_NAME);
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
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.GreaterOrEqual(timeUntilExplosion, lifeTime, "Exploded Before it should", seed);
        var finalDirection = Utils.GetDirectionVector(weaponMovement.transform.position, finalGrenadePosition);
        AssertWrapper.AreNotEqual(Utils.Vector2ToDegrees(initialDirection), Utils.Vector2ToDegrees(finalDirection), "Did Not Change Direction", seed);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
        var blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
        float blastSupposedLifetime = blast.GetComponent<Blast>().GetLifeTime();
        var actualLifetime = 0f;
        var acceptedDelta = 0.3f;
        // WHEN
        do
        {
            actualLifetime += Time.deltaTime;
            yield return null;

            blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
            blastShockWave = GameObject.FindGameObjectWithTag(SHOCK_WAVE_TAG.GetDescription());
        } while (blast != null || blastShockWave != null);

        var finalTargetPosition = TestUtils.GetEnemyPosition();

        AssertWrapper.IsNull(blast, seed);
        AssertWrapper.IsNull(blastShockWave, seed);
        AssertWrapper.AreEqual(blastSupposedLifetime, actualLifetime, "Blast Lifetime not as expected", seed, acceptedDelta);
        AssertWrapper.AreNotEqual(initialTargetPosition.magnitude, finalTargetPosition.magnitude, "Did Not Knockback", seed);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

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
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.IsTrue(!TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Finish On Edges", seed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Direction",
            seed,
            acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}