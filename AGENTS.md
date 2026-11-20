# Halalit - AI Agent Development Guide

## Project Overview

Halalit is a Unity 2022.3.3f1 2D space shooter game featuring:
- **Player ship**: "Halalit" with weapons and movement
- **Attacks/Weapons**: Multiple attack types (long/mid/short range)
- **Enemies**: Multiple enemy types with AI behaviors
- **Asteroids**: Obstacles with physics interactions
- **Pickup Claw**: Item collection system
- **Stats System**: Power, critical hits, luck, rate, etc.
- **Items**: Upgrades, utilities, and valuables

The project has **205 C# files** organized into **13 assembly definitions** with strict dependency management.

---

## Folder Structure

### Core Game Logic (`Assets/GameLogic/`)

#### **Attacks** (Assembly: `Attacks`)
- **Dependencies**: `UtilsAndModels`, `Health`
- **Location**: `Assets/GameLogic/Attacks/`
- **Structure**:
  - `LongRange/`: `BallShot.cs`, `LaserBeam/` (LaserBeamBase, LaserBeamProjected, LaserBeamRetracting), `MirrorBallShot.cs`
  - `MidRange/`: `BlastShot.cs`, `Boomerang.cs`, `Grenade.cs`, `KnockbackWave.cs`, `Shotgun/` (Shotgun, ShotgunShot)
  - `ShortRange/`: `Spikes.cs`, `Sword.cs`
  - `Utils/`: `Blast.cs`, `ConsecutiveAttack.cs`
  - `Infra/`: `AttacksBank.cs` - Central registry mapping `AttackName` enum to prefabs
- **Key Concepts**:
  - All attacks implement damage through `AttackBehaviour` (in UtilsAndModels)
  - `AttackBehaviour` contains `AttackStats` (Power, Luck, CriticalHit multiplier) and `AttackShotType` (DESCRETE vs CONSECUTIVE)
  - DESCRETE attacks damage on `OnTriggerEnter2D` only
  - CONSECUTIVE attacks damage continuously in `OnTriggerStay2D`
  - Attacks are instantiated as projectiles from prefabs in `Assets/Resources/Prefabs/Attacks/`

#### **Enemies** (Assembly: `Enemies`)
- **Dependencies**: `Attack`, `UtilsAndModels`, `Items`
- **Location**: `Assets/GameLogic/Enemies/`
- **Structure**:
  - `Base/`: `EnemySharedBehavior.cs` (auto-destroys enemies outside world bounds), `EnemiesDestructor.cs`, `EnemyDropper.cs`
  - `SpecialMovementEnemies/`: `LameEnemy.cs`, `ZigZagEnemy.cs`, `GreekEnemy.cs`, `SinusEnemy.cs`
  - `WaitAttackEnemies/`: State machine with Wait and Attack phases
  - `MoveAimAttackEnemies/`: State machine (Move → Aim → Attack) for `ShootingInRangeEnemy`, `ShootingLaserRangeEnemy`, `ShootingLaserAsteriskEnemy`
  - `Spawner/`: `EnemyBank.cs`, `SpawnHole.cs`, `SpawnHoleInstantiator.cs`
  - `Utils/`: `EnemyMovementUtils.cs`, `EnemyUtils.cs`
  - `Explosions/`: `EnemyExplosionInitiator.cs`
- **Key Concepts**:
  - Enemies use state machines (enum-based) for behavior
  - All enemies have `Health` component
  - Enemies are tracked by `Tag.ENEMY`

#### **Health** (Assembly: `Health`)
- **Dependencies**: `UtilsAndModels`
- **Location**: `Assets/GameLogic/Health/`
- **Files**: `Health.cs`
- **Key Concepts**:
  - `Health` component handles damage/healing for Halalit, enemies, and asteroids
  - Dynamic health bars created at runtime
  - Supports both collision-based (`CollisionHarmer`) and trigger-based (`TriggerHarmer`) damage
  - Differentiates between DESCRETE (OnTriggerEnter2D) and CONSECUTIVE (OnTriggerStay2D) damage via `AttackShotType`
  - Invokes death events: `GameOverEvent` (Halalit), `DeathEvent` (enemies/asteroids)
  - Uses `InternalsVisibleTo` for test access

