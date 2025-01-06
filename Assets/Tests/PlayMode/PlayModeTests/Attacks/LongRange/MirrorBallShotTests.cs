using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MirrorBallShotTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.MIRROR_BALL_SHOT;
    private const Tag ATTACK_TAG = Tag.SHOT;

    private static readonly Vector2[] AMOUNT_OF_BOUNCES_ENEMIES_POSITIONS = new Vector2[]
    {
        new Vector2(-2.66f, 1.29f),
        new Vector2(4.23f, 2.56f),
        new Vector2(-2.57f, 3.6f),
        new Vector2(4.19f, 5.37f),
        new Vector2(-2.62f, 5.86f),
        new Vector2(4.5f, -0.11f)
    };

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
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", _currentSeed);

    }

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland();
        yield return null;

        TestUtils.SetRandomEnemyPosition();
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition = Vector2.zero, thisShotPosition = Vector2.zero;
        Vector2 startShotDirection = Vector2.zero, endShotDirection = Vector2.zero;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            if (shot != null) 
            {
                thisShotPosition = shot.transform.position;
            }
            if (thisShotPosition != lastShotPosition && startShotDirection == Vector2.zero) 
            {
                startShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
            }
            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        endShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
        AssertWrapper.AreNotEqual(startShotDirection.x, endShotDirection.x, "Didn't Change Direction", _currentSeed);
        AssertWrapper.AreNotEqual(startShotDirection.y, endShotDirection.y, "Didn't Change Direction", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", _currentSeed);
        
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }
  
    [UnityTest]
    public IEnumerator BounceDirectionCheck()
    {
        // GIVEN
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland();
        yield return null;
        TestUtils.SetEnemyPosition(new Vector2(5, 1), 0);
        TestUtils.RotaeEnemy(90);
        yield return null;

        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(TestUtils.GetEnemyPosition(), weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        Vector2 lastShotPosition;
        Vector2 endShotDirection;
        Vector2 thisShotPosition = Vector2.zero;
        Vector2 startShotDirection = Vector2.zero;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            if (shot != null)
            {
                thisShotPosition = shot.transform.position;
            }
            if (thisShotPosition != lastShotPosition && startShotDirection == Vector2.zero)
            {
                startShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
            }
            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        endShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
        AssertWrapper.AreNotEqual(startShotDirection.x, endShotDirection.x, "Didn't Change Direction");
        AssertWrapper.AreNotEqual(startShotDirection.y, endShotDirection.y, "Didn't Change Direction");
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges");
    }

    [UnityTest]
    public IEnumerator AmountOfBouncesCheck()
    {
        // GIVEN
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        int actualNumberOfHits = 0;
        var touchOnJoystick = Vector2.right;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        for (int i = 0; i < AMOUNT_OF_BOUNCES_ENEMIES_POSITIONS.Length; i++)
        {
            objectLoader.LoadEnemyInExternalSafeIsland();
            yield return null;
            TestUtils.SetEnemyPosition(AMOUNT_OF_BOUNCES_ENEMIES_POSITIONS[i], i);
        }
        yield return null;

        var originalTargetsHealth = TestUtils.GetAllEnemiesHealth().First();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        var supposedNumberOfHits = shot.GetComponent<MirrorBallShot>().GetMaxBounces() + 1;

        // WHEN
        do
        {
            yield return null;
            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN
        var newTargetsHealth = TestUtils.GetAllEnemiesHealth();

        foreach (var newTargetHealth in newTargetsHealth) 
        {
            if (newTargetHealth < originalTargetsHealth) 
            {
                actualNumberOfHits++;
            }
        }

        AssertWrapper.AreEqual(supposedNumberOfHits, actualNumberOfHits, "Wrong Number of Hits");
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        TestUtils.SetTestMode();
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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", _currentSeed);

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