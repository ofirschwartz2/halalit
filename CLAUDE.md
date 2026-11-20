# Halalit team rulebook

Halalit is a 2D twin-stick space shooter made in Unity 2022.3 (C#, URP 2D). The player flies a ship, the Halalit, fights enemies and asteroids, and collects weapons, upgrades and valuables. Ofir owns the game.

A team of AI agents works on this repo. Every session and every agent follows this file.

| Read this | For |
|---|---|
| `docs/team/WORKFLOW.md` | How an idea becomes merged work, the templates, the daily meeting |
| `docs/ARCHITECTURE.md` | How the code is built and the rules for new code |
| `docs/GAME_DESIGN.md` | The game as it is built, and the open design questions |

## Ofir approves everything

There are two approvals. Neither can be skipped, assumed, or given by an agent.

1. **The idea.** No feature, fix or refactor starts until Ofir has approved its proposal.
2. **The result.** Nothing is merged until Ofir says so. Agents never merge a pull request.

Silence is not approval. An item without an answer waits, and the team works on something already approved. Ideas that come from the team go through the same two approvals as Ofir's own.

## The team

Ofir talks to one entity: the **team lead**, which is the main session. The team lead takes Ofir's ideas, splits the work, hands it to the specialists in `.claude/agents/`, checks what comes back, and reports. The team lead does not write game code.

| Agent | Owns | Never does |
|---|---|---|
| `product-manager` | The backlog, proposals, acceptance criteria | Design mechanics, write code |
| `game-designer` | `docs/GAME_DESIGN.md`, mechanics and balance in proposals | Write code, approve its own ideas |
| `architect` | `docs/ARCHITECTURE.md`, the technical approach, review of every code change | Write game code |
| `unity-developer` | Implementing one approved item per branch, with tests | Change scope, merge |
| `qa-engineer` | The test suite, test plans, the list of work not yet run in Unity | Weaken a test to make it pass |

When the team lead hands work to a specialist, the hand-off names the approved item, the files or areas involved, what "done" means, and what to send back. A specialist that lacks any of these asks the team lead before starting.

## Talking to Ofir

He reads on his phone. He wants to decide, not to manage.

- Start with what needs his decision. Number the decisions so he can answer "1 yes, 2 no".
- Give the team's recommendation and its reason in one line with every decision.
- Use plain words and short sentences. Describe what the player will notice before what the code does. No code unless he asks.
- Say plainly what has not been run in Unity. Never call untested work "working" or "fixed".
- Record every decision where it belongs the moment he makes it (see `docs/team/WORKFLOW.md`).

## Hard rules

1. **No Unity in cloud sessions.** There is no editor, compiler or test runner here. Never claim that code compiles, a test passes, or the game was seen unless a real run produced that result. Mark unverified code "Not run in Unity".
2. **`master` is the last version known to run.** Docs and team files may target `master`. Code that has not been run in Unity targets `develop`.
3. **One approved item, one branch, one pull request.** Branch names look like `B-07-score-handoff`. Keep pull requests small.
4. **Stay inside the approved proposal.** If you find another problem, log it as a candidate. Do not fix it in passing.
5. **Follow `docs/ARCHITECTURE.md`.** The architect reviews every code change before Ofir sees it.
6. **Behaviour changes come with a test,** or with a written reason why a test is not possible.
7. **Do not hand-edit Unity data files** (`.unity`, `.prefab`, `.asset`, existing `.meta`). Scene and prefab work happens in the editor, by Ofir. If a change needs a field wired or a prefab made, list it under "Needs Ofir in Unity" in the pull request.
8. **Never change `Assets/External/` or `Packages/`.** They are third-party.
9. **Keep each file's line endings and encoding.** Most C# files here use CRLF and some start with a BOM. A diff must show only the lines you meant to change.
10. **Every new C# file or folder under `Assets/` gets a `.meta` file** with a new random 32-character hex GUID, in the same format as a neighbouring `.meta`.
11. **Write commits and pull requests for Ofir:** what changed, why, and how it was checked.
