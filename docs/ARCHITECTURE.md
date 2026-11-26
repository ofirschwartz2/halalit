# Halalit architecture and coding rules

The rulebook for anyone, human or agent, who writes code in this repo: how the code is built today, the rules new code must follow, and the known debts.
Checked by reading files at commit `e8b1c95` (2025-07-26): C#, `.asmdef`, `.meta`, scene and prefab YAML, git history. Nothing was compiled or run. "By reading" marks a conclusion that still needs a run in Unity.
Updated 2026-11-26 after cards #153 to #219, read on `develop` at `00a0ea6`. Those cards are on `develop` only and none has run in Unity, so "fixed" in section 7 means fixed in code on `develop`.
Unity 2022.3.3f1, C#, 2D physics. Ofir approves every idea and every change.

## 1. System map

Game code is `Assets/GameLogic` (205 C# files). Tests are `Assets/Tests`. Third-party code is `Assets/External`. Prefabs are `Assets/Resources/Prefabs` (81; folders mirror the code). Scenes are `Assets/Scenes/{MainMenu,Playground,Leaderboard}.unity`.

| Assembly | Folder (files) | Owns | May reference (today's references) |
|---|---|---|---|
| `UtilsAndModels` | `Assets/GameLogic/UtilsAndModels` (77) | Shared kernel: enums, event bus, DTOs and shared components (`AttackBehaviour`, `ItemStats`, utility configs), interfaces, random generators, `Utils`, shared physics (`KinematicMovement`, `Knockbackee`, `SpeedLimiter`, `ForceField`) | TextMeshPro package |
| `Health` | `Assets/GameLogic/Health` (7) | `Health`, collision and trigger harmers | `UtilsAndModels` |
| `Items` | `Assets/GameLogic/Items` (43) | Item banks, drop logic, pickups (attack items, upgrades, utilities, valuables) | `UtilsAndModels` |
| `World` | `Assets/GameLogic/World` (4) | Camera follow, edge force-field setup, `RecycleBin` (dead code) | `UtilsAndModels` |
| `Attacks` | `Assets/GameLogic/Attacks` (16) | The 10 player attacks, `AttacksBank`, `Blast`, `ConsecutiveAttack` | `UtilsAndModels`, `Health` |
| `Asteroid` | `Assets/GameLogic/Asteroids` (7) | Asteroid spawn, movement, split, destruction | `UtilsAndModels`, `Items` |
| `Enemies` | `Assets/GameLogic/Enemies` (29) | Enemy behaviours, state machines, spawner, destructor | `UtilsAndModels`, `Items`, `Attacks` |
| `NewAssembly` (Meta) | `Assets/GameLogic/Meta` (8) | Menus, sign-in, cloud save, leaderboard, score, game over, utility button | `UtilsAndModels`, TextMeshPro, Unity Services (Core, Authentication, CloudSave, Leaderboards) |
| `Player` | `Assets/GameLogic/Player` (12) | Ship movement, weapon, attack toggle, pickup claw, `UtilityManager` | `UtilsAndModels`, `Items`, `Attacks`, `NewAssembly`, `External` |
| `Setup` | `Assets/GameLogic/Setup` (2) | Root random seed, static world references | `UtilsAndModels`, `Items`, `Asteroid`, `Enemies`, `NewAssembly` |
| `External` | `Assets/External` | Third-party: Joystick Pack, GabrielAguiar VFX, TMP resources. Do not edit | none |
| `TestsPlayMode` | `Assets/Tests/PlayMode/PlayModeTests` (28) | PlayMode tests and test infra | the 10 game assemblies, test runner |
| `TestsEditMode` | `Assets/Tests/EditMode` (1) | Scene wiring tests, Editor only | `TestsPlayMode`, 9 game assemblies (not `World`), test runner |

Allowed dependency directions are the ones that exist:
- `UtilsAndModels` references no game assembly. Every game assembly references it.
- `Health`, `Items`, `World` reference only `UtilsAndModels`.
- `Attacks → Health`. `Asteroid → Items`. `Enemies → Items, Attacks`.
- `Player → Items, Attacks, NewAssembly, External`. `Setup → Items, Asteroid, Enemies, NewAssembly`.
- Nothing references `Player`, `Setup` or `World` except tests. There are no cycles.
- Features without a link talk through the event bus (3.1) or through interfaces in `UtilsAndModels` (`IUtility`, `ISpeedForceController`, `IEngineFireController`).

Anomalies:
- The Meta assembly is still named `NewAssembly` (`Assets/GameLogic/Meta/Meta.asmdef`); tests reference it by that name. The `Asteroids` folder compiles to assembly `Asteroid`.
- `Attacks → Health` is declared, but no file in `Attacks` uses a type from `Health`.
- `Player → NewAssembly` exists only because `UtilityManager` holds a concrete `Meta.UI.UtilityButton`. `Setup → NewAssembly` exists only for `PlayerStats` in `OriginalRandomSeed`.
- All 11 runtime asmdefs target every platform. There is no Editor-only assembly for game code, so editor code compiles into player builds unless guarded (D1).
- `UtilsAndModels` also holds test-only `TestingRandomGenerator` and editor-only `JsonToInspector`.
- Package references are GUIDs that cannot be resolved in a plain checkout (no `Library/`). The package names above come from the `using` lines.

## 2. Runtime flow

Playground scene objects, as `object [tag]: components`:
- `Setup [Setup]`: `OriginalRandomSeed`, `SpeedLimiter`, `UtilsGameObjectsInitializer`.
- `EventSystem [EventSystem]`: `Event` plus the 9 `*Event` components.
- `Halalit [Halalit]`: `HalalitMovement`, `Health`, `Knockbackee`, `GameOver`. Children: `Weapon [Weapon]` (`WeaponMovement`, `WeaponAttack`, `AttackToggle`), `ClawShooter [PickupClawShooter]`, `Engine/EngineFire`, `UtilityManager`.
- `Items [ItemsFactory]`: `ItemsBank`, `DropsBank`, `ItemDropper`, `ItemRankPicker`, `ItemOptions`. `Attacks [Attacks]`: `AttacksBank`. `Projectiles`: parent object for instantiated attacks.
- `EnemiesContainer`: `EnemyExplosionInitiator`; `EnemySpawner [EnemiesSpawner]` (`SpawnHoleInstantiator`, `EnemyBank`); `EnemyDestructor` (`EnemiesDestructor`).
- `AsteroidsContainer`: `AsteroidInstantiator [AsteroidInstantiator]` (`AsteroidExternalInstantiator`, `AsteroidInternalInstantiator`, `AsteroidInitiator`); `AsteroidDestructor` (`AsteroidsGlobalDestructor`).
- `ValuablesContainer`: `ValuableDropper`. `HUD/Score [Score]`: `Score`. `HUD/UtilityButton`: `UtilityButton`. `World`: `CameraMovement`, `EdgesForceFieldsManager`, 8 `ForceField`.
- `ObjectLoader`: `ObjectLoader`, a script from the test assembly (D16).
- Other scenes: `MainMenu.unity` holds `MainMenu` and `Authentication`; `Leaderboard.unity` holds `LeaderBoard` and `Authentication`.

Script execution order: only `Health` (100) and `Knockbackee` (300) are set, in their `.cs.meta` files. The other 203 game scripts are 0. The reason for 100 and 300 is not recorded. Unity gives no order guarantee between objects at the same value (Unity manual, "Order of execution for event functions").

Scene start, `Awake`:
- `OriginalRandomSeed` picks the root seed: the test seed if `IsTestMode`, else `PlayerStats._dailySeed` on a daily run, else a random one.
- It gives one child seed each, in a fixed order, to `EnemyBank`, `SpawnHoleInstantiator`, `AsteroidExternalInstantiator`, `AsteroidInternalInstantiator`, `ItemsBank`, `ItemRankPicker`, `ItemDropper`, which it finds by tag.
- `UtilsGameObjectsInitializer` hands `Utils` the `InternalWorld` and `ExternalSafeIsland` objects. `SpeedLimiter` and `AttacksBank` fill static fields.
- `EnemyBank` builds its shuffled enemy list. Subscribers attach to events.

Scene start, `Start`:
- Each `*Event` snapshots its delegates (3.1).
- `ItemsBank` generates stock, `DropsBank` builds drop tables from it, `AttackToggle` loads the first attack.
- `SpawnHoleInstantiator` starts a coroutine that polls once a second. `AsteroidExternalInstantiator` spawns from `Update`.

Two of these steps rely on an order that nothing pins (D13): `EnemyBank.Awake` needs its seed already set, and `DropsBank.Start` needs `ItemsBank.Start` to have run.

Per physics tick (`FixedUpdate` in 33 classes, fixed step 0.02 s):
- Player: ship movement, weapon orbit, `WeaponAttack.HumbleFixedUpdate`, claw, camera follow.
- Enemies: movement and state machines, each ending with `SpeedLimiter.LimitSpeed`; `EnemySharedBehavior` destroys enemies outside the world; `SpawnHole` runs its states.
- World: `KinematicMovement` moves asteroids, attacks move and check lifetime, `ForceField` pushes.
- Physics callbacks apply harm (`Health`), knockback (`Knockbackee`) and pickups.
- `Update` exists in 3 classes only: `AsteroidExternalInstantiator`, `UtilityManager` and the unused `Utility`.

Death and game over. When `Health` reaches 0 it raises an event chosen by its own tag:
- `Enemy`: `ENEMY_DEATH` → `EnemiesDestructor` rolls a valuable drop, raises the item-drop and explosion events, destroys the enemy.
- `Asteroid`: `ASTEROID_DEATH` → `AsteroidsGlobalDestructor` raises the item-drop and split events, rolls a valuable drop, destroys the asteroid.
- `Halalit`: `GameOverEvent` → `GameOver.OnGameOver` loads `MainMenu` at once. Nothing copies the score to `PlayerStats` first (D2). `MainMenu.Awake` (async) then runs sign-in and `PlayerStats.InitAllAsync`, and fills the score texts.

## 3. Core patterns

### 3.1 Event bus
- Each `*Event` MonoBehaviour on the `EventSystem` object declares `public static event EventHandler<TArgs>` fields. There are 9 classes and 12 events.
- In `Start()` it copies the current delegates into a static dictionary keyed by `EventName`. `Invoke(eventName, initiator, args)` calls the copy (`DeathEvent.InvokeTargetDeath` and `GameOverEvent.InvokeGameOver` are the same thing under other names).
- Subscribers add handlers in `Awake` through `SetEventListeners()` and remove them in `OnDestroy` through `DestroyEventListeners()`. All 13 subscribers do exactly this.
- Limitation: delegates are immutable, so the dictionary is a snapshot. A handler added after the event object's `Start()` is never called, so objects created at runtime cannot subscribe. `-=` does not remove a handler from the snapshot, so a destroyed subscriber is still called until the scene reloads. `Invoke` before `Start()` throws on first load, or uses the previous scene's snapshot.
- Today every subscriber is a scene object that lives as long as the scene, except `Health` on spawned enemies and asteroids, whose subscription never fires (D5).
- Example: `Assets/GameLogic/UtilsAndModels/Models/Event/ItemEvent.cs`, subscriber `Assets/GameLogic/Items/Infra/ItemDropper.cs`.

Rules when extending:
- New event: append an `EventName` member, add the static event and its dictionary line to the matching `*Event`. For a new family add `XEvent` and `XEventArguments : EventArgs` in the same folder, put the component on `EventSystem`, and add it to `PlaygroundTests.EventSystemTests`.
- Subscribe only from objects that are in the scene at load. Use the `SetEventListeners` / `DestroyEventListeners` pair.
- Never invoke an event from `Awake` or `Start`.
- Pass `this` as initiator. `DeathEvent` handlers cast it to `MonoBehaviour` to find the dying object.
- Handlers of shared events filter by `arguments.Name`. Copy the shape of `WeaponAttack._upgradeActions` with `IsRelevantUpgradeEvent`.

### 3.2 Health and harm
- `Health` sits on everything damageable: Halalit, `Enemy.prefab`, `Asteroid.prefab`. Its serialized `_harmers` (`List<Tag>`) lists the tags that hurt it.
- Collision: `Health` asks the other object's `CollisionHarmer` for the amount. `FixedCollisionHarmer` returns a constant, `SpeedCollisionHarmer` derives it from speed.
- Trigger: `Health` reads `AttackBehaviour.ShotType` on the other object. `DESCRETE` is handled in `OnTriggerEnter2D`, `CONSECUTIVE` in `OnTriggerStay2D`. The amount comes from a `TriggerHarmer` (`DescreteTriggerHarmer`, or `ConsecutiveTriggerHarmer` which hits once per `AttackStats.Rate` seconds).
- Trigger harmers delegate to `IHarmer.GetHarm()`, implemented by `AttackBehaviour`: `Power`, or `Power × CriticalHit` on a `Luck` roll. Harmer components are looked up on the object, then on its parents.
- Knockback is separate: `Knockbackee` with its own `_knockbackers` tag list.
- `Tag` (`Assets/GameLogic/UtilsAndModels/Models/Enum/Tag.cs`): each member's `[Description]` is the Unity tag string. Code compares with `Tag.X.GetDescription()`, which uses reflection on every call. `Health`, `Knockbackee` and `ForceField` convert their tag lists once and keep the strings.
- Example: `Assets/GameLogic/Health/Health.cs`.

Rules when extending:
- To make X hurt Y, add X's tag to Y's `_harmers` in the prefab and give X a harmer component. `Health` itself does not change.
- A collision harmer needs a `CollisionHarmer` on the object or a parent. A trigger harmer needs `AttackBehaviour` on the collider's own GameObject and a `TriggerHarmer` on that object or a parent. `Health` dereferences all of them without a null check.
- A new damageable kind needs a branch in `Health.InvokeDeathEvent` and a destructor that listens. Only Halalit, Enemy and Asteroid raise an event today; any other tag dies silently.
- New harm logic is a new `CollisionHarmer` or `TriggerHarmer` subclass, not a branch inside `Health`.
- A new tag is appended to `Tag` and created in Unity's Tag Manager with the same string as its `[Description]`.

Per-object health events (#201):
- The bus (3.1) cannot serve spawned objects. A component that must react to its own object's health uses the `IHealth` interface (`Assets/GameLogic/UtilsAndModels/Models/Interface/IHealth.cs`): `event Action<int> Wounded`, `GetCurrentHealth()`, `TryChangeHealth(int)`. Assemblies that do not reference `Health` use it this way.
- `Health.TryChangeHealth` raises `Wounded` with the health left, only for a hit that hurts and does not kill. A zero hit, a heal or a killing hit raises nothing.
- Get it with `GetComponent<IHealth>()` in `Awake`, subscribe there, unsubscribe in `OnDestroy`. Example: `Assets/GameLogic/Enemies/Base/WoundedRetreat.cs`.
- Add a new per-object health event to `IHealth` the same way. Do not add a static event for it.

### 3.3 Attacks
- `AttackName` lists the 10 attacks. `AttacksBank` (scene) maps each to a prefab in a static dictionary. Every attack prefab carries `AttackBehaviour` (`ShotType` plus `AttackStats`: Power, CriticalHit, Luck, Rate, Weight) and its own script under `Assets/GameLogic/Attacks/{ShortRange,MidRange,LongRange}`.
- `AttackToggle` holds the current attack and swaps it on `PLAYER_ATTACK_ITEM_PICKUP`. Each tick `WeaponAttack.HumbleFixedUpdate(touch)` reads it.
- `DESCRETE`: when the joystick passes `_attackJoystickEdge` and the cooldown (`Rate × _cooldownMultiplier`) is over, instantiate the prefab under `Projectiles`.
- `CONSECUTIVE`: instantiate once, then call `StartConsecitiveAttack`, `UpdateConsecitiveAttack` each tick, and `StopConsecitiveAttack` on release. Only `LaserBeamBase` derives from `ConsecutiveAttack`.
- Attacks that spawn parts pass stats on with `AttackBehaviour.Copy` (`Shotgun`, `Spikes`, `BlastShot`, `Grenade`, `Blast`). Attacks destroy themselves on lifetime end, on hit, or when they leave the `InternalWorld` trigger.
- Stats travel through the prefab asset: `AttackToggle` writes `AttackStats` onto the prefab and `WeaponAttack` reads them back every tick (D9).
- Example: `Assets/GameLogic/Attacks/MidRange/BlastShot.cs`; flow in `Assets/GameLogic/Player/Weapon/WeaponAttack.cs`.

Rules when extending:
- A new attack is an `AttackName` member, a behaviour script, a prefab variant of `HalalitAttack.prefab`, an `AttacksBank` entry and an attack item (3.4).
- `AttackToggle` maps `ItemName` to `AttackName` by member name, so the two names must match. `ItemName` members have explicit numbers (#118): append a new one with the next free number.
- Movement and lifetime checks go in `FixedUpdate`. Tunables are serialized fields on the attack script.
- Do not add new writes to a prefab asset.

### 3.4 Items, drops and ranks
- `ItemName` covers attack items, upgrades and utilities. `ItemsBank` holds a prefab and stock bounds per item and generates the stock in `Start` with its seeded generator.
- Attack items get `AttackStats`: `ItemRankPicker` picks an `ItemRank` (COMMON to LEGENDARY) from `_rankChances`, `ItemOptions` returns the `AttackStatsRange` for that rank, and `GetRandom` rolls each stat. Upgrades and utilities get empty `UpgradeStats` / `UtilityStats`.
- `DropsBank` holds, per `Dropper`, a table of `ItemName` to chance (`1 / OneOfChance`). On a death the destructor raises `NEW_ITEM_DROP`. `ItemDropper` rolls once, picks uniformly among the items whose chance covers the roll, and takes one from `ItemsBank.GetItem`. At zero stock `NO_STOCK` removes the item from every table.
- Valuables: the destructor rolls `PotentialValuableDrops` with the dying object's generator and raises `NEW_VALUABLE_DROP`; `ValuableDropper` spawns it. `Score` adds points on `PLAYER_VALUABLE_PICKUP`.
- Pickups derive from `AttackItem`, `Upgrade` or `Valuable`. The subclass sets `_itemName` in `Start`; the base raises the pickup event on contact with Halalit and destroys the item. Upgrades carry a `Dictionary<EventProperty, float>`.
- Utilities: on `PLAYER_UTILITY_PICKUP`, `UtilityManager` builds an `IUtility` through `UtilityFactory`. The HUD button activates it. `UtilityManager.Update` polls for the end.
- Example: `Assets/GameLogic/Items/Infra/ItemsBank.cs`, `Assets/GameLogic/Items/Upgrade/Upgrade.cs`.

Rules when extending:
- New pickup: subclass the matching base, append to `ItemName`, give the prefab an `ItemStats`, register it in `ItemsBank` and `DropsBank` (and `ItemOptions` for attacks, the test `ObjectLoader._itemPrefabs` if tests load it). Do not write another standalone pickup `OnTriggerEnter2D`; the three utility pickups did (D6).
- New upgrade: add the consumer too. Only `HALALIT_VITALITY` and `WEAPON_STAMINA` have one (D14).
- New utility: implement `IUtility` and put its end condition behind the interface, not in a new type check in `UtilityManager.Update`.

### 3.5 Enemies
- Shared behaviour is the base prefab `Assets/Resources/Prefabs/Enemies/Enemy.prefab`: `Health`, `Knockbackee`, `FixedCollisionHarmer`, `EnemySharedBehavior` (seeded generator, out-of-world destroy), `EnemyDropper` (its `Dropper` id), `PotentialValuableDrops`. Every enemy is a variant of it.
- Special movement enemies are one script each: `LameEnemy`, `ZigZagEnemy`, `SinusEnemy`, `GreekEnemy`.
- `MoveAimAttackStateMachine` cycles MOVING → AIMING → ATTACKING on timers. It holds serialized references to `MoveAimAttackMove` and to the abstract `MoveAimAttackAim` and `MoveAimAttackAttack` (3 concrete pairs).
- `WaitAttackStateMachine` switches WAITING ↔ ATTACKING using the abstract `WaitAttackWait` and `WaitAttackAttack`.
- A state machine here is an enum in `Models/Enum` plus a `switch` in `FixedUpdate`; the steps are separate MonoBehaviours wired in the prefab. `SpawnHole`, `Spikes` and `PickupClawStateMachine` use the same shape.
- Spawner: `EnemyBank` pre-builds a shuffled list of (prefab, seed) and the group sizes. `SpawnHoleInstantiator` places a `SpawnHole` on a schedule at a free seeded position. The hole opens, instantiates its group with scripts disabled (except the names in `_alwaysEnabledScripts`), grows the enemies, enables them and closes.
- Example: `Assets/GameLogic/Enemies/MoveAimAttackEnemies/Base/MoveAimAttackStateMachine.cs`.

Rules when extending:
- New enemy: a prefab variant of `Enemy.prefab` (or of the `MoveAimAttackEnemy` / `WaitAtackEnemy` prefabs), a new `Dropper` member appended, entries in `EnemyBank._enemyPrefabsBank` and `DropsBank`.
- New behaviour for an existing machine: subclass the abstract step. Do not copy a state machine to add a state.
- End every enemy `FixedUpdate` with `SpeedLimiter.LimitSpeed(_rigidBody)`. Turn at edges with `Utils.DidHitEdge(tag)`.
- Take random values from `EnemySharedBehavior._seedfulRandomGenerator`, in `Start` or later.
- Pause marker (#201): every script that moves, aims or attacks an enemy implements the empty `IPausableEnemyBehaviour` (`Models/Interface`) and sits on the enemy's root. `WoundedRetreat` pauses an enemy by switching these off with `enabled = false`. No `Pause()` method, no type checks, no script names. Today: the 4 special-movement scripts, both state machines, `ShotDodger`. `TargetFacer` (#200) predates the rule and is not marked yet.
- A script that leaves live shots or rays behind clears them in `OnDisable`. `MoveAimAttackStateMachine.OnDisable` calls `StopAiming()` / `StopAttacking()`; a new aim or attack step with live objects overrides those.

### 3.6 Randomness
- `SeedfulRandomGenerator` wraps `System.Random(seed)`. A component that needs one derives from `SeedfulRandomGeneratorUser` (9 classes); `SetInitialSeedfulRandomGenerator(seed)` throws if called twice.
- Seeds flow down a tree: `OriginalRandomSeed` → 7 scene systems; `EnemyBank` → each enemy (`EnemyEntity.Seed`, applied by `SpawnHole`); the asteroid instantiators → each asteroid (`AsteroidInitiator.InitAsteroid`).
- The seeded tree decides: enemy order, group sizes and spawn positions; the asteroid line, positions, scales and split count; item stock, ranks and stats; item and valuable drop rolls; the start direction of Greek and Sinus enemies.
- Since #183 and #184, every gameplay roll is seeded: each enemy and asteroid has its own generator, and combat rolls (critical hits, shotgun, enemy aim) use one too. `SeedlessRandomGenerator` is left for visuals (engine particles, item spin), the root seed of a non-daily run, and two `Utils` helpers with no caller (D8). `UnityEngine.Random` is not used.
- Example: `Assets/GameLogic/Setup/OriginalRandomSeed.cs`.

Rules when extending:
- New gameplay randomness uses a seeded generator: the owner's (`EnemySharedBehavior`, `AsteroidSharedBehavior`) or a new `SeedfulRandomGeneratorUser` seeded by whatever creates it.
- A new scene-level user is added at the end of `OriginalRandomSeed.SetAllInitialSeedfulRandomGenerators`. The call order defines every seed.
- Seedless is for visuals only. A seedless result must never decide whether a seeded draw happens (`AsteroidInternalInstantiator.cs:127` did, until #183).
- Do not use the generator in `Awake`; it may not be assigned yet.

### 3.7 Test hooks
- Tests read private state through members that exist only in the editor:
```csharp
#if UNITY_EDITOR
    internal float GetLifeTime()
    {
        return _lifetime;
    }
#endif
```
- "Humble" methods: the Unity callback only reads input and passes it to a method that takes the input as parameters. That method is `internal` in the editor and `private` in builds (`#if UNITY_EDITOR internal #else private #endif`). Six exist: `WeaponAttack.HumbleFixedUpdate`, `WeaponMovement.TryChangeWeaponPosition`, `HalalitMovement.TryMove`, `PickupClawShooter.TryShootClaw`, `AttackToggle.SetNewAttack`, `ItemDropper.DropNewItem`.
- `[assembly: InternalsVisibleTo("TestsPlayMode")]`, inside `#if UNITY_EDITOR`, exists in `Attacks`, `Enemies` (in `ShotDodger.cs`), `Health`, `Items`, `Player` and Meta. It does not exist in `Asteroid`, `Setup`, `UtilsAndModels` or `World`. One per assembly is enough; do not add a second.
- Example: `Assets/GameLogic/Player/Weapon/WeaponAttack.cs`.

Rules when extending:
- Game code never calls an editor-only member.
- Hooks read or inject. No test-only branch in game logic beyond the existing `WeaponAttack._isTesting`.
- The first hook in an assembly without the attribute adds `InternalsVisibleTo("TestsPlayMode")` under `#if UNITY_EDITOR`.

## 4. Code conventions actually used

Counts are regex counts over the 205 game files.
- Files: one type per file, file name equals type name. A top folder is an assembly; subfolders follow features (`Attacks/MidRange/Shotgun`). Shared bases sit in `Base`, `Infra` or `Utils`. Enums, event classes, DTOs and interfaces go to `Assets/GameLogic/UtilsAndModels/Models/{Enum,Event,DataObject,Interface}`.
- Namespaces: 163 of 205 files use the global namespace. That includes every gameplay MonoBehaviour written before 2025 except `PickupClawAnimator`. Enums are in `Assets.Enums` (22 files); `Utils` and the generators are in `Assets.Utils`. The usual header is `using Assets.Enums; using Assets.Utils; using UnityEngine;`.
- Serialized fields: `[SerializeField]` on its own line, then `private Type _camelCase;` (278 of 296 attributes; 270 of 295 names start with `_`). Tunables are serialized fields on the component, set in the prefab or the scene.
- Constants shared across files are `public const` UPPER_SNAKE_CASE in `Assets/GameLogic/UtilsAndModels/Utils/InGameUtils/Constants.cs`. Local ones are `private const` in the class.
- Methods: PascalCase, short, one job. Predicates have no side effects and are named `Is…`, `Should…` or `Did…` (61 of 78 bool methods), often inside `#region Predicates`. `Try…` (43 methods) means "do it if the condition holds".
- `#region` blocks group Init, Destroy, Events, Accessors and Predicates in larger classes (33 files, 110 regions).
- Enum members are UPPER_SNAKE_CASE. `[Description]` holds the Unity-facing string: tag, layer, scene name, animator parameter.
- Unity messages: gameplay runs in `FixedUpdate`. `Awake` is for own setup and subscriptions, `Start` for anything that needs other objects. Access modifiers on messages are mixed (`void Start()` and `private void Awake()` are both common).
- Types: `public class` in 156 of 177 classes; the rest are internal. Braces on their own line, 4 spaces.
- Git stores LF line endings; 133 of 205 files start with a UTF-8 BOM. There is no `.editorconfig`. Keep what the file has.
- Misspelled names are part of the API: `DESCRETE`, `StartConsecitiveAttack`, `BAR_BORDEDR`, `WEAPON_STRENGH`, `GetRorationOutwards`. Do not fix them in passing; a rename touches many files and the tests.

The 2025 utility code diverges (`Assets/GameLogic/Items/Utility`, `Assets/GameLogic/Player/Utility`, `Assets/GameLogic/Meta/UI`, `Models/Interface`, the `*Configuration` classes):
- It adds the namespaces `Items.Utility`, `Items.Factory`, `Meta.UI`, `Assets.Player` and `Assets.Models`.
- It writes `[SerializeField] private float duration = 5f;` inline and without underscore. All 18 inline attributes are in this code.
- It uses plain C# classes, a factory and ScriptableObject configs loaded by `Resources.Load` with string paths, where older code uses components wired in prefabs.
- It uses string literals (`CompareTag("Enemy")`), PascalCase `static readonly` fields, expression-bodied members, `Debug.Log` on normal paths, `Update` for gameplay, and a `Should…` method that changes state (`NitroUtility.ShouldDeactivate`).

New code follows the majority style, not this one.

## 5. Testing architecture

- 101 tests: 91 `[UnityTest]` in 24 PlayMode classes under `Assets/Tests/PlayMode/PlayModeTests/<Feature>/`, and 10 `[Test]` in `Assets/Tests/EditMode/PlaygroundTests.cs`. There is no CI. Tests run from the Unity Test Runner.
- PlayMode tests run the real `Playground` scene. Template: `Assets/Tests/PlayMode/PlayModeTests/Attacks/LongRange/BallShotTests.cs`.
  1. `[SetUp]`: `_currentSeed = TestUtils.SetRandomSeed();` then `SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);`. This puts `OriginalRandomSeed` in test mode with that seed.
  2. The body is marked `// GIVEN`, `// WHEN`, `// THEN`. `TestUtils.SetTestMode()` switches off `AsteroidsContainer` and `EnemiesContainer`, stops `WeaponAttack` from reading the joystick, and sets time scale to 5 through `TestTimeController`.
  3. Actors come from the scene's `ObjectLoader`: `LoadEnemyInExternalSafeIsland(seed)`, `LoadAsteroidInExternalSafeIsland(seed)`, `LoadItemInExternalSafeIsland(ItemName)`, `LoadValuableInExternalSafeIsland()`. They appear parked in `ExternalSafeIsland`; move them with `TestUtils.Set…Position`.
  4. Drive the game through the humble methods (3.7) with `yield return null` between steps. Random test input comes from `TestUtils.GetRandomTouch…`, seeded by the same seed.
  5. Assert with `AssertWrapper.X(..., message, _currentSeed)`. A failure prints the seed; rerun with `TestUtils.SetRandomSeed(<seed>)`.
  6. `[TearDown]`: `TestUtils.DestroyAllGameObjects()` and `TestTimeController.ResetTimeScale()`.
- `Assets/Tests/PlayMode/PlayModeTests/TestInfra/TestUtils.cs` holds scene getters by `Tag`, setup helpers, default `AttackStats` and shot-tracking coroutines. Add helpers there, not in test classes.
- EditMode `PlaygroundTests` checks scene wiring: tagged objects exist and carry the expected components. It opens no scene itself, so it checks whichever scene is open in the editor.
- Not referenced by any test (by grep): enemy behaviours and state machines, the spawner, `ItemsBank`, `DropsBank`, `ItemRankPicker`, upgrades, the score handoff, menus, the event-bus snapshot, one attack hitting two victims (except `MirrorBallShot`).
- Static state survives between tests: `OriginalRandomSeed.IsTestMode`, time scale, static events (D22).

A test for a new feature: one class per feature in the matching folder, following the six steps. One behaviour per `[UnityTest]`. Pass `_currentSeed` to every assert. Use `acceptedDelta` for physics results. If the feature adds a scene object, add it to `PlaygroundTests`. If the test must load a new prefab, add it to `ObjectLoader._itemPrefabs` in the scene.

## 6. Rules for new code

1. Do not add an assembly, an asmdef reference, a package or a namespace without Ofir's approval. A type shared by two feature assemblies goes in `UtilsAndModels`.
2. No `UnityEditor` API or `using UnityEditor` in `Assets/GameLogic` outside `#if UNITY_EDITOR`. Game code never calls a member that exists only under `#if UNITY_EDITOR`.
3. Enums are stored as ints in scenes and prefabs. Append new members at the end; never insert, reorder or delete (D11).
4. Tags, layers and scene names come from `Tag`, `Layer`, `SceneName` with `GetDescription()`. No string literals for them.
5. Events: subscribe in `Awake` via `SetEventListeners()`, unsubscribe in `OnDestroy` via `DestroyEventListeners()`. Bus events (3.1) only from objects present at scene load; a spawned object listens to its own components through an interface event, as `IHealth.Wounded` (3.2). Never invoke from `Awake` or `Start`.
6. No runtime state on assets: never write to a prefab asset or a ScriptableObject at runtime.
7. Gameplay randomness comes from a `SeedfulRandomGenerator` in the seed tree. `SeedlessRandomGenerator` is for visuals only. No `UnityEngine.Random`.
8. No `Debug.Log` in `Update`, `FixedUpdate`, coroutine loops or physics callbacks. Never log tokens or credentials.
9. Per-tick gameplay goes in `FixedUpdate`. One-frame input such as `GetMouseButtonDown` is read in `Update`.
10. No `Find…`, `GetComponent` or `Resources.Load` per tick. Serialize the reference or cache it in `Awake` / `Start`.
11. Reuse, do not copy. If a new class would differ from an existing one by a few lines, add a parameter or subclass a base (`AttackItem`, `Upgrade`, `Valuable`, `ConsecutiveAttack`, the enemy step classes, `KinematicMovement`). The pushing and pulling magnets are the example not to repeat.
12. No type checks on concrete classes (`if (x is NitroUtility)`). Put the behaviour behind the interface or base class.
13. `Awake` sets up the object's own state; work that needs another object goes in `Start`. If an order dependency remains, pin it with Script Execution Order and state it in the pull request.
14. Follow section 4: own-line `[SerializeField]`, `_camelCase`, pure `Is/Should/Did` predicates, tunables as serialized fields. Test hooks only as in 3.7; no test types in runtime assemblies or scenes.
15. Every behaviour change ships with a PlayMode test in the section 5 shape. Nothing counts as tested until it has run in Unity; say so when you could not run it.

## 7. Known debts

Each row was re-checked in the files; none was observed at runtime. In "Where", paths that start with `Assets/` or `ProjectSettings/` are relative to the repo root; the shorter ones are relative to `Assets/GameLogic/`. Fix size: S is under a day, M is days, L is more.

| # | Problem | Where | Why it matters | Fix |
|---|---|---|---|---|
| D1 | **Fixed by #153.** Editor-only code in player assemblies | `MinMaxRange.cs`, `JsonToInspector.cs`, `OriginalRandomSeed.cs` now guarded by `#if UNITY_EDITOR`; Joystick Pack editor scripts moved into an Editor-only asmdef (an exception to hard rule 8, recorded on #171) | A player build is still unproven until one is made in Unity | done |
| D2 | **Fixed by #154.** Score never reached `PlayerStats` | `Meta/Score/Score.cs:82` `SetGameStats` now runs on `GameOverEvent`; a first score counts (`_highScore ?? 0`) | High score, daily score and leaderboard get the run's score | done |
| D3 | **Fixed by #156.** Stale build scene list | `ProjectSettings/EditorBuildSettings.asset` now lists exactly MainMenu, Playground, Leaderboard | The leaderboard scene can load | done |
| D4 | **Fixed by #159 and #204.** Daily seeds were hardcoded | `Meta/PlayerStats.cs:162` takes the seed from `DailySeed.GetSeed(date)`; the day follows Israel time | Every day has its own daily | done |
| D5 | Event snapshot (3.1); `Health` subscribes per entity and never checks it is the player | `Assets/GameLogic/UtilsAndModels/Models/Event/*Event.cs`; `Health/Health.cs:48,233-241` | Late subscribers are ignored and destroyed ones still called; a vitality upgrade raises max health on every `Health` in the snapshot | M |
| D6 | 2025 utility code: duplication and drift | `Assets/GameLogic/Items/Utility/*` (Pulling vs Pushing: `*Utility` identical after renaming, `*Field` one line and a debug block apart, plus pickups, configs and tests); `Player/Utility/UtilityManager.cs:78-83,168-183` (type checks); `Items/Utility/Utility.cs` (abstract, no subclass); `UtilsAndModels/Models/DataObject/BaseUtilityConfiguration.cs:7` (`IsActive` state on a ScriptableObject, read by `Items/Utility/NitroUtility.cs:15`) | Every fix must be made twice; each new utility needs edits in `UtilityManager`; state on an asset is shared by all users | M |
| D7 | Logging on hot paths; token in logs | `Assets/GameLogic/UtilsAndModels/Utils/InGameUtils/Utils.cs:355-357,364-365` via `Enemies/Base/EnemySharedBehavior.cs:7` (per enemy per tick); `Player/Halalit/HalalitMovement.cs:94`; `Items/Utility/NitroUtility.cs:91`; `Items/Utility/PullingMagnetField.cs:54-57`; `Meta/Authentication.cs:38` (access token) | Frame time and log noise; a credential in device logs | S |
| D8 | **Mostly fixed by #183 and #184.** Mixed determinism | Left: `SeedfulRandomGenerator.GetInsideUnitCircle` (`UtilsAndModels/Random/SeedfulRandomGenerator.cs:38`) still returns a point in a square; `Utils.GetRandomVector` and `Utils.Get180RandomNormalizedVector` (`Utils.cs:89,115`) are seedless and have no caller | Drop forces lean to the diagonals; a new caller of the two helpers would break the daily | S |
| D9 | Prefab asset mutated at runtime | `Assets/GameLogic/Player/Weapon/AttackToggle.cs:33,90` write; `Player/Weapon/WeaponAttack.cs:95` reads back each tick | The asset is shared state: stats outlive the scene and, in the editor, can outlive play mode (by reading) | M |
| D10 | Sign-in initialised repeatedly | `Assets/GameLogic/Meta/Authentication.cs:10` (`InitializationTask` never assigned); `Meta/MainMenu.cs:31`; `Meta/PlayerStats.cs:43`. Also `async void Awake` in `Meta/MainMenu.cs:18` and `Meta/Score/LeaderBoard.cs:22` | `Initialize()` and `SetupEvents()` run on every menu load, so handlers stack up; an exception leaves the menu half set up | S |
| D11 | Stale `List<Tag>` values. `PICKUP_CLAW_SHOOTER` and `ITEMS_FACTORY` were inserted mid-enum (commits `c84e769`, `0f25bec`, March 2024) | `Assets/Resources/Prefabs/Asteroids/Asteroid.prefab:241` (`_harmers` 17, 18 now mean `BAR_FILL`, `BAR_ICON`; were `ENEMY_SHOT`, `EXPLOSION`); `Assets/Resources/Prefabs/Enemies/Enemy.prefab:166` (`_knockbackers` 7, 23); `Assets/Resources/Prefabs/Enemies/MoveAimAttackEnemies/MoveAimAttackEnemy.prefab:28,32`; `Assets/Scenes/Playground.unity:10517` | By reading: asteroids no longer list enemy shots or explosions as harmers; shooting enemies no longer list enemy shots or asteroids; shock waves no longer knock enemies back | S |
| D12 | **Fixed by #209.** `_singleHitter` hit one victim in total | `Health/Health.cs:176` now passes the victim to `HandleTriggerHarmer`. Still open: `DescreteTriggerHarmer._singleHitter` is a `[SerializeReference]` bool that Unity may not save, so hit-once may never be on in play (idea #220) | Check #220 in Unity | S |
| D13 | Init order not pinned | `Assets/GameLogic/Enemies/Spawner/EnemyBank.cs:20-44` uses its generator in `Awake`; `Items/Infra/DropsBank.cs:42` needs `ItemsBank._allItems` from `ItemsBank.Start`; the `catch` at `DropsBank.cs:47` cannot fire (`TryGetValue` does not throw) | A scene edit can silently stop spawns or drops | S |
| D14 | Upgrades without effect | `Assets/GameLogic/Items/Upgrade/**`: 7 of 9 upgrades have no consumer. `Health/Health.cs:235` ignores `HALALIT_HP_ADDITION` and adds 1 | Pickups do nothing (listed as To Do in `Halalit tasks.txt`) | M |
| D15 | **Fixed by #118.** `ItemName` attack members aliased `AttackName` ints | `UtilsAndModels/Models/Enum/ItemName.cs` members now have explicit numbers | An 11th attack takes the next free number | done |
| D16 | Test code in runtime scene and assemblies | `ObjectLoader` component in `Assets/Scenes/Playground.unity`; `UtilsAndModels/Utils/InGameUtils/TestingRandomGenerator.cs`; `ExternalSafeIsland` check in `Enemies/Base/EnemySharedBehavior.cs:7` | By reading: a player build carries a missing script and test-only branches | M |
| D17 | Consecutive harm cooldown is shared and accumulates | `Assets/GameLogic/Health/Harm/Trigger/ConsecutiveTriggerHarmer.cs:27-31` (`_nextHarmTime += Rate`, one timer per harmer) | By reading: a beam that idles then touches a target hits once per tick until it catches up; two targets share one hit rate | S |
| D18 | Input and lookups in the physics tick | `Assets/GameLogic/Player/PickupClaw/PickupClawShooter.cs:30-38` (`GetMouseButtonDown` in `FixedUpdate`); `UtilsAndModels/Utils/InGameUtils/Utils.cs:244` (`FindGameObjectWithTag("Halalit")` per call); `Attacks/ShortRange/Sword.cs:53` | A click is missed when its frame has no physics step; avoidable per-tick cost | S |
| D19 | Dead code and clutter | 239 `Assets/InitTestScene*.unity` plus metas; `Assets/GameLogic/World/RecycleBin/*`; `UtilsAndModels/Utils/InGameUtils/Utils.cs:89,115,123` (unused methods); `EventName.NEW_ATTACK_SWITCH`; `Health._contributors`; wrong names in `InternalsVisibleTo` (`Attacks/MidRange/Boomerang.cs:7`, `Meta/PlayerStats.cs:13`, `Meta/Score/Score.cs:11`); 3 `.DS_Store`; `app.config`, `packages.config` | Noise that readers and agents copy | S |
| D20 | Unity 2022.3.3f1 predates the CVE-2025-59489 fix | `ProjectSettings/ProjectVersion.txt`; Unity advisory `unity.com/security/sept-2025-01` lists 2022.3.62f2 as patched | Shipped builds would carry a known high-severity flaw | M |
| D21 | Asset Store packs committed | `Assets/External/GabrielAguiarProductions/*` (`Assets/External` is 247 MB) | Licence exposure if the repo is public (from the assessment notes; not checkable in the files) | M; L if history must be purged |
| D22 | Test infra leaks | 4 of 24 `[TearDown]`s call `TestTimeController.ResetTimeScale()`; `Assets/Tests/PlayMode/PlayModeTests/Items/NitroFuelTests.cs:59-65` invokes its own lambda, not the button | By reading: time scale leaks into later tests; the nitro test does not exercise activation | S |

Verdicts on the two unverified candidates from the assessment:
- **`_singleHitter` (D12): confirmed, by reading; fixed by #209.** `Health.HandleHealth` passes `other`, the harmer's own GameObject, as `target`. So `DescreteTriggerHarmer._harmedTargets` only ever holds the harmer. With `_singleHitter` on, every call after the first hit returns 0, whoever the victim is. `KnockbackWaveTests.DoubleHitCheck` uses one enemy, so it passes either way. Fix: pass the victim's `gameObject` and add a two-enemy test.
- **`Utils.ShiftVectorByOffsetDegree`: confirmed, dormant.** `Utils.cs:125-131` adds an offset in radians to an angle in degrees and passes the sum to `Mathf.Cos` / `Mathf.Sin`. It has no caller in code, scenes or prefabs. Delete it; `Utils.AddAngleToVector` already rotates a direction by degrees.

Not verifiable in this workspace: anything that needs a compile, a test run or the editor; the real `Awake` / `Start` order behind D13; the runtime effect of D11, D12 and D17; which package each Meta GUID maps to; the licence and visibility facts behind D21.
