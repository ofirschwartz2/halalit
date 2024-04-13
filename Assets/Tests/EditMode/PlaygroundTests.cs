using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlaygroundTests
{

    [Test]
    public void HUDTests()
    {
        var attackJoystick = TestUtils.GetAttackJoystick();
        var movementJoystick = TestUtils.GetMovementJoystick();
        var halalitHealthBar = TestUtils.GetHalalitHealthBar();
        var scoreText = TestUtils.GetScoreText();
        var parentRotationCounteractor = halalitHealthBar.GetComponent<ParentRotationCounteractor>();
        var scoreScript = scoreText.GetComponent<Score>();


        AssertWrapper.IsNotNull(attackJoystick);
        AssertWrapper.IsNotNull(movementJoystick);
        AssertWrapper.IsNotNull(halalitHealthBar);
        AssertWrapper.IsNotNull(scoreText);
        AssertWrapper.IsNotNull(parentRotationCounteractor);
        AssertWrapper.IsNotNull(scoreScript);
    }
    
    [Test]
    public void WorldTests()
    {
        // INTERNAL WORLD

        var internalWorld = TestUtils.GetInternalWorld();
        var internalWorldTransform = internalWorld.transform;
        var internalWorldBoxCollider2D = internalWorld.GetComponent<BoxCollider2D>();

        AssertWrapper.IsNotNull(internalWorld);
        AssertWrapper.IsNotNull(internalWorldTransform);
        AssertWrapper.IsNotNull(internalWorldBoxCollider2D);

        Transform bottomEdgeForceField = internalWorld.transform.Find("BottomEdgeForceField");
        var bottomEdgeForceFieldBoxCollider2D = bottomEdgeForceField.GetComponent<BoxCollider2D>();
        var bottomEdgeForceFieldScript = bottomEdgeForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(bottomEdgeForceField);
        AssertWrapper.IsNotNull(bottomEdgeForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(bottomEdgeForceFieldScript);

        Transform topEdgeForceField = internalWorld.transform.Find("TopEdgeForceField");
        var topEdgeForceFieldBoxCollider2D = topEdgeForceField.GetComponent<BoxCollider2D>();
        var topEdgeForceFieldScript = topEdgeForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(topEdgeForceField);
        AssertWrapper.IsNotNull(topEdgeForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(topEdgeForceFieldScript);

        Transform leftEdgeForceField = internalWorld.transform.Find("LeftEdgeForceField");
        var leftEdgeForceFieldBoxCollider2D = leftEdgeForceField.GetComponent<BoxCollider2D>();
        var leftEdgeForceFieldScript = leftEdgeForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(leftEdgeForceField);
        AssertWrapper.IsNotNull(leftEdgeForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(leftEdgeForceFieldScript);

        Transform rightEdgeForceField = internalWorld.transform.Find("RightEdgeForceField");
        var rightEdgeForceFieldBoxCollider2D = rightEdgeForceField.GetComponent<BoxCollider2D>();
        var rightEdgeForceFieldScript = rightEdgeForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(rightEdgeForceField);
        AssertWrapper.IsNotNull(rightEdgeForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(rightEdgeForceFieldScript);

        Transform bottomRightCornerForceField = internalWorld.transform.Find("BottomRightCornerForceField");
        var bottomRightCornerForceFieldBoxCollider2D = bottomRightCornerForceField.GetComponent<BoxCollider2D>();
        var bottomRightCornerForceFieldScript = bottomRightCornerForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(bottomRightCornerForceField);
        AssertWrapper.IsNotNull(bottomRightCornerForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(bottomRightCornerForceFieldScript);

        Transform bottomLeftCornerForceField = internalWorld.transform.Find("BottomLeftCornerForceField");
        var bottomLeftCornerForceFieldBoxCollider2D = bottomLeftCornerForceField.GetComponent<BoxCollider2D>();
        var bottomLeftCornerForceFieldScript = bottomLeftCornerForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(bottomLeftCornerForceField);
        AssertWrapper.IsNotNull(bottomLeftCornerForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(bottomLeftCornerForceFieldScript);

        Transform topRightCornerForceField = internalWorld.transform.Find("TopRightCornerForceField");
        var topRightCornerForceFieldBoxCollider2D = topRightCornerForceField.GetComponent<BoxCollider2D>();
        var topRightCornerForceFieldScript = topRightCornerForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(topRightCornerForceField);
        AssertWrapper.IsNotNull(topRightCornerForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(topRightCornerForceFieldScript);

        Transform topLeftCornerForceField = internalWorld.transform.Find("TopLeftCornerForceField");
        var topLeftCornerForceFieldBoxCollider2D = topLeftCornerForceField.GetComponent<BoxCollider2D>();
        var topLeftCornerForceFieldScript = topLeftCornerForceField.GetComponent<ForceField>();

        AssertWrapper.IsNotNull(topLeftCornerForceField);
        AssertWrapper.IsNotNull(topLeftCornerForceFieldBoxCollider2D);
        AssertWrapper.IsNotNull(topLeftCornerForceFieldScript);

        // EXTERNAL WORLD

        var externalWorld = TestUtils.GetExternalWorld();
        var externalWorldTransform = externalWorld.transform;
        var externalWorldSpriteRenderer = externalWorld.GetComponent<SpriteRenderer>();
        var externalWorldBoxCollider2Ds = externalWorld.GetComponents<BoxCollider2D>();

        AssertWrapper.IsNotNull(externalWorld);
        AssertWrapper.IsNotNull(externalWorldTransform);
        AssertWrapper.IsNotNull(externalWorldSpriteRenderer);
        AssertWrapper.AreEqual(externalWorldBoxCollider2Ds.Length, 4, "ExternalWorld Should Have 4 BoxCollider2Ds");

        var externalTopEdge = TestUtils.GetExternalTopEdge();
        var externalTopEdgeTransform = externalTopEdge.transform;
        var externalTopEdgeBoxCollider2D = externalTopEdge.GetComponents<BoxCollider2D>();

        AssertWrapper.IsNotNull(externalTopEdge);
        AssertWrapper.IsNotNull(externalTopEdgeTransform);
        AssertWrapper.AreEqual(externalTopEdgeBoxCollider2D.Length, 2, "ExternalTopEdge Should Have 2 BoxCollider2Ds");

        var externalBottomEdge = TestUtils.GetExternalBottomEdge();
        var externalBottomEdgeTransform = externalBottomEdge.transform;
        var externalBottomEdgeBoxCollider2D = externalBottomEdge.GetComponents<BoxCollider2D>();

        AssertWrapper.IsNotNull(externalBottomEdge);
        AssertWrapper.IsNotNull(externalBottomEdgeTransform);
        AssertWrapper.AreEqual(externalBottomEdgeBoxCollider2D.Length, 2, "ExternalBottomEdge Should Have 2 BoxCollider2Ds");

        var externalLeftEdge = TestUtils.GetExternalLeftEdge();
        var externalLeftEdgeTransform = externalLeftEdge.transform;
        var externalLeftEdgeBoxCollider2D = externalLeftEdge.GetComponents<BoxCollider2D>();

        AssertWrapper.IsNotNull(externalLeftEdge);
        AssertWrapper.IsNotNull(externalLeftEdgeTransform);
        AssertWrapper.AreEqual(externalLeftEdgeBoxCollider2D.Length, 2, "ExternalLeftEdge Should Have 2 BoxCollider2Ds");

        var externalRightEdge = TestUtils.GetExternalRightEdge();
        var externalRightEdgeTransform = externalRightEdge.transform;
        var externalRightEdgeBoxCollider2D = externalRightEdge.GetComponents<BoxCollider2D>();

        AssertWrapper.IsNotNull(externalRightEdge);
        AssertWrapper.IsNotNull(externalRightEdgeTransform);
        AssertWrapper.AreEqual(externalRightEdgeBoxCollider2D.Length, 2, "ExternalRightEdge Should Have 2 BoxCollider2Ds");

    }

    [Test]
    public void CameraTests()
    {
        var Camera = TestUtils.GetCamera();
        var mainCameraTransform = Camera.transform;
        var mainCameraCamera = Camera.GetComponent<Camera>();
        var mainCameraAudioListener = Camera.GetComponent<AudioListener>();
        // why is is in Assembly-CSharp?
        //var mainCameraMovementScript = Camera.GetComponent<CameraMovement>();

        AssertWrapper.IsNotNull(Camera);
        AssertWrapper.IsNotNull(mainCameraTransform);
        AssertWrapper.IsNotNull(mainCameraCamera);
        AssertWrapper.IsNotNull(mainCameraAudioListener);
    }

    [Test]
    public void AsteroidsContainerTests() 
    {

        var asteroids = TestUtils.GetAsteroidsContainer();
        var asteroidsTransform = asteroids.transform;

        AssertWrapper.IsNotNull(asteroids);
        AssertWrapper.IsNotNull(asteroidsTransform);

        var asteroidInstantiator = asteroids.transform.Find("AsteroidInstantiator");
        var asteroidInstantiatorScript = asteroidInstantiator.GetComponent<AsteroidInitiator>();
        var asteroidExternalInstantiator = asteroidInstantiator.GetComponent<AsteroidExternalInstantiator>();
        var asteroidInternalInstantiator = asteroidInstantiator.GetComponent<AsteroidInternalInstantiator>();

        AssertWrapper.IsNotNull(asteroidInstantiator);
        AssertWrapper.IsNotNull(asteroidInstantiatorScript);
        AssertWrapper.IsNotNull(asteroidExternalInstantiator);
        AssertWrapper.IsNotNull(asteroidInternalInstantiator);

        var asteroidDestructor = asteroids.transform.Find("AsteroidDestructor");
        var asteroidDestructorScript = asteroidDestructor.GetComponent<AsteroidGlobalDestructor>();

        AssertWrapper.IsNotNull(asteroidDestructor);
        AssertWrapper.IsNotNull(asteroidDestructorScript);
    }

    [Test]
    public void EnemiesContainerTests()
    {
        var enemiesContainer = TestUtils.GetEnemiesContainer();
        var enemiesTransform = enemiesContainer.transform;

        AssertWrapper.IsNotNull(enemiesContainer);
        AssertWrapper.IsNotNull(enemiesTransform);

        var enemySpawner = enemiesContainer.transform.Find("EnemySpawner");
        var enemyBankScript = enemySpawner.GetComponent<EnemyBank>();
        var spawnerSpawnHoleInstantiatorScript = enemySpawner.GetComponent<SpawnHoleInstantiator>();

        AssertWrapper.IsNotNull(enemySpawner);
        AssertWrapper.IsNotNull(enemyBankScript);
        AssertWrapper.IsNotNull(spawnerSpawnHoleInstantiatorScript);

        var enemyDestructor = enemiesContainer.transform.Find("EnemyDestructor");
        var enemyDestructorScript = enemyDestructor.GetComponent<EnemiesDestructor>();

        AssertWrapper.IsNotNull(enemyDestructorScript);
        AssertWrapper.IsNotNull(enemyDestructor);
    }

    [Test]
    public void HalalitTests() 
    {

        // HALALIT
        var halalit = TestUtils.GetHalalit();
        var halalitTransform = halalit.transform;
        var halalitSpriteRenderer = halalit.GetComponent<SpriteRenderer>();
        var halalitRigidbody2D = halalit.GetComponent<Rigidbody2D>();
        var halalitCircleCollider2D = halalit.GetComponent<CircleCollider2D>();
        var halalitHalalitMovement = halalit.GetComponent<HalalitMovement>();
        var halalitKnockbackee = halalit.GetComponent<Knockbackee>();
        var halalitHealth = halalit.GetComponent<Health>();

        AssertWrapper.IsNotNull(halalit);
        AssertWrapper.IsNotNull(halalitTransform);
        AssertWrapper.IsNotNull(halalitSpriteRenderer);
        AssertWrapper.IsNotNull(halalitRigidbody2D);
        AssertWrapper.IsNotNull(halalitCircleCollider2D);
        AssertWrapper.IsNotNull(halalitHalalitMovement);
        AssertWrapper.IsNotNull(halalitKnockbackee);
        AssertWrapper.IsNotNull(halalitHealth);


        // ENGINE
        Transform engine = halalit.transform.Find("Engine");
        var engineTransform = engine.transform;
        var engineSpriteRenderer = engine.GetComponent<SpriteRenderer>();

        AssertWrapper.IsNotNull(engineSpriteRenderer);
        AssertWrapper.IsNotNull(engineTransform);

        var engineFire = engine.transform.Find("EngineFire");
        var engineFireTransform = engineFire.transform;
        var engineFireParticleSystem = engineFire.GetComponent<ParticleSystem>();
        var engineFireScript = engineFire.GetComponent<EngineFire>();

        AssertWrapper.IsNotNull(engineFire);
        AssertWrapper.IsNotNull(engineFireTransform);
        AssertWrapper.IsNotNull(engineFireParticleSystem);
        AssertWrapper.IsNotNull(engineFireScript);

        // WEAPON
        Transform weapon = halalit.transform.Find("Weapon");
        var weaponTransform = weapon.transform;
        var weaponSpriteRenderer = weapon.GetComponent<SpriteRenderer>();
        var weaponWeaponMovement = weapon.GetComponent<WeaponMovement>();
        var weaponWeaponAttack = weapon.GetComponent<WeaponAttack>();
        var weaponAttackToggle = weapon.GetComponent<AttackToggle>();

        AssertWrapper.IsNotNull(weapon);
        AssertWrapper.IsNotNull(weaponTransform);
        AssertWrapper.IsNotNull(weaponSpriteRenderer);
        AssertWrapper.IsNotNull(weaponWeaponMovement);
        AssertWrapper.IsNotNull(weaponWeaponAttack);
        AssertWrapper.IsNotNull(weaponAttackToggle);

        // PICKUP CLAW SHOOTER
        Transform pickupClawShooter = halalit.transform.Find("Pickup Claw Shooter");
        var pickupClawShooterTransform = pickupClawShooter.transform;
        var pickupClawShooterScript = pickupClawShooter.GetComponent<PickupClawShooter>();

        AssertWrapper.IsNotNull(pickupClawShooter);
        AssertWrapper.IsNotNull(pickupClawShooterTransform);
        AssertWrapper.IsNotNull(pickupClawShooterScript);

    }

    [Test]
    public void AttacksTests() 
    {
        // ATTACKS
        var attacks = TestUtils.GetAttacks();
        var attacksTransform = attacks.transform;
        var attacksBankScript = attacks.GetComponent<AttacksBank>();

        AssertWrapper.IsNotNull(attacks);
        AssertWrapper.IsNotNull(attacksTransform);
        AssertWrapper.IsNotNull(attacksBankScript);
    }

    [Test]
    public void EventSystemTests() 
    {
        // EVENT SYSTEM
        var eventSystem = TestUtils.GetEventSystem();
        var eventSystemTransform = eventSystem.transform;
        var eventSystemEventSystem = eventSystem.GetComponent<EventSystem>();
        var eventSystemStandaloneInputModule = eventSystem.GetComponent<StandaloneInputModule>();
        var eventSystemEvent = eventSystem.GetComponent<Event>();
        var eventSystemDeathEvent = eventSystem.GetComponent<DeathEvent>();
        var eventSystemAsteroidEvent = eventSystem.GetComponent<AsteroidEvent>();
        var eventSystemItemEvent = eventSystem.GetComponent<ItemEvent>();
        var eventSystemItemsBankEvent = eventSystem.GetComponent<ItemsBankEvent>();
        var eventSystemEnemyExplosionEvent = eventSystem.GetComponent<EnemyExplosionEvent>();
        var eventSystemValuableEvent = eventSystem.GetComponent<ValuableEvent>();
        var eventSystemItemDropEvent = eventSystem.GetComponent<ItemDropEvent>();
        var eventSystemValuableDropEvent = eventSystem.GetComponent<ValuableDropEvent>();

        AssertWrapper.IsNotNull(eventSystem);
        AssertWrapper.IsNotNull(eventSystemTransform);
        AssertWrapper.IsNotNull(eventSystemEventSystem);
        AssertWrapper.IsNotNull(eventSystemStandaloneInputModule);
        AssertWrapper.IsNotNull(eventSystemEvent);
        AssertWrapper.IsNotNull(eventSystemDeathEvent);
        AssertWrapper.IsNotNull(eventSystemAsteroidEvent);
        AssertWrapper.IsNotNull(eventSystemItemEvent);
        AssertWrapper.IsNotNull(eventSystemItemsBankEvent);
        AssertWrapper.IsNotNull(eventSystemEnemyExplosionEvent);
        AssertWrapper.IsNotNull(eventSystemValuableEvent);
        AssertWrapper.IsNotNull(eventSystemItemDropEvent);
        AssertWrapper.IsNotNull(eventSystemValuableDropEvent);
    }

    [Test]
    public void ValuablesContainerTests()
    {
        // VALUABLES CONTAINER
        var valuablesContainer = TestUtils.GetValuablesContainer();
        var valuablesContainerTransform = valuablesContainer.transform;
        var valuablesContainerValuableDropper = valuablesContainer.GetComponent<ValuableDropper>();

        AssertWrapper.IsNotNull(valuablesContainer);
        AssertWrapper.IsNotNull(valuablesContainerTransform);
        AssertWrapper.IsNotNull(valuablesContainerValuableDropper);
    }
}
