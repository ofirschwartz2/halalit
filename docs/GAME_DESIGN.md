# Halalit: game design as built

First draft, 2026-11-20. Written by the game designer agent for Ofir and the team.

- This describes the game as the code and data files have it today. It is not a wish list.
- No agent on the team has run the game. Unity is not installed in our workspace. Everything here comes from reading code, prefabs and scenes.
- **Computed** means worked out from the numbers, not observed. **Scene today** means how the committed Playground scene is set up, which is narrower than what the code supports.
- Paths are relative to the repo root. Short forms: `GL/` = `Assets/GameLogic/`, `PF/` = `Assets/Resources/Prefabs/`, `PG` = `Assets/Scenes/Playground.unity`.
- Ofir: please correct anything that is wrong. Section 10 has the questions only you can answer.

## Vision (decided by Ofir, 2026-11-21)

This section is what Ofir wants the game to become. Everything after it describes the game as built.

- **Spelunky-style depth.** The game was born from Spelunky. It should be full of specific interactions between a specific enemy, a specific weapon and a specific place or situation. In his words: "extremely detailed and thoughtful", so that "every little thing was very thoughtful and thought through".
- **Rich and distinct.** Many enemies and many weapons. No enemy plays like another enemy, and no weapon plays like another weapon.
- **The daily.** Every day has one daily run. It is random, and it is exactly the same for every player who plays that day, from one seed. The best players of that daily win.
- **Weapons as NFTs.** Weapons become NFTs with their own stats, and dailies reward players with them. This is a direction only. Nothing blockchain is built until Ofir says the game is ready.

**Milestone 1: Friends daily (Ofir, 2026-11-21).** The north star for now. It is done when a few of Ofir's friends install the app, play the same daily for a week, and a shared leaderboard shows who won each day. In his words: "I can take an application and send her between a few of my friends where we can play on a daily basis, where it's really fun." It needs, in order: Unity running somewhere, a phone build, the daily (the same run for everyone, scores saved, a leaderboard), a run worth replaying (difficulty that ramps, a result screen), and enough distinct weapons and enemies. NFTs come after it.

What this means for the team: the backlog is ranked by Milestone 1, and every offer for a weapon, an enemy or a behavior lists its specific interactions with what already exists. An offer that is an existing thing with different numbers is not an offer.

## 1. The game in one paragraph

Halalit is a top-down twin-stick shooter played in one fixed rectangular arena in space. You fly a small ship, the Halalit. The left stick turns and thrusts it. The right stick aims a weapon that circles the ship and fires by itself when the stick is pushed far. Enemies come out of spawn holes. Asteroids drift across in a stream. Destroyed enemies and asteroids drop valuables (silver, gold, diamond), which are the only source of score, and sometimes items: a new weapon that replaces yours, an upgrade, or a one-use utility. You collect drops by touching them or by tapping them to send a pickup claw. The run ends when the ship's health reaches zero, and the game returns to the main menu. There are no levels, no waves and no win state, and today the score is not saved.

Core loop:

1. Fly and dodge. Enemy bodies and enemy shots hurt. Asteroids only push.
2. Aim and hold fire to destroy enemies and asteroids.
3. Collect what they drop: valuables for score, items for power.
4. Fire a utility (speed boost or magnet) when in trouble.
5. Repeat until health runs out. Back to the main menu.

**Scene today:** a run holds 15 enemies and a stream of 30 asteroids in total. After that nothing new appears, and the run still ends only on death.

## 2. Controls and the player ship

**Input.** The code expects a touch screen held in landscape (`GL/World/Camera/CameraMovement.cs:26` forces landscape-left). There are two fixed on-screen joysticks from the Joystick Pack: movement at bottom-left, attack at bottom-right. There is one utility button, and a tap anywhere in the world sends the claw. There is no keyboard, gamepad or pause code. With a mouse you can hold only one stick at a time.

**Movement** (`GL/Player/Halalit/HalalitMovement.cs`). The ship turns quickly toward the stick direction and thrusts the way it faces. It has momentum and drag, so it drifts and then slows when you let go. Computed: top speed is about 7.5 units/s along an axis. On a full diagonal it is about 5.3, because the thrust is scaled by each stick axis separately (lines 78-90). The ship's own speed limit of 15 is never reached by thrust alone.