#### **Player** (Assembly: `Player`)
- **Dependencies**: `UtilsAndModels`, `Attacks`, `External`, `Items`, `Meta`
- **Location**: `Assets/GameLogic/Player/`
- **Structure**:
  - `PickupClaw/`: Pickup claw system with state machine (`PickupClawStateMachine.cs`)
    - States: `MOVING_TO_TARGET`, `GRABBING`, `RETURNING_TO_HALALIT_WITH_TARGET`, `RETURNING_TO_HALALIT_WITHOUT_TARGET`
    - Components: `PickupClawGrabber.cs`, `PickupClawMovement.cs`, `PickupClawShooter.cs`, `PickupClawAnimator.cs`, `PickupClawUtils.cs`
    - Claw automatically retracts when exceeding maneuver radius from Halalit
- **Key Concepts**:
  - Halalit uses weapon system (details in Items assembly)
  - Movement likely controlled via joystick (see `External/Joystick Pack`)

#### **Items** (Assembly: `Items`)
- **Dependencies**: `UtilsAndModels`
- **Location**: `Assets/GameLogic/Items/`
- **Structure**:
  - `Attack/`: Attack item pickups (`BallShotItem.cs`, `LaserBeamItem.cs`, etc.)
  - `Upgrade/`: Permanent upgrades
    - `HalalitUpgrade/`: `HalalitImmunity.cs`, `HalalitVigor.cs`, `HalalitVitality.cs`
    - `WeaponUpgrade/`: `WeaponDexterity.cs`, `WeaponStamina.cs`, `WeaponStrength.cs`
    - `ClawUpgrade/`: `ClawAgility.cs`, `ClawRange.cs`, `ClawStability.cs`
  - `Utility/`: Temporary powerups (`NitroFuel.cs`, `PullingMagnet.cs`, `PushingMagnet.cs`)
  - `Valuables/`: Currency items (`Diamond.cs`, `Gold.cs`, `Silver.cs`)
  - `Infra/`: `ItemsBank.cs`, `ItemDropper.cs`, `DropsBank.cs`, `ItemMovement.cs`

#### **Asteroids** (Assembly: `Asteroids`)
- **Dependencies**: `UtilsAndModels`, `Items`
- **Location**: `Assets/GameLogic/Asteroids/`
- **Files**: `AsteroidDropper.cs`, `AsteroidExternalInstantiator.cs`, `AsteroidInitiator.cs`, `AsteroidInternalInstantiator.cs`, `AsteroidMovement.cs`, `AsteroidSharedBehavior.cs`, `AsteroidsGlobalDestructor.cs`

#### **UtilsAndModels** (Assembly: `UtilsAndModels`)
- **Dependencies**: `Unity.TextMeshPro`
- **Location**: `Assets/GameLogic/UtilsAndModels/`
- **Structure**:
  - `Models/DataObject/`: Data classes
    - `AttackBehaviour.cs`: Implements `IHarmer`, calculates damage with critical hits
    - `AttackStats.cs`: Power, Luck, CriticalHit, Rate, etc.
    - `AttackOptions.cs`, `AttackStatsRange.cs`
    - `ItemStats.cs`, `ItemDropDto.cs`, `ItemStockDto.cs`
    - `EnemyEntity.cs`
    - Various configuration classes (Nitro, PullingMagnet, PushingMagnet, etc.)
  - `Models/Enum/`: All enums
    - `AttackName.cs`: BALL_SHOT, LASER_BEAM, MIRROR_BALL_SHOT, BOOMERANG, BLAST_SHOT, GRENADE, KNOCKBACK_WAVE, SHOTGUN, SWORD, SPIKES
    - `AttackShotType.cs`: DESCRETE, CONSECUTIVE
    - `Tag.cs`: HALALIT, ENEMY, ASTEROID, SHOT, etc.
    - `ItemName.cs`, `ValuableName.cs`, `SceneName.cs`, etc.
  - `Models/Event/`: Event system with custom event classes
  - `Models/Interface/`: `IHarmer.cs`, `IUtility.cs`, `ISpeedForceController.cs`, etc.
  - `Random/`: `SeedfulRandomGenerator.cs`, `SeedlessRandomGenerator.cs` (for deterministic testing)
  - `Utils/InGameUtils/`: 
    - `Utils.cs`: Utility functions (rotation, distance, world bounds checking)
    - `Constants.cs`
    - `CommonPhysics/`: `ForceField.cs`, `KinematicMovement.cs`, `Knockbackee.cs`, `SpeedLimiter.cs`, `ParentRotationCounteractor.cs`
  - `Utils/EditorUtils/`: `JsonToInspector.cs`

