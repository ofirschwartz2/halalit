using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BlastShotTests
{
    private const Tag ATTACK_TAG = Tag.EXPLOSIVE;
    private const Tag BLAST_TAG = Tag.EXPLOSION;
    private const Tag SHOCK_WAVE_TAG = Tag.EXPLOSION_SHOCK_WAVE;
    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_TARGET_NAME);
                break;
            default:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITHOUT_TARGET_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
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
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

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

            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(!TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Finished On Edges", seed);
        AssertWrapper.AreEqual(supposedLifetime, actualLifetime, "Lifetime not as expected", seed, acceptedDelta);
        AssertWrapper.AreEqual(supposedLastPosition.x, lastShotPosition.x, "Last position not as expected", seed, acceptedDelta);
        AssertWrapper.AreEqual(supposedLastPosition.y, lastShotPosition.y, "Last position not as expected", seed, acceptedDelta);

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

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetUpShot(AttackName.BLAST_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(2); // MUST BE SMALL TO HIT BEFORE MinLifeTime
        var originalTargetHealth = TestUtils.GetTargetHealth();
        yield return null;
        var initialTargetPosition = TestUtils.GetTargetPosition();
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
        float timeUntilHit = 0;
        var lifeTime = shot.GetComponent<BlastShot>().GetLifeTime();

        // WHEN
        do
        {
            timeUntilHit += Time.deltaTime;
            yield return null;

        } while (shot != null);

        // THEN
        AssertWrapper.GreaterOrEqual(lifeTime, timeUntilHit, "Didn't Hit Target Fast As Expected", seed);

        // GIVEN
        var blast = GameObject.FindGameObjectWithTag(BLAST_TAG.GetDescription());
        GameObject blastShockWave;
        float blastExpectedLifetime = blast.GetComponent<Blast>().GetLifeTime();
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

        AssertWrapper.IsNull(blast, seed);
        AssertWrapper.IsNull(blastShockWave, seed);
        AssertWrapper.AreEqual(blastExpectedLifetime, actualLifetime, "Blast Lifetime not as expected", seed, acceptedDelta);
        
        var newTargetHealth = TestUtils.GetTargetHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

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
}