**Weapon** (`GL/Player/Weapon/`). The weapon sits 0.6 units from the ship's centre, on the side the attack stick points to. A small push only aims. A push of 80% or more fires (`WeaponAttack.cs:224`), and keeps firing while held. The ship holds exactly one attack at a time. It starts with a Ball Shot.

**Health** (`GL/Health/Health.cs`). The ship has 30 health, shown as a bar in the HUD. Two things damage it: touching an enemy (1 per touch) and enemy shots. Asteroids and the player's own explosions do not. Nothing restores health. There is no safe time after a hit. Hitting an enemy or an asteroid also knocks the ship back.

**Pickup claw** (`GL/Player/PickupClaw/`). Tap a spot. If an item or valuable lies within 2 units of the tap, a claw leaves the ship toward it. Only one claw exists at a time. Taps on the joysticks are ignored. The claw flies out at speed 3, grabs the first collectable it comes within 1 unit of (not always the one tapped), and carries it back at speed 2. If the claw gets more than 5 units from the ship it lets go and returns empty. The pickup counts when the carried item touches the ship. The tap is read as a mouse click (`PickupClawShooter.cs:32`). How that behaves while two thumbs hold the sticks could not be checked here.

**Camera.** Follows the ship and stops at the arena edges. It shows about 18 × 10 units of a 43 × 24 arena (at 16:9). Units are Unity world units. The ship is about 1 unit across.

## 3. Weapons

Scripts are in `GL/Attacks/`, prefabs in `PF/Attacks/HalalitAttacks/`. There are two shot types (`AttackShotType`):

- **Discrete:** every shot spawns a new object, then a cooldown runs. It damages on touch.
- **Consecutive:** one object exists while the stick is held. It damages on a timer while it touches a target. Only the laser is consecutive.

| Attack | Type | What it does | What makes it different |
|---|---|---|---|
| Ball Shot | Discrete | Straight bullet, speed 10. Gone on the first enemy or asteroid. | The baseline. Starting weapon. |
| Laser Beam | Consecutive | Beam grows from the weapon at speed 12 until it meets an enemy, asteroid or the outer wall. Hurts what it touches once every `Rate` seconds. | No range limit. One target at a time, no piercing. |
| Knockback Wave | Discrete | A wave front that flies 0.7 s at speed 10 (about 7 units) and widens. Passes through everything. | Pushes every enemy it touches. See note 1. |
| Boomerang | Discrete | Flies a fixed 2 s loop, about 5 units out and back to the ship. Gone on the first hit. | Curved path that returns to wherever the ship is. |
| Mirror Ball Shot | Discrete | Bullet, speed 10. On a hit it bounces straight away from the target, up to 5 times. The 6th hit removes it. | Can hit many times. Does not bounce off walls. |
| Sword | Discrete | A 0.15 s swing through a 60° arc at the weapon. | Melee. Hits everything in the arc. |
| Blast Shot | Discrete | Slow orb (speed 5, about 6.5 units). Explodes on contact or at the end of its range. | Area damage. See note 3. |
| Grenade | Discrete | Thrown solid object. Lands about 1.7 units away, bounces off things, explodes after 2 s, never on contact. | Delayed area damage. See note 3. |
| Shotgun | Discrete | 2 to 5 pellets in a ±10° cone. Each lives about 1 s and slows down. | Burst. Every pellet does full Power. |
| Spikes | Discrete | Three spikes thrust from the ship toward the aim (centre and ±15°): 0.2 s out, 0.1 s hold, 0.5 s back. | Very short reach. The spikes move only about 0.7 units out from the ship's centre. |

The explosion from Blast Shot and Grenade grows for 0.5 s to a radius of about 1.7 units (computed from `Blast.prefab`) and hits each target once.

Notes on behaviour that looks unintended:

1. Knockback Wave is meant to hurt each target once (the test `KnockbackWaveTests.DoubleHitCheck` says so). The check is given the wave instead of the target (`GL/Health/Health.cs:174`, `GL/Health/Harm/Trigger/DescreteTriggerHarmer.cs:24`). So one wave damages only the first thing it touches. It still pushes all of them.
2. Laser ticks that are missed while the beam touches nothing are saved up and paid out one per physics step on the next contact (`GL/Health/Harm/Trigger/ConsecutiveTriggerHarmer.cs:27`).
3. Explosions can damage only six of the nine enemy types, and no asteroids. See the table in section 5.

### The stats