#### **Meta** (Assembly: `Meta`)
- **Dependencies**: `UtilsAndModels`, `Unity.Services.Core`, `Unity.Services.CloudSave`, `Unity.Services.Authentication`, `Unity.Services.Leaderboards`, `Unity.TextMeshPro`
- **Location**: `Assets/GameLogic/Meta/`
- **Files**: `PlayerStats.cs`, `GameOver.cs`, `MainMenu.cs`, `Score/` (Score.cs, LeaderBoard.cs), `Authentication.cs`, `UI/` (UtilityButton.cs)

#### **Setup** (Assembly: `Setup`)
- **Dependencies**: `UtilsAndModels`, `Items`, `Asteroids`, `Enemies`, `Meta`
- **Location**: `Assets/GameLogic/Setup/`
- **Purpose**: Game initialization and setup

#### **World** (Assembly: `World`)
- **Dependencies**: `UtilsAndModels`
- **Location**: `Assets/GameLogic/World/`
- **Structure**:
  - `EdgeForceFields/`: `EdgesForceFieldsManager.cs` - Manages world boundaries
  - `Camera/`: `CameraMovement.cs`
  - `RecycleBin/`: Old code (ConfigFileReader, OldAttackToggle)

#### **External** (Assembly: `External`)
- **Location**: `Assets/External/`
- **Contents**:
  - `Joystick Pack/`: Virtual joystick controls (likely for mobile)
  - `TextMesh Pro/`: Text rendering
  - `GabrielAguiarProductions/Unique_Projectiles_Volume_1/`: Particle effects and VFX materials

### Resources (`Assets/Resources/`)
- `Prefabs/`: All game object prefabs (attacks, enemies, items, explosions, UI)
- `Sprites/`: Game graphics
- `Materials/`: Material assets (mostly in External folder)
- `Animations/`: Animation clips
- `Rendering/Shaders/`: Custom shaders (`LaserBeam.shadergraph`)
- `UtilityConfigs/`: Configuration scriptable objects

### Scenes (`Assets/Scenes/`)
- `Playground.unity`: Main game/testing scene
- `MainMenu.unity`: Menu scene
- `Leaderboard.unity`: Leaderboard scene

### Tests (`Assets/Tests/`)
- **EditMode** (`Assets/Tests/EditMode/`): 1 test file (`PlaygroundTests.cs`)
- **PlayMode** (`Assets/Tests/PlayMode/PlayModeTests/`): 24 test files covering:
  - Attacks: All attack types (BallShot, LaserBeam, MirrorBallShot, BlastShot, Boomerang, Grenade, KnockbackWave, Shotgun, Spikes, Sword)
  - Items: NitroFuel, PullingMagnet, PushingMagnet, Valuables
  - Movement: HalalitMovement, SpeedLimit
  - Claws: PickupClaw
  - Stats: CriticalHit, Power, Rate
  - Weapon: HalalitWeapon
  - Asteroids: AsteroidInteractions
  - EdgeForceFields
  - `TestInfra/`: Test utilities (`AssertWrapper.cs`, `ObjectLoader.cs`, `TestTimeController.cs`, `TestUtils.cs`)

---

## Assembly Definition References

The project strictly enforces dependencies through `.asmdef` files. **DO NOT add references that violate this structure:**

```
Asteroids → UtilsAndModels, Items
Attacks → UtilsAndModels, Health
Enemies → Attack, UtilsAndModels, Items
Health → UtilsAndModels
Items → UtilsAndModels
Meta → UtilsAndModels, Unity.Services.Core, Unity.Services.CloudSave, Unity.Services.Authentication, Unity.Services.Leaderboards, Unity.TextMeshPro
Player → UtilsAndModels, Attacks, External, Items, Meta
Setup → UtilsAndModels, Items, Asteroids, Enemies, Meta
UtilsAndModels → Unity.TextMeshPro
World → UtilsAndModels
External → (no references)
TestsEditMode → [all assemblies + test framework]
TestsPlayMode → [all assemblies + test framework]
```

