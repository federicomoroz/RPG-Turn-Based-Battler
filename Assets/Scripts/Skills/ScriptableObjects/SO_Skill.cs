using System.Collections;
using UnityEngine;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newSkillData", menuName = "Data/Skill Data/Skill")]
    public abstract class SO_Skill : ScriptableObject
    {
        #region Fields
        public Sprite icon;
        public string Name;
        public string description;
        public int costMP;
        public Scope scope = new Scope();
        public Invocation invocation = new Invocation();
        public Message message = new Message();
        public Affect affect = new Affect();
        public Priority priority;
        [Range(1, 2)]
        public ushort animationVariant;
        #endregion
        #region Commands
        public IEnumerator Execute(BattleUnit user, Vector3 targetPosition, Vector3 executionPosition, System.Action completeCallback)
        {
            yield return user.StartCoroutine(PreparationPhase(user, executionPosition, completeCallback));
            yield return user.StartCoroutine(ExecutionPhase(user, targetPosition));
            yield return user.StartCoroutine(ResolutionPhase(user));
            FinalizeExecution(user);
        }
        public abstract void Trigger(BattleUnit user);
        #endregion
        #region Helpers / Utils
        protected IEnumerator MoveTo(BattleUnit user, Vector3 position)
        {
            yield return user.Data.MovementType.Execute(user, position);
        }
        protected IEnumerator PerformAttack(BattleUnit user, AnimationClip animation)
        {
            user.ChangeAnimationState(animation);
            while (!user.IsAnimationFinished(animation.name))
                yield return null;
        }
        protected AnimationClip SelectAttackAnimation(    
            AnimationClip anim1, 
            AnimationClip anim2
            )        
            => animationVariant > 1 ? anim2 : anim1;        
        protected IEnumerator ReturnToStartPosition(BattleUnit user)
        {
            yield return MoveTo(user, user.unitBase.transform.position);
        }
        #endregion
        #region Phases
        protected abstract IEnumerator PreparationPhase(BattleUnit user, Vector3 executionPosition, System.Action completeCallback);
        protected abstract IEnumerator ExecutionPhase(BattleUnit user, Vector3 targetPosition);
        protected abstract IEnumerator ResolutionPhase(BattleUnit user);
        private void FinalizeExecution(BattleUnit user) 
        {            
            user.ChangeAnimationState(user.Data.Motions.Idle);
        }        
        #endregion
    }
}