Every weapon item carries five stats (`GL/UtilsAndModels/Models/DataObject/AttackStats.cs`).

| Stat | What the code does with it | In the item data today |
|---|---|---|
| Power | Damage per hit (`AttackBehaviour.cs:17`). | 0 to 40, by weapon and rank |
| CriticalHit | On a critical hit, damage is Power × CriticalHit, rounded down. | 0 on every item |
| Luck | Percent chance that a hit is critical. | 0 on every item |
| Rate | Discrete: seconds between shots = Rate × the weapon's cooldown multiplier, 0.3 (`WeaponAttack.cs:133`). Consecutive: seconds between damage ticks = Rate. Lower is faster. | 0.22 to 1.3 |
| Weight | **Nothing. No code reads it.** It is only generated and stored. | 0 on every item |

So critical hits work in code (and have tests) but have no effect with today's data. The item's rank is also never read or shown after the stats are rolled.

Ofir's task file lists critical hit and luck (1.3), rate (1.4) and stamina (1.5) as To Do. The code and tests for them exist (`Assets/Tests/PlayMode/PlayModeTests/Stats/`). The file may be out of date.

## 4. Items

Items and valuables are physical objects. They drift, get knocked around by enemies and asteroids, and are deleted if they leave the arena (`GL/Items/Infra/ItemMovement.cs:86`). They never time out.

**Attack items.** One per attack, ten in all. A pickup replaces the current attack at once (`GL/Player/Weapon/AttackToggle.cs:57`). The old attack is lost. There is no choice and no inventory.

**Upgrades** (`GL/Items/Upgrade/`).

| Upgrade | Meant to change | What happens today |
|---|---|---|
| Weapon Stamina | Fire rate | Works. Cooldown multiplier drops by 0.1 per pickup: 0.3, 0.2, 0.1, 0. Discrete attacks only (`WeaponAttack.cs:209`). |
| Halalit Vitality | Max health | Max health +1. Current health does not change and nothing heals, so no practical effect (`Health.cs:233`). |
| Halalit Immunity, Halalit Vigor | Defence, knockback resistance | Nothing. No code listens for them. |
| Claw Range, Claw Agility, Claw Stability | Claw reach, speed, steadiness | Nothing. |
| Weapon Dexterity, Weapon Strength | Weapon shake, weapon drag | Nothing. Shake and drag do not exist yet. |

**Utilities** (`GL/Player/Utility/UtilityManager.cs`, `GL/Items/Utility/`). The ship has one utility slot. The HUD button shows a two-letter label, and a tap fires it. A new pickup replaces an unused utility. A pickup made while one is running waits in a single queue place. Values are in `Assets/Resources/UtilityConfigs/`.

| Utility | Label | Effect |
|---|---|---|
| Nitro fuel | NF | Thrust ×2 while fuel lasts. Fuel 100, burned only while thrusting. Computed: 3 to 5 s of full thrust, top speed about 15. |
| Pushing magnet | M> | Leaves a field where the ship was. It stays there. Radius 3, lasts 5 s. Pushes enemies away, harder when close. |
| Pulling magnet | M< | The same field, but it pulls enemies in. |

Magnets move enemies only, not asteroids, shots or loot.

**Ranks.** Six: Common, Uncommon, Rare, Exclusive, Epic, Legendary. Only attack items have a rank. One random number picks it (`GL/Items/Infra/ItemRankPicker.cs:12`). Chances in `PG`: 70%, 20%, 7%, 2%, 0.7%, 0.3%. The rank selects a stat range and each stat is rolled inside it. This happens for the whole stock when the run starts, not at the drop.

**Stock** (`GL/Items/Infra/ItemsBank.cs:28`). Each item gets a fixed stock at run start, between a minimum and a maximum. A drop takes one. When an item runs out it is removed from every drop table (`DropsBank.cs:83`). Stock never refills.

| Item | Stock per run (`PG`) |
|---|---|
| Each of the ten attack items | 3 to 5 |
| Halalit Vitality | 30 to 40 |
| Claw Range / Halalit Immunity / Weapon Stamina | 5 to 7 / 3 to 5 / 1 to 3 |
| Nitro, Pushing magnet, Pulling magnet | 90 to 100 each |
| The other five upgrades | Not in the bank |

**Who drops what.** Two separate rolls happen on every enemy or asteroid death.