---

## Coding Conventions

### Naming Conventions
- **Private fields**: `_camelCase` (underscore prefix) e.g. `_rigidBody`, `_speed`, `_healthBar`
- **Public/Protected fields**: `PascalCase` e.g. `ShotType`, `AttackStats`
- **Methods**: `PascalCase` e.g. `GetHarm()`, `TryChangeHealth()`
- **Local variables**: `camelCase` e.g. `criticalDamage`, `itemToGrab`
- **Constants**: `UPPER_SNAKE_CASE` (in enums and Tag descriptions)
- **Enums**: `PascalCase` for type, `UPPER_CASE` for values e.g. `AttackShotType.DESCRETE`

### Code Organization
- **Regions**: Code is organized with `#region` blocks:
  - `#region Init` - Initialization (Awake, Start, Setup methods)
  - `#region Destroy` - Cleanup (OnDestroy, DestroyEventListeners)
  - `#region Accessors` / `#region Getters` - Public getters
  - `#region Events` - Unity event methods (OnCollisionEnter2D, OnTriggerStay2D, etc.)
  - `#region Predicates` - Boolean check methods (Is..., Should...)
  - `#region State Changes` - State machine transitions
- **Unity Event Order**: Awake → Start → FixedUpdate/Update → OnTrigger/OnCollision → OnDestroy
- **SerializeField**: Used for inspector-editable private fields

### Architecture Patterns
- **State Machines**: Enum-based with switch statements (see PickupClawStateMachine, enemy AI)
- **Event System**: Custom event classes (e.g. `DeathEvent`, `GameOverEvent`, `ItemEvent`) with static event invocation
- **Component Communication**: GameObject.Find, GetComponent, Tags
- **Singletons**: Static dictionaries for banks (AttacksBank, ItemsBank, EnemyBank)
- **InternalsVisibleTo**: Used for exposing internal/test methods to test assemblies via `[assembly: InternalsVisibleTo("TestsPlayMode")]`
- **Testing**: 
  - Tests use `SeedfulRandomGenerator` for deterministic random values
  - `TestUtils` provides helper methods
  - `AssertWrapper` includes seed in assertions for reproducibility
  - Tests load `Playground` scene via `SceneManager.LoadScene`

### Unity-Specific Conventions
- **Tags**: Defined in `Tag` enum with descriptions (HALALIT, ENEMY, ASTEROID, SHOT, etc.)
- **Layers**: Defined in `Layer` enum
- **Prefabs**: Loaded via `Resources.Load()` e.g. `Resources.Load(Constants.RESOURCE_DYNAMIC_BAR_PREFAB)`
- **Destruction**: Objects auto-destroy when leaving world bounds (checked in FixedUpdate)

---

## Prefabs and Scripts Relationship

### Key Prefab Types
1. **Attack Prefabs** (`Assets/Resources/Prefabs/Attacks/`):
   - Must have attached attack script (e.g. BallShot.cs, LaserBeam.cs)
   - Must have `AttackBehaviour` component with stats
   - Rigidbody2D for physics
   - Collider2D (trigger) for hit detection
   - Registered in `AttacksBank` with corresponding `AttackName` enum

2. **Enemy Prefabs** (`Assets/Resources/Prefabs/Enemies/`):
   - Must have `Health` component
   - Must have enemy behavior script (e.g. LameEnemy, ShootingInRangeEnemy)
   - Must have `EnemySharedBehavior` for auto-cleanup
   - Tag: `ENEMY`

3. **Item Prefabs** (`Assets/Resources/Prefabs/Items/`):
   - Must have `ItemMovement` component
   - Must have specific item script (e.g. BallShotItem, HalalitVitality)
   - Tag varies by type (attacks, upgrades, valuables)

4. **Explosion Prefabs** (`Assets/Resources/Prefabs/Explosions/`):
   - Particle systems for visual effects
   - Auto-destroy after animation

### Script-Prefab Dependencies
- Scripts reference prefabs via Resources.Load or SerializeField assignments in inspector
- Banks (AttacksBank, ItemsBank, EnemyBank) are SerializeField Lists that map enums to prefabs
- **Never hardcode prefab paths in multiple places** - use centralized bank systems

