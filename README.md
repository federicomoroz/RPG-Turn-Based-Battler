# RPG Turn-Based Battler

A modular, data-driven turn-based battle system built in Unity for retro 2D pixel-art RPGs. Designed around a manager architecture with object pooling, ScriptableObject configuration, and a coroutine-driven execution pipeline.

---

## Features

- **Turn-based combat** — command tokens sorted by priority and unit speed; sequential execution with configurable delay
- **Skill system** — ScriptableObject-defined skills with physical/special damage types, elemental affinity, accuracy, evasion, and scope (single/all targets, ally/enemy side)
- **Damage formula** — base power × level/strength scaling, with configurable variance and defense reduction
- **Projectile system** — multiple movement types (targeted linear, targeted curve, distance linear), pooled lifetimes, child-projectile spawning on impact
- **Audio system** — pooled SFX with per-frame audio cues, looping, pause/resume; background music control
- **Visual effects** — pooled particle impacts, ScriptableObject-defined VFX database
- **Object pooling** — generic stack-based pools for MonoBehaviours, plain classes, and prefabs; type-safe via `IPoolable<T>`
- **Battle UI** — character-by-character battle log with auto-scroll, command selection window, character stat display
- **Utility layer** — fluent `Timer` with loop support, bounded `GameValue` with callbacks, `ExtensionMethods` for chainable delegate registration

---

## Architecture

```
Assets/Scripts/
├── Battle/
│   ├── BattleManager.cs       # Turn orchestration, damage calculation, token execution
│   ├── Turn.cs                # Per-turn phase management (WIP)
│   └── UnitBase.cs            # MonoBehaviour wrapper linking GameObject to BattleUnit
│
├── Characters/
│   └── BattleUnit.cs          # Unit stats, HP/MP pools, animations, affect processing
│
├── Skills/
│   ├── SkillHandler.cs        # Bridges SO_Skill data to AffectToken generation
│   ├── Movement/              # Projectile movement strategies
│   └── ScriptableObjects/     # SO_Skill, SO_Projectile, SO_UnitData definitions
│
├── Managers/
│   ├── BattleManager.cs       # PersistentSingleton — combat loop
│   ├── GameManager.cs         # Frame rate, time scale, assembly reflection cache
│   ├── PoolManager.cs         # Global pool registry (reflection-based discovery)
│   ├── FXManager.cs           # VFX pool creation and dispatch
│   ├── SoundManager.cs        # Music + SFX queue management
│   └── TimerManager.cs        # Active timer lifecycle (deferred add/remove)
│
├── Pooling/
│   ├── Pool.cs                # Abstract Stack<T> base
│   ├── ObjectPool.cs          # MonoBehaviour activate/deactivate pool
│   ├── ClassPool.cs           # Plain class pool
│   └── PrefabPool.cs          # Instantiation-based prefab pool
│
├── Projectiles/
│   ├── Projectile.cs          # IPoolable projectile (movement + view + config)
│   ├── ObjectView.cs          # Sprite-sheet frame animator with per-frame SFX
│   └── Factory.cs             # Static pooled-projectile creation helper
│
├── FX/
│   ├── Impact.cs              # IPoolable particle player (fluent API)
│   ├── SFX.cs                 # IPoolable audio source
│   └── GlobalSfxDatabase.cs   # ScriptableObject audio clip registry
│
├── UI/
│   ├── WindowBase.cs          # Base window with Open/Close
│   ├── WindowBattleCommand.cs # Action selection with cursor
│   ├── WindowBattleLog.cs     # Typewriter battle log with auto-scroll
│   ├── WindowCharacterStats.cs# Unit status display
│   ├── ButtonPlus.cs          # Unity Button + OnSelect/OnDeselect events
│   └── FullScreenSprite.cs    # Orthographic background scaling
│
├── Singleton/
│   └── Singleton.cs           # Singleton<T> + PersistentSingleton<T>
│
└── Tools/
    ├── GameValue.cs            # Bounded numeric value (min/max, callbacks)
    ├── Timer.cs                # Fluent pooled timer with loop support
    ├── StructsAndEnums.cs      # All shared types (enums, structs, tokens)
    ├── ExtensionMethods.cs     # RegisterMultipleCallbacks fluent helper
    ├── Vocab.cs                # Battle dialogue string constants
    └── Debugger.cs             # Development keyboard shortcuts
```

---

## Core Systems

### Turn Execution Flow

```
Player selects action
  └─► CommandToken created {unit, skill, target}
        └─► BattleManager collects all tokens
              └─► SortCommandTokens()
                    sort by: priority DESC → speed DESC
                    └─► Perform() coroutine
                          ├─► skill.Execute() animation coroutine
                          ├─► SkillHandler generates AffectToken {action, type, value, accuracy}
                          ├─► BattleUnit.ApplyAffectToken()
                          │     ├─► CalculateEvadeChance() → may skip
                          │     ├─► CalculateFinalDamage() → defense reduction
                          │     └─► HealHp() / TakeHp()
                          ├─► WindowBattleLog displays message
                          └─► Wait _timeBetweenTurns (0.75 s)
```