Valuables (`GL/Enemies/Base/EnemiesDestructor.cs:43`): the dropper's list is tried in order, and the first success wins. At most one valuable drops.

| Dropper | Tried in this order | Resulting chance |
|---|---|---|
| Any enemy | Silver 30%, Gold 30%, Diamond 30% | Silver 30%, Gold 21%, Diamond 14.7%, nothing 34.3% |
| Asteroid | Diamond 40%, Gold 20%, Silver 20% | Diamond 40%, Gold 12%, Silver 9.6%, nothing 38.4% |

Items (`GL/Items/Infra/ItemDropper.cs:70`): one number from 0 to 1 is rolled. Every item in the dropper's table whose chance is at least that number is a candidate, and one candidate is picked at random. So the chance that anything drops equals the best single chance in the table, not the sum.

| Dropper | Drop table in `PG` | Resulting chance per death |
|---|---|---|
| Asteroid | Shotgun 1 in 2, Boomerang 1 in 2 | 25% each |
| Greek, Sinus | Knockback Wave 1 in 3, Ball Shot 1 in 3 | 16.7% each |
| Lame | Knockback Wave 1 in 3, Ball Shot 1 in 2 | Wave 16.7%, Ball Shot 33.3% |
| The other six enemies | No table | Never drop items |

Each table also lists item number 19. Until February 2025 that number meant Nitro fuel. Commit `0887b39` then added `ITEM_BASE` to the item list ahead of it, so 19 now means `ITEM_BASE`, which is not in the bank, and the entry is skipped. Nitro fuel was probably meant to drop from these tables (see #118 in `docs/team/backlog-review-2026-11-20.md`). The skip assumes the bank starts before the drop table. Both run in `Start` with no set order, so it was not confirmed.

**Scene today:** no upgrade and no utility is in any drop table, so none can drop. Six of the ten attacks cannot be obtained. The three enemy types that do spawn have no table. In practice the only item drops are Shotgun and Boomerang from asteroids.

**Collecting.** Fly into the item, or use the claw (section 2). All ten weapon items use the same sprite (they inherit it from `PF/Items/attacks/Attack.prefab`). So do all upgrades, and all utilities. The player cannot tell which item it is, or its rank, before picking it up.

## 5. Enemies and hazards

Scripts are in `GL/Enemies/`, prefabs in `PF/Enemies/`. Every enemy has 7 health, does 1 damage per touch, shows a small health bar, and is knocked back by the ship, by other enemies and by asteroids. The arena wall stops all of them, and all but the two Following types steer away from it.

| Enemy | Behaviour |
|---|---|
| Lame | Drifts in one random direction at about 1 unit/s. No attack. |
| ZigZag | Moves in 1 s legs, turning 90° one way, then the other. |
| Sinus | Travels along one axis while swinging from side to side every 2 s. |
| Greek | A square-wave path: forward, sideways, forward, back sideways. 2 s per leg, with strong bursts. |
| Following (interval) | Spins for 3 s, then charges at where the ship was for up to 5 s. Repeats. |
| Following (radius) | The same charge, but only when 3 s have passed and the ship is within 10 units. |
| Shooting in range | Moves 3 s, aims 1 s, then fires one bullet (speed 10, 1 damage) roughly at the ship, within ±20°. |
| Laser asterisk | Moves 1 s, shows 8 aim lines for 1 s, then fires 8 lasers in a star for about 0.9 s. |
| Laser range | Moves 3 s, shows the two edges of a 40° cone for 1 s, then sweeps one laser across it for up to 3 s. |

**Enemy lasers.** Range 6 units, Power 1, Rate 0 (`PF/Attacks/EnemyAttacks/LaserBeam/EnemiesLaserBeam.prefab`). With Rate 0 the damage timer never moves forward (`ConsecutiveTriggerHarmer.cs:27`). By the code, a laser then deals 1 damage on every physics step the ship stays in it, up to 50 per second. The ship's 30 health would last 0.6 s. This is computed and needs a play check.

**Spawn system** (`GL/Enemies/Spawner/`).

- *Enemy bank.* A fixed count per enemy type. At run start the whole list is shuffled and cut into groups of 1 to 5.
- *Pacing.* A spawn hole opens every N seconds. N comes from a list, and the last value repeats. **Scene today:** one value, 5 s, so there is no ramp.
- *Where.* A random spot with nothing else within 7 units. If 10 tries fail, that hole is skipped and the next try comes 6 s later (`SpawnHoleInstantiator.cs:57`). Computed: this limits holes to a central band of about 26 × 7 units, and the ship, asteroids, enemies and uncollected loot all block spots.
- *The hole.* 1 s opening, 1 s open while the group grows out of it in a ring, 1 s closing. While they grow, enemies cannot hit or be hit.
- *The end.* When the bank is empty the spawner deletes itself (`SpawnHoleInstantiator.cs:47`). No more enemies come in that run.
- **Scene today:** the bank holds 5 Laser range, 5 Laser asterisk and 5 Shooting in range. The other six types are set to 0.

**Asteroids** (`GL/Asteroids/`).

- One stream per run. A random side is chosen at the start. All asteroids drift the same way at 2 units/s, spinning.
- One spawns every 3 s, up to 30 in total. Then the stream stops for good (`AsteroidExternalInstantiator.cs:43`). Computed: the spawn line is wider than the arena, so up to half of them can pass outside it.
- Size is a random scale from 1 to 5. Health is 5 at every size.
- They cannot be stopped. They shove the ship, enemies and loot. Enemies and loot pushed out of the arena are deleted. They do not damage the ship.
- Splitting: a destroyed asteroid of scale 2 or more (rounded) has a 50% chance to split into 2 to 5 pieces of half the scale, rounded down, fanned 40° apart. Scale-2 pieces can split once more.
- Asteroids bounce off each other and are removed when they leave the arena.

**Explosions.** The explosion on enemy death is visual only: no damage, no push (`PF/Explosions/Explosion.prefab` has no collider). The weapon explosion is described in section 3. It also spawns a shock wave object, but nothing reacts to that object's tag, so it pushes nothing (compare "Booms Knockback", To Do on Ofir's list).

**Who can hurt whom**, as stored in the prefabs and scene:

| Target | Damaged by |
|---|---|
| The ship | Enemy touch, enemy shots |
| Lame, ZigZag, Sinus, Greek, both Following | Player shots, Knockback Wave, explosions, enemy shots, asteroid hits |
| The three shooting enemies | Player shots, Knockback Wave |
| Asteroids | Player shots, Knockback Wave |

The last two rows hold tag numbers that no longer match the tag list (`PF/Enemies/MoveAimAttackEnemies/MoveAimAttackEnemy.prefab`, `PF/Asteroids/Asteroid.prefab` against `GL/UtilsAndModels/Models/Enum/Tag.cs`). Explosions were probably meant to hurt them too. An asteroid hit does 0.2 × its speed plus a little for spin, rounded down, which is 0 at normal speed.

## 6. The world

- **Arena.** One rectangle, 43.2 × 23.8 units, centred on the origin (the `InternalWorld` collider in `PG`). The ship starts in the middle.
- **Edge force fields** (`GL/World/EdgeForceFields/`). A strip 1.35 units wide runs inside each wall, with a square in each corner. They act on the ship only. A ship moving toward the wall is pushed back with a force of 60 plus 3.9 × its speed toward the wall (ship thrust is 15). Otherwise it gets a gentle push of 3 back into the arena. Corners push harder, in impulses of 90.
- **Walls.** Behind the strips is a solid wall for the ship and for enemies. Most shots, all loot and all asteroids pass through it and are deleted outside.
- **External safe island.** A 10 × 25 box outside the left wall. Its one rule in play: an enemy whose centre is outside the arena is deleted, unless it is in this box (`GL/Enemies/Base/EnemySharedBehavior.cs:7`). The PlayMode tests use the box as a staging area. As far as the colliders show, the player cannot reach it.
- **Game over, exactly.** The run ends when the ship's health is 0 or less after a hit (`GL/Health/Health.cs:199` and `:245`). `GL/Meta/GameOver.cs:52` then loads the main menu at once. That is the only live rule.
- **A switched-off rule.** Instant death when the ship's centre is outside the arena and not in the safe island is commented out (`GameOver.cs:37`). A test still expects an asteroid that crushes the ship into a corner to end the run (`Assets/Tests/PlayMode/PlayModeTests/Asteroid/AsteroidInteractionsTests.cs`). What happens today when an asteroid pins the ship against a wall could not be told from the code.

## 7. Score and meta

