using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("TestsEditMode")]

internal static class TestUtils
{
    #region Constants
    // Scenes:
    public const string PLAYGROUND_SCENE_NAME = "Playground";
    public const string MAIN_MENU_SCENE_NAME= "MainMenu";

    // GameObjects names:
    public const string OBJECT_LOADER_NAME = "ObjectLoader";
    public const string ASTEROID_CONTAINER_NAME = "AsteroidsContainer";
    public const string ENEMIES_CONTAINER_NAME = "EnemiesContainer";
    public const string EXTERNAL_SAFE_ISLAND_NAME = "ExternalSafeIsland";

    // Resources paths:
    public const string SPRITE_CIRCLE_PATH = "Sprites/Circle";
    public const string ENEMY_MINE_PREFAB_PATH = "Prefabs/Attacks/EnemyAttacks/EnemyMine";
    public const string SHIELD_RAM_PREFAB_PATH = "Prefabs/Enemies/WaitAttackEnemies/ShieldRam";
    public const string COIN_THIEF_PREFAB_PATH = "Prefabs/Enemies/WaitAttackEnemies/CoinThief";
    public const string MIRROR_SHIELD_PREFAB_PATH = "Prefabs/Enemies/SpecialMovementEnemies/MirrorShield";
    public const string BALL_SHOT_PREFAB_PATH = "Prefabs/Attacks/HalalitAttacks/LongRange/BallShot/BallShot";
    public const string ENEMIES_BALL_SHOT_PREFAB_PATH = "Prefabs/Attacks/EnemyAttacks/EnemiesBallShot";
    public const string SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH = "Prefabs/Enemies/MoveAimAttackEnemies/ShootingInRangeEnemy";
    public const string ZIG_ZAG_ENEMY_PREFAB_PATH = "Prefabs/Enemies/SpecialMovementEnemies/ZigZagEnemy";
    public const string FOLLOWING_ENEMY_RADIUS_PREFAB_PATH = "Prefabs/Enemies/WaitAttackEnemies/FollowingEnemyRadius";
    public const string FOLLOWING_ENEMY_INTERVAL_PREFAB_PATH = "Prefabs/Enemies/WaitAttackEnemies/FollowingEnemyInterval";
    public const string SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH = "Prefabs/Enemies/MoveAimAttackEnemies/ShootingLaserAsteriskEnemy";
    public const string SHOOTING_LASER_RANGE_ENEMY_PREFAB_PATH = "Prefabs/Enemies/MoveAimAttackEnemies/ShootingLaserRangeEnemy";
    public const string ENEMIES_PREFABS_PATH = "Prefabs/Enemies";

    // Defaults:
    public const int ENEMIES_SEEDED_NUMBERS_LIST_DEFAULT_LENGTH = 5;
    public const int DEFAULT_RADIUS_OF_TARGET_POSITION_AROUND_HALALIT = 5;
    public const ItemRank DEFAULT_ITEM_RANK_1 = ItemRank.COMMON;
    public const ItemRank DEFAULT_ITEM_RANK_2 = ItemRank.RARE;
    public const int DEFAULT_POWER_1 = 1;
    public const int DEFAULT_POWER_2 = 5;
    public const float DEFAULT_CRITICAL_HIT_1 = 0.5f;
    public const float DEFAULT_CRITICAL_HIT_2 = 1.5f;
    public const int DEFAULT_LUCK_1 = 0;
    public const int DEFAULT_LUCK_2 = 100;
    public const float DEFAULT_RATE_1 = 2;
    public const float DEFAULT_RATE_2 = 0.5f;
    public const float DEFAULT_WEIGHT = 0;
    public static readonly AttackStats DEFAULT_ATTACK_STATS_1 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_1, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_2 = new(DEFAULT_ITEM_RANK_2, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_2, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_3 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_2, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_4 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_2, DEFAULT_LUCK_2, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_5 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_6 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_2, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly Vector2 DEFAULT_POSITION_TO_THE_RIGHT = new(5, 0);
    #endregion

    #region Turned off game objects
    public static GameObject _asteroidContainer;
    public static GameObject _enemiesContainer;
    #endregion

    #region Random Seeds
    internal static int SetRandomSeed(int seed = 0)
    {
        if (seed == 0)
        {
            seed = TestingRandomGenerator.Range(int.MinValue, int.MaxValue);
        }
        OriginalRandomSeed.IsTestMode = true;
        OriginalRandomSeed.TestSeed = seed;
        TestingRandomGenerator.SetSeed(seed);
        Debug.Log("Testing Random Seed set to: " + seed);
        return seed;
    }

    // Daily Twist (#239): the twist OriginalRandomSeed gives the next Playground load in test mode
    internal static void SetTestTwist(DailyTwistName dailyTwistName)
    {
        OriginalRandomSeed.TestTwist = dailyTwistName;
    }

    // Also clears the run's twist, which is static and would otherwise outlive the test (D22)
    internal static void ResetTestTwist()
    {
        OriginalRandomSeed.TestTwist = DailyTwistName.NONE;
        DailyTwists.SetCurrent(DailyTwists.Get(DailyTwistName.NONE));
    }

    // Daily Twist (#239): the next Playground load runs as a player's run, not a test run, so its seed and twist come from PlayerStats.
    // The seed comes from the date, as when the menu signs in (#273)
    internal static void SetPlayerRun(bool isDailyRun, System.DateTime dailyDate)
    {
        OriginalRandomSeed.IsTestMode = false;
        PlayerStats._isDailyRun = isDailyRun;
        PlayerStats._dailyDate = dailyDate;
        PlayerStats.SetDailySeeds();
    }

    // Back to test mode, with the PlayerStats values a run has before the menu signs in
    internal static void ResetPlayerRun()
    {
        OriginalRandomSeed.IsTestMode = true;
        PlayerStats._isDailyRun = false;
        PlayerStats._dailySeed = null;
        PlayerStats._dailyDate = System.DateTime.MinValue;
    }
    #endregion


    #region Scene SetUp
    internal static void DestroyAllGameObjects()
    {
        var allGameObjects = Object.FindObjectsOfType<GameObject>();
        foreach (var gameObject in allGameObjects)
        {
            Object.Destroy(gameObject);
        }
    }

    internal static void SetUpShot(AttackName attackName, AttackStats attackStats = null)
    {
        var attackToggle = GetAttackToggle();
        attackToggle.SetNewAttack(attackName, attackStats ?? DEFAULT_ATTACK_STATS_1);
    }


    internal static void SetEnemyPosition(Vector2 enemyPosition, int index = 0)
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[index].transform.position = enemyPosition;
    }

