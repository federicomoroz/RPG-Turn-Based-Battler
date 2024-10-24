using System.Collections.Generic;
using UnityEngine;

namespace BattleUnits
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class BattleUnit : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private SO_UnitData _data;
        public UnitBase unitBase { get; private set; }
        [HideInInspector] public Transform shootPoint;    
  
        private Animator _animator;
        private SpriteRenderer _sr;
        #endregion
        #region Properties
        public SO_UnitData Data { get; private set; }
        public Animator Animator => _animator;     
        public SpriteRenderer Sr => _sr;
        #endregion
        #region Setup
        public BattleUnit SetUnitBase(UnitBase unitBase) 
        { 
            this.unitBase = unitBase; 
            return this;
        }
        #endregion
        #region Skills
        public void TriggerSkill() => BattleManager.TriggerSkill();
        #endregion
        #region Animation
        private void SetAnimations()
        {
            var animatorOverride = new AnimatorOverrideController(_animator.runtimeAnimatorController);

            Dictionary<string, AnimationClip> motionMap = new Dictionary<string, AnimationClip>
            {
                { "Idle", _data.Motions.Idle },
                { "Melee1", _data.Motions.Melee1 },
                { "Melee2", _data.Motions.Melee2 },
                { "Range1", _data.Motions.Range1 },
                { "Range2", _data.Motions.Range2 },
                { "Damage", _data.Motions.Damage },
                { "DashStart", _data.Motions.Dash.Start },
                { "DashLoop", _data.Motions.Dash.Loop },
                { "DashEnd", _data.Motions.Dash.End },
                { "JumpStart", _data.Motions.Jump.Start },
                { "JumpLoop", _data.Motions.Jump.Loop },
                { "JumpEnd", _data.Motions.Jump.End }
            };

            foreach (var state in motionMap)
            {
                if (state.Value == null) 
                    continue;            

                animatorOverride[state.Key] = state.Value;            
            }

            _animator.runtimeAnimatorController = animatorOverride;
        }
        public void ChangeAnimationState(AnimationClip animation) { Animator.Play(animation.name); }
        public bool IsAnimationFinished(string animationName)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1;
        }
        public AnimationClip GetAnimationClip(Animator animator, string animationName)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)            
                    return clip;            
            }
            return null;
        }
        #endregion
        #region Stats Actions
        public void HealHp(int amount) => Data.Stats.hp.Add(amount);
        public void HealMp(int amount) => Data.Stats.mp.Add(amount);
        public void TakeHp(int amount) => Data.Stats.hp.Subtract(amount);
        public void TakeMp(int amount) => Data.Stats.mp.Subtract(amount);
        #endregion
        #region Lifecycle
        private void Awake()
        {
            if(_data == null)
            {
                Debug.LogWarning($"BattleUnitData missing");
                return;
            }

            Data = Instantiate(_data);
            _animator = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();

            SetAnimations();
        
            shootPoint = transform.GetChild(0).transform;

            if (Data.Motions != null)
                ChangeAnimationState(Data.Motions.Idle);
        }
        #endregion
    }
}
