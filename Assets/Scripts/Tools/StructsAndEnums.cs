using UnityEngine;

namespace BattleUnits
{
    public enum States
    {
        Waiting,
        Ready,
        Performing,
        Dead,
    }
}
namespace Sprites
{
    [System.Serializable]
    public struct UnitSpritesSet
    {    
        public Sprite[] spritesheet;
        public Sprite portrait;
    }
    [System.Serializable]
    public struct SpriteContainer
    {
        public Sprite sprite;
        public bool playSfx;
        public AudioClip sfx;
        [Range(0,1)]
        public float volume;
    }

    [System.Serializable]
    public struct PhasedAnimation
    {
        public AnimationClip 
            Start, 
            Loop, 
            End;
    }
}
namespace Sound
{
    [System.Serializable] public struct ProjectileSfxContainer
    {
        public AudioClip
            cast,
            loop,
            impact;
    }
    [System.Serializable] public struct GlobalSfx
    {
        public string name;
        public AudioClip sfx;
    }
}
namespace Skills
{
    public enum MovementType
    {
        TargetedLinear,
        TargetedCurve,
        DistanceLinear,
        None,
    }
    [System.Serializable]
    public struct MovementData
    {
        public bool rotate => _rotate;
        public int speed => _speed;
        public MovementType movementType => _movementType;
        public AnimationCurve trajectoryCurve => _trajectoryCurve;
        public int trajectoryLength => _trajectoryLength;

        [SerializeField]
        private bool _rotate;
        [SerializeField]
        [Range(0f, 20f)]
        private int _speed;
        [SerializeField]
        private MovementType _movementType;

        [SerializeField]
        private AnimationCurve _trajectoryCurve;
        [SerializeField]
        private int _trajectoryLength;
    }
    [System.Serializable]
    public struct SkillSet
    {
        public SO_Skill defaultAttack;
        public SO_Skill[] physicalSkills;
        public SO_Skill[] specialSkills;
        public SO_Skill defend;
    }
    public struct AffectToken
    {
        public AffectAction Action { get; }
        public AffectType Type { get; }
        public int Value { get; }
        public bool IsEvadable { get; }
        public float SuccessRatio { get; }
        public Element Element { get; }
        public System.Action OnProcessCompleteCallback { get; }

        public AffectToken(
            AffectAction action, 
            AffectType type, 
            int value, 
            bool isEvadable, 
            float sucessRatio, 
            Element element,
            System.Action onProcessCompleteCallback)
        {
            Action = action;
            Type = type;
            Value = value;
            IsEvadable = isEvadable;
            SuccessRatio = sucessRatio;  
            Element = element;
            OnProcessCompleteCallback = onProcessCompleteCallback;
        }
    }
    public enum AffectAction
    {
        HPDamage,
        MPDamage,
        HPRecover,
        MPRecover,
        HPDrain,
        MPDrain,
    }
    public enum AffectType
    {
        Physical,
        Special,
    }
    [System.Serializable]
    public struct Affect
    {
        public AffectAction action;
        public AffectType type;
        [Range(0, 999)]
        public int basePower;
        [Range(0, 100)]
        public int variance;
        [HideInInspector]
        public Element element;

    }
    public enum Side
    {
        None,
        Enemy,
        Ally,
        All,
        User,
    }
    [System.Serializable]
    public struct Scope
    {
        public Side side;
        public Number number;
    }
    [System.Serializable]
    public struct Invocation
    {
        [Range(0, 1)]
        public float accuracy;
        public bool isEvadable;
    }

    [System.Serializable]
    public struct Message
    {
        public string line1;
        public string line2;
    }
    public enum Number
    {
        One,
        All,
    }
    public enum Priority
    {
        LOW,
        REGULAR,
        HIGH,
    }
    [System.Serializable]
    public struct Effectiveness
    {
        [HideInInspector] public string name;
        public Element element;
        public Type type;

        public enum Type
        {
            Half,
            Neutral,
            Double,
            Inmune,
        }
    }
}
namespace Commander
{
    [System.Serializable]
    public struct CommandToken
    {
        public Skills.SO_Skill Skill;

        public BattleUnits.UnitBase 
            User, 
            Target;
        public System.Action OnCompleteCallback;
    }

    [System.Serializable]
    public struct CommandRequest
    {
        public CommandToken commandToken;
        public System.Action<CommandToken> OnComplete;

        public CommandRequest(CommandToken commandToken, System.Action<CommandToken> onComplete)
        {
            this.commandToken = commandToken;
            OnComplete = onComplete;
        }          
    }
}
namespace Projectiles
{
    public enum DestructionCondition
    {
        OnImpact,
        OnTime,
        OnAnimationEnd,
    }
}
namespace Pool
{
    [System.Serializable]
    public struct PoolablePrefabSlot
    {
        [HideInInspector]
        public string name => prefab.name;

        public GameObject prefab;
        public int amount;
    }
}