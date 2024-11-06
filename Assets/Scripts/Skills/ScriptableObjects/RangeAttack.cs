using System.Collections;
using UnityEngine;
using Projectiles;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newRangeAttackData", menuName = "Data/Skill Data/Range Attack")]
    public class RangeAttack : SO_Skill
    {
        #region Constants
        private const float Y_TARGETPOS_OFFSET = 1;
        #endregion
        #region Fields
        private Vector3 target;
        #endregion
        #region Delegates
        private event System.Action _onComplete;
        #endregion
        #region Dependencies
        [SerializeField]
        private SO_Projectile _projectile;
        #endregion
        #region Commands
        public override void Trigger(BattleUnit user)
        {
            var p = Factory.CreateProjectile(_projectile)
                .RegisterCompleteCallback(ref _onComplete)
                .SetRotation(user.transform.localScale.x == -1 ? Quaternion.Euler(0, 0, 180) : Quaternion.identity);

            if (_projectile.movementData.movementType == MovementType.None)
                p.SetPosition(target);
            else
                p.SetPosition(user.shootPoint.position)
                 .SetTarget(new Vector3(target.x, target.y + Y_TARGETPOS_OFFSET, target.y));

            p.Execute();
            target = Vector3.zero;
        }
        #endregion
        #region Phases
        protected override IEnumerator PreparationPhase(BattleUnit user, Vector3 executionPosition, System.Action completeCallback)
        {
            _onComplete = completeCallback;
            yield return null;
        }
        protected override IEnumerator ExecutionPhase(BattleUnit user, Vector3 targetPosition)
        {
            target = targetPosition;
            AnimationClip anim = SelectAttackAnimation(user.Data.Motions.Range1, user.Data.Motions.Range2);
            yield return PerformAttack(user, anim);       
        }
        protected override IEnumerator ResolutionPhase(BattleUnit user)
        { 
            _onComplete = null;
            yield return null;
        }
        #endregion
    }
}