---

## Shaders and VFX

### Current Shader Work
- **LaserBeam.shadergraph**: Custom shader graph for laser beam effects (located in `Assets/Resources/Rendering/Shaders/`)
- **External Materials**: Gabriel Aguiar Productions Unique Projectiles Volume 1 provides particle effects and scrolling textures for:
  - Laser beams (various styles)
  - Explosions/impacts
  - Projectile trails
  - Fire/flame effects

### VFX Status (from backlog)
- ✅ **Done**: Laser beam, Ball shot, Mirror ball shot, Blast shot
- ✅ **Done**: Engine particles
- 🚧 **In Progress**: Knockback wave, Booms (explosions)
- ❌ **To Do**: Boomerang, Sword, Shotgun, Grenade, Spikes

---

## Tests - How to Run

### Test Count
- **Total**: 25 test files
  - **EditMode**: 1 test file
  - **PlayMode**: 24 test files

### Running Tests in Unity Editor
1. Open project in Unity 2022.3.3f1
2. Window → General → Test Runner
3. Select PlayMode or EditMode tab
4. Click "Run All" or select specific test

### Test Infrastructure
- **Scene**: Tests use `Playground.unity` scene
- **Setup**: Each test calls `TestUtils.SetRandomSeed()` and loads Playground scene
- **Test Mode**: `TestUtils.SetTestMode()` disables certain runtime behaviors
- **Object Loading**: `ObjectLoader` creates test objects dynamically
- **Assertions**: `AssertWrapper` includes seed for reproducibility
- **Teardown**: `TestUtils.DestroyAllGameObjects()` cleans up after each test

### CI/CD (GitHub Actions)
See the GitHub Actions workflow (`.github/workflows/unity-tests.yml`) for automated testing setup using GameCI.

---

## What Agents MUST NOT Do

### 🚫 Critical Restrictions

1. **NEVER Hand-Edit Unity Meta Files**
   - `.meta` files contain GUIDs that Unity manages
   - Editing manually breaks prefab/asset references
   - Exception: You may delete `.meta` files alongside their corresponding assets if deleting both

2. **NEVER Mass-Reformat Code**
   - Respect existing style (underscore-prefixed private fields, regions, etc.)
   - Make surgical changes only
   - Don't run auto-formatters that change whitespace across entire files

3. **NEVER Touch ProjectSettings Without Good Reason**
   - `ProjectSettings/` contains Unity configuration
   - Changes here can break builds or require manual re-setup
   - Exception: Well-justified changes (e.g. adding build target, input settings)

4. **NEVER Add Assembly References That Break Dependency Chain**
   - The assembly structure is intentional and prevents circular dependencies
   - See "Assembly Definition References" section above
   - Violating this will cause compilation errors

5. **NEVER Delete or Modify External Assets**
   - `Assets/External/` contains third-party assets (Joystick Pack, TextMesh Pro, Gabriel Aguiar Productions)
   - These are licensed/imported - don't modify without owner approval

6. **NEVER Change Scene Hierarchy Without Testing**
   - Scripts rely on GameObject.Find, tags, and parent-child relationships
   - Changing hierarchy can break runtime references
   - Always test in PlayMode after scene changes

7. **NEVER Commit Unity-Generated Folders**
   - Library/, Temp/, Obj/, Logs/, UserSettings/ are local-only
   - These are in `.gitignore` for a reason
   - Never force-add these to git

8. **NEVER Break the Stats System in Progress**
   - The backlog shows "Attacks stats + calibration" is in progress
   - Changes to AttackStats, AttackBehaviour damage calculation require careful testing
   - Coordinate with owner before modifying core stats logic

---

## Current Technical State

### Compilation Status
*Note: This VM does not have Unity installed, so compilation status was assessed via static code analysis. No syntax errors were found in reviewed files. The project should compile cleanly in Unity 2022.3.3f1.*

### Known Issues (from backlog "Current work" section)
- ❌ **In Progress**: "Laser is not destroyed with the enemy"
- ✅ **Fixed**: Multiple laser-related issues (positioning, damage, force field behavior)
- ✅ **Fixed**: Blast explosions, attack variants, projectile instantiation