### Damage Formula

```
variance       = skill.affect.variance * 0.01
finalBasePower = Random(basePower ± variance)
damage         = basePower * 4 + (level * strength * (basePower / 32))

// Defense reduction (BattleUnit)
defenseFactor  = 100 / (100 + defense)
finalDamage    = damage * defenseFactor

// Evasion (BattleUnit)
evadeChance    = 0.5 * (speed / 100) + (1 - skill.accuracy)
isEvaded       = Random(0–100) <= evadeChance
```

### Skill Configuration (ScriptableObject)

Each skill is a `SO_Skill` asset:

| Field | Type | Description |
|-------|------|-------------|
| `affect.action` | `AffectAction` | HPDamage, HPRecover, HPDrain, MPDamage, MPRecover, MPDrain |
| `affect.type` | `AffectType` | Physical / Special |
| `affect.basePower` | int (0–999) | Base damage/heal value |
| `affect.variance` | float (0–100%) | Random spread around base power |
| `affect.element` | Element | Elemental affinity |
| `invocation.accuracy` | float (0–1) | Hit probability |
| `invocation.isEvadable` | bool | Whether unit speed can dodge it |
| `scope.side` | Side | Enemy / Ally / All / User |
| `scope.number` | Number | One / All |
| `priority` | Priority | LOW / REGULAR / HIGH |

### Object Pooling

All frequently-spawned objects (projectiles, SFX, impacts, timers) use the generic pool system:

```csharp
// Get from pool
var projectile = PoolManager.Instance.GetObject<Projectile>();

// Return to pool
PoolManager.Instance.ReturnObject<Projectile>(projectile);

// Create a prefab pool (FXManager, SoundManager)
PoolManager.Instance.CreatePrefabPool<Impact>(prefab, initialSize);
```

Implementing `IPoolable<T>` requires `Initialize()` (first creation) and `Dispose()` (return to pool).

### GameValue

Bounded numeric wrapper used for HP, MP, and other stats:

```csharp
var hp = new GameValue(0, 100);   // min=0, max=100
hp.Add(30);
hp.Subtract(10);
float ratio = hp.Ratio;           // 0.0 – 1.0

hp.OnChange  += val => UpdateHealthBar(val);
hp.OnZero    += ()  => TriggerDeath();
hp.OnMax     += ()  => ShowFullHPEffect();
```

### Timer

Fluent pooled timer routed through `TimerManager`:

```csharp
Timer.Get(2.0f)
     .OnComplete(() => NextTurn())
     .OnLoop(() => PulseEffect())
     .SetLoops(3)
     .Start();
```

---

## Key Types (StructsAndEnums.cs)

```csharp
// Scheduled battle action
struct CommandToken { BattleUnit unit; SO_Skill skill; BattleUnit target; }

// Resolved effect ready to apply
struct AffectToken  { AffectAction action; AffectType type; float value;
                      bool isEvadable; float accuracy; Element element;
                      Action onComplete; }

enum AffectAction   { HPDamage, MPDamage, HPRecover, MPRecover, HPDrain, MPDrain }
enum AffectType     { Physical, Special }
enum Priority       { LOW, REGULAR, HIGH }
enum MovementType   { TargetedLinear, TargetedCurve, DistanceLinear, None }
enum Side           { None, Enemy, Ally, All, User }
enum Number         { One, All }
```

---

## Project Setup

**Requirements:** Unity 2022+ (URP), TextMesh Pro

1. Clone the repository
2. Open in Unity Hub
3. Open the battle scene from `Assets/Scenes/`
4. Assign `SO_UnitData` assets to the `BattleUnit` components in the scene
5. Assign `SO_Skill` assets to unit skill sets
6. Run — use `Space` to execute tokens, `V` to sort (debug keys via `Debugger.cs`)

**Debug controls (in-editor):**

| Key | Action |
|-----|--------|
| `Space` | Execute command tokens |
| `V` | Sort command tokens |
| `T` | Toggle slow motion (0.2× time scale) |
| `M` | Toggle background music |
| `P` | Pause/resume music |
| `Backspace` | Stop music |

---

## Work in Progress

- `Turn.cs` — per-turn phase handler (framework stubbed, logic commented out)
- `WindowCharacterStats.cs` — unit status panel (placeholder)
- MP damage / drain / recovery execution path
- Elemental effectiveness multipliers (enums defined, not integrated)
- Movement folder — projectile trajectory strategies

---

## Language Breakdown

| Language | % |
|----------|---|
| C# | 54.3% |
| ShaderLab | 37.9% |
| HLSL | 7.8% |
