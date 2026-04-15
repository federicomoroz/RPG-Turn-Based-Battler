# RPG Turn-Based Battler

A modular, data-driven turn-based battle system built in Unity (URP, 2D). Designed as a reusable framework: drop in new skills, units, and controllers without touching existing code.

---

## Table of Contents

- [System Overview](#system-overview)
- [Turn Execution — Finite State Machine](#turn-execution--finite-state-machine)
- [Skill Hierarchy](#skill-hierarchy)
- [Character Composition](#character-composition)
- [Damage Calculation Pipeline](#damage-calculation-pipeline)
- [Projectile System](#projectile-system)
- [Controller System](#controller-system)
- [Object Pooling](#object-pooling)
- [GameValue](#gamevalue)
- [Timer](#timer)
- [Generic Pool System](#generic-pool-system)
- [Design Patterns](#design-patterns)
- [Project Setup](#project-setup)

---

## System Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         SETUP SCENE                              │
│   ScenarioSelectorUI → BackgroundSelectorUI → CharacterPickerUI  │
│                        BattleBuilderManager                      │
│                        BattleLauncher ──────────────────────────►│ async scene load
└──────────────────────────────────────────────────────────────────┘
                                │
                                │ BattleConfig (static bridge)
                                ▼
┌──────────────────────────────────────────────────────────────────┐
│                         BATTLE SCENE                             │
│                                                                  │
│  BattleManager ◄──── TurnManager ◄──── SO_TurnOrderStrategy      │
│       │                                                          │
│       │  for each unit in turn order:                           │
│       │                                                          │
│       ▼                                                          │
│  BattleUnit.RequestCommandToken()                                │
│       │                                                          │
│       ├──[Human]──► SO_HumanController ──► UI event             │
│       │              player picks skill/target                   │
│       │                                                          │
│       └──[AI]─────► SO_AIController                             │
│                      auto-selects default skill + target         │
│                                │                                 │
│                                ▼ CommandToken                    │
│  TurnExecutor.Execute()                                          │
│       ├── Anticipation                                           │
│       ├── Execution   ──────────────────► SO_Skill coroutines    │
│       ├── Outcome     ──────────────────► AffectTokenFactory     │
│       └── Resolution  ──────────────────► BattleUnit effects     │
│                                                                  │
│  Managers: PoolManager · SoundManager · FXManager · TimerManager │
└──────────────────────────────────────────────────────────────────┘
```

---

## Turn Execution — Finite State Machine

Each `CommandToken` runs through four sequential phases inside `TurnExecutor`. Every phase delegates to virtual methods on `SO_Skill`, making the phases the extension points for new behaviour.

```
CommandToken arrives
        │
        ▼
┌───────────────────────────────────────────────────┐
│  1. ANTICIPATION                                  │
│                                                   │
│  • Deactivate previous guard                      │
│  • Clear stale parry flag                         │
│  • Redirect to valid target if original is dead   │
│  • SO_Skill.PlayAnticipation()  ← virtual hook    │
└───────────────────┬───────────────────────────────┘
                    │
                    ▼
┌───────────────────────────────────────────────────┐
│  2. EXECUTION                                     │
│                                                   │
│  • SO_Skill.MoveToExecutionPosition()  ← virtual  │
│      Melee  → dash to MeleeStopPoint              │
│      Range  → yield null (no movement)            │
│      Guard  → yield null                          │
│                                                   │
│  • SO_Skill.PlayAttackAnimation()      ← virtual  │
│      animation event fires TriggerSkill()         │
│      ┌─────────────────────────────┐              │
│      │ SO_Skill.OnImpact event     │              │
│      │ MeleeAttack → plays SFX,    │              │
│      │              FireImpact()   │              │
│      │ RangeAttack → spawns        │              │
│      │              projectile,    │              │
│      │              FireImpact()   │              │
│      └─────────────────────────────┘              │
│                                                   │
│  • WaitUntil(ImpactReceived)                      │
└───────────────────┬───────────────────────────────┘
                    │
                    ▼
┌───────────────────────────────────────────────────┐
│  3. OUTCOME EVALUATION (synchronous)              │
│                                                   │
│  ITurnOutcomeEvaluator.Evaluate(ctx)              │
│      ├── HasImpact == false  → NoImpact           │
│      ├── accuracy roll miss  → Miss               │
│      └── otherwise           → Hit                │
│                                                   │
│  Hit  → AffectTokenFactory.Create()               │
│          BattleUnit.ReceiveAffectToken()           │
│          └── parry check                          │
│          └── evasion check                        │
│          └── apply HP/MP change                   │
│          └── apply on-hit status effects          │
│          └── play damage animation                │
│          └── trigger death sequence if HP == 0    │
│                                                   │
│  Miss → SO_Skill.PlayMissReaction()   ← virtual   │
│                                                   │
│  Parry counter → recursive Execute() call         │
│                  (parties swapped)                │
└───────────────────┬───────────────────────────────┘
                    │
                    ▼
┌───────────────────────────────────────────────────┐
│  4. RESOLUTION                                    │
│                                                   │
│  • SO_Skill.MoveToRestPosition()      ← virtual   │
│      Melee  → dash back to HomePosition           │
│      Range  → yield null                          │
│      Guard  → yield null (stays in guard pose)    │
│                                                   │
│  • RestoreSortingOrder                            │
│  • Return to Idle animation                       │
│  • CommandToken.OnCompleteCallback?.Invoke()      │
│  • WaitForSeconds(_timeBetweenActions)            │
└───────────────────────────────────────────────────┘
```

---

## Skill Hierarchy

Skills are **ScriptableObject assets** — no scene objects needed. The hierarchy uses **Template Method**: `SO_Skill` defines the algorithm skeleton; concrete subclasses fill in the steps.

```
ScriptableObject
└── SO_Skill  (abstract)
    │   Fields: scope, affect, invocation, priority, icon, name, costMP
    │   Hooks:  PlayAnticipation(), MoveToExecutionPosition(),
    │           PlayAttackAnimation(), MoveToRestPosition(), PlayMissReaction()
    │   Events: OnImpact
    │
    ├── Attack  (abstract)
    │   │   Static events: OnParryWindowOpened, OnParryWindowClosed
    │   │
    │   ├── MeleeAttack  ────────────────────────────────────────────────────┐
    │   │   [CreateAssetMenu]                                                │
    │   │   MoveToExecutionPosition → dash to MeleeStopPoint                │
    │   │   PlayAttackAnimation     → Melee1 or Melee2 clip                 │
    │   │   MoveToRestPosition      → dash back to HomePosition             │
    │   │   Trigger()               → play SFX + FireImpact()               │
    │   │                                                                    │
    │   └── RangeAttack  ────────────────────────────────────────────────────┤
    │       [CreateAssetMenu]                                                │
    │       MoveToExecutionPosition → yield null                            │
    │       PlayAttackAnimation     → Range1 or Range2 clip                 │
    │       Trigger()               → spawn Projectile → FireImpact()       │
    │       MoveToRestPosition      → yield null                            │
    │                                                                        │
    ├── Defend  (abstract)                                                   │
    │   │   DefenseMultiplier (configurable), ActivateVisual/DeactivateVisual│
    │   │                                                                    │
    │   ├── GuardDefend  ── animation-based guard visual                    │
    │   └── VFXDefend    ── projectile-based guard VFX (shield orb, etc.)   │
    │                                                                        │
    ├── ParryStance                                                          │
    │       Sets BattleUnit.IsParryReady = true                             │
    │       HasImpact = false (no damage applied)                           │
    │                                                                        │
    ├── Parry                                                                │
    │       DamageMultiplier > 1 (counter bonus)                            │
    │       Triggered as a recursive counter turn                           │
    │                                                                        │
    ├── Death  (abstract)                                                    │
    │   └── DeathDefault ── damage animation + VFXDissolve shader           │
    │                                                                        │
    └── PassSkill                                                            │
            No-op, used by dummy/pass controllers                           │
                                                                            │
─────────────────────────────────────── Movement (separate hierarchy) ──────┘

SO_Movement  (abstract ScriptableObject)
    Execute(BattleUnit user, Vector3 target) : IEnumerator
    │
    ├── Dash      ── phased dash with start / loop / end animations
    ├── Jump      ── parabolic arc via AnimationCurve
    └── Teleport  ── dissolve out → reposition → dissolve in

MovementStrategy  (abstract plain class — runtime execution)
    Execute(transform, target, speed, rotate, onArrive) : MovementStrategy
    │
    ├── LinearMovement  ── Vector3.Lerp over time
    └── CurveMovement   ── AnimationCurve for arced trajectories

SO_Movement owns the "what" (SFX, animation clips).
MovementStrategy owns the "how" (interpolation math, timer-driven tick).
```

---

## Character Composition

`BattleUnit` is a thin MonoBehaviour coordinator. All visual concerns are separated into `UnitPresenter`. Data is owned by a runtime-cloned `SO_UnitData` instance (so shared assets are never mutated).

```
┌──────────────────────────────────────────────────────────┐
│  BattleUnit  (MonoBehaviour)                             │
│                                                          │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  SO_UnitData  (Instantiate() clone at runtime)      │ │
│  │                                                     │ │
│  │  SO_UnitStats                                       │ │
│  │    hp : GameValue  ──OnZero──► EnterDeadState()     │ │
│  │    mp : GameValue                                   │ │
│  │    speed, physicalStrength, specialStrength         │ │
│  │    physicalDefense, specialDefense, level           │ │
│  │                                                     │ │
│  │  SO_UnitMotions                                     │ │
│  │    Idle, Melee1, Melee2, Range1, Range2             │ │
│  │    Damage, Parry, Dash (start/loop/end)             │ │
│  │    Jump (start/loop/end)                            │ │
│  │                                                     │ │
│  │  SkillSet                                           │ │
│  │    defaultAttack : Attack                           │ │
│  │    parry         : Parry                            │ │
│  │    death         : Death                            │ │
│  │    skills[]      : SO_Skill[]                       │ │
│  │                                                     │ │
│  │  SO_Movement  (MovementType asset)                  │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                          │
│  ┌─────────────────────────────────────────────────────┐ │
│  │  UnitPresenter  (plain C#)                          │ │
│  │    SpriteRenderer  (visual)                         │ │
│  │    Animator + AnimatorOverrideController            │ │
│  │    ShadowController                                 │ │
│  │    PlayAnimation(), IsAnimationDone()               │ │
│  │    RaiseSortingOrder() / RestoreSortingOrder()      │ │
│  └─────────────────────────────────────────────────────┘ │
│                                                          │
│  SO_Controller  ──► command token negotiation            │
│                                                          │
│  Guard state                                            │
│    IsGuarding, _guardDefenseMultiplier                  │
│    ActiveDefendVFX : Projectile                         │
│                                                          │
│  Parry state                                            │
│    IsParryReady                                         │
│                                                          │
│  Status effects                                         │
│    List<StatusEffectInstance>                           │
│    ApplyStatusEffect() / RemoveStatusEffect()           │
│    ProcessTurnEffects()  ← called at start of unit turn │
└──────────────────────────────────────────────────────────┘

GameValue  (bounded numeric with callbacks)
┌─────────────────────────────────────────────┐
│  minValue, maxValue, currentValue           │
│  Ratio  →  currentValue / maxValue  (0–1)   │
│  Add() / Subtract() / SetValue()            │
│  OnChange  : Action<int>                    │
│  OnZero    : Action         ◄── death hook  │
│  OnMax     : Action                         │
└─────────────────────────────────────────────┘
```

---

## Damage Calculation Pipeline

A **Chain of Responsibility** built from `IDamageModifier` steps. Adding a new damage modifier (crits, weather, buffs) means implementing the interface and calling `.Add()` on the pipeline — no existing code changes.

```
AffectTokenFactory.Create(skill, attacker, target)
        │
        ▼
DamageContext  { Skill, Attacker, Target, Element, CurrentValue }
        │
        ▼
DamageCalculationPipeline
        │
        ├──[1]── BaseFormulaModifier
        │           finalBase = Random(basePower ± variance)
        │           strength  = physicalStrength | specialStrength
        │           value     = (finalBase * 4
        │                     + level * strength * (finalBase / 32))
        │                     * skill.DamageMultiplier
        │           → ctx.CurrentValue = value
        │
        ├──[2]── ElementModifier  (placeholder — extend here)
        │           multiply ctx.CurrentValue by element effectiveness
        │
        ├──[N]── (future: CritModifier, WeatherModifier, BuffModifier…)
        │
        ▼
int finalDamage = Mathf.RoundToInt(ctx.CurrentValue)
        │
        ▼
AffectToken { action, type, finalDamage, accuracy, isEvadable, element, onComplete }
        │
        ▼
BattleUnit.ReceiveAffectToken()
        │
        ├── parry check     → counter turn (recursive Execute)
        ├── evasion check   → 0.5 * (speed/100) + (1 - accuracy)
        ├── defense reduce  → 100 / (100 + defense) × damage
        └── apply HP/MP + status effects + death sequence
```

---

## Projectile System

`Projectile` is composed of two independent responsibilities assembled at runtime. Configuration is entirely data-driven via `SO_Projectile`.

```
Factory.CreateProjectile(SO_Projectile data)
    │  (pulls from PoolManager)
    ▼
Projectile  (MonoBehaviour, IPoolable)
│
├── MovementHandler  ─────────────── "where does it go?"
│     │  Reads MovementData from SO_Projectile
│     │
│     ├── MovementType.TargetedLinear  → LinearMovement strategy
│     ├── MovementType.TargetedCurve   → CurveMovement strategy
│     ├── MovementType.DistanceLinear  → LinearMovement (fixed distance)
│     └── MovementType.None            → stays at spawn position
│                                        (used for impact-at-cast-site VFX)
│
├── ObjectView  ──────────────────── "what does it look like?"
│     │  Reads SO_ObjectView from SO_Projectile
│     │  Sprite-sheet frame animation at configurable speed
│     │  Per-frame AudioClip hooks
│     └── SpriteRenderer layer management
│
└── SO_Projectile  (data)
      movementData    : MovementData   (type, speed, AnimationCurve)
      viewData        : SO_ObjectView  (sprites[], animSpeed, SFX[])
      destructionCondition:
        ├── OnImpact        → Dispose() when MovementHandler arrives
        ├── OnTime          → Timer pool → Dispose() after lifeTime
        └── OnAnimationEnd  → ObjectView callback → Dispose()
      impacts[]       → child Projectiles spawned on arrival
      lifeTime        : float

Lifetime flow:
  Execute()
    ├── SetMovement()  → MovementHandler ticks via Timer pool
    ├── SetView()      → ObjectView animates each frame
    └── on arrival/time/animation end:
          _view.Stop()
          CheckImpactSpawn()  → spawns child projectiles
          Dispose()           → onComplete callback + return to pool
```

---

## Controller System

`SO_Controller` is a **Strategy** object serialized directly onto `BattleUnit`. Swapping between human and AI control is a single asset reference change in the Inspector — no conditional code paths in `BattleUnit` or `BattleManager`.

```
SO_Controller  (abstract ScriptableObject)
│   RequestCommandToken(self, allies, enemies, onComplete) : IEnumerator
│   OnParryOpportunity(self, onParryConfirmed)             : virtual no-op
│
├── SO_HumanController
│     RequestCommandToken → fires static event OnCommandRequested
│                           UI subscribes: shows SkillSelectorUI
│                           player picks skill + target
│                           onComplete(token) → resumes coroutine
│     OnParryOpportunity  → fires OnParryInputRequired
│                           UI shows timed prompt
│                           player presses button → onParryConfirmed()
│
├── SO_AIController
│     RequestCommandToken → selects defaultAttack skill
│                           TargetResolver.Primary() picks target
│                           onComplete(token) immediately
│     OnParryOpportunity  → inherited no-op (AI never parries)
│
└── SO_PassController
      RequestCommandToken → yields null (skips turn)

Parry window flow (human player):
  Attack animation event
    → BattleUnit.TriggerParryWindowOpen()
    → BattleManager.OpenParryWindow()
    → Attack.OnParryWindowOpened fires
    → defender.NotifyParryOpportunity()
    → SO_HumanController.OnParryInputRequired fires
    → UI shows [PARRY] prompt
         player presses in time → onParryConfirmed() → IsParryReady = true
         window closes          → Attack.OnParryWindowClosed
```

---

## Object Pooling

A generic, type-safe pool hierarchy covers all frequently-spawned objects (Projectiles, SFX, Impacts, Timers). Zero allocation at runtime after warmup.

```
IPool<T>
└── Pool<T>  (abstract, Stack<T> storage)
    │   Pull()  → pop from stack (or Create() if empty)
    │   Push()  → push back onto stack
    │   Fill()  → pre-warm on Initialize()
    │
    ├── ObjectPool<T>  (MonoBehaviour — SetActive true/false)
    ├── ClassPool<T>   (plain C# — no GameObject overhead)
    └── PrefabPool<T>  (Instantiate on first fill, reuse after)

IPoolable<T>
    Initialize() → called once after creation
    Dispose()    → called to return to pool

PoolManager  (PersistentSingleton)
    GetObject<T>()          → type-keyed lookup
    ReturnObject<T>(item)   → returns to correct pool
    CreatePrefabPool<T>()   → FXManager / SoundManager use this at startup

Usage examples:
    var proj  = PoolManager.GetObject<Projectile>();
    var sfx   = PoolManager.GetObject<SFX>();
    var timer = PoolManager.GetObject<Timer>();

    timer.SetTime(2f).OnComplete(() => NextTurn()).Start();
    // timer returns itself to the pool after firing
```

---

## GameValue

`GameValue` is a self-contained bounded number. Every stat that has a min, a max, and needs to notify the rest of the system when it changes (HP, MP, timer progress) is a `GameValue`.

```
                    ┌─────────────────────────────────────────────┐
                    │               GameValue                     │
                    │                                             │
  maxValue ───────► │  ████████████████████░░░░░░░░  75 / 100   │
                    │                                             │
                    │  _currentValue  (clamped 0 … maxValue)     │
                    │  Ratio = currentValue / maxValue  → 0…1    │
                    └─────────────────┬───────────────────────────┘
                                      │
               ┌──────────────────────┼──────────────────────┐
               │ every SetValue()     │                       │
               ▼                      ▼                       ▼
        OnValueChanged           OnZero fires           OnMax fires
     (value, ratio)              when HP = 0            when HP = max
           │                         │                       │
      BarDisplay                EnterDeadState()        full-heal VFX
      updates bar              (registered in Setup)    (future hook)

API — all methods return `this` for fluent chaining:
  hp.Add(30)                  → heals 30
  hp.Subtract(45)             → takes 45 damage (clamped at 0)
  hp.IncreaseRatio(0.1f)      → heals 10% of current value
  hp.SetValue(100)            → hard set
  hp.Ratio                    → 0.0 … 1.0  (drives health bar fill)

  hp.RegisterZeroCallback(EnterDeadState)
  hp.RegisterMaxCallback(PlayFullHealEffect)
  hp.RegisterValueChangedCallback((val, ratio) => bar.SetFill(ratio))
```

**Why a class and not a struct?**
Callbacks survive across frames without boxing. Multiple systems (UI, battle logic, audio) register independently on the same instance — no central coordinator needed.

---

## Timer

`Timer` **is a** `GameValue`. Its `_currentValue` starts at 0 and ticks up to `maxValue` (the duration). When it reaches `maxValue`, `OnMax` fires — the timer reuses the same boundary event that `GameValue` already provides.

```
  GameValue
  └── Timer  (also IPoolable<Timer>)

  TIME ──────────────────────────────────────────────────►
  0                    duration                          MAX
  │░░░░░░░░░░░░░░░░░░░░│████████████████████████████████│
  │◄── Tick() each frame: Add(Time.deltaTime) ─────────►│
                        │                               │
                    OnValueChanged                    OnMax
                  (optional progress             OnTimeOutHandler()
                   callback)                          │
                                               ┌──────┴───────┐
                                               │              │
                                           isLoop?          no loop
                                               │              │
                                        ┌──────┴──────┐       │
                                    has loops     infinite     │
                                    remaining?    loop         │
                                        │              │       │
                                    _currentLoop++  onLoop  onComplete
                                    Reset()         Reset()    │
                                                           SetOff()
                                                         (→ pool)

Fluent API:
  PoolManager.GetObject<Timer>()     // pull from pool (zero allocation)
    .SetTime(2.0f)                   // sets maxValue = 2, starts ticking
    .SetLoops(3)                     // repeat 3 times before completing
    .RegisterLoopCallback(OnEachLoop)
    .RegisterCompleteCallback(OnDone);

  // stop early:
  timer.SetOff()   → enqueued into TimerManager._inactiveTimers
                   → Dispose() called next frame → pool returns it

TimerManager (PersistentSingleton) — tick distribution:

  ┌──────────────────────────────────────────────────────┐
  │  Update() every frame                                │
  │                                                      │
  │  _activeTimers ──► timer.Tick()  (for each)          │
  │                                                      │
  │  _inactiveTimers ──► Remove + Dispose()  (deferred)  │
  │  _pendingTimers  ──► Add to _activeTimers (deferred) │
  └──────────────────────────────────────────────────────┘

  Deferred queues prevent ConcurrentModificationException:
  a timer completing inside Tick() enqueues itself into
  _inactiveTimers and is safely removed after the loop ends.
```

**Why inherit `GameValue`?**
A timer IS a value progressing from 0 to max with boundary callbacks. Inheriting means `Timer` gets `Add()`, `Ratio`, `OnZero`, `OnMax`, and `OnValueChanged` for free. The only new concept is looping and pool lifecycle.

---

## Generic Pool System

The pool system has two layers: a **type hierarchy** that defines behaviour, and a **`PoolManager`** that owns all pools and auto-discovers what to pool at startup.

### Pool Type Hierarchy

```
                      IPoolable<T>
                  ┌───────────────────┐
                  │  Initialize() → T  │  called once on creation
                  │  Dispose()         │  called to return to pool
                  └───────────────────┘
                           ▲
          ┌────────────────┼─────────────────┐
          │                │                 │
       Timer           Projectile          SFX
      (ClassPool)    (PrefabPool)       (PrefabPool)
       Impact           (etc.)
      (PrefabPool)

                         IPool<T>
                  ┌───────────────────┐
                  │  Pull() → T        │
                  │  Push(T)           │
                  └───────────────────┘
                           ▲
                      Pool<T>  (abstract)
                  Stack<T> _pooledObjects
                  Fill(capacity) on Initialize()
                           │
               ┌───────────┼────────────┐
               │           │            │
        ObjectPool<T>  ClassPool<T>  PrefabPool<T>
        MonoBehaviour   plain C#      MonoBehaviour
        SetActive(true)  new T()      Instantiate(prefab)
        SetActive(false) .Initialize()
        on Pull/Push
```

### The Three Pool Flavours

```
┌──────────────────────────────────────────────────────────────────┐
│  ClassPool<T>                                                    │
│  Constraint: T : IPoolable<T>, new()                            │
│                                                                  │
│  Use for: plain C# objects with no GameObject (Timer)           │
│                                                                  │
│  Fill():   new T().Initialize()  → push to Stack                │
│  Pull():   pop from Stack                                        │
│  Push(t):  push back to Stack                                    │
│                                                                  │
│  ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐     capacity = 5                     │
│  │T │ │T │ │T │ │T │ │T │  ←  all pre-created at startup        │
│  └──┘ └──┘ └──┘ └──┘ └──┘                                       │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  ObjectPool<T>                                                   │
│  Constraint: T : MonoBehaviour, IPoolable<T>                    │
│                                                                  │
│  Use for: auto-generated GameObjects (no designer-made prefab)   │
│                                                                  │
│  Fill():   new GameObject().AddComponent<T>()                   │
│  Pull():   SetActive(true)   → visible in scene                 │
│  Push(t):  SetActive(false)  → hidden, waiting in pool          │
│                                                                  │
│  Scene hierarchy (optional _useObjectsContainer):               │
│  Pool Objects/                                                   │
│    o_Timer  (inactive)                                           │
│    o_Timer  (inactive)                                           │
│    o_Timer  ← currently active (pulled)                         │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│  PrefabPool<T>  : ObjectPool<T>                                  │
│  Constraint: T : MonoBehaviour, IPoolable<T>, IPoolablePrefab   │
│                                                                  │
│  Use for: designer-built prefabs (Projectile, SFX, Impact)      │
│  Difference: Fill() uses Instantiate(_prefab) instead of        │
│              new GameObject().AddComponent<T>()                  │
│                                                                  │
│  Created manually via PoolManager.CreatePrefabPool<T>(prefab, n)│
│  (FXManager and SoundManager call this at startup)              │
│                                                                  │
│  Multiple pools of the same type T, keyed by prefab name:       │
│  Projectile pools: { "FireBall" → PrefabPool,                   │
│                      "Arrow"   → PrefabPool,                    │
│                      "IceShard"→ PrefabPool }                   │
└──────────────────────────────────────────────────────────────────┘
```

### PoolManager — Auto-Discovery and Dual Registry

```
PoolManager  (PersistentSingleton)
│
│  Awake() → Initialize()
│       │
│       ├── (optional) create "Pool Objects" container in hierarchy
│       │
│       └── CreatePools()
│               │
│               │  Reflection scan across all assemblies:
│               │  "find every non-abstract class that implements IPoolable<T>"
│               │  (excludes IPoolablePrefab — those are created on demand)
│               │
│               ├── MonoBehaviour subtype? → ObjectPool<T>
│               └── plain class?           → ClassPool<T>
│
│  Two internal registries:
│
│  _pools : Dictionary<Type, Pool>
│  ┌────────────────┬────────────────────────┐
│  │ typeof(Timer)  │ ClassPool<Timer>        │
│  │ typeof(SFX)    │ ObjectPool<SFX>         │ ← auto-discovered
│  │ typeof(Impact) │ ObjectPool<Impact>      │
│  └────────────────┴────────────────────────┘
│
│  _prefabPools : Dictionary<Type, Dictionary<string, Pool>>
│  ┌──────────────────┬──────────────────────────────────────┐
│  │ typeof(Projectile│ { "FireBall" → PrefabPool,           │
│  │                  │   "Arrow"   → PrefabPool }           │ ← manual
│  │ typeof(Impact)   │ { "Spark"   → PrefabPool,           │
│  │                  │   "Slash"   → PrefabPool }           │
│  └──────────────────┴──────────────────────────────────────┘

Static API (no Instance reference needed at call sites):
  PoolManager.GetObject<Timer>()                    → ClassPool pull
  PoolManager.ReturnObject<Timer>(t)               → ClassPool push

  PoolManager.GetPrefabObject<Projectile>("Arrow") → PrefabPool pull
  PoolManager.ReturnPrefabObject<Projectile>(p)    → PrefabPool push

  PoolManager.CreatePrefabPool<Impact>(prefab, 10) → register new pool

Object lifetime — Projectile example:
  ┌────────┐  GetObject   ┌───────────┐  Execute()  ┌──────────┐
  │  Pool  │ ──────────► │ Projectile│ ──────────► │  Scene   │
  │(stack) │             │ (active)  │             │(flying)  │
  └────────┘             └─────┬─────┘             └────┬─────┘
      ▲                        │                        │
      │                        │ on arrival/time/anim   │
      └────────────────────────┘ Dispose() ─────────────┘
         ReturnObject push          (onComplete fires first)
```

---

## Design Patterns

### Template Method — `SO_Skill`

`SO_Skill` defines the fixed algorithm: Anticipation → Execution → Outcome → Resolution. Each phase calls a **virtual method** that concrete subclasses override. Adding a new skill type means overriding only the methods that differ; the rest inherit sensible defaults.

```csharp
// SO_Skill defines the skeleton:
public virtual  IEnumerator PlayAnticipation(TurnContext ctx)      { yield return null; }
public abstract IEnumerator MoveToExecutionPosition(TurnContext ctx);
public abstract IEnumerator PlayAttackAnimation(TurnContext ctx);
public virtual  IEnumerator MoveToRestPosition(TurnContext ctx)    { /* move to Home */ }
public virtual  IEnumerator PlayMissReaction(TurnContext ctx)      { yield return null; }

// MeleeAttack only overrides what's different:
public override IEnumerator MoveToExecutionPosition(ctx) { /* dash to melee stop */ }
public override IEnumerator PlayAttackAnimation(ctx)     { /* play Melee1/2 clip */ }
public override IEnumerator MoveToRestPosition(ctx)      { /* dash back */ }

// RangeAttack stays in place:
public override IEnumerator MoveToExecutionPosition(ctx) { yield return null; }
public override IEnumerator MoveToRestPosition(ctx)      { yield return null; }
```

---

### Strategy — `SO_Controller` / `SO_TurnOrderStrategy` / `MovementStrategy`

Three independent strategy axes, each swappable via Inspector without code changes:

| Context | Strategy Interface | Concrete Implementations |
|---|---|---|
| `BattleUnit` | `SO_Controller` | `SO_HumanController`, `SO_AIController`, `SO_PassController` |
| `TurnManager` | `SO_TurnOrderStrategy` | `SpeedBasedTurnOrder`, *(extendable)* |
| `MovementHandler` | `MovementStrategy` | `LinearMovement`, `CurveMovement` |

---

### Chain of Responsibility — `DamageCalculationPipeline`

Each `IDamageModifier` reads `DamageContext.CurrentValue`, applies its transformation, and writes back. The pipeline is built once in `AffectTokenFactory` and reused per turn. New damage modifiers slot in at any position in the chain.

```csharp
var pipeline = new DamageCalculationPipeline()
    .Add(new BaseFormulaModifier())   // initializes CurrentValue
    .Add(new ElementModifier())       // multiplies by element match
    .Add(new CritModifier())          // (future) random crit multiplier
    .Add(new BuffModifier());         // (future) reads active StatModifiers
```

---

### Factory — `AffectTokenFactory` / `Factory` (Projectiles)

Two factories for different concerns:

- **`AffectTokenFactory`**: receives a `SO_Skill`, runs the damage pipeline, returns an `AffectToken` struct ready to be applied to a unit. Injected into `TurnExecutor` via constructor — swappable for tests.
- **`Factory.CreateProjectile(SO_Projectile)`**: pulls a `Projectile` from `PoolManager`, sets its data, and returns a fluent builder chain. The caller configures position, rotation, target, and callbacks before calling `.Execute()`.

---

### Observer / Event Bus — `SO_HumanController` / `Attack` / `BattleEventBus`

Three distinct event mechanisms at different scopes:

| Mechanism | Scope | Used for |
|---|---|---|
| `SO_HumanController.OnCommandRequested` (static event) | Cross-scene | UI listens for "show skill picker" |
| `Attack.OnParryWindowOpened/Closed` (static event) | Cross-object | UI + BattleManager react to parry window |
| `BattleEventBus` (instance) | Per-battle | Battle-scoped events without static coupling |
| `BattleManager.OnBattleEnd` (static event) | Cross-object | Units clean up guard/status on battle end |

---

### Object Pool — `Pool<T>` / `PoolManager`

All transient objects implement `IPoolable<T>`. `PoolManager` (PersistentSingleton) keeps a type-keyed dictionary of pools. Callers never use `new` or `Instantiate` at runtime — they call `GetObject<T>()` and `ReturnObject<T>()`. Pool size is configured per-pool and never grows.

---

### Command — `CommandToken` / `AffectToken`

Two command objects encapsulate intent:

- **`CommandToken`** — "who does what to whom": `{User, Skill, Target, Targets[], OnCompleteCallback}`. Queued up before turn execution, sorted by priority/speed, then consumed sequentially.
- **`AffectToken`** — "what damage lands": `{Action, Type, Value, IsEvadable, Accuracy, Element, OnProcessComplete, Source, OnParry}`. Created mid-turn by `AffectTokenFactory`, passed to `BattleUnit.ReceiveAffectToken()`.

---

### Flyweight — ScriptableObjects as Shared Data

`SO_UnitData`, `SO_Skill`, `SO_Projectile`, `SO_Movement`, and all other ScriptableObjects are shared assets. At battle start, `BattleUnit.Setup()` calls `Instantiate(_data)` to create a **per-unit runtime clone** — the original asset is never mutated. Skills and projectiles are never cloned; they are stateless data read by the execution code.

---

### Presenter — `UnitPresenter`

`BattleUnit` delegates all rendering concerns to a plain C# `UnitPresenter` it owns:

```
BattleUnit  (logic: stats, guard, parry, status effects, skill dispatch)
    └── UnitPresenter  (visual: SpriteRenderer, Animator, Shadow, sorting order)
```

`BattleUnit` never references `SpriteRenderer` or `Animator` directly — it calls `Presenter.PlayAnimation(clip)`, `Presenter.RaiseSortingOrder()`, etc. This makes it trivial to change the rendering approach without touching combat logic.

---

### State — `UnitState` / `OutcomeType` / Guard & Parry flags

`BattleUnit.CurrentState` is derived from `IsDead`. `TargetResolver` uses `scope.CompatibleStates` bitmask to filter valid targets, automatically skipping dead units without any manual checks.

`OutcomeType` (`Hit`, `Miss`, `NoImpact`) drives the branch inside `TurnExecutor` via a switch expression — adding a new outcome type is a single new `case`.

Guard and Parry are expressed as boolean flags + a defense multiplier rather than a formal state machine, keeping the implementation lightweight.

---

## Project Setup

**Requirements:** Unity 2022+ (URP), TextMesh Pro

1. Clone the repository
2. Open in Unity Hub → open project
3. Open `Assets/Scenes/` — run the Setup scene or the Battle scene directly
4. Assign `SO_UnitData` assets to `BattleUnit` components
5. Assign `SO_Skill` assets to unit skill sets
6. Assign a `SO_Controller` asset (`HumanController` or `AIController`) to each unit

**Debug controls (play mode):**

| Key | Action |
|-----|--------|
| `Space` | Execute queued command tokens |
| `V` | Sort command tokens (debug) |
| `T` | Toggle 0.2× slow motion |
| `M` | Toggle background music |
| `P` | Pause / resume music |
| `Backspace` | Stop music |

---

## Language Breakdown

| Language | % |
|----------|---|
| C# | 54.3% |
| ShaderLab | 37.9% |
| HLSL | 7.8% |