### Test Health
- Tests appear well-structured with proper setup/teardown
- Use deterministic seeding for reproducibility
- Cover major game systems (attacks, movement, items, enemies)
- **Recommendation**: Run full test suite in Unity to verify current state

---

## Unity Packages and Dependencies

### Core Packages (from `Packages/manifest.json`)
- **Unity 2D**: Animation, Sprite, Tilemap, Pixel Perfect, PSD Importer, Sprite Shape
- **URP**: Universal Render Pipeline 14.0.8
- **Unity Services**: Cloud Save 3.2.1, Leaderboards 2.2.1, Authentication
- **TextMesh Pro**: 3.0.9
- **Test Framework**: 1.1.33
- **Post Processing**: 3.2.2
- **AI Navigation**: 1.1.3

### External Assets
- Joystick Pack (mobile controls)
- TextMesh Pro (bundled)
- Gabriel Aguiar Productions - Unique Projectiles Volume 1 (VFX)

---

## Recommendations for Agents

### Best Practices
1. **Always read existing code first** before making changes
2. **Follow the established patterns** (state machines, event system, regions)
3. **Write tests** for new gameplay features (use existing test patterns)
4. **Respect assembly boundaries** - don't add dependencies willy-nilly
5. **Use SerializeField for inspector values** rather than public fields
6. **Tag and organize code with regions** to match existing style
7. **Test in PlayMode** before committing - many issues only appear at runtime
8. **Check world bounds** - objects outside bounds should auto-destroy
9. **Use the event system** for cross-component communication when possible
10. **Consult backlog** (`Halalit tasks.txt`) to understand current priorities

### Safe Changes
- ✅ Adding new attack types (follow existing attack pattern)
- ✅ Adding new enemy behaviors (follow state machine pattern)
- ✅ Adding new items/upgrades (follow item pattern)
- ✅ Fixing bugs in existing systems
- ✅ Adding tests for existing features
- ✅ Improving VFX/shaders (in progress work)
- ✅ Adding documentation/comments

### Changes Requiring Owner Approval
- ⚠️ Refactoring core systems (AttackBehaviour, Health, state machines)
- ⚠️ Changing assembly structure
- ⚠️ Modifying external assets
- ⚠️ Major architectural changes
- ⚠️ Changing Unity version or packages
- ⚠️ ProjectSettings modifications

---

## Quick Reference

### Finding Things
- **Attacks**: `Assets/GameLogic/Attacks/`
- **Enemy AI**: `Assets/GameLogic/Enemies/`
- **Player/Claw**: `Assets/GameLogic/Player/`
- **Stats/Data**: `Assets/GameLogic/UtilsAndModels/Models/DataObject/`
- **Enums**: `Assets/GameLogic/UtilsAndModels/Models/Enum/`
- **Tests**: `Assets/Tests/PlayMode/PlayModeTests/`
- **Prefabs**: `Assets/Resources/Prefabs/`

### Key Files
- `AttackBehaviour.cs` - Damage calculation
- `Health.cs` - Health/damage system
- `AttacksBank.cs` - Attack prefab registry
- `ItemsBank.cs` - Item prefab registry
- `EnemyBank.cs` - Enemy prefab registry
- `Utils.cs` - Utility functions
- `Constants.cs` - Global constants
- `TestUtils.cs` - Test helpers

### Common Tasks
- **Add new attack**: Create script in Attacks/, prefab in Resources/Prefabs/Attacks/, add to AttacksBank, add AttackName enum
- **Add new enemy**: Create script in Enemies/, prefab in Resources/Prefabs/Enemies/, add to EnemyBank
- **Add new test**: Create in Tests/PlayMode/PlayModeTests/, follow existing test pattern (SetUp → UnityTest → TearDown)
- **Modify stats**: Edit AttackStats/ItemStats in UtilsAndModels/Models/DataObject/

---

## Project Philosophy (from Owner Rules)

The owner emphasizes:
1. **SOLID principles** - this project is built for long-term maintenance
2. **Consult before refactoring** - don't change existing code structure without discussion
3. **High conviction required** - only make changes you're confident about
4. **Assembly integrity** - the assembly structure is intentional and enforced

This is a **long-term project** meant to be maintained by AI agents over the next year. Code quality, testability, and maintainability are paramount.
