# How the Halalit team works

Ofir gives ideas and approves. The team does everything else. This file is the process. Ofir approved it on 2026-11-21.

## Three places

- **The team lead chat.** Ofir talks only to the team lead. Ideas go in there and decisions come back there.
- **The board.** A page in Claude that shows every card by stage. Ofir approves on it with a tap. The team lead updates it at every meeting.
- **GitHub.** The record: one issue per card, one pull request per change. The board is a view of it. When the two disagree, GitHub is right and the team lead fixes the board.

## Stages

Every card is a GitHub issue. Its stage is a label. An open issue with no stage label is an idea.

| Stage | Label | Meaning | Who moves it on |
|---|---|---|---|
| Ideas | none | Logged, not yet shaped | Product manager or game designer, by writing the offer |
| Offered | `offered` | A one-screen offer waits for Ofir's yes or no | Ofir only |
| Approved | `approved` | Ofir said yes. Queued by rank | Team lead, by handing out the work |
| Building | `building` | A specialist holds it. The card names the agent | Developer, QA, architect |
| Your review | `your-review` | Built and reviewed. The pull request waits for Ofir's OK | Ofir only |
| Waiting for Unity | `waiting-for-unity` | Merged to `develop`, not yet run in Unity | QA, once it has run |
| Done | closed | Ran in Unity. A rejected card is also closed, and its closing comment says so | Team lead |

Every decision Ofir makes is written as a comment on the issue or pull request it belongs to, with the date and his words. That comment is the record.

## Shelves

Every card sits on one shelf, which is also a label. Ofir's goal is many weapons, enemies and behaviors, so those are the first three shelves.

| Shelf | Label | Owner | What goes on it |
|---|---|---|---|
| Weapons | `weapon` | Game designer | A new player attack |
| Enemies | `enemy` | Game designer | A new enemy type |
| Behaviors | `behavior` | Game designer | A new way for existing things to act |
| Foundations | `foundation` | Product manager | Fixes, cleanup, tests, tools, docs |

The game designer keeps about five fresh offers on the first three shelves at all times.

## Approval 1: the offer

An offer fits on one phone screen:

> **Name** (2 or 3 words) and shelf.
> **What the player gets:** at most 2 short sentences.
> **Why it is different:** 1 sentence.
> **How it works:** up to 4 bullets with starting numbers.
> **Interactions:** 2 or 3 specific ones with a named enemy, weapon or place, each marked "works today" or "needs code".
> **Needs Ofir in Unity:** numbered steps, or "Nothing".
> **Size:** S, M or L. **Risk without Unity:** low, medium or high.
> **Feels right when:** 1 line Ofir can check by playing.

A Foundations offer says "What you get" and "Done means" (2 to 4 checks) in place of the player lines.

Ofir's vision is Spelunky-style depth (see `docs/GAME_DESIGN.md`). An offer that is an existing thing with different numbers is not an offer.

Ofir answers on the board or in chat: yes, no, or "change this". The architect then adds the technical approach as a comment for the developer. Ofir does not need to read it.

## Build and review

1. The team lead hands the approved card to the `unity-developer`, with the offer and the architect's approach.
2. The developer works on one branch and opens one pull request.
3. The `qa-engineer` adds or checks the tests and writes the Unity test plan.
4. The `architect` reviews the change against `docs/ARCHITECTURE.md` and the offer. If it fails, it goes back to the developer. Ofir is not asked until it passes.

## Approval 2: the result

The pull request description is written for Ofir:

> **What changed:** what the player or the project gets, in two or three lines.
> **Card:** link to the issue.
> **How it was checked:** tests added, the review verdict, and "Not run in Unity" when that is the case.
> **Needs Ofir in Unity:** anything to wire, build or look at, as numbered steps. "Nothing" if nothing.
> **Risk:** what could be wrong.

Ofir answers "merge" or "fix this", on the board or in chat. The team lead then merges. Specialists never merge.

Standing permission (Ofir, 2026-11-21): the team lead may merge team files, docs and repo housekeeping without asking each time, and reports each merge in the next brief. Game code and Unity data always wait for Ofir's OK.

## Working without Unity

Right now the team cannot run Unity, and Ofir has asked it to work anyway and test later. These rules keep that safe.

- `master` stays the last version known to run. Only docs, team files and housekeeping go straight to `master`.
- Code pull requests target `develop`. Ofir's approval there means "this is what I asked for". It does not mean "this works".
- Every code pull request says "Not run in Unity". After the merge, its card moves to Waiting for Unity.
- The team prefers work that is low-risk without a compiler: removing dead files, small fixes with clear call sites, guards, tests, docs. Large refactors wait for Unity.
- When Unity is available, on Ofir's PC or on GitHub's test machines, the QA engineer runs the whole suite on `develop` and the team fixes what fails. Then Ofir approves one pull request from `develop` to `master`.

## The team meeting

Once a day, at 08:36 Israel time, a scheduled task wakes the team lead chat. The team lead first schedules the next day's meeting, then runs the playbook and ends with the brief. Ofir can also ask for a meeting at any time.

Every Sunday the meeting includes the roster check in `docs/team/ROSTER.md`. The team grows and shrinks through offers that Ofir approves.

## Team lead playbook

1. **Sync.** Read Ofir's decisions on the board and in chat. Read the open pull requests and the issues by stage label.
2. **Record.** Write each decision on its issue or pull request, with the date and his words. Move the labels. Merge what he approved.
3. **Check in.** Ask each specialist for its update and hand out the next work: `product-manager` (the backlog and Foundations offers), `game-designer` (the menu), `architect` (approaches and reviews), `unity-developer` (builds), `qa-engineer` (tests and the Waiting for Unity list).
4. **Work.** Take approved cards in rank order through build and review. Stop a card at the first thing that needs Ofir.
5. **Update the board.** Every card's stage, owner and links match GitHub.
6. **Report.** Send the brief, and add it as a comment on the "Daily meeting log" issue.

The team lead checks what comes back from a specialist before passing it on. It reads the diff or the document, and it does not repeat a claim it has not checked.

## The brief

> **Done:** what finished since the last meeting.
> **Needs your decision:** numbered, each with the team's recommendation.
> **Blocked:** what is stuck, and what would unblock it.
> **Next:** what the team does until tomorrow.

- Keep it to half a phone screen. Ofir asked for short and precise writing on 2026-11-21.
- If nothing needs him, say so in one line.
- He can answer with numbers: "1 yes, 2 no, 3 change the name".

## What Ofir does

- Gives ideas and answers decisions.
- Does the Unity editor work the team lists for him: wiring fields, prefabs, scenes, art.
- Plays the game. Only he can say how it feels.

## What the team never does

- Start unapproved work, or widen approved work.
- Merge without Ofir's OK, apart from the standing permission above.
- Describe work that was not run in Unity as working.
- Leave a decision unrecorded.
