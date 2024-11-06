using System.Collections;
using UnityEngine;
using Managers;
using Projectiles;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newMeleeAttackData", menuName = "Data/Skill Data/Melee Attack")]
    public class MeleeAttack : SO_Skill
    {
        #region Delegates
        private event System.Action _onComplete;
        #endregion          
        #region Constants
        private const float ON_ATTACK_Y_OFFSET = -0.016f;
        private const float ON_ATTACK_X_OFFSET = -2f;
        #endregion
        #region Fields
        [SerializeField]
        [Range(0, 1f)]
        private float _sfxVolume = 0.85f;
        #endregion
        #region Dependencies
        [SerializeField]
        private AudioClip sfx;
        [SerializeField]
        private SO_Projectile impact;
        #endregion
        #region Commands
        public override void Trigger(BattleUnit user)
        {
            SoundManager.PlaySound(sfx, _sfxVolume);
        }
        #endregion
        #region Phases
        protected override IEnumerator PreparationPhase(BattleUnit user, Vector3 executionPosition, System.Action completeCallback)
        {
            _onComplete = completeCallback;
            Vector3 attackPosition = CalculateAttackPosition(user, executionPosition);
            yield return MoveTo(user, attackPosition);
        }
        protected override IEnumerator ExecutionPhase(BattleUnit user, Vector3 targetPosition)
        {
            AnimationClip anim = SelectAttackAnimation(user.Data.Motions.Melee1, user.Data.Motions.Melee2);
            yield return PerformAttack(user, anim);
        }
        protected override IEnumerator ResolutionPhase(BattleUnit user)
        {
            yield return ReturnToStartPosition(user);
            _onComplete?.Invoke();
            _onComplete = null;
        }
        #endregion
        #region Helpers / Utils
        private Vector3 CalculateAttackPosition(BattleUnit user, Vector3 executionPosition)
        {
            float direction = Mathf.Sign(executionPosition.x - user.unitBase.transform.position.x);
            float xOffset = ON_ATTACK_X_OFFSET * direction;
            return new Vector3(executionPosition.x + xOffset, executionPosition.y + ON_ATTACK_Y_OFFSET, 0);
        }
        #endregion
    }
}