- **Valuables and score.** Silver is 10, Gold 20, Diamond 30 (`GL/Meta/Score/Score.cs`, values in `PG`). Score is the sum of collected valuables. Kills and time score nothing. The HUD shows "Score: N".
- **Main menu.** PLAY, DAILY, LDRBRD and EXIT, plus a high score line and a daily score line.
- **Online.** Anonymous sign-in to Unity Gaming Services. Cloud Save keys `HighScore` and `DailyScore_<date>`. One leaderboard, id "1".
- **Daily run.** The same scene as PLAY, with the random seed fixed for the date. The DAILY button is enabled only while today has no saved score, so the intent is one attempt per day (`GL/Meta/MainMenu.cs:25`).

Unfinished, confirmed in code:

1. **The score never leaves the run.** `Score.SetGameStats()` is the only code that hands the score to the menu, and nothing calls it. So no high score, daily score or leaderboard entry is ever written.
2. Even if it were called, a first-time player's score would be ignored. A score is compared with an empty high score, which is always false (`Score.cs:73`).
3. Daily seeds are a fixed list for 13 Dec 2024 to 1 Jan 2025 (`GL/Meta/PlayerStats.cs:154`). On any other date the seed is 0, so every daily is the same.
4. The seed does not cover everything. Enemy directions, drop throws, shotgun spread and the asteroid split roll use unseeded randomness (`GL/UtilsAndModels/Random/SeedlessRandomGenerator.cs`). Two players on the same daily get different runs.
5. The Leaderboard scene is not in the build list (`ProjectSettings/EditorBuildSettings.asset`), so the LDRBRD button cannot open it in a build. The list still names nine deleted test scenes.
6. There is no daily leaderboard and no way to enter a player name.
7. Death goes straight to the menu. There is no result screen.
8. The menu signs in on every load. By code reading, without network DAILY stays off and PLAY still works. Not tested.

Ofir's task file marks "Server: daily and leaderboard", "Level design" and "Move to phone" as To Do.

## 8. Numbers

All values are from `PG` or from prefabs under `PF/`.

**Ship and weapon**

| Value | Number |
|---|---|
| Health | 30 (the bar is scaled for 50) |
| Thrust / drag / mass | 15 / 2 / 1 |
| Ship speed limit / global speed cap | 15 / 20 |
| Fire threshold on the attack stick | 0.8 |
| Cooldown multiplier | 0.3 |
| Starting attack | Ball Shot, Power 1, Rate 1 (a shot every 0.3 s) |
| Knockback multiplier, ship / enemies | 3 / 1 |
| Claw: tap radius / reach / grab radius | 2 / 5 / 1 |
| Claw speed: out / back loaded / back empty | 3 / 2 / 6 |

**Weapon stats by rank** (`ItemOptions` in `PG`). C, U, R, Ex, Ep, L are the six ranks. CriticalHit, Luck and Weight are 0 everywhere.

| Attack | Power C | U | R | Ex | Ep | L | Rate, C to L |
|---|---|---|---|---|---|---|---|
| Ball Shot | 1-2 | 2-3 | 3-5 | 5-10 | 10-15 | 15-20 | 0.45-0.5 to 0.35-0.4 |
| Laser Beam | 1-2 | 1-3 | 1-4 | 2-5 | 3-6 | 3-7 | 0.4-0.5 to 0.22-0.27 |
| Knockback Wave | 1-2 | 2-3 | 3-4 | 4-5 | 5-6 | 6-7 | 0.9-1 to 0.5-0.65 |
| Boomerang | 1-2 | 2-4 | 4-7 | 7-12 | 12-17 | 17-20 | 0.65-0.7 to 0.4-0.5 |
| Mirror Ball Shot | 1-2 | 2-3 | 3-5 | 5-10 | 10-15 | 15-20 | 0.5-0.55 to 0.4-0.45 |
| Sword | 1-5 | 5-7 | 7-12 | 12-17 | 17-20 | 22-25 | 0.65-0.7 to 0.4-0.45 |
| Blast Shot | 0-5 | 5-12 | 12-19 | 19-27 | 27-35 | 35-40 | 1.2-1.3 to 0.8-0.95 |
| Grenade | 1-5 | 5-12 | 12-19 | 19-27 | 27-35 | 35-40 | 1.2-1.3 to 0.8-0.95 |
| Shotgun | 1-2 | 2-3 | 3-4 | 4-5 | 5-6 | 6-7 | 0.8-0.9 to 0.4-0.55 |
| Spikes | 1-5 | 5-8 | 8-13 | 13-17 | 17-25 | 25-30 | 0.65-0.7 to 0.4-0.45 |