    internal static void SetRandomEnemyPosition(float radiusOfEnemyPositionAroundHalalit = 5)
    {
        TesingWithOneEnemyValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(
                Tag.ENEMY.GetDescription()), 
                radiusOfEnemyPositionAroundHalalit);
    }

    internal static void SetAsteroidPosition(Vector2 asteroidPosition, int index = 0)
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
        SetGameObjectPosition(target[index], asteroidPosition);
    }

    internal static void SetRandomAsteroidPosition(float radiusOfAsteroidPositionAroundHalalit = 5)
    {
        TesingWithOneAsteroidValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription()),
            radiusOfAsteroidPositionAroundHalalit);
    }

    internal static void SetRandomItemPosition(float radiusOfItemPositionAroundHalalit = 5)
    {
        TesingWithOneItemValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription()), 
            radiusOfItemPositionAroundHalalit);
    }

    internal static void SetItemPosition(Vector2 position, int index = 0) 
    {
        TesingWithOneItemValidation();

        SetGameObjectPosition(
            GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription())[index],
            position);
    }

    internal static void SetValuablePosition(Vector2 valuablePosition, int index = 0)
    {
        SetGameObjectPosition(
            GameObject.FindGameObjectsWithTag(Tag.VALUABLE.GetDescription())[index],
            valuablePosition);
    }

    // Drops a valuable through the game's own drop event, so it is a full valuable prefab (with its Valuable script and inner collider).
    // The drop event instantiates the valuable synchronously, so the new valuable is returned in the same frame.
    internal static GameObject DropValuable(ValuableName valuableName, Vector2 position)
    {
        var valuablesBeforeDrop = GetValuables();
        ValuableDropEvent.Invoke(EventName.NEW_VALUABLE_DROP, null, new(Dropper.DEFAULT, position, Vector2.zero, valuableName));

        return GetValuables().First(valuable => !valuablesBeforeDrop.Contains(valuable));
    }

    // Drops a valuable on the Halalit and waits until the Halalit has picked it up, so the score rises by its value.
    // Waits at most a few physics steps; the caller asserts the score, so a missed pickup fails there.
    internal static IEnumerator CollectValuable(ValuableName valuableName)
    {
        var maxFixedUpdatesToCollect = 10;
        var valuable = DropValuable(valuableName, GetHalalit().transform.position);
        for (int i = 0; i < maxFixedUpdatesToCollect && valuable != null; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    internal static ValuableTarnish GetTarnish(GameObject valuable)
    {
        var tarnish = valuable.GetComponent<ValuableTarnish>();
        if (tarnish == null)
        {
            throw new System.Exception($"ValuableTarnish missing on {valuable.name} (Needs Ofir in Unity, #231)");
        }
        return tarnish;
    }

    // Loads the Mine Layer's mine straight from its prefab, as the Mine Layer would lay it (no movement, its own lifetime).
    internal static GameObject LoadEnemyMine(Vector2 position)
    {
        var minePrefab = Resources.Load<GameObject>(ENEMY_MINE_PREFAB_PATH);
        if (minePrefab == null)
        {
            throw new System.Exception($"EnemyMine prefab not found at Resources/{ENEMY_MINE_PREFAB_PATH}");
        }
        return Object.Instantiate(minePrefab, position, Quaternion.identity);
    }

    // Loads the Shield Ram straight from its prefab, seeded, with its state machine off so it neither spins nor charges.
    internal static GameObject LoadShieldRam(Vector2 position, int seed)
    {
        var shieldRam = LoadArmoredEnemy("ShieldRam", SHIELD_RAM_PREFAB_PATH, position, seed);
        shieldRam.GetComponent<WaitAttackStateMachine>().enabled = false;
        return shieldRam;
    }

    // Loads the Mirror Shield straight from its prefab, seeded, standing still and not turning: tests set its rotation by hand.
    internal static GameObject LoadMirrorShield(Vector2 position, int seed)
    {
        var mirrorShield = LoadArmoredEnemy("MirrorShield", MIRROR_SHIELD_PREFAB_PATH, position, seed);
        if (mirrorShield.GetComponent<TargetFacer>() == null || mirrorShield.GetComponent<LameEnemy>() == null)
        {
            throw new System.Exception("MirrorShield prefab lacks TargetFacer or LameEnemy on its root (Needs Ofir in Unity)");
        }
        if (!mirrorShield.GetComponent<FrontArmor>().IsReflecting())
        {
            throw new System.Exception("MirrorShield prefab's FrontArmor does not reflect (Needs Ofir in Unity)");
        }
        mirrorShield.GetComponent<LameEnemy>().StopMovement();
        mirrorShield.GetComponent<TargetFacer>().enabled = false;
        return mirrorShield;
    }

    // Loads an enemy with a FrontArmor on its root straight from its prefab, with its seed.
    private static GameObject LoadArmoredEnemy(string enemyName, string prefabPath, Vector2 position, int seed)
    {
        var enemyPrefab = Resources.Load<GameObject>(prefabPath);
        if (enemyPrefab == null)
        {
            throw new System.Exception($"{enemyName} prefab not found at Resources/{prefabPath} (Needs Ofir in Unity)");
        }
        if (enemyPrefab.GetComponent<FrontArmor>() == null)
        {
            throw new System.Exception($"{enemyName} prefab has no FrontArmor on its root (Needs Ofir in Unity)");
        }
        var enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return enemy;
    }

    // Loads a wait-attack enemy straight from its prefab, seeded, with its state machine off, so a test drives its wait and attack by hand.
    internal static GameObject LoadEnemyWithoutStateMachine(string prefabPath, Vector2 position, int seed)
    {
        var enemyPrefab = Resources.Load<GameObject>(prefabPath);
        if (enemyPrefab == null)
        {
            throw new System.Exception($"Enemy prefab not found at Resources/{prefabPath}");
        }
        var enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        enemy.GetComponent<WaitAttackStateMachine>().enabled = false;
        return enemy;
    }

    // Loads the Coin Thief straight from its prefab, seeded, with its state machine on, so it waits, steals and eats as in a run.
    internal static GameObject LoadCoinThief(Vector2 position, int seed)
    {
        var coinThiefPrefab = Resources.Load<GameObject>(COIN_THIEF_PREFAB_PATH);
        if (coinThiefPrefab == null)
        {
            throw new System.Exception($"CoinThief prefab not found at Resources/{COIN_THIEF_PREFAB_PATH} (Needs Ofir in Unity)");
        }
        if (coinThiefPrefab.GetComponent<WaitAttackLootWait>() == null || coinThiefPrefab.GetComponent<WaitAttackStealAttack>() == null)
        {
            throw new System.Exception("CoinThief prefab lacks WaitAttackLootWait or WaitAttackStealAttack on its root (Needs Ofir in Unity)");
        }
        var enemyDropper = coinThiefPrefab.GetComponent<EnemyDropper>();
        if (enemyDropper == null || enemyDropper.GetDropper() != Dropper.COIN_THIEF)
        {
            throw new System.Exception("CoinThief prefab's EnemyDropper is not set to COIN_THIEF (Needs Ofir in Unity)");
        }
        var coinThief = Object.Instantiate(coinThiefPrefab, position, Quaternion.identity);
        coinThief.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return coinThief;
    }

    // Loads an enemy that should carry ShotDodger straight from its prefab, with its seed, and switches off its own movement,
    // so that only a dodge moves it. Its Start has not run yet: wait a frame before setting its health.
    internal static GameObject LoadDodgingEnemy(string prefabPath, Vector2 position, int seed)
    {
        var enemyPrefab = Resources.Load<GameObject>(prefabPath);
        if (enemyPrefab == null || enemyPrefab.GetComponent<ShotDodger>() == null)
        {
            throw new System.Exception($"ShotDodger missing on {prefabPath} (Needs Ofir in Unity)");
        }

        var enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);

        var moveAimAttackStateMachine = enemy.GetComponent<MoveAimAttackStateMachine>();
        if (moveAimAttackStateMachine != null)
        {
            moveAimAttackStateMachine.enabled = false;
        }

        var zigZagEnemy = enemy.GetComponent<ZigZagEnemy>();
        if (zigZagEnemy != null)
        {
            zigZagEnemy.enabled = false;
        }

        return enemy;
    }

    // Loads an enemy straight from its prefab, seeded, with all its scripts on, as it is after its spawn hole closes.
    internal static GameObject LoadEnemy(string prefabPath, Vector2 position, int seed)
    {
        var enemyPrefab = Resources.Load<GameObject>(prefabPath);
        if (enemyPrefab == null)
        {
            throw new System.Exception($"Enemy prefab not found at Resources/{prefabPath}");
        }
        var enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return enemy;
    }

    internal static WoundedRetreat GetWoundedRetreat(GameObject enemy)
    {
        var woundedRetreat = enemy.GetComponent<WoundedRetreat>();
        if (woundedRetreat == null)
        {
            throw new System.Exception("WoundedRetreat missing on enemy prefab (Needs Ofir in Unity, #201)");
        }
        return woundedRetreat;
    }

    // Mimic (#214)
    public const string MIMIC_PREFAB_PATH = "Prefabs/Enemies/WaitAttackEnemies/Mimic";

    // Loads the Mimic straight from its prefab, seeded, with all its scripts on, so it hides, wakes and charges as in a run.
    internal static GameObject LoadMimic(Vector2 position, int seed)
    {
        var mimicPrefab = Resources.Load<GameObject>(MIMIC_PREFAB_PATH);
        if (mimicPrefab == null)
        {
            throw new System.Exception($"Mimic prefab not found at Resources/{MIMIC_PREFAB_PATH} (Needs Ofir in Unity)");
        }
        if (mimicPrefab.GetComponent<WaitAttackDisguisedWait>() == null)
        {
            throw new System.Exception("Mimic prefab lacks WaitAttackDisguisedWait on its root (Needs Ofir in Unity)");
        }
        var enemyDropper = mimicPrefab.GetComponent<EnemyDropper>();
        if (enemyDropper == null || enemyDropper.GetDropper() != Dropper.MIMIC)
        {
            throw new System.Exception("Mimic prefab's EnemyDropper is not set to MIMIC (Needs Ofir in Unity)");
        }
        var mimic = Object.Instantiate(mimicPrefab, position, Quaternion.identity);
        mimic.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return mimic;
    }

    // Laser beams fired by enemies: consecutive attacks on the EnemyShots layer.
    internal static List<ConsecutiveAttack> GetEnemyLaserBeams()
    {
        var enemyShotsLayer = LayerMask.NameToLayer(Layer.ENEMY_SHOTS.GetDescription());
        return Object.FindObjectsOfType<ConsecutiveAttack>().Where(laserBeam => laserBeam.gameObject.layer == enemyShotsLayer).ToList();
    }

    // Loads a Ball Shot straight from its prefab, flying in the given direction (BallShot flies along its transform.up).
    internal static GameObject LoadBallShot(Vector2 position, Vector2 direction)
    {
        var ballShotPrefab = Resources.Load<GameObject>(BALL_SHOT_PREFAB_PATH);
        if (ballShotPrefab == null)
        {
            throw new System.Exception($"BallShot prefab not found at Resources/{BALL_SHOT_PREFAB_PATH}");
        }
        return Object.Instantiate(ballShotPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
    }

    // Loads an enemy Ball Shot straight from its prefab, flying in the given direction, fired by owner (null: no owner, as a player shot).
    internal static GameObject LoadEnemyShot(Vector2 position, Vector2 direction, Transform owner)
    {
        var enemyShotPrefab = Resources.Load<GameObject>(ENEMIES_BALL_SHOT_PREFAB_PATH);
        if (enemyShotPrefab == null)
        {
            throw new System.Exception($"EnemiesBallShot prefab not found at Resources/{ENEMIES_BALL_SHOT_PREFAB_PATH}");
        }
        var enemyShot = Object.Instantiate(enemyShotPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
        if (owner != null)
        {
            ShotOwner.Mark(enemyShot, owner);
        }
        return enemyShot;
    }

    internal static Grudge GetGrudge(GameObject enemy)
    {
        var grudge = enemy.GetComponent<Grudge>();
        if (grudge == null)
        {
            throw new System.Exception("Grudge missing on enemy prefab (Needs Ofir in Unity, #216)");
        }
        return grudge;
    }

    // Leech (#215)
    public const string LEECH_PREFAB_PATH = "Prefabs/Enemies/SpecialMovementEnemies/Leech";

    // Loads the Leech straight from its prefab, seeded, with all its scripts on, so it drifts and latches as in a run.
    internal static GameObject LoadLeech(Vector2 position, int seed)
    {
        var leechPrefab = Resources.Load<GameObject>(LEECH_PREFAB_PATH);
        if (leechPrefab == null)
        {
            throw new System.Exception($"Leech prefab not found at Resources/{LEECH_PREFAB_PATH} (Needs Ofir in Unity)");
        }
        if (leechPrefab.GetComponent<LeechEnemy>() == null || leechPrefab.GetComponent<LameEnemy>() != null)
        {
            throw new System.Exception("Leech prefab needs LeechEnemy and no LameEnemy on its root (Needs Ofir in Unity)");
        }
        var enemyDropper = leechPrefab.GetComponent<EnemyDropper>();
        if (enemyDropper == null || enemyDropper.GetDropper() != Dropper.LEECH)
        {
            throw new System.Exception("Leech prefab's EnemyDropper is not set to LEECH (Needs Ofir in Unity)");
        }
        var leech = Object.Instantiate(leechPrefab, position, Quaternion.identity);
        leech.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return leech;
    }

    // Mender (#230)
    public const string MENDER_PREFAB_PATH = "Prefabs/Enemies/SpecialMovementEnemies/Mender";

    // Loads the Mender straight from its prefab, seeded, with all its scripts on, so it follows and heals as in a run.
    internal static GameObject LoadMender(Vector2 position, int seed)
    {
        var menderPrefab = Resources.Load<GameObject>(MENDER_PREFAB_PATH);
        if (menderPrefab == null)
        {
            throw new System.Exception($"Mender prefab not found at Resources/{MENDER_PREFAB_PATH} (Needs Ofir in Unity, #230)");
        }
        if (menderPrefab.GetComponent<MenderEnemy>() == null || menderPrefab.GetComponent<LameEnemy>() != null)
        {
            throw new System.Exception("Mender prefab needs MenderEnemy and no LameEnemy on its root (Needs Ofir in Unity, #230)");
        }
        var enemyDropper = menderPrefab.GetComponent<EnemyDropper>();
        if (enemyDropper == null || enemyDropper.GetDropper() != Dropper.MENDER)
        {
            throw new System.Exception("Mender prefab's EnemyDropper is not set to MENDER (Needs Ofir in Unity, #230)");
        }
        var mender = Object.Instantiate(menderPrefab, position, Quaternion.identity);
        mender.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        return mender;
    }

    // Loads a Tripwire post straight from its AttacksBank prefab, flying in the given direction (it flies along its transform.up).
    internal static GameObject LoadTripwirePost(Vector2 position, Vector2 direction)
    {
        var tripwirePrefab = AttacksBank.GetAttackPrefab(AttackName.TRIPWIRE);
        return Object.Instantiate(tripwirePrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
    }

    // Crash Damage (#232)
    internal static CrashDamage GetCrashDamage(GameObject enemy)
    {
        var crashDamage = enemy.GetComponent<CrashDamage>();
        if (crashDamage == null)
        {
            throw new System.Exception($"CrashDamage missing on {enemy.name} (Needs Ofir in Unity, #232)");
        }
        return crashDamage;
    }

    // Chain Spark (#229)
    // Loads a Chain Spark straight from its AttacksBank prefab, flying in the given direction (BallShot flies along its transform.up).
    internal static GameObject LoadChainSpark(Vector2 position, Vector2 direction)
    {
        var chainSparkPrefab = AttacksBank.GetAttackPrefab(AttackName.CHAIN_SPARK);
        return Object.Instantiate(chainSparkPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
    }

    // Interrupt (#238)
    internal static Interrupt GetInterrupt(GameObject enemy)
    {
        var interrupt = enemy.GetComponent<Interrupt>();
        if (interrupt == null)
        {
            throw new System.Exception($"Interrupt missing on {enemy.name} (Needs Ofir in Unity, #238)");
        }
        return interrupt;
    }

    // Endless Ramp (#248)
    // Takes groups from the enemy bank until fills * GetEnemiesPerFill() enemies are out: the first fill, then the refills.
    // The bank refills itself and never runs out, so this asks at most that many times: a bad bank fails here instead of hanging the editor.
    internal static List<List<EnemyEntity>> TakeEnemyGroups(EnemyBank enemyBank, int fills, int seed)
    {
        var enemiesPerFill = enemyBank.GetEnemiesPerFill();
        AssertWrapper.Greater(enemiesPerFill, 0, "The scene's enemy bank has no enemies", seed);

        var enemiesToTake = fills * enemiesPerFill;
        var enemyGroups = new List<List<EnemyEntity>>();
        var takenEnemies = 0;
        for (int i = 0; i < enemiesToTake && takenEnemies < enemiesToTake; i++)
        {
            var enemyGroup = enemyBank.GetNextSpawnHoleEnemiesList();
            AssertWrapper.IsTrue(enemyGroup != null, $"The enemy bank ran out after {takenEnemies} enemies, though it refills itself", seed);
            enemyGroups.Add(enemyGroup);
            takenEnemies += enemyGroup.Count;
        }

        AssertWrapper.AreEqual(enemiesToTake, takenEnemies, $"Took {takenEnemies} enemies for {fills} fills of {enemiesPerFill}: a group spans two fills, or the bank gave empty groups", seed);
        return enemyGroups;
    }

    // Tether Twins (#237)
    public const string TETHER_TWIN_PREFAB_PATH = "Prefabs/Enemies/SpecialMovementEnemies/TetherTwin";

    // Makes a pair of Tether Twins from one paired entry, as a spawn hole does, seeded, with all their scripts on.
    // The second twin is moved before any physics step. Frozen twins neither move nor turn, so a test sets their line by hand.
    internal static List<GameObject> LoadTetherTwins(Vector2 firstPosition, Vector2 secondPosition, int seed, bool frozen = true)
    {
        var tetherTwinPrefab = Resources.Load<GameObject>(TETHER_TWIN_PREFAB_PATH);
        if (tetherTwinPrefab == null)
        {
            throw new System.Exception($"TetherTwin prefab not found at Resources/{TETHER_TWIN_PREFAB_PATH} (Needs Ofir in Unity, #237)");
        }
        var tetherTwin = tetherTwinPrefab.GetComponent<TetherTwin>();
        if (tetherTwin == null || tetherTwinPrefab.GetComponent<LameEnemy>() != null)
        {
            throw new System.Exception("TetherTwin prefab needs TetherTwin and no LameEnemy on its root (Needs Ofir in Unity, #237)");
        }
        if (tetherTwin.GetTetherPrefab() == null)
        {
            throw new System.Exception("TetherTwin prefab's Tether Prefab field is empty (Needs Ofir in Unity, #237)");
        }
        var enemyDropper = tetherTwinPrefab.GetComponent<EnemyDropper>();
        if (enemyDropper == null || enemyDropper.GetDropper() != Dropper.TETHER_TWIN)
        {
            throw new System.Exception("TetherTwin prefab's EnemyDropper is not set to TETHER_TWIN (Needs Ofir in Unity, #237)");
        }

        var twins = SpawnHole.InstantiateEnemyEntity(new EnemyEntity(tetherTwinPrefab, seed, seed + 1), firstPosition, null);
        SetGameObjectPosition(twins[1], secondPosition);
        if (frozen)
        {
            foreach (var twin in twins)
            {
                twin.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        return twins;
    }

    internal static void SetGameObjectPosition(GameObject gameObject, Vector2 position) 
    {
        gameObject.transform.position = position;
    }

    private static void SetRandomGameObjectPosition(GameObject gameObject, float radiusOfTargetPositionAroundHalalit)
    {
        gameObject.transform.position = GetRandomVector2OnCircle(radiusOfTargetPositionAroundHalalit);
    }

    private static Vector2 GetRandomVector2OnCircle(float radius = 1)
    {
        float angle = TestingRandomGenerator.Range(0, 2 * Mathf.PI, true);
        return Utils.RadianToVector2(angle) * radius;
    }

    internal static void RotaeEnemy(float degrees) 
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        enemy[0].transform.rotation = Quaternion.Euler(0, 0, degrees);
    }

    // Takes the menu load off the game over event, so that a death does not load the real menu,
    // which signs in for real. It takes effect one frame later.
    // GameOverEvent calls a snapshot of its handlers taken in Start(), so removing the handler is not enough:
    // the second GameOverEvent takes a new snapshot, without the menu load, in its own Start().
    internal static void UnhookMenuLoadFromGameOver()
    {
        GetHalalit().GetComponent<GameOver>().DestroyEventListeners();
        GetEventSystem().AddComponent<GameOverEvent>();
    }

    internal static void KillHalalit()
    {
        var health = GetHalalit().GetComponent<Health>();
        health.TryChangeHealth(-health.GetHealth());
    }

    internal static void SetEnemiesHealth(int health)
    {
        var enemies = GetEnemies();
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Health>().SetHealth(health);
        }
    }

    internal static void SetAsteroidsHealth(int health) 
    {
        var asteroids = GetAsteroids();
        foreach (var asteroid in asteroids)
        {
            asteroid.GetComponent<Health>().SetHealth(health);
        }
    }

    internal static void SetAsteroidScale(float scale, int index = 0)
    {
        var asteroids = GetAsteroids();
        asteroids[index].transform.localScale = new Vector3(scale, scale, 0);
    }

    // keepEnemiesContainer (#248): leaves the enemy spawner running, for tests of the spawner itself
    internal static void SetTestMode(bool keepEnemiesContainer = false)
    {
        OriginalRandomSeed.IsTestMode = true;
        
        _asteroidContainer = GameObject.Find(ASTEROID_CONTAINER_NAME);
        _asteroidContainer.SetActive(false);

        if (!keepEnemiesContainer)
        {
            _enemiesContainer = GameObject.Find(ENEMIES_CONTAINER_NAME);
            _enemiesContainer.SetActive(false);
        }

        var weaponAttack = GetWeaponAttack();
        weaponAttack.SetIsTesting(true);

        TestTimeController.SetTestTimeScale();
    }

    #endregion

    #region SceneGetters
    internal static GameObject GetValuablesContainer()
    {
        return GameObject.FindGameObjectWithTag(Tag.VALUABLES_CONTAINER.GetDescription());
    }

    internal static GameObject GetSetup() 
    {
        return GameObject.FindGameObjectWithTag(Tag.SETUP.GetDescription());
    }

    internal static GameObject GetEventSystem() 
    {
        return GameObject.FindGameObjectWithTag(Tag.EVENT_SYSTEM.GetDescription());
    }

    internal static GameObject GetAttacks() 
    {
        return GameObject.FindGameObjectWithTag(Tag.ATTACKS.GetDescription());
    }

    internal static GameObject GetHalalit()
    {
        return GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
    }

    internal static GameObject GetEnemiesContainer() 
    {
        return GameObject.FindGameObjectWithTag(Tag.ENEMIES_CONTAINER.GetDescription());
    }

    internal static GameObject GetAsteroidsContainer()
    {
        return GameObject.FindGameObjectWithTag(Tag.ASTEROIDS_CONTAINER.GetDescription());
    }

    internal static GameObject GetCamera()
    {
        return GameObject.FindGameObjectWithTag(Tag.MAIN_CAMERA.GetDescription());
    }

    internal static int GetScore() 
    {
        var ScoreText = GetScoreText();
        var score = ScoreText.GetComponent<Score>();
        return score.GetScore();
    }

    internal static ValuableName GetValuableName(GameObject valuable) 
    {
        return valuable.GetComponent<Valuable>().GetValuableName();
    }

    internal static List<KeyValuePair<ValuableName, int>> GetValuableValues() 
    {
        var score = GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription());
        if (score == null)
        {
            Debug.LogError("Score GameObject not found!");
            return new List<KeyValuePair<ValuableName, int>>(); // Return an empty list to avoid further errors
        }
        var scoreScript = score.GetComponent<Score>();
        if (scoreScript == null)
        {
            Debug.LogError("Score component not found on the Score GameObject!");
            return new List<KeyValuePair<ValuableName, int>>(); // Return an empty list to avoid further errors
        }
        return scoreScript.GetValuableValues();
    }

    internal static HalalitMovement GetHalalitMovement() 
    {
        GameObject halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        return halalit.GetComponent<HalalitMovement>();
    }

    internal static ItemDropper GetItemDropper() 
    {
        var itemsFactory = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription());
        return itemsFactory.GetComponent<ItemDropper>();
    }

    internal static ItemsBank GetItemsBank()
    {
        var itemsFactory = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription());
        return itemsFactory.GetComponent<ItemsBank>();
    }

    internal static DropsBank GetDropsBank()
    {
        var itemsFactory = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription());
        return itemsFactory.GetComponent<DropsBank>();
    }

    internal static EnemyBank GetEnemyBank()
    {
        var enemiesSpawner = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription());
        return enemiesSpawner.GetComponent<EnemyBank>();
    }

    internal static SpawnHoleInstantiator GetSpawnHoleInstantiator()
    {
        var enemiesSpawner = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription());
        return enemiesSpawner.GetComponent<SpawnHoleInstantiator>();
    }

    internal static GameObject GetInternalWorld() 
    {
        return GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription());
    }

    internal static GameObject GetExternalWorld()
    {
        return GameObject.FindGameObjectWithTag(Tag.EXTERNAL_WORLD.GetDescription());
    }

    internal static GameObject GetExternalTopEdge() 
    {
        return GameObject.FindGameObjectWithTag(Tag.TOP_EDGE.GetDescription());
    }

    internal static GameObject GetExternalBottomEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.BOTTOM_EDGE.GetDescription());
    }

    internal static GameObject GetExternalLeftEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.LEFT_EDGE.GetDescription());
    }

    internal static GameObject GetExternalRightEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.RIGHT_EDGE.GetDescription());
    }

    internal static GameObject GetAttackJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.ATTACK_JOYSTICK.GetDescription());
    }

    internal static GameObject GetMovementJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.MOVEMENT_JOYSTICK.GetDescription());
    }

    internal static GameObject GetScoreText() 
    {
        return GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription());
    }

    internal static GameObject GetHalalitHealthBar()
    {
        return GameObject.FindGameObjectWithTag(Tag.HALALIT_HEALTH_BAR.GetDescription());
    }

    internal static GameObject GetShot()
    {
        return GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
    }

    internal static GameObject GetPickupClaw()
    {
        return GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
    }

    internal static GameObject GetItem()
    {
        return GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription());
    }

    internal static GameObject[] GetItems()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription());
    }

    internal static GameObject[] GetValuables()
    {
        return GameObject.FindGameObjectsWithTag(Tag.VALUABLE.GetDescription());
    }

    internal static GameObject GetEnemy()
    {
        return GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
    }

    internal static GameObject[] GetEnemies()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
    }

    internal static GameObject GetAsteroid()
    {
        return GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription());
    }

    internal static AsteroidMovement GetAsteroidMovement() 
    {
        return GetAsteroid().GetComponent<AsteroidMovement>();
    }

    internal static GameObject[] GetAsteroids()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
    }

    internal static PickupClawShooter GetPickupClawShooter()
    {
        var pickupClawShooter = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW_SHOOTER.GetDescription());
        return pickupClawShooter.GetComponent<PickupClawShooter>();
    }

    internal static float GetPickupClawManeuverRadius()
    {
        return GetPickupClawShooter().GetPickupClawPrefab().GetComponent<PickupClawStateMachine>().GetPickupClawManeuverRadius();
    }

    internal static PickupClawStateMachine GetPickupClawStateMachine(GameObject pickupClaw)
    {
        return pickupClaw.GetComponent<PickupClawStateMachine>();
    }

    internal static WeaponAttack GetWeaponAttack()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponAttack>();
    }

    internal static AttackToggle GetAttackToggle()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<AttackToggle>();
    }

    internal static WeaponMovement GetWeaponMovement()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponMovement>();
    }

    internal static BoxCollider2D GetInternalWorldBoxCollider2D()
    {
        return GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription()).GetComponent<BoxCollider2D>();
    }

    internal static Vector2 GetEnemyPosition()
    {
        TesingWithOneEnemyValidation();

        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.transform.position;
    }

    internal static Vector2 GetItemPosition()
    {
        TesingWithOneItemValidation();

        var item = GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription());
        return item.transform.position;
    }

    internal static Vector2 GetEnemyMovementDirection()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.GetComponent<Rigidbody2D>().velocity.normalized;
    }
    internal static Vector2 GetEnemyNearestPositionToHalalit()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return GetNearestPositionToHalalit(enemy);
    }

    internal static Vector2 GetAsteroidNearestPositionToHalalit()
    {
        TesingWithOneAsteroidValidation();

        var asteroid = GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription());
        return GetNearestPositionToHalalit(asteroid);
    }

    internal static float GetEnemyHealth()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.GetComponent<Health>().GetHealth();
    }

    internal static float GetAsteroidHealth()
    {
        TesingWithOneAsteroidValidation();
        return GetAsteroid().GetComponent<Health>().GetHealth();
    }

    internal static List<int> GetAllEnemiesHealth()
    {
        TesingWithMultipleEnemiesValidation();
        var enemies = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription()).ToList();
        return enemies.Select(enemy => enemy.GetComponent<Health>().GetHealth()).ToList();
    }

    internal static Vector2 GetNearestPositionToHalalit(GameObject gameObject)
    {
        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var bounds = gameObject.GetComponent<Collider2D>().bounds;
        return bounds.ClosestPoint(halalit.transform.position);
    }
    #endregion

    #region Joystick Touchs
    internal static Vector2 GetRandomTouch()
    {
        return new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
    }

    internal static Vector2 GetRandomTouchUnderAttackTrigger(WeaponAttack weaponAttack)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
        } while (randomTouch.magnitude >= weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }

    internal static Vector2 GetRandomTouchOverAttackTrigger(float attackJoystickEdge)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
        } while (randomTouch.magnitude < attackJoystickEdge);

        return randomTouch;
    }

    internal static Vector2 GetTouchOverAttackTriggetTowardsPosition(Vector2 position, float attackJoystickEdge)
    {
        var direction = position.normalized;
        return direction * (attackJoystickEdge + 0.1f);
    }
    #endregion

    #region Predicates
    internal static bool IsGameOver()
    {
        return SceneManager.GetActiveScene().name == MAIN_MENU_SCENE_NAME;
    }

    internal static bool IsSomewhereOnInternalWorldEdges(Vector2 position)
    {
        Vector3 position3 = new(position.x, position.y, 1); // TODO: why our InternalWorldBoxCollider2DBoxCollider2D Z=1?

        var boxCollider = GetInternalWorldBoxCollider2D();
        var edgeThreshold = 0.05f;
        var bounds = boxCollider.bounds;

        var largerBounds = new Bounds(bounds.center, bounds.size * (1 + edgeThreshold));
        var smallerBounds = new Bounds(bounds.center, bounds.size * (1 - edgeThreshold));

        return largerBounds.Contains(position3) && !smallerBounds.Contains(position3);
    }

    internal static bool AreCollidersTouch(Collider2D collider1, Collider2D collider2)
    {
        ContactFilter2D contactFilter = new();
        contactFilter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int overlapCount = collider1.OverlapCollider(contactFilter, results);

        bool isColliding = false;
        for (int i = 0; i < overlapCount; i++)
        {
            if (results[i] == collider2)
            {
                isColliding = true;
                break;
            }
        }

        return isColliding;
    }

    internal static bool SomeCollidersTouch(List<Collider2D> colliders)
    {
        foreach (Collider2D collider1 in colliders)
        {
            foreach (Collider2D collider2 in colliders)
            {
                if (collider1 != collider2 && AreCollidersTouch(collider1, collider2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsPartlyInsideTheWorld(GameObject gameObject)
    {
        var internalWorldCollider = GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription()).GetComponent<Collider2D>();
        var gameObjectCollider = gameObject.GetComponent<Collider2D>() != null ?
            gameObject.GetComponent<Collider2D>() : 
            gameObject.GetComponentInChildren<Collider2D>();

        return AreCollidersTouch(gameObjectCollider, internalWorldCollider);
    }
    #endregion

    #region Validations
    internal static void TesingWithOneEnemyValidation()
    {
        var enemy = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (enemy.Length != 1)
        {
            throw new System.Exception("There should be only one Enemy in the scene. There are " + enemy.Length + " enemies in the scene");
        }
    }

    internal static void TesingWithOneAsteroidValidation() 
    {
        var asteroid = GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
        if (asteroid.Length != 1)
        {
            throw new System.Exception("There should be only one Asteroid in the scene. There are " + asteroid.Length + " asteroids in the scene");
        }
    }

    internal static void TesingWithOneItemValidation()
    {
        var item = GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription());
        if (item.Length != 1)
        {
            throw new System.Exception("There should be only one Item in the scene. There are " + item.Length + " items in the scene");
        }
    }

    internal static void TesingWithMultipleEnemiesValidation() 
    {
        var enemies = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (enemies.Length <= 1)
        {
            throw new System.Exception("There should be multiple Enemies in the scene. There are " + enemies.Length + " enemies in the scene");
        }
    }

    internal static void AssertAttackPrefabHas<T>(AttackName attackName, int seed) where T : Component
    {
        GameObject attackPrefab = null;
        try
        {
            attackPrefab = AttacksBank.GetAttackPrefab(attackName);
        }
        catch (KeyNotFoundException)
        {
            AssertWrapper.Fail($"{attackName} is not in AttacksBank (Needs Ofir in Unity)", seed);
        }

        if (attackPrefab == null || attackPrefab.GetComponent<T>() == null)
        {
            AssertWrapper.Fail($"{attackName} prefab has no {typeof(T).Name} component (Needs Ofir in Unity)", seed);
        }
    }

    // Fails unless every AttackBehaviour in the attack's prefab, its parts included, is marked IsMelee.
    internal static void AssertMeleeAttack(AttackName attackName, int seed)
    {
        AssertAttackPrefabHas<AttackBehaviour>(attackName, seed);
        var attackBehaviours = AttacksBank.GetAttackPrefab(attackName).GetComponentsInChildren<AttackBehaviour>(true);

        if (attackBehaviours.Any(attackBehaviour => !attackBehaviour.IsMelee))
        {
            AssertWrapper.Fail($"{attackName} prefab has an AttackBehaviour without IsMelee (Needs Ofir in Unity)", seed);
        }
    }
    #endregion

    #region Shot Actions
    public static IEnumerator GetDescreteShotPositionHittingTarget()
    {
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;

        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
            yield return lastShotPosition;
        } 
        while (shot != null);
    }

    public static IEnumerator GetConsecutiveShotPositionHitingTarget(int expectedHits, float attackCooldown, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;
        int hits = 0;
        float newHealth;
        float lastEnemyHealth;
        do
        {
            lastEnemyHealth = GetEnemyHealth();
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            newHealth = GetEnemyHealth();
            if (newHealth < lastEnemyHealth)
            {
                hits++;
            }
        }
        while (hits != expectedHits);

        yield return lastShotPosition;
    }

    public static IEnumerator GetTimeBetweenProjectedDescreteShot(int expectedShotCount, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        int shotsCount = 0;
        float startTime = Time.time;

        do
        {
            bool isShotProjected = weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            shotsCount += isShotProjected ? 1 : 0;
        }
        while (shotsCount != expectedShotCount);

        yield return Time.time - startTime;
    }

    public static IEnumerator GetTimeBetweenHittingConsecutiveShot(int expectedHits, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        int hits = 0;
        float lastTargetHealth = GetEnemyHealth();
        float newTargetHealth;
        float lastHitTime = 0;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;
        Vector2 newLastShotPosition;

        do
        {
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            newLastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            newTargetHealth = GetEnemyHealth();
            if (newLastShotPosition == lastShotPosition && newTargetHealth != lastTargetHealth)
            {
                lastTargetHealth = newTargetHealth;
                hits++;

                if (hits != expectedHits)
                {
                    lastHitTime = Time.time;
                }
            }
        }
        while (hits != expectedHits);

        yield return Time.time - lastHitTime;
    }
    #endregion

    #region Charge Lance
    // Charge Lance (#236)
    // Holds the attack stick at the given touch for the given time: one weapon tick per physics step, as in play.
    internal static IEnumerator HoldAttack(WeaponAttack weaponAttack, Vector2 attackJoystickTouch, float seconds)
    {
        var holdEndTime = Time.time + seconds;
        while (Time.time < holdEndTime)
        {
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return new WaitForFixedUpdate();
        }
    }

    // One push to the right from a standstill, facing right, so two pushes differ only by the thrust factor.
    internal static Vector2 PushHalalitOnce(HalalitMovement halalitMovement)
    {
        halalitMovement.transform.rotation = Quaternion.identity;
        halalitMovement.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        halalitMovement.TryMove(1, 0, Time.fixedDeltaTime);
        return halalitMovement.GetCurrentForce();
    }

    internal static GameObject GetEnemyShot()
    {
        return GameObject.FindGameObjectWithTag(Tag.ENEMY_SHOT.GetDescription());
    }
    #endregion
}