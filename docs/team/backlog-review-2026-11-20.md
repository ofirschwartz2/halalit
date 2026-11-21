# Halalit backlog review, 2026-11-20: candidates for Ofir

**Nothing in this file is approved.** Every row is a candidate. Ofir approves or rejects each one.

**Team lead note, 2026-11-21.** This file is a dated snapshot and is not kept up to date. The live backlog is the GitHub issues. Pull request #150, opened by a Cursor agent on 2026-11-20, already adds a GameCI test workflow, so NEW-03 should start from that file.

Written 2026-11-20 by the product manager agent. Sources: "Halalit tasks.txt" (now GitHub issues #115 to #149), the team lead's review notes, and TODO comments in `Assets/GameLogic`. Each item was checked against the files at commit `e8b1c95`. Nobody has run Unity, so nothing here was tested in play.

**IDs.** `#NNN` is an existing GitHub issue from Ofir's list. Its title is kept. `NEW-NN` comes only from the review or a TODO. It has no issue yet and needs one created once Ofir approves it. IDs are never reused.

## 1. How to read this

- An item moves through: candidate → approved → in progress → waiting for Unity test → done.
- Only Ofir moves an item from candidate to approved. Only Ofir merges.
- Size: S = under about 50 changed lines in one area. M = one feature area. L = crosses assemblies or needs design first.
- Risk = the risk of doing the work without a Unity test. Source = tasks.txt, review or TODO.

## 2. Can start now (no Unity needed to do the work)

Ranked by value to a playable, shippable game, then by risk. Paths are under `Assets/GameLogic` unless shown in full.

| ID | Title | Type | Why it matters | Size | Risk | Source |
|---|---|---|---|---|---|---|
| NEW-01 | Player build cannot compile: editor-only code in runtime assemblies | bug | Four places use `UnityEditor` outside editor-only code. No phone build is possible until fixed. Blocks #122. | S | low | review |
| NEW-02 | Score never reaches `PlayerStats` at game over | bug | `Score.SetGameStats()` has no caller. High score, daily score and the leaderboard never update. Part of #124. | S | low | review |
| NEW-03 | CI on GitHub Actions: run the 101 tests and an Android build on every PR | infra | The team cannot run Unity. CI gives each change a compile and test result. Needs a Unity licence secret from Ofir. | M | low | review |
| NEW-04 | Build scene list is stale | bug | It lists 9 deleted test scenes and leaves out Leaderboard, so the LeaderBoard menu button cannot open its screen. Part of #124. | S | low | review |
| #118 | Utilities | bug | Nitro stopped dropping with commit `0887b39` (2025-02-23). An enum insert moved it from 19 to 20; the drop table still says 19. The two magnets are in no drop list. | S | medium | tasks.txt |
| NEW-05 | Daily run gets the same seed every day | bug | Seeds are hard-coded for 13-12-24 to 01-01-25 only. Every other date gets seed 0. Part of #124; see question 1. | S | low | review |
| NEW-06 | Remove per-tick logs and the access-token log | cleanup | Three log lines per enemy per physics tick, one more per tick while moving. That costs frame time, most of all on a phone. The sign-in token is printed to the log. | S | low | review |
| NEW-07 | Sign-in setup repeats on every menu load | bug | `Authentication.InitializationTask` is never set, so `Initialize()` runs twice per menu load and stacks event handlers. Marked `// TODO: FIX`. Part of #124. | S | low | review, TODO |
| NEW-08 | Knockback wave damages only its first target | bug | The single-hit check records the wave instead of the victim (`Health/Health.cs:170`). Every later target takes 0 damage. | S | medium | review |
| NEW-09 | Event bus calls a stale copy of its listeners | bug | Objects created after scene start never get events; destroyed ones still do. Must ship with a "player only" check in `Health`, or every enemy gains max HP on a vitality pickup. | M | medium | review |
| NEW-10 | Repo clutter | cleanup | 478 leftover `InitTestScene` files, 3 `.DS_Store`, dead code in `World/RecycleBin`, `app.config`, `packages.config`. Add ignore rules so they stay out. | S | low | review, TODO |
| NEW-11 | Daily run is only partly seeded | design | Enemy directions, critical hits, shotgun spread and drop forces use the unseeded generator. Two players on one daily seed get different runs. Ofir decides what must match. | M | medium | review |
| NEW-12 | Attack stats are written onto prefab assets | cleanup | `AttackToggle` writes stats into the prefab itself. State carries over between runs and dirties prefabs in the editor. | M | medium | review |
| NEW-13 | Utility code drift | cleanup | The two magnet classes are identical after renaming. `UtilityManager` switches on type. Nitro keeps its on/off state on the shared config asset. | M | medium | review |
| NEW-14 | README and a one-page design note | docs | The repo has neither. Nothing explains how to open, test or build the game. | S | low | review |
| #117 | Tests | infra | 101 tests exist. None cover enemy behaviour, the score handoff or the drop table. New tests prove nothing until NEW-03 can run them. | M | high | tasks.txt |
| NEW-15 | Small leftovers | cleanup | `GetInsideUnitCircle` returns a point in a square. `ShiftVectorByOffsetDegree` mixes degrees and radians (unused). The Meta assembly is still named `NewAssembly`. Helpers marked `TODO: DELETE`. | S | low | review, TODO |

## 3. Needs Unity (editor work, scenes or prefabs, play-feel, art, builds)

Same ranking. Risk here means "if we tried it blind". High means wait for Unity.

| ID | Title | Type | Why it matters | Size | Risk | Source |
|---|---|---|---|---|---|---|
| #123 | Level design | design | The only game scene is a test setup: 15 enemies (three shooting types; six other types set to 0), 30 incoming asteroids, then nothing spawns. Only death ends a run. | L | high | tasks.txt |
| #122 | Move to phone | feature | Still open after NEW-01: Android settings are defaults (`com.DefaultCompany.Halalit`, ARMv7 only; Google Play needs 64-bit). The claw fires on mouse input read in `FixedUpdate`; with two thumbs down a tap may not register (unconfirmed). | L | high | tasks.txt |
| NEW-16 | Upgrade Unity from 2022.3.3f1 to 2022.3.62f2 or later | infra | 2022.3.3f1 predates the fix for CVE-2025-59489. Unity says builds made with older editors are vulnerable. Do this before any release build. | S | high | review |
| #124 | Server: daily challenges and leaderboard | feature | Left after NEW-02, 04, 05 and 07: decide the scope (question 1), set up the UGS dashboard, test with a real account. | M | high | tasks.txt |
| #115 | Attack stats + game calibration | design | A balance pass by play. Also: 7 of 9 upgrade pickups have no code, no upgrade is in any drop list, and 7 of 10 attacks can never be reached in the current scene. | L | high | tasks.txt |
| #130 | Add weight to each attack (movement delay and shakiness) | feature | `Weight` is rolled for every item and never read. The weapon snaps straight to the stick. Do it before #129 and #131. | M | high | tasks.txt |
| #129 | Add dexterity to the firearm (counter to shakiness) | feature | The `WEAPON_DEXTERITY` pickup exists. Nothing handles it. Depends on #130. | S | high | tasks.txt |
| #131 | Add strength to the firearm (counter to movement delay) | feature | The `WEAPON_STRENGH` pickup exists. Nothing handles it. Depends on #130. | S | high | tasks.txt |
| #143 | Booms knockback | feature | No explosion tag is in any knockback list (prefab data). Push strength comes from the source's speed, which is 0 for a blast. Needs code, prefab edits and tuning. | M | high | tasks.txt |
| #148 | Fix enemies | bug | The list gives no detail (question 8). Needs a play session to name the problems. | M | high | tasks.txt |
| #149 | Laser is not destroyed with the enemy | bug | Both laser enemies already destroy their beams in `OnDestroy` (added 2024-02-02). Needs a play check. It may only need closing. | S | high | tasks.txt |
| #141 | Make the pickup claw homing | feature | The claw already steers to its target on every physics tick. Ofir must say what "homing" should add. | M | high | tasks.txt |
| #142 | Make a bat claw | feature | Nothing in code or assets describes it (question 7). | M | high | tasks.txt |
| #134 to #140 | Booms shaders / particle systems; Boomerang, Sword, Shotgun, Grenade, Knockback wave, Spikes shader / particle system | design | Seven issues. Made in the Unity editor and judged by eye. | M each | high | tasks.txt |

CVE source for NEW-16: Unity security advisory, https://unity.com/security/sept-2025-01

## 4. Top 5 to approve first

All five are valuable and safe to write without Unity. Each stops at "waiting for Unity test" until Ofir or CI runs it.

### 1. NEW-01: Player build cannot compile

*Why first:* nothing reaches a phone until this compiles, and it is the smallest and safest change here.

- **Problem.** Four places use editor-only code in assemblies that are built for every platform. The editor hides it. Any player build stops at script compilation.
- **What will change.**
  - `UtilsAndModels/Models/DataObject/MinMaxRange.cs`: put `using UnityEditor` and `MinMaxRangeDrawer` inside `#if UNITY_EDITOR`.
  - `UtilsAndModels/Utils/EditorUtils/JsonToInspector.cs`: remove the unused `using UnityEditor`.
  - `Setup/OriginalRandomSeed.cs` line 31: guard the call the same way as the method it calls (lines 56 to 73).
  - `Assets/External/Joystick Pack/Scripts/Editor/` (4 files): wrap each in `#if UNITY_EDITOR`. Today they compile into the all-platform `External` assembly.
  - `BoomerangTests.cs`, `SwordTests.cs`: drop the unused `using UnityEditor.VersionControl`.
- **Done means.**
  1. No script in an all-platform assembly names `UnityEditor` outside an editor guard.
  2. Nothing changes in the editor: the Min/Max drawer still shows and all 101 tests behave as before.
  3. A player build gets past script compilation.
- **Verify later in Unity.** Run all tests in the Test Runner. Then build for Android from Build Settings. Or let NEW-03 do both.
- **Size** S (about 20 lines in 9 files, guards only). **Risk** low. More compile errors may hide behind these four. Only a first build will show them.

### 2. NEW-02: Score never reaches `PlayerStats` at game over

*Why second:* score is the only goal in the game and none is ever recorded. The fix is about 20 lines in one file.

- **Problem.** `Score.SetGameStats()` (`Meta/Score/Score.cs:56`) is never called from code, a scene or a prefab. `GameOver` loads the menu directly. Also `_score > PlayerStats._highScore` (line 73) is always false for a new player, because `_highScore` is null.
- **What will change.** `Meta/Score/Score.cs` only: listen to `GameOverEvent.GameOver` and call `SetGameStats()` there. Treat a null high score as "no score yet". No scene or prefab change.
- **Done means.**
  1. After a run, the menu shows the new high score when it beats the old one. A new player's first score is stored.
  2. After a daily run, the menu shows the daily score and the Daily button is off for that day.
  3. A new high score reaches `PlayerStats.SavePlayerHighScoreAsync` (Cloud Save and leaderboard "1").
  4. One new PlayMode test: pick up valuables, die, check `PlayerStats`.
- **Verify later in Unity.** Play one normal run and one daily run while signed in. Check the menu texts and the UGS dashboard.
- **Size** S. **Risk** low. Unknown: whether leaderboard "1" exists in the UGS dashboard. The repo cannot show that.

### 3. NEW-03: CI on GitHub Actions

*Why third:* it gives every later change a compile and test result without anyone opening Unity.

- **Problem.** No agent can compile or run tests. Ofir is the only test loop. The repo has no `.github` folder.
- **What will change.** One new file, `.github/workflows/ci.yml`, built on the GameCI actions. Job 1 runs the EditMode and PlayMode tests. Job 2 builds an Android player. No game file changes.
- **Needs from Ofir, once.** Add Unity licence secrets to the repo. GameCI documents the steps.
- **Done means.**
  1. Every pull request shows pass or fail for the tests and for the Android build.
  2. The test report is attached to the run, with the seed that `AssertWrapper` prints on a failure.
  3. Merging stays Ofir's decision. No branch rules change.
- **Verify.** It verifies itself on its own pull request. Expect the Android job to fail until NEW-01 is merged. That failure confirms NEW-01.
- **Size** M. **Risk** low for the game. Unknowns: whether GameCI has an image for 2022.3.3f1, run time with 247 MB of third-party assets, and how the PlayMode tests behave on a headless runner.

### 4. NEW-04: Build scene list is stale

*Why fourth:* one settings file. It makes the Leaderboard screen reachable, so NEW-02 becomes visible to players.

- **Problem.** `ProjectSettings/EditorBuildSettings.asset` lists MainMenu, Playground and 9 scenes under `Assets/Scenes/Tests/` that were deleted. `Leaderboard.unity` is missing. `MainMenu.LeaderBoard()` loads it by name, and that only works for listed scenes.
- **What will change.** That one file: remove the 9 dead entries and add `Assets/Scenes/Leaderboard.unity` with the guid from its `.meta` file.
- **Done means.**
  1. The list is MainMenu, Playground, Leaderboard, in that order. Every path exists and every guid matches.
  2. The LeaderBoard button opens the screen. Back returns to the menu.
  3. Tests still load Playground by name.
- **Verify later in Unity.** Open Build Settings: three scenes, none marked deleted. Press Play in MainMenu, click LeaderBoard, then Back.
- **Size** S. **Risk** low.

### 5. #118: Utilities

*Why fifth:* it brings back a feature that broke silently in 2025. The change is one enum and four numbers.

- **Problem.** Commit `0887b39` inserted `ITEM_BASE` into `ItemName` before `NITRO_FUEL`, so nitro moved from 19 to 20. The drop table in `Playground.unity` still says 19, which now means `ITEM_BASE`. That item has no stock, so `DropsBank` skips it. The two magnets were never in the drop table. No utility can drop in a real run. Tests miss this because they spawn items directly.
- **What will change.**
  - `UtilsAndModels/Models/Enum/ItemName.cs`: give all 23 members explicit numbers equal to today's values. A later insert can then never shift saved data.
  - `Assets/Scenes/Playground.unity`: in `_allDefaultDropsList`, four entries `ItemName: 19` become `20`. A text edit of four numbers.
  - Not included: drop chances for the magnets. They need Ofir's numbers (question 4).
- **Done means.**
  1. Every `ItemName` member keeps its number (0 to 22).
  2. Asteroids drop nitro fuel again, with the chance already set in the scene.
  3. A new test fails when the drop table names an item that the item bank does not stock.
- **Verify later in Unity.** Play and break asteroids until nitro drops. Pick it up. The utility button shows "NF". Press it: speed rises and the engine fire changes colour.
- **Size** S. **Risk** medium. This is the only scene-file edit among the five. The enum change alone is low risk.

## 5. About the imported issues

35 issues, #115 to #149, were imported today from "Halalit tasks.txt". That file was last edited on 2024-02-02, so some issues describe work that is already in the code.

- **What the team can do without Unity.** No issue can be finished without Unity. Code-only parts can start: #118 (proposal 5), #124 (as NEW-02, NEW-04, NEW-05, NEW-07), #122 (as NEW-01 and NEW-06), #117 (writing tests; they run once NEW-03 exists).
- **Unity editor or art work.** Shaders and particle systems: #116, #133 to #140. Sprites: #121, #144 to #147. Feel and balance by play: #115, #129 to #131, #143. Device work: #122. Design first: #123, #132.
- **Look done in code. Check in play, then close.** #126 (critical hit and luck are applied in `AttackBehaviour.GetHarm`; PR #103). #127 (rate sets the cooldown in `WeaponAttack`; PR #97). #149 (see table).
- **Containers.** #116, #119, #120, #121 and #133 only group other issues.
- **Wrong label.** #118 is labelled `infra` and its text speaks of "utility code". In this game utilities are player pickups (nitro fuel, magnets). It should be `gameplay`.
- **`ai-ready` labels I disagree with: all 17.** None can be built and accepted without Unity or an answer from Ofir.
  - #125: "capability" has no definition. The "capability stat" in the issue text is the import tool's guess.
  - #126, #127: already in the code. Nothing to build.
  - #128: stamina already exists as a pickup that shortens the cooldown (`Player/Weapon/WeaponAttack.cs:209`). The issue asks for a value per firearm, which is a different design.
  - #129, #130, #131: the code can be written, but this is aim lag and shake. It cannot be tuned or accepted without playing.
  - #135 to #140: shader and particle work is done in the Unity editor and judged by eye. For #138: two grenade effect prefabs exist (`my_grenade_vfx_*`) and nothing uses them.
  - #141: the acceptance text is already true. The claw steers to its target every tick.
  - #142: no code or asset says what a bat claw is.
  - #143: needs prefab edits, tuning, and Ofir's word on which explosions push what.
  - #149: probably fixed in 2024. Needs a play check, not a code change.

## 6. Questions for Ofir about his own list

1. **#124, "10. Server: daily and leatherboard".** For the daily run, which do you want: (a) the seed is computed from the date on the device and the score is saved per player, as today; or (b) the seed and a daily leaderboard come from the server?
2. **#123, "9. Level design".** The game scene today is a test setup (see #123 in section 3). Do you want separate levels that end, or one endless run that gets harder?
3. **#122, "8. Move to phone".** Which stores for the first release: Google Play only, or the App Store too?
4. **#118, "4. Utilities".** Nitro fuel, pushing magnet and pulling magnet are merged. We assume no more are planned. Which objects should drop the two magnets, and how often (for example 1 in 3 from asteroids)?
5. **#115, "1. Attacks stats + calibration of all other staff in the game to it."** Only the `HALALIT_VITALITY` and `WEAPON_STAMINA` pickups do anything. `HALALIT_IMMUNITY`, `HALALIT_VIGOR`, `CLAW_RANGE`, `CLAW_AGILITY`, `CLAW_STABILITY`, `WEAPON_DEXTERITY` and `WEAPON_STRENGH` have no code. Which of these seven must work in the first release?
6. **#125, "1.2. Add the capability of the fire arm".** What is "capability"? One example of what changes for the player is enough.
7. **#142, "5.2. Make a bat claw".** What does the bat claw do that the pickup claw does not?
8. **#148, "fix enemies".** Which enemies, and what goes wrong? One line per problem is enough. The list was last edited in a commit named "- Attack stats completed (power) - All noticable bugs fixed", so this may be out of date.

## 7. Not for the agents

- **Sprites** (#121, #144 to #147). Drawing is for Ofir or an artist. Agents can wire finished art in Unity later.
- **Shaders and particle effects** (#134 to #140). Built and judged in the Unity editor. Ofir or a VFX artist.
- **Game feel and balance** (#115, and tuning for #129 to #131 and #143). Ofir plays and picks the numbers. Agents change the code.
- **Unity and UGS accounts.** Licence secrets for NEW-03, the editor install for NEW-16, the UGS dashboard (leaderboard "1", Cloud Save). Ofir.
- **Stores.** Google Play and App Store accounts, signing keys, the real company name and package id (still `DefaultCompany`). Ofir.
- **Testing on a real phone.** Ofir.
- **Asset Store packs in a public repo.** `Assets/External/GabrielAguiarProductions` (two packs) is committed and the repo is public. The review notes say the packs are paid. The standard Asset Store licence does not allow sharing pack files publicly (please check). Only the owner can make the repo private. Ofir decides: private repo, or we remove the packs. The finished attack effects depend on them. On a private repo, CI (NEW-03) uses GitHub's monthly free minutes.
- **Approving and merging.** Ofir, always.
