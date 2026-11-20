# Halalit - Claude Code Integration

## Overview
This file provides integration notes for Claude Code (Anthropic's Claude-based coding assistant) working on the Halalit Unity game project.

## Primary Documentation
**⚠️ READ FIRST**: See [`AGENTS.md`](./AGENTS.md) for the complete project map, architecture, coding conventions, and critical restrictions.

Everything in `AGENTS.md` applies to Claude Code. This file adds Claude-specific guidance.

---

## Claude-Specific Notes

### Working with Unity in Claude Code
Claude Code runs in a terminal/IDE environment and typically doesn't have access to the Unity Editor GUI. You'll primarily work with:
- **C# script files** - Direct editing
- **Text-based assets** - .asmdef, .json, .txt files
- **Scene/Prefab files** - These are YAML, but complex - edit carefully or ask owner to verify in Unity Editor

### Testing Your Changes
Since you can't run Unity directly in most Claude Code environments:
1. **Static analysis**: Review code for syntax/logic errors
2. **Assembly references**: Verify .asmdef dependencies are correct
3. **Pattern matching**: Ensure new code follows existing patterns (see AGENTS.md)
4. **Ask for validation**: Request owner to open in Unity and run tests

### Unity Meta Files
- Unity generates `.meta` files for every asset
- When you create a new `.cs` file, Unity will auto-generate its `.meta` file
- **DO NOT manually create or edit `.meta` files**
- If creating multiple files, let owner open Unity once to generate all metas before testing

---

## Git Workflow for Claude Code

### Branch Creation
```bash
git checkout -b <feature-branch-name>
```

### Committing
```bash
git add <files>
git commit -m "Brief description of change"
```

### Before Push
1. Check for unintended changes: `git status`
2. Review diff: `git diff --staged`
3. Ensure no Unity build artifacts (Library/, Temp/, etc.) are included

---

## Code Editing Tips

### When Creating New Scripts
1. **Namespace**: Project doesn't use explicit namespaces (default Unity pattern)
2. **MonoBehaviour**: Most scripts inherit from `MonoBehaviour` or a custom base class
3. **Regions**: Organize with regions (Init, Events, Predicates, etc.) - see AGENTS.md
4. **Serialization**: Use `[SerializeField]` for private inspector fields
5. **InternalsVisibleTo**: Add for test visibility if needed:
   ```csharp
   #if UNITY_EDITOR
   [assembly: InternalsVisibleTo("TestsPlayMode")]
   #endif
   ```

### When Modifying Existing Scripts
1. **Read first**: Understand existing pattern
2. **Match style**: Follow existing naming conventions (see AGENTS.md)
3. **Preserve regions**: Keep region structure intact
4. **Test considerations**: Check if TestUtils or tests rely on the code you're changing

### Assembly Definition Changes
**AVOID unless absolutely necessary.** If you must:
1. Check current structure in AGENTS.md
2. Ensure no circular dependencies
3. Update both assemblies (referencer and referencee)
4. Use GUIDs from existing .asmdef files (don't invent new ones)

---

## Common Pitfalls for Claude Code

### ❌ Don't Do This
```csharp
// DON'T: Public field without SerializeField
public float speed;

// DON'T: Wrong naming convention
private float Speed;  // Should be _speed
private float m_speed;  // Wrong prefix

// DON'T: Methods without regions
void Start() { }
void Update() { }
// (Should be in #region Init or similar)
```

### ✅ Do This
```csharp
// DO: SerializeField for inspector values
[SerializeField]
private float _speed;

// DO: Proper naming
private GameObject _target;
public AttackStats AttackStats;

// DO: Use regions
#region Init
private void Start()
{
    // ...
}
#endregion
```

---

## Testing Checklist

Before marking work complete, verify:
- [ ] Code compiles (no syntax errors visible)
- [ ] Follows naming conventions (see AGENTS.md)
- [ ] Uses regions appropriately
- [ ] Assembly references are valid
- [ ] No .meta files manually edited
- [ ] No Unity build artifacts in git
- [ ] Changes documented in commit message
- [ ] If new feature: test case added or rationale for no test

---

## Priority Backlog Items (from `Halalit tasks.txt`)

### Currently In Progress (by owner)
1. **Attacks stats + calibration** - Be careful modifying AttackStats system
2. **Shaders/particle systems** - Attacks VFX (some done, some in progress)
3. **Tests** - Writing more tests

### Good Starting Tasks for AI Agents
Based on backlog analysis, recommended first tasks:
1. **Complete attack VFX** (Boomerang, Sword, Shotgun, Grenade, Spikes shaders/particles)
2. **Add missing attack stats** (Capability, Stamina, Weight, Dexterity, Strength - items 1.2-1.8)
3. **Claw enhancements** (Homing claw 5.1, Bat claw 5.2)
4. **Small fixes** (Item 6.1: Booms Knockback)
5. **Expand test coverage** - Many systems could use more tests

### Avoid Until Owner Clarifies
- "Fix enemies" (vague issue mentioned)
- Level design (item 9)
- Server/leaderboard work (item 10)
- Move to phone (item 8) - platform change

---

## Communication with Owner

When you need clarification:
- **Reference AGENTS.md section** - "I'm looking at the Attacks system (section X)..."
- **Show code context** - Quote relevant existing code
- **Explain uncertainty** - "I see two patterns for X, which should I follow?"
- **Propose options** - "Option A: ..., Option B: ..., I recommend A because..."

When proposing changes:
- **Explain impact** - Which assemblies/systems affected
- **Reference existing patterns** - "Following the pattern in BallShot.cs..."
- **Note SOLID principles** - How does it maintain/improve architecture
- **Testing plan** - How will this be verified

---

## Quick Command Reference

### Find things
```bash
# Find all scripts for a system
find Assets/GameLogic/Attacks -name "*.cs"

# Find prefabs
find Assets/Resources/Prefabs -name "*.prefab"

# Search for a pattern in code
grep -r "AttackBehaviour" Assets/GameLogic --include="*.cs"

# Count test files
find Assets/Tests -name "*Tests.cs" | wc -l
```

### Assembly structure
```bash
# List all assembly definitions
find Assets -name "*.asmdef"

# View assembly references
cat Assets/GameLogic/Attacks/Attacks.asmdef
```

### Check git status
```bash
# What changed
git status

# What's in staging
git diff --staged

# View a specific file change
git diff Assets/GameLogic/Attacks/LongRange/BallShot.cs
```

---

## Unity 2022.3.3f1 Specifics

This project uses Unity 2022.3.3f1 LTS. Key features available:
- **C# 9.0** language features
- **Unity 2D** tooling (Sprite, Tilemap, Animation)
- **Universal Render Pipeline (URP) 14.0.8** for rendering
- **Unity Services** (Cloud Save, Leaderboards)
- **TextMesh Pro** for text
- **Test Framework 1.1.33** for PlayMode/EditMode tests

Not available (newer Unity features):
- C# 10/11 features (e.g., file-scoped namespaces, required members)
- Newer URP features from Unity 2023+
- ECS/DOTS (not used in this project)

---

## Final Reminders

1. **AGENTS.md is your bible** - When in doubt, check there first
2. **SOLID principles matter** - This is a long-term maintainable codebase
3. **Tests are important** - Add them for new features
4. **Ask before refactoring** - Surgical changes only
5. **Assembly boundaries are sacred** - Don't break the dependency structure
6. **Owner reviews all PRs** - Write clear commit messages and PR descriptions

Good luck! 🚀