Seconds between shots are Rate × 0.3. A Common Ball Shot fires about every 0.14 s, a Common Blast Shot about every 0.38 s. The starting Ball Shot is slower than any Ball Shot item. A Common Blast Shot can roll Power 0.

**Enemies.** All have health 7, touch damage 1, and the same valuables list.

| Enemy | In the bank today | Tuned values |
|---|---|---|
| Lame | 0 | Force 1, speed limit 1 |
| ZigZag | 0 | Force 2, 1 s legs, 90° turns |
| Sinus | 0 | Forward force 2 (limit 2), side force 0.5, 2 s swing |
| Greek | 0 | Force 10, 2 s per leg |
| Following (interval) | 0 | Wait 3 s, charge force 10 for up to 5 s |
| Following (radius) | 0 | Wait 3 s and ship within 10, charge force 10 for up to 5 s |
| Shooting in range | 5 | Move 3 s, aim 1 s, attack 1 s. Bullet speed 10, ±20° |
| Laser asterisk | 5 | Move 1 s, aim 1 s, attack 1 s. 8 beams, range 6 |
| Laser range | 5 | Move 3 s, aim 1 s, attack 3 s. 40° sweep, range 6 |

**Pacing and asteroids**

| Value | Number |
|---|---|
| Spawn hole interval | 5 s, a single value |
| Enemies per hole | 1 to 5 |
| Clear radius needed / tries | 7 / 10 |
| Hole phases: opening / open / closing | 1 s / 1 s / 1 s |
| Asteroid interval / total | 3 s / 30 |
| Asteroid speed / scale / health | 2 / 1 to 5 / 5 |
| Split chance / pieces / fan angle | 50% / 2 to 5 / 40° |

**Run size, computed.** If no spawn attempt fails, the last enemy group appears between about 15 and 75 s, and the last asteroid at about 90 s. The expected score is about 12 points per enemy killed and about 15 per asteroid destroyed, if every valuable is collected.

Not found: any number for difficulty over time, enemy health scaling, or a score multiplier. No such system exists.

## 9. Gaps in the player experience

**This section is the designer's opinion.** These are observations of what a player would likely miss today. It is not a plan.

- **No arc.** Content is finite and flat. The spawn interval never changes. After 15 enemies and 30 asteroids the arena is empty. A good player cannot finish a run. They have to die to leave.
- **No way back from damage.** No healing and no safe time after a hit. Computed, an enemy laser can empty the health bar in under a second.
- **Most of the build system is out of reach.** In the committed scene two weapons drop besides the starter, and no upgrades or utilities. Of nine upgrades one changes play. Critical hit, Luck and Weight are zero or unused.
- **Pickups are blind.** Items of a kind look the same and rank is never shown. A pickup replaces the weapon with no choice, so a Common item can overwrite a Legendary one.
- **The HUD says little.** It does not show the current weapon, its rank or its stats. Utility labels are two letters.
- **No onboarding.** Nothing explains the sticks, the 80% fire threshold, the tap claw or the utility button.
- **No sound.** There are no audio sources or audio clips in the game's own assets.
- **Thin feedback.** Some shots have muzzle and trail effects. I found no hit effect that would fire in 2D, no flash or shake when the ship is hurt, and no death sequence. Death cuts to the menu without a score summary. There is no pause.
- **Nothing to come back for.** No score is saved, the daily is the same every day, and the leaderboard cannot be opened.
- **Score rewards collecting, not fighting.** A kill is worth nothing unless the valuable is picked up. The claw brings loot back slowly and drops it if the ship moves away.
- **Phone fit is unproven.** The claw tap needs a third finger while both thumbs are on the sticks. The sticks are fixed in place. Thrust is weaker on diagonals.
- **Enemy variety is mostly unused.** Six of nine types never spawn. By the sprite data, most types share one sprite with different tints. New sprites are To Do on Ofir's list.
- **Asteroids are loot bags, not danger.** They do not damage the ship, and a huge one has the same 5 health as a small one.

## 10. Open questions for Ofir

Answered questions keep their number. Each shows Ofir's answer and its date.

1. Which platform is the first release for: Android phone only, or also iOS or PC?
   - **Answered (Ofir, 2026-11-21):** iOS first, Android later.
