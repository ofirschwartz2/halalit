# The team roster

The team lead owns this file. The roster grows and shrinks with the work. Ofir approves every change.

## On the team now

| Role | File | Joined | Why it is here |
|---|---|---|---|
| Product manager | `.claude/agents/product-manager.md` | 2026-11-20 | Keeps the backlog and the Foundations shelf |
| Game designer | `.claude/agents/game-designer.md` | 2026-11-20 | Keeps the menu of weapons, enemies and behaviors |
| Architect | `.claude/agents/architect.md` | 2026-11-20 | Guards the code against drift and reviews every change |
| Unity developer | `.claude/agents/unity-developer.md` | 2026-11-20 | Builds approved cards |
| QA engineer | `.claude/agents/qa-engineer.md` | 2026-11-20 | Tests, test plans, the Waiting for Unity list |

The team lead is the main session, not a file.

## The bench

Roles the team expects to need. Each joins when its trigger happens, through an offer.

| Role | Joins when | Would own |
|---|---|---|
| Security engineer | The first card that touches blockchain, wallets, accounts or anything of value is approved | Threat review of those cards, and cheat resistance of the daily |
| Backend engineer | The first server-side card is approved, such as a daily leaderboard or score validation | Server code and its tests |
| Technical artist | Ofir wants shader and particle cards built by the team | Shaders, effects, the look of new content |
| Sound designer | Ofir decides the first release has sound | The sound list and mixing rules |

## The roster check

Every Sunday meeting the team lead answers three questions:

1. **Who was idle?** A role with no work for two weeks, and none waiting in Offered or Approved, is a candidate to retire.
2. **What has no owner?** An offered or approved card that needs a skill no role has is a reason to add a role, usually from the bench.
3. **Who is the bottleneck?** A role that held up cards in three meetings in a row is a reason to split it or add a second one.

Ofir can also ask for a change at any time.

## Changing the team

A team change is an offer on the board, on the Foundations shelf, like any feature:

> **Add a security engineer**, or **Retire the sound designer**.
> **Why now:** the trigger that happened.
> **What it owns**, and **what it never does.**
> **Cost:** each added role makes meetings longer and uses more of Ofir's Claude plan.

After Ofir approves:

- **Add.** The team lead writes the role file in `.claude/agents/`, in the shape of the existing ones, adds the role to this file and to the table in `CLAUDE.md`, and merges it under the standing permission for team files.
- **Retire.** The team lead first hands the role's open work to another role. Then it moves the role file to `docs/team/retired-roles/`, so the role can return unchanged, and updates both tables.

No role adds or removes itself, and the team lead does not change the roster without Ofir's yes.
