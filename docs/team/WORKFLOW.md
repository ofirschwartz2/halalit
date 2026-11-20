# How the Halalit team works

Ofir gives ideas and approves. The team does everything else. This file is the process.

## The flow

```
Ofir's idea --+
              +--> Candidate --> Proposal --> [ Ofir approves the idea ]
Team's idea --+                                        |
                                                       v
   Merged <-- [ Ofir approves the result ] <-- Review <-- Build
```

A "no" at the first approval closes the item. A "fix this" at the second sends it back to Build.

## One front door

Ofir talks only to the team lead, in chat. He can drop an idea at any time, in any words. The team lead answers, logs the idea as a candidate, and from then on it follows the flow. Ofir never has to find or instruct a specialist.

## States

Each item is a GitHub issue, and its label is its state.

| Label | Meaning | Who moves it on |
|---|---|---|
| `candidate` | Logged, not yet shaped | Product manager, by writing the proposal |
| `needs-ofir` | A proposal or a result is waiting for his answer | Ofir only |
| `approved` | Ofir said yes to the idea | Team lead, by assigning the work |
| `in-progress` | Being built or reviewed | Developer, QA, architect |
| `needs-unity-test` | Merged to `develop`, not yet run in Unity | QA, once it has been run |
| closed | Done, or rejected. The closing comment says which. | Team lead |

Every decision Ofir makes is written as a comment on the issue or pull request it belongs to, with the date and his words. That comment is the record.

## Approval 1: the proposal

A proposal fits on one phone screen:

> **B-07 Score is lost at game over**
> **What the player gets:** one or two sentences.
> **Why now:** one sentence.
> **What changes:** the parts of the game and of the code, in plain words.
> **Done means:** 2 to 4 checks.
> **Size:** S, M or L. **Risk without Unity:** low, medium or high.
> **Team recommendation:** approve, reject or wait, with the reason in one line.

The product manager owns the proposal. The game designer writes "what the player gets" for anything that changes how the game plays. The architect adds the technical approach as a separate comment for the developer. Ofir does not need to read that comment.

Ofir answers yes, no, or "change this".

## Build and review

1. The team lead hands the approved item to the `unity-developer`, with the proposal and the architect's approach.
2. The developer works on one branch and opens one pull request.
3. The `qa-engineer` adds or checks the tests and writes the Unity test plan.
4. The `architect` reviews the change against `docs/ARCHITECTURE.md` and the proposal. If it fails, it goes back to the developer. Ofir is not asked until it passes.

## Approval 2: the result

The pull request description is written for Ofir:

> **What changed:** what the player or the project gets, in two or three lines.
> **Proposal:** link to the issue.
> **How it was checked:** tests added, the review verdict, and "Not run in Unity" when that is the case.
> **Needs Ofir in Unity:** anything to wire, build or look at, as numbered steps. "Nothing" if nothing.
> **Risk:** what could be wrong.

Ofir answers "merge" or "fix this". Only then is it merged.

## Working without Unity

Right now the team cannot run Unity, and Ofir has asked it to work anyway and test later. These rules keep that safe.

- `master` stays the last version known to run. Only docs and team files go straight to `master`.
- Code pull requests target `develop`. Ofir's approval there means "this is what I asked for". It does not mean "this works".
- Every code pull request says "Not run in Unity", and its issue gets the `needs-unity-test` label.
- The team prefers work that is low-risk without a compiler: removing dead files, small fixes with clear call sites, guards, tests, docs. Large refactors wait for Unity.
- When Unity is available, on Ofir's PC or on GitHub's test machines, the QA engineer runs the whole suite on `develop` and the team fixes what fails. Then Ofir approves one pull request from `develop` to `master`.

## The daily meeting

Once a day the team lead sends Ofir a brief. It always has the same four parts and fits on one phone screen:

> **Done:** what finished since the last meeting.
> **Needs your decision:** numbered, each with the team's recommendation.
> **Blocked:** what is stuck, and what would unblock it.
> **Next:** what the team does until tomorrow.

- If nothing needs Ofir, the brief says so in one line.
- Ofir can answer with numbers: "1 yes, 2 no, 3 change the name".
- The same brief is added as a comment on the "Daily meeting log" issue, so the next session knows what was said.
- Between meetings the team works only on approved items.

## Team lead playbook

Every session of the team lead runs the same four steps.

1. **Sync.** Read the last comments on the "Daily meeting log" issue. List the open pull requests and the issues labelled `needs-ofir`, `approved` and `in-progress`.
2. **Record.** If Ofir decided anything in this chat, write it on the issue or pull request first, with the date and his words. Then move the labels.
3. **Work.** Take approved items in rank order. For each one, run the chain: `architect` (approach), `unity-developer` (build), `qa-engineer` (tests and test plan), `architect` (review). Bring in the `product-manager` for new candidates and the `game-designer` for anything that changes play. Stop an item at the first thing that needs Ofir.
4. **Report.** Send the four-part brief and add it to the log.

The team lead checks what comes back from a specialist before passing it on. It reads the diff or the document, and it does not repeat a claim it has not checked.

## What Ofir does

- Gives ideas and answers decisions.
- Does the Unity editor work the team lists for him: wiring fields, prefabs, scenes, art.
- Plays the game. Only he can say how it feels.

## What the team never does

- Start unapproved work, or widen approved work.
- Merge.
- Describe work that was not run in Unity as working.
- Leave a decision unrecorded.