2. How long should one good run last: about 2, 5 or 15 minutes?
3. How should a run end: only by dying while it gets harder, or by clearing a set number of waves or levels?
4. Is the committed Playground setup (15 enemies of three types, four drop tables, 90 to 100 of each utility in stock) the game you want, or a debug state left from the laser work?
5. Who is the player: someone after a quick arcade session, or someone who wants builds and depth?
   - **Partly answered (Ofir, 2026-11-21):** friends playing a daily, with Spelunky-style depth (the vision above). Run length stays open (question 2).
6. What should the daily run be: one attempt per day on a shared seed with its own leaderboard, or something else?
   - **Partly answered (Ofir):** one random run a day, the same for every player, from one seed (2026-11-21). It has its own leaderboard (#172, approved 2026-11-22). The day follows Israel time (#204, 2026-11-25).
   - Still open: how many tries a day (as built, one), and who wins (question 13).
7. Should a weapon pickup always replace the current weapon, or should the player choose?
8. Should the ship be able to regain health during a run?
9. Should asteroids be able to kill the ship, by damage or by crushing it out of the arena? Today they only push.
10. Should score come only from collected valuables, or also from kills or time survived?
11. Are the seven upgrades that do nothing, and the Weight stat, still planned as written in your task file, or may we propose changes?
12. Will the first release have sound, and is there a reference game for the look and feel?
    - **Partly answered (Ofir, 2026-11-21):** the game was born from Spelunky, the model for its depth. Sound and the look stay open.
13. Who wins each day: the highest score or the last ship standing? It comes from the daily replay idea (#244) and is tied to questions 3 and 10.
    - **Open.** Ofir, 2026-11-27: "I don't have the answer for it." And: "I'm okay with just going with one decision, and worst case we can change it later."
    - **Working rule until he decides:** the highest score wins, as built.
    - *Highest score:* every pickup is a risk taken for points, as in Spelunky, whose daily ranks by money. A bold player can beat a careful one. But the last ship standing in the replay may lose the day.
    - *Longest survival:* matches the replay's crown. But caution wins, and valuables stop mattering, along with Coin Thief (#164) and Coin Toss (#198). It needs a run nobody can outlast: as built, a cleared arena is safe forever.
    - *Survival, with score breaking ties:* plays like pure survival, because times rarely tie.
    - *Score with a survival multiplier:* valuables are worth more the longer the ship lives, for example ×2 after 2 minutes (a starting value). Greed still scores, and the last ships in the replay have the most to gain. Coin Toss coins must not get the multiplier, or throwing and picking them up again farms score.
    - **Designer's lean, not a decision:** keep the highest score, show each ship's score in the replay, and add the multiplier if replays show that early deaths often win.

## 11. Ideas parked for proposal

An idea here is not approved. Nobody works on one until Ofir approves a written proposal, as happened for item 5.

1. Give a run a shape: a spawn schedule that ramps, and a result screen when it ends.
   - Approved as two cards, auto-approved 2026-11-27: #248 (endless ramp) and #249 (results card).
2. Make drops readable: an icon per weapon, a colour per rank, and a HUD tag for the current weapon.
3. Add a way to recover health, for example a rare heal drop or a Vitality that also heals.
4. Make the claw work without a third finger: collect automatically in range, or the homing claw already on Ofir's list.
5. Make the daily real: a seed derived from the date, a fully seeded run, and a daily leaderboard.
   - **Built in `develop`** (#159, #183, #184, #204, #172, #190). **Not run in Unity.**
6. Daily replay video ([#244](https://github.com/ofirschwartz2/halalit/issues/244)), Ofir's idea, 2026-11-27: everyone's ship in one arena, dropping out as each player died, the last ship standing crowned. Who wins is question 13. Step 1 (the in-game replay) is approved, auto-approved 2026-11-27. The share-as-video step is a later offer.
7. Show my place in each daily ([#245](https://github.com/ofirschwartz2/halalit/issues/245)), Ofir's idea, 2026-11-27: for example "12th of 40". Approved, auto-approved 2026-11-27.
8. NFT pilot ([#246](https://github.com/ofirschwartz2/halalit/issues/246)), Ofir's idea, 2026-11-27: a pilot NFT, visible in a see-through cockpit. Parked until after Milestone 1. On iOS an NFT must stay cosmetic (App Store rule 3.1.1).
