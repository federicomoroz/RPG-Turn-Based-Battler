using System.Collections;
using UnityEngine;
using Projectiles;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newRangeAttackData", menuName = "Data/Skill Data/Range Attack")]
    public class RangeAttack : SO_Skill
    {
        #region Delegates
        private event System.Action _onComplete;
        #endregion                
        #region Dependencies
        private BattleUnit user, target;
        [SerializeField]
        private SO_Projectile _projectile;
        #endregion
        #region Commands
        public override IEnumerator Execute(BattleUnit user, BattleUnit target, System.Action completeCallback)
        {
            
            _onComplete = completeCallback;
            this.user = user;
            this.target = target;

            AnimationClip animation = user.Data.Motions.Range1;

            if (animationVariant > 1)
                animation = user.Data.Motions.Range2;

            user.ChangeAnimationState(animation);

            while(!user.IsAnimationFinished(animation.name))
                yield return null;
  
            user.ChangeAnimationState(user.Data.Motions.Idle);
        }
        public override void Trigger()
        {
            var p = Factory.CreateProjectile(_projectile)
                .SetCompleteCallback(ref _onComplete)           
                .SetRotation(user.transform.localScale.x == -1 ? Quaternion.Euler(0,0,180) : Quaternion.identity);

            if (_projectile.movementData.movementType == MovementType.None)
                p.SetPosition(target.transform.position);
            else
                p.SetPosition(user.shootPoint.position)
                .SetTarget(new Vector3(target.transform.position.x, target.transform.position.y + 1, 0));

            p.Execute();

            user = null;
            target = null;
        }
        #endregion
    }
}