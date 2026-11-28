# Halalit architecture and coding rules

The rulebook for anyone, human or agent, who writes code in this repo: how the code is built today, the rules new code must follow, and the known debts.
Checked by reading files at commit `e8b1c95` (2025-07-26): C#, `.asmdef`, `.meta`, scene and prefab YAML, git history. Nothing was compiled or run. "By reading" marks a conclusion that still needs a run in Unity.
Updated 2026-11-26 after cards #153 to #219, read on `develop` at `00a0ea6`. Those cards are on `develop` only and none has run in Unity, so "fixed" in section 7 means fixed in code on `develop`.
Sections 3.7, 3.8 and 5 updated 2026-11-27 after #239 (Daily Twist), read on `develop` at `c3913e9`.
Section 5 null checks added 2026-11-27 after #254, read on `develop` at `6d8f5ae`.
Updated 2026-11-28 after #251, #268, #275, #278, #280, #284, #286, #291, #292, #293, #294 and #297 (new section 3.9 for Meta), read on `develop` at `11842c6`. None of these has run in Unity.
Unity 2022.3.3f1, C#, 2D physics. Ofir approves every idea and every change.

## 1. System map

Game code is `Assets/GameLogic` (272 C# files on `develop`). Tests are `Assets/Tests`. Third-party code is `Assets/External`. Prefabs are `Assets/Resources/Prefabs` (81; folders mirror the code). Scenes are `Assets/Scenes/{MainMenu,Playground,Leaderboard}.unity`.

| Assembly | Folder (files) | Owns | May reference (today's references) |
|---|---|---|---|
| `UtilsAndModels` | `Assets/GameLogic/UtilsAndModels` (97) | Shared kernel: enums, event bus, DTOs and shared components (`AttackBehaviour`, `ItemStats`, utility configs), interfaces, random generators, `Utils`, shared physics (`KinematicMovement`, `Knockbackee`, `SpeedLimiter`, `ForceField`) | TextMeshPro package |
| `Health` | `Assets/GameLogic/Health` (9) | `Health`, collision and trigger harmers | `UtilsAndModels` |
| `Items` | `Assets/GameLogic/Items` (52) | Item banks, drop logic, pickups (attack items, upgrades, utilities, valuables) | `UtilsAndModels` |
| `World` | `Assets/GameLogic/World` (4) | Camera follow, edge force-field setup, `RecycleBin` (dead code) | `UtilsAndModels` |
| `Attacks` | `Assets/GameLogic/Attacks` (27) | The player attacks, `AttacksBank`, `Blast`, `ConsecutiveAttack`, `ChargedAttack` | `UtilsAndModels`, `Health` |
| `Asteroid` | `Assets/GameLogic/Asteroids` (7) | Asteroid spawn, movement, split, destruction | `UtilsAndModels`, `Items` |
| `Enemies` | `Assets/GameLogic/Enemies` (42) | Enemy behaviours, state machines, spawner, destructor | `UtilsAndModels`, `Items`, `Attacks` |
| `NewAssembly` (Meta) | `Assets/GameLogic/Meta` (20) | Menus, sign-in, player names, cloud save, leaderboard and your place, score, results card, daily replay, utility button (3.9) | `UtilsAndModels`, TextMeshPro, Unity Services (Core, Authentication, CloudSave, Leaderboards) |
| `Player` | `Assets/GameLogic/Player` (12) | Ship movement, weapon, attack toggle, pickup claw, `UtilityManager` | `UtilsAndModels`, `Items`, `Attacks`, `NewAssembly`, `External` |
| `Setup` | `Assets/GameLogic/Setup` (2) | Root random seed, static world references | `UtilsAndModels`, `Items`, `Asteroid`, `Enemies`, `NewAssembly` |
| `External` | `Assets/External` | Third-party: Joystick Pack, GabrielAguiar VFX, TMP resources. Do not edit | none |
| `TestsPlayMode` | `Assets/Tests/PlayMode/PlayModeTests` (60) | PlayMode tests and test infra | the 10 game assemblies, test runner |
| `TestsEditMode` | `Assets/Tests/EditMode` (13) | Scene wiring tests and tests of pure rules, Editor only | `TestsPlayMode`, 9 game assemblies (not `World`), test runner |

Allowed dependency directions are the ones that exist:
- `UtilsAndModels` references no game assembly. Every game assembly references it.
- `Health`, `Items`, `World` reference only `UtilsAndModels`.
- `Attacks → Health`. `Asteroid → Items`. `Enemies → Items, Attacks`.
- `Player → Items, Attacks, NewAssembly, External`. `Setup → Items, Asteroid, Enemies, NewAssembly`.
- Nothing references `Player`, `Setup` or `World` except tests. There are no cycles.
- Features without a link talk through the event bus (3.1) or through interfaces in `UtilsAndModels` (`IUtility`, `ISpeedForceController`, `IEngineFireController`).

Anomalies:
- The Meta assembly is still named `NewAssembly` (`Assets/GameLogic/Meta/Meta.asmdef`); tests reference it by that name. The `Asteroids` folder compiles to assembly `Asteroid`.
- `Attacks → Health` is now used: `ChainSpark` and `ChargeLance` read `FrontArmor` (#235, #236).
- `Player → NewAssembly` exists only because `UtilityManager` holds a concrete `Meta.UI.UtilityButton`. `Setup → NewAssembly` exists only for `PlayerStats` in `OriginalRandomSeed`.
- All 11 runtime asmdefs target every platform. There is no Editor-only assembly for game code, so editor code compiles into player builds unless guarded (D1).
- `UtilsAndModels` also holds test-only `TestingRandomGenerator` and editor-only `JsonToInspector`.
- Package references are GUIDs that cannot be resolved in a plain checkout (no `Library/`). The package names above come from the `using` lines.

## 2. Runtime flow

Playground scene objects, as `object [tag]: components`:
- `Setup [Setup]`: `OriginalRandomSeed`, `SpeedLimiter`, `UtilsGameObjectsInitializer`.
- `EventSystem [EventSystem]`: `Event` plus the 9 `*Event` components.
- `Halalit [Halalit]`: `HalalitMovement`, `Health`, `Knockbackee`, `GameOver`, and `DailyTrackRecorder` once Ofir adds it (#294). Children: `Weapon [Weapon]` (`WeaponMovement`, `WeaponAttack`, `AttackToggle`), `ClawShooter [PickupClawShooter]`, `Engine/EngineFire`, `UtilityManager`.
- `Items [ItemsFactory]`: `ItemsBank`, `DropsBank`, `ItemDropper`, `ItemRankPicker`, `ItemOptions`. `Attacks [Attacks]`: `AttacksBank`. `Projectiles`: parent object for instantiated attacks.
- `EnemiesContainer`: `EnemyExplosionInitiator`; `EnemySpawner [EnemiesSpawner]` (`SpawnHoleInstantiator`, `EnemyBank`); `EnemyDestructor` (`EnemiesDestructor`).
- `AsteroidsContainer`: `AsteroidInstantiator [AsteroidInstantiator]` (`AsteroidExternalInstantiator`, `AsteroidInternalInstantiator`, `AsteroidInitiator`); `AsteroidDestructor` (`AsteroidsGlobalDestructor`).
- `ValuablesContainer`: `ValuableDropper`. `HUD/Score [Score]`: `Score`. `HUD/UtilityButton`: `UtilityButton`. `HUD/ResultsCard`: `ResultsCard`, once Ofir makes it (#292). `World`: `CameraMovement`, `EdgesForceFieldsManager`, 8 `ForceField`.
- `ObjectLoader`: `ObjectLoader`, a script from the test assembly (D16).
- Other scenes: `MainMenu.unity` holds `MainMenu` and `Authentication`, plus `PlayerNamePanel` once wired (#275); `Leaderboard.unity` holds `LeaderBoard` and `Authentication`, plus `DailyReplay` once wired (#297).

Script execution order: only `Health` (100) and `Knockbackee` (300) are set, in their `.cs.meta` files. The other 203 game scripts are 0. The reason for 100 and 300 is not recorded. Unity gives no order guarantee between objects at the same value (Unity manual, "Order of execution for event functions").

Scene start, `Awake`:
- `OriginalRandomSeed` picks the root seed: the test seed if `IsTestMode`, else `PlayerStats._dailySeed` on a daily run, else a random one.
- It gives one child seed each, in a fixed order, to `EnemyBank`, `SpawnHoleInstantiator`, `AsteroidExternalInstantiator`, `AsteroidInternalInstantiator`, `ItemsBank`, `ItemRankPicker`, `ItemDropper`, which it finds by tag.
- `UtilsGameObjectsInitializer` hands `Utils` the `InternalWorld` and `ExternalSafeIsland` objects. `SpeedLimiter` and `AttacksBank` fill static fields.
- `EnemyBank` builds its shuffled enemy list. Subscribers attach to events.

Scene start, `Start`:
- Each `*Event` snapshots its delegates (3.1).
- `ItemsBank` generates stock, `DropsBank` builds drop tables from it, `AttackToggle` loads the first attack.
- `SpawnHoleInstantiator` starts a coroutine that checks every 0.1 s (#248). `AsteroidExternalInstantiator` spawns from `Update`.

Two of these steps rely on an order that nothing pins (D13): `EnemyBank.Awake` needs its seed already set, and `DropsBank.Start` needs `ItemsBank.Start` to have run.

Per physics tick (`FixedUpdate` in 33 classes, fixed step 0.02 s):
- Player: ship movement, weapon orbit, `WeaponAttack.HumbleFixedUpdate`, claw, camera follow.
- Enemies: movement and state machines, each ending with `SpeedLimiter.LimitSpeed`; `EnemySharedBehavior` destroys enemies outside the world; `SpawnHole` runs its states.
- World: `KinematicMovement` moves asteroids, attacks move and check lifetime, `ForceField` pushes.
- Physics callbacks apply harm (`Health`), knockback (`Knockbackee`) and pickups.
- `Update` exists in 4 classes only: `AsteroidExternalInstantiator`, `UtilityManager`, the unused `Utility`, and `DailyReplay`, a viewer in the Leaderboard scene that runs on unscaled time.

Death and game over. When `Health` reaches 0 it raises an event chosen by its own tag:
- `Enemy`: `ENEMY_DEATH` → `EnemiesDestructor` rolls a valuable drop, raises the item-drop and explosion events, destroys the enemy.
- `Asteroid`: `ASTEROID_DEATH` → `AsteroidsGlobalDestructor` raises the item-drop and split events, rolls a valuable drop, destroys the asteroid.
- `Halalit`: `GameOverEvent` has three handlers, in no pinned order. `Score` hands the score to `PlayerStats` (D2). `DailyTrackRecorder` hands over the ship's track after a daily. `GameOver.OnGameOver` opens the results card, which freezes the run and saves (3.9). With no wired card, `GameOver` logs a warning and loads `MainMenu` as before.

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
- The bus (3.1) cannot serve spawned objects. A component that must react to its own object's health uses the `IHealth` interface (`Assets/GameLogic/UtilsAndModels/Models/Interface/IHealth.cs`): `event Action<int> Wounded`, `event Action<GameObject> HarmedBy` (the shot or laser that hurt it, #216), `GetCurrentHealth()`, `GetMaxHealth()` (#230: a heal through `TryChangeHealth` never goes above it), `TryChangeHealth(int)`, `ShowHealthBar()`. Assemblies that do not reference `Health` use it this way.
- `Health.TryChangeHealth` raises `Wounded` with the health left, only for a hit that hurts and does not kill. A zero hit, a heal or a killing hit raises nothing.
- Get it with `GetComponent<IHealth>()` in `Awake`, subscribe there, unsubscribe in `OnDestroy`. Example: `Assets/GameLogic/Enemies/Base/WoundedRetreat.cs`.
- Add a new per-object health event to `IHealth` the same way. Do not add a static event for it.

### 3.3 Attacks
- `AttackName` lists the attacks (18 on `develop`). `AttacksBank` (scene) maps each to a prefab in a static dictionary. Every attack prefab carries `AttackBehaviour` (`ShotType` plus `AttackStats`: Power, CriticalHit, Luck, Rate, Weight) and its own script under `Assets/GameLogic/Attacks/{ShortRange,MidRange,LongRange}`.
- `AttackToggle` holds the current attack and swaps it on `PLAYER_ATTACK_ITEM_PICKUP`. Each tick `WeaponAttack.HumbleFixedUpdate(touch)` reads it.
- `DESCRETE`: when the joystick passes `_attackJoystickEdge` and the cooldown (`Rate × _cooldownMultiplier`) is over, instantiate the prefab under `Projectiles`.
- `CONSECUTIVE`: instantiate once, then call `StartConsecitiveAttack`, `UpdateConsecitiveAttack` each tick, and `StopConsecitiveAttack` on release. Only `LaserBeamBase` derives from `ConsecutiveAttack`.
- Charged (#236): a prefab with a `ChargedAttack` (`Assets/GameLogic/Attacks/Utils/ChargedAttack.cs`) takes this path before the two above. Only `ChargeLance` uses it.
  - `WeaponAttack` caches the prefab's `ChargedAttack` only when the attack changes. A switch in the middle of a charge ends the charge.
  - Holding past the edge, once the cooldown is over, charges. While it charges, the weapon's colour steps towards `_fullChargeColor`, and the ship's thrust is multiplied by `HalalitMovement.SetWeaponThrustFactor` (it stacks with latched Leeches). Letting go after `_minHoldTime` instantiates the prefab and calls `Release(chargeLevel)` on the new instance. The cooldown runs from the release.
  - `AttackBehaviour.SetArmorBreaking` is runtime state on the instance: not serialized, not copied by `Copy`, false by default. `FrontArmor.IsBlocking` lets an armor-breaking attack through, so a full lance is neither blocked nor reflected.
- Attacks that spawn parts pass stats on with `AttackBehaviour.Copy` (`Shotgun`, `Spikes`, `BlastShot`, `Grenade`, `Blast`). Attacks destroy themselves on lifetime end, on hit, or when they leave the `InternalWorld` trigger.
- Stats travel through the prefab asset: `AttackToggle` writes `AttackStats` onto the prefab and `WeaponAttack` reads them back every tick (D9).
- Example: `Assets/GameLogic/Attacks/MidRange/BlastShot.cs`; flow in `Assets/GameLogic/Player/Weapon/WeaponAttack.cs`.

Rules when extending:
- A new attack is an `AttackName` member, a behaviour script, a prefab variant of `HalalitAttack.prefab`, an `AttacksBank` entry and an attack item (3.4).
- `AttackToggle` maps `ItemName` to `AttackName` by member name, so the two names must match. `ItemName` members have explicit numbers (#118): append a new one with the next free number.
- Movement and lifetime checks go in `FixedUpdate`. Tunables are serialized fields on the attack script.
- Do not add new writes to a prefab asset.
- A new charge-and-release attack subclasses `ChargedAttack`. `Release` writes only to the new instance, never to the prefab the weapon reads.

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
- Endless ramp (#248): enemies never run out.
  - `EnemyBank` refills itself the moment its last group is taken: the same enemies, reshuffled, with new seeds and group sizes, all drawn from its own generator in the same order as the first fill. So `NoMoreEnemies()` is true only for an empty scene bank.
  - `SpawnHoleInstantiator` checks every 0.1 s. The gaps are scene data, `_instantiationRate`, one row per hole; the last row holds after the end. The daily twist's multiplier is applied in `GetInstantiationGap` only.
  - Retry rule: the next gap starts only once a hole is placed. A failed spot search looks again 1 s later and keeps its row.
  - A spot needs no collider other than the `InternalWorld` trigger within `_instantiationDistanceFree` (7 in the scene). The free area is about 26.5 × 7.1 units, and the ship or one enemy in the middle covers more than half its width. Resting items never expire, so a crowded arena makes searches fail.
  - A spawn hole has no collider, and its enemies' colliders stay off for its first 2 s, so a fresh hole does not block its spot. At gaps of 2 s or less two groups can come out of one place (idea #288). There is no cap on live enemies.
- Paired enemies (#237): one bank entry, two bodies.
  - The prefab carries a script that implements `IPairedEnemy` (`Models/Interface`). `EnemyBank` then gives the `EnemyEntity` a `PartnerSeed` (3.6).
  - `SpawnHole.InstantiateEnemyEntity` makes both bodies, seeds each, calls `PairWith(partner)` on the first, and gives every body its own spawn point. A hole can therefore let out more bodies than entries.
  - Tether Twins: `TetherTwin` keeps the spacing and drift; `Tether` is a separate object, tagged Enemy Shot, owned by the first twin through `ShotOwner.Mark`. It harms on a timer (`TetherState`), drops while a twin is switched off (`TetherTwin.OnDisable`), turns into the survivor's flail, and destroys itself when both twins are gone.
- Interrupt (#238): a player hit during an enemy's wind-up cancels the attack.
  - The seam is `IInterruptibleEnemyBehaviour` (`Models/Interface`): `IsInInterruptWindow(windupTime)`, `InterruptAttack(stunDuration)` and `event Action AttackFinished`. Both state machines implement it. Each has a `STUNNED` state, appended to its enum: no force, then a fresh move or wait with no new seeded draw.
  - `WaitAttackWait.IsWindingUp` is virtual and false by default, so a wait that does not override it is never interrupted. `MoveAimAttackAim.IsAimEnding` serves the shooters.
  - The `Interrupt` component listens to `IHealth.HarmedBy` and `AttackFinished`. It keeps only player attacks: an enemy attack carries a `ShotOwner` or the Enemy Shot tag. It checks its own `enabled`, because C# events still reach a script the spawn hole switched off.
  - Braced: after an interrupt, the next wind-up cannot be interrupted until an attack finishes normally. Interrupt shows it with a white flash, then a darker tint.
  - `Interrupt.ApplyColor` is the only code that sets an enemy's `SpriteRenderer.color`.
- Example: `Assets/GameLogic/Enemies/MoveAimAttackEnemies/Base/MoveAimAttackStateMachine.cs`.

Rules when extending:
- New enemy: a prefab variant of `Enemy.prefab` (or of the `MoveAimAttackEnemy` / `WaitAtackEnemy` prefabs), a new `Dropper` member appended, entries in `EnemyBank._enemyPrefabsBank` and `DropsBank`.
- New behaviour for an existing machine: subclass the abstract step. Do not copy a state machine to add a state.
- End every enemy `FixedUpdate` with `SpeedLimiter.LimitSpeed(_rigidBody)`. Turn at edges with `Utils.DidHitEdge(tag)`.
- Take random values from `EnemySharedBehavior._seedfulRandomGenerator`, in `Start` or later.
- Pause marker (#201): every script that moves, aims or attacks an enemy implements the empty `IPausableEnemyBehaviour` (`Models/Interface`) and sits on the enemy's root. `WoundedRetreat` pauses an enemy by switching these off with `enabled = false`. No `Pause()` method, no type checks, no script names. Today: the special-movement scripts (`LameEnemy`, `ZigZagEnemy`, `SinusEnemy`, `GreekEnemy`, `LeechEnemy`, `MenderEnemy`, `TetherTwin`), both state machines, `ShotDodger`, `TargetFacer` and `Grudge`.
- A script that leaves live shots or rays behind clears them in `OnDisable`. `MoveAimAttackStateMachine.OnDisable` calls `StopAiming()` / `StopAttacking()`; a new aim or attack step with live objects overrides those.
- `OnDestroy` must cope with a script whose `Start` never ran: a spawn hole keeps scripts off, and the enemy can be destroyed in the hole. Check what `Start` makes before using it, as `ShootingLaserAsteriskAttack.OnDestroy` does since #291.
- A new interruptible enemy implements `IInterruptibleEnemyBehaviour` on its driver and gets the `Interrupt` component in its prefab. A new state is appended to the machine's enum, with a case in its `switch`.
- No other script writes an enemy's `SpriteRenderer.color`. Two writers undo each other. A new tint goes through `Interrupt.ApplyColor`, or through a card that gives the colour one shared owner.
- Paired enemies come only through `IPairedEnemy` and the bank's partner seed. Do not spawn a second body from the enemy's own scripts.
- Never add Enemy Shot to a `FrontArmor` or `Knockbackee` list. Both judge a harmer by its transform, a reflect would delete the tether, and `Knockbackee` expects a kinematic harmer to have `KinematicMovement`.
- Tests never read the enemy bank until it returns null or empty: it refills for ever. Use `TestUtils.TakeEnemyGroups`, which asks a fixed number of times.

### 3.6 Randomness
- `SeedfulRandomGenerator` wraps `System.Random(seed)`. A component that needs one derives from `SeedfulRandomGeneratorUser` (10 classes); `SetInitialSeedfulRandomGenerator(seed)` throws if called twice.
- Seeds flow down a tree: `OriginalRandomSeed` → 7 scene systems; `EnemyBank` → each enemy (`EnemyEntity.Seed`, applied by `SpawnHole`; a paired entry also draws `PartnerSeed` right after its own seed, #237); the asteroid instantiators → each asteroid (`AsteroidInitiator.InitAsteroid`).
- The seeded tree decides: enemy order, group sizes and spawn positions; the asteroid line, positions, scales and split count; item stock, ranks and stats; item and valuable drop rolls; the start direction of Greek and Sinus enemies.
- Since #183 and #184, every gameplay roll is seeded: each enemy and asteroid has its own generator, and combat rolls (critical hits, shotgun, enemy aim) use one too. `SeedlessRandomGenerator` is left for visuals (engine particles, item spin), the root seed of a non-daily run, and two `Utils` helpers with no caller (D8). `UnityEngine.Random` is not used.
- Example: `Assets/GameLogic/Setup/OriginalRandomSeed.cs`.

Rules when extending:
- New gameplay randomness uses a seeded generator: the owner's (`EnemySharedBehavior`, `AsteroidSharedBehavior`) or a new `SeedfulRandomGeneratorUser` seeded by whatever creates it.
- A new scene-level user is added at the end of `OriginalRandomSeed.SetAllInitialSeedfulRandomGenerators`. The call order defines every seed.
- Seedless is for visuals only. A seedless result must never decide whether a seeded draw happens (`AsteroidInternalInstantiator.cs:127` did, until #183).
- Do not use the generator in `Awake`; it may not be assigned yet.
- Every `EnemyBank` draw happens inside `FillEnemyLists`, so the first fill and every refill draw the same way (#248). A new per-entry draw goes there too.
- Adding a prefab to the scene's enemy bank, or making an entry paired, changes every daily's enemy order. Ship it in a build everyone installs, as with a twist change (3.8).

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
- "Humble" methods: the Unity callback only reads input and passes it to a method that takes the input as parameters. That method is `internal` in the editor and `private` in builds (`#if UNITY_EDITOR internal #else private #endif`). Six exist: `WeaponAttack.HumbleFixedUpdate`, `WeaponMovement.TryChangeWeaponPosition`, `HalalitMovement.TryMove`, `PickupClawShooter.TryShootClaw`, `AttackToggle.SetNewAttack`, `ItemDropper.DropNewItem`. `SpawnHoleInstantiator.GetInstantiationGap` (#248) and `SpawnHole.InstantiateEnemyEntity` (#237) use the same `internal` / `private` switch so tests can call them.
- `[assembly: InternalsVisibleTo("TestsPlayMode")]`, inside `#if UNITY_EDITOR`, exists in every game assembly except `Setup` and `World`:
  - once in `Asteroid` (`AsteroidExternalInstantiator.cs`, #239), `Enemies` (`ShotDodger.cs`), `Health` (`Health.cs`), `UtilsAndModels` (`EnemyTarget.cs`, #163) and Meta (`GameOver.cs`);
  - repeated in older files: 10 copies in `Attacks`, 6 in `Player`, 2 in `Items`. The copies do no harm. One per assembly is enough, so search the assembly's folder before adding one.
- Three attributes name an assembly that does not exist, so they grant nothing (D19): `"TestsMode"` in `Attacks/MidRange/Boomerang.cs`, and `"PlayModeTests"` in `Meta/PlayerStats.cs` and `Meta/Score/Score.cs`. Meta still reaches the tests through `GameOver.cs`.
- For EditMode tests, `Player` also grants `TestsEditMode` (`WeaponMovement.cs`). The test assembly `TestsPlayMode` grants `TestsEditMode` in `TestUtils.cs`, which is how EditMode tests use `AssertWrapper` and `TestUtils`.
- Example: `Assets/GameLogic/Player/Weapon/WeaponAttack.cs`.

Rules when extending:
- Game code never calls an editor-only member.
- Hooks read or inject. No test-only branch in game logic beyond the existing `WeaponAttack._isTesting`.
- The first hook in an assembly without the attribute adds `InternalsVisibleTo("TestsPlayMode")` under `#if UNITY_EDITOR`.

### 3.8 Daily twist
- Every daily has one twist, the same on every phone that day (#239). PLAY and test runs have none, unless a test sets one.
- The data lives in `UtilsAndModels`:
  - `DailyTwistName` (`Models/Enum`): one member per twist, with `NONE` first.
  - `DailyTwist` (`Models/DataObject`): a plain class with a name, a title, a description and one readonly field per knob. Each knob has a `Get…(sceneValue)` method that returns the scene's own number when the knob is at its neutral value.
  - `DailyTwists` (`Random`): the table `PICKABLE`, the pick, and the run's twist `Current`.
- The pick rotates (#273). `PickForDate(israelDate)` deals the days in blocks of `PICKABLE.Length`, and each block gives every twist once. The order inside a block is a Fisher-Yates shuffle whose swaps take the digits of `DailySeed.GetHash(blockNumber)`. So one twist comes at most 2 days in a row, and none is missing for more than 4 days (with 3 twists). `PickForSeed` is gone.
- The pick uses whole numbers only and creates no generator, so no seeded draw moves (3.6). `DailySeed.GetSeed` is `GetHash(GetDayNumber(date))`; the twist shares that hash.
- `OriginalRandomSeed.Awake` calls `DailyTwists.SetCurrent` on every Playground load. It sets `TestTwist` in test mode, the pick for `PlayerStats._dailyDate` on a daily run, and `NONE` otherwise. Setting it on every load is what stops PLAY from keeping the last daily's twist.
- Consumers read `DailyTwists.Current` in `Start`, never in `Awake`. Every `Awake` in the scene runs before any `Start`, so no Script Execution Order is needed.
- There are three knobs, each read once in `Start`:
  - start health: `Health`, only where the serialized `_followsDailyTwist` ("Follows Daily Twist") is on. That is the Halalit in the Playground scene, never a prefab.
  - spawn gap multiplier: `SpawnHoleInstantiator`, applied in one place only, so it never compounds.
  - smallest asteroid scale: `AsteroidExternalInstantiator`, still one draw per asteroid.
- `MainMenu` shows today's twist, picked from `PlayerStats._dailyDate`. `DailyTwistBanner` shows the run's twist on the HUD for a few seconds.
- Tests:
  - EditMode `Assets/Tests/EditMode/DailyTwistTests.cs`: 12 pinned dates (`PinnedTwists`), every block has every twist once, at most 2 days in a row, the table, the knob methods, and that no prefab follows the twist.
  - PlayMode `DailyTwist/DailyTwistTests.cs`: each knob in the scene, a daily on each day of one block, and the PLAY branch. `TestUtils.SetPlayerRun` takes the daily's date.
  - Call `TestUtils.SetTestTwist` before loading the scene, and `ResetTestTwist` in `[TearDown]`.
- Example: `Assets/GameLogic/UtilsAndModels/Random/DailyTwists.cs`.

Rules when extending:
- A twist that uses the existing knobs: append a member to `DailyTwistName` and one row to the end of `PICKABLE`. Never insert, reorder or delete in either (rule 3, and version skew below). In the same pull request, work out the new `PinnedTwists` values outside Unity, with a copy of the `DailySeed` hash and of the block pick. An appended row changes the block length, so it re-deals every date.
- A new knob: add a readonly field with a neutral value to `DailyTwist`, a constructor parameter and a `Get…(sceneValue)` method. Give every existing row the neutral value. Add one consumer that reads the knob in `Start`. Test the method in EditMode and the consumer in PlayMode.
- A knob never writes to a prefab asset (rule 6). It never adds, removes or reorders a seeded draw. Change a number that is read in `Start`, or the range of an existing draw, as Big Rocks does. This is why Lucky Day waits for D9.
- A knob on a component that also sits on prefabs is opt-in and off by default, as `_followsDailyTwist` is. Add an EditMode check that no prefab turns it on, as `NoPrefabFollowsTheTwist` does.
- Version skew: any of these gives every date a new twist: a change to `PICKABLE` (an appended row too), to the block shuffle (`GetBlockOrder`), or to `DailySeed.GetDayNumber` or `GetHash` (which also changes every seed). Phones on an older build then play a different twist from phones on a newer one. Ship such a change only in a build everyone installs. A failing `PinnedTwists` is that warning, so find out why before changing the expected values.
- The twist is a pure function of the date, so a leaderboard can name a day's twist without saving anything. After the list changes, past days come out wrong.
- Changes that re-deal the dates should share one build. #242 (Lucky Day and Bounty) is the next one.

### 3.9 Meta: services, results card and replay
The Meta assembly is the only one that calls Unity Services (Authentication, Cloud Save, Leaderboards). The test assemblies do not reference those packages, and adding one needs Ofir (rule 1). So no test can reach the SDK, and Meta code is split so that almost everything else can be tested.

- Thin SDK classes. Only these call the SDK, and they hold as few rules as they can:
  - `Authentication`: sign-in, `UpdatePlayerNameAsync` (#275).
  - `PlayerStats`: private Cloud Save scores and the leaderboard submits.
  - `PlayerPlace` (#245): your entry on a list, or null when Unity answers `EntryNotFound` or `NotFound`, and the list's player count. Unity's `Rank` starts at 0. A past day's `LeaderboardVersionEntry` is copied into a `LeaderboardEntry`, so both lists use one row type.
  - `DailyTrackStore` (#244): the only class that reads or writes replay tracks.
  - Known exception: `LeaderBoard` still loads its lists from the SDK itself.
- Pure rules. Static classes with no SDK types and no scene, tested in EditMode: `PlayerNameRules`, `DailyLeaderboard`, `PlayerPlace.GetPlaceText` / `GetOrdinal`, `DailyReplayRules`, `DailyTrackCodec`. They take plain data: `DailyTrack` and `DailyReplayEntry` live in `UtilsAndModels/Models/DataObject`.
- The results card and the death flow (#249):
  - `GameOver` finds the `ResultsCard` in `Start` and calls `Open()` on a death. `Open` sets the time scale to 0 and waits one frame, because nothing pins whether `Score` has handed the score over yet. Then it shows the score and best and calls `PlayerStats.SaveRunScoresAsync`.
  - Saves are chained: each waits for the one before, and `PlayerStats.InitAllAsync` waits for the run's save too, so two saves never overlap.
  - After a daily the card shows your place and hides Again, so the daily keeps its one try.
  - Every button puts the time scale back before it loads a scene. `OnDestroy` puts it back only while it is still 0.
- Async pattern, used by `ResultsCard`, `PlayerNamePanel`, `LeaderBoard` and `DailyReplay`:
  - `async void` only at a Unity entry point (`Awake`, a button method), with a `try` / `catch` around the awaits. The work itself is `async Task`.
  - After every `await`, check `this == null` before touching the scene: the scene may have been unloaded.
  - A busy flag (`_isSaving`, `_isLoading`) ignores a second tap. A load number drops a result that comes back after Close or a newer Open (`DailyReplay`).
  - A failed call is logged once and shown as a line of text ("Could not load"), never thrown on. Where being offline is expected, log a warning, not an error (`ResultsCard`, `PlayerNamePanel`).
  - Copy a value before the first `await` if a later run can change it (`PlayerStats.SavePlayerHighScoreAsync`).
- Unwired guard: a component that waits for Ofir's scene wiring checks its fields in `Awake`, logs one warning or error that names them, and sets `enabled = false`. A half-wired scene then still loads, and the PlayMode tests still run. `PlaygroundTests` checks the Playground's wiring.
- Swappable services: when a PlayMode test must run a scene component that calls a service, the component keeps those calls in `Func<Task…>` fields. `Awake` sets them to the real calls; a test swaps them through an editor-only `SetServices` hook (3.7). Example: `ResultsCard`. No test saves or loads for real.
- Replay record (#294): `DailyTrackRecorder` sits on the Halalit.
  - In `FixedUpdate` it samples the position every 0.25 s of game time, as 0 to 1 across the `InternalWorld` box, plus every score change and the survival time. Sampling stops at 7,200 samples (30 minutes); the survival time and the final score keep counting.
  - On game over it hands the JSON to `PlayerStats._dailyTrack`, only after a daily, as `Score` hands over the score.
  - `PlayerStats.SaveDailyScoreAsync` saves the track just before the leaderboard submit, only on the run's own day. A failed track save is logged and never blocks the score.
  - `DailyTrackCodec` writes the JSON with `JsonUtility`: a version number, one byte per axis in Base64. Read scores by time (index × `SampleSeconds`), never as a position index.
- Replay watch (#297): `LeaderBoard.Watch` shows only on a finished day (`DailyLeaderboard.CanWatch`).
  - It opens `DailyReplay`, which calls `DailyTrackStore.LoadDayAsync`. That reads the archived list's top 20 plus you, then each player's track by player id, all at once. A track that is missing or cannot be read means no ship for that player.
  - `DailyReplay.Update` plays it on unscaled time.
  - The crown rule lives in `DailyReplayRules.CompareForCrown` only (Ofir's provisional rule: highest score, then longer survival).
- Cloud Save public data:
  - A track is the public key `DailyTrack_<dd-MM-yy>`, written with `PublicWriteAccessClassOptions`. Any player can read it; only its owner can write it. Scores stay in private keys (`HighScore`, `DailyScore_<dd-MM-yy>`).
  - The writer and the reader must build the key from the same Israel date. `DailyTrackStoreTests` pins this.
  - A modified client can write anything to its own public data. Never put private data there, and never decide a score or a place from it.
  - Limits: 5 MiB and 2,000 keys per player per access class. Deleting old tracks is idea #295.
- `PlayerPrefs`: first used by `PlayerNamePanel` for its "name chosen" flag (#275). It lives on the device only, so do not use it for anything that must follow the player's account.

Rules when extending:
- A new SDK call goes in a thin SDK class, never in a screen. Until it has compiled on the Mac, the pull request lists it under "Unconfirmed SDK calls", with the package version from `Packages/packages-lock.json`.
- A new rule goes in a pure static class with an EditMode test.
- A new screen component uses the unwired guard and the async pattern. If a PlayMode test must reach it, its service calls are swappable.
- Code that changes the time scale puts it back on every way out.

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

- 439 tests on `develop` (2026-11-28, by grep): 317 `[UnityTest]` in 56 PlayMode classes under `Assets/Tests/PlayMode/PlayModeTests/<Feature>/`, and 122 `[Test]` in 13 EditMode classes under `Assets/Tests/EditMode/`. Most EditMode tests cover pure rules (3.8, 3.9); `PlaygroundTests` checks scene wiring. Tests run from the Unity Test Runner.
- CI: `.github/workflows/unity-tests.yml` (#155; on `develop`, not yet on `master`) runs on pull requests and on demand. Its test job runs the EditMode and PlayMode tests in Unity 2022.3.3f1. It is skipped, not failed, until Ofir adds the three Unity secrets `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD` (idea #270). Until then no test has run in CI, so passing checks on a pull request say nothing about the tests.
- PlayMode tests run the real `Playground` scene. Template: `Assets/Tests/PlayMode/PlayModeTests/Attacks/LongRange/BallShotTests.cs`.
  1. `[SetUp]`: `_currentSeed = TestUtils.SetRandomSeed();` then `SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);`. This puts `OriginalRandomSeed` in test mode with that seed.
  2. The body is marked `// GIVEN`, `// WHEN`, `// THEN`. `TestUtils.SetTestMode()` switches off `AsteroidsContainer` and `EnemiesContainer`, stops `WeaponAttack` from reading the joystick, and sets time scale to 5 through `TestTimeController`.
  3. Actors come from the scene's `ObjectLoader`: `LoadEnemyInExternalSafeIsland(seed)`, `LoadAsteroidInExternalSafeIsland(seed)`, `LoadItemInExternalSafeIsland(ItemName)`, `LoadValuableInExternalSafeIsland()`. They appear parked in `ExternalSafeIsland`; move them with `TestUtils.Set…Position`.
  4. Drive the game through the humble methods (3.7) with `yield return null` between steps. Random test input comes from `TestUtils.GetRandomTouch…`, seeded by the same seed.
  5. Assert with `AssertWrapper.X(..., message, _currentSeed)`. A failure prints the seed; rerun with `TestUtils.SetRandomSeed(<seed>)`.
  6. `[TearDown]`: `TestUtils.DestroyAllGameObjects()`. Then call `TestTimeController.ResetTimeScale()` only if the test saved a scale, which `SetTestMode` and `TestTimeController.SetTestTimeScale` do.
     - `ResetTimeScale` puts back the last saved scale. That is 0 if nothing has been saved yet, and a scale of 0 stops time for the tests that follow.
     - If some tests in a class save no scale, set a flag where a test saves one and check it in `[TearDown]`, as `SameSeedTests` and `DailyTwistTests` do.
     - Also undo every static the test set: `TestUtils.ResetTestTwist()` after `SetTestTwist`, and `TestUtils.ResetPlayerRun()` after `SetPlayerRun` (D22).
- Null checks in tests (#254, on `develop`, not run in Unity):
  - `AssertWrapper.IsNull` and `IsNotNull` take a `UnityEngine.Object` and follow Unity's `== null`. A destroyed object counts as null, and so does the editor's stand-in for a missing component or an unassigned serialized field. Their self-test is `Assets/Tests/EditMode/AssertWrapperTests.cs`.
  - Do not give them an `object` overload. A Unity object passed as `object` or as an interface would then quietly get NUnit's plain reference check, which counts a destroyed object or a stand-in as present.
  - Any other null check uses `==` or `!=` on a typed Unity reference: a `GameObject`, a component, or a generic `T` constrained to `Component`. Never on an `object`, an interface or an unconstrained generic. `is null`, `?.` and `??` skip Unity's check in the same way.
- Logs in tests (#291):
  - The test runner already fails a test on any logged error or exception, so a test for "no error" needs no extra call.
  - Do not use `LogAssert.NoUnexpectedReceived()` in this suite. It also fails on plain info logs, and the Playground logs some on every load (the seed, the first attack), so it fails even when the code is right.
  - An expected warning or error is declared with `LogAssert.Expect` before it happens, as `DailyReplayTests` does.
- Enemy spawner tests: `TestUtils.SetTestMode(keepEnemiesContainer: true)` keeps the spawner running. Read the bank only through `TestUtils.TakeEnemyGroups` (3.5). A death in a test opens the results card; `TestUtils.UnhookResultsCardFromGameOver()` stops that, and `TestUtils.IsGameOver` means "the card is open, or the menu loaded".
- `Assets/Tests/PlayMode/PlayModeTests/TestInfra/TestUtils.cs` holds scene getters by `Tag`, setup helpers, default `AttackStats` and shot-tracking coroutines. Add helpers there, not in test classes.
- EditMode `PlaygroundTests` checks scene wiring: tagged objects exist and carry the expected components. It opens no scene itself, so it checks whichever scene is open in the editor.
- Not referenced by any test (by grep, 2026-11-28): `ItemRankPicker`, upgrades, `MainMenu`, `LeaderBoard`, `PlayerNamePanel`, the event-bus snapshot, one attack hitting two victims (except `MirrorBallShot`). The menu and leaderboard screens sign in to Unity's servers when they open, so no PlayMode test can load them; their rules are tested in EditMode (3.9).
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
8. No `Debug.Log` in `Update`, `FixedUpdate`, coroutine loops or physics callbacks. Never log tokens, player ids or credentials, and no game code reads `AccessToken` (`AuthenticationTests` enforces it, #284). Never turn on the Unity Services verbose-logging switches (the scripting defines `ENABLE_UNITY_SERVICES_VERBOSE_LOGGING` and `ENABLE_UNITY_AUTHENTICATION_VERBOSE_LOGGING`): with them the SDK logs request headers, sign-in token included.
9. Per-tick gameplay goes in `FixedUpdate`. One-frame input such as `GetMouseButtonDown` is read in `Update`.
10. No `Find…`, `GetComponent` or `Resources.Load` per tick. Serialize the reference or cache it in `Awake` / `Start`.
11. Reuse, do not copy. If a new class would differ from an existing one by a few lines, add a parameter or subclass a base (`AttackItem`, `Upgrade`, `Valuable`, `ConsecutiveAttack`, the enemy step classes, `KinematicMovement`). The pushing and pulling magnets are the example not to repeat.
12. No type checks on concrete classes (`if (x is NitroUtility)`). Put the behaviour behind the interface or base class.
13. `Awake` sets up the object's own state and caches references: its own components and serialized links (`WeaponAttack` since #236). Work that reads another object's state goes in `Start`. If an order dependency remains, pin it with Script Execution Order and state it in the pull request.
14. Follow section 4: own-line `[SerializeField]`, `_camelCase`, pure `Is/Should/Did` predicates, tunables as serialized fields. Test hooks only as in 3.7; no test types in runtime assemblies or scenes.
15. Every behaviour change ships with a PlayMode test in the section 5 shape, or an EditMode test for a pure rule (3.9). Nothing counts as tested until it has run in Unity; say so when you could not run it.

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
| D7 | Logging on hot paths. **Token half fixed by #284**: sign-in logs only "Player signed in.", and `AuthenticationTests` fails if game code reads `AccessToken` | Still open, idea #285: `Assets/GameLogic/UtilsAndModels/Utils/InGameUtils/Utils.cs:374-376,383-384` via `Enemies/Base/EnemySharedBehavior.cs:7` (per enemy per tick); `Player/Halalit/HalalitMovement.cs:101`; `Items/Utility/NitroUtility.cs:91`; `Items/Utility/PullingMagnetField.cs:54-57` | Frame time and log noise on a phone | S |
| D8 | **Mostly fixed by #183 and #184.** Mixed determinism | Left: `SeedfulRandomGenerator.GetInsideUnitCircle` (`UtilsAndModels/Random/SeedfulRandomGenerator.cs:38`) still returns a point in a square; `Utils.GetRandomVector` and `Utils.Get180RandomNormalizedVector` (`Utils.cs:89,115`) are seedless and have no caller | Drop forces lean to the diagonals; a new caller of the two helpers would break the daily | S |
| D9 | Prefab asset mutated at runtime | `Assets/GameLogic/Player/Weapon/AttackToggle.cs:33,90` write; `Player/Weapon/WeaponAttack.cs:95` reads back each tick | The asset is shared state: stats outlive the scene and, in the editor, can outlive play mode (by reading) | M |
| D10 | Sign-in initialised repeatedly. **Handlers fixed by #284**: a static `_areEventsSetUp` (`Meta/Authentication.cs:13`) makes `SetupEvents` add them once per app run | Still open, card #176: `Assets/GameLogic/Meta/Authentication.cs:10` (`InitializationTask` never assigned), so `Initialize()` runs twice per menu load (`Meta/MainMenu.cs:32-36`, then `Meta/PlayerStats.cs:44-48`) and once per leaderboard load. Also `async void Awake` in `Meta/MainMenu.cs:20` and `Meta/Score/LeaderBoard.cs:42` | Unity Services start-up and the sign-in check repeat; two loads can overlap; an exception leaves the menu half set up | S |
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
