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
        #region Fields
        private Vector3 hitPosition;
        #endregion
        #region Dependencies
        [SerializeField]
        private AudioClip sfx;
        [SerializeField]
        private SO_Projectile impact;
        #endregion
        #region Delegates
        private event System.Action _onComplete;
        #endregion
        #region Commands
        public override IEnumerator Execute(BattleUnit user, BattleUnit target, System.Action completeCallback)
        {   
            yield return user.Data.MovementType          
                .Execute(user, target.unitBase.attackerBase.position);           

            yield return PerformAttack(user, target);

            yield return user.Data.MovementType  
                .Execute(user, user.unitBase.transform.position);

            completeCallback?.Invoke();
            user.ChangeAnimationState(user.Data.Motions.Idle);

            yield break;
        }
        public override void Trigger()
        {
            SoundManager.PlaySound(sfx);
            CheckImpactSpawn();
            //hacerle daño al enemigo
        }
        #endregion
        #region Helpers
        private IEnumerator PerformAttack(BattleUnit user, BattleUnit target)
        {
            hitPosition = new Vector3(target.transform.position.x, target.transform.position.y + 1, 0);
            //view        
            user.ChangeAnimationState(user.Data.Motions.Melee);
        
            //evento de feedback visual y audio
            while (!user.IsAnimationFinished(user.Data.Motions.Melee.name))
                yield return null;
            hitPosition = Vector3.zero;
        }
        protected int CalculateDamage(BattleUnit user, BattleUnit target)
        {
            int damage = user.Data.Stats.physicalStrength - (target.Data.Stats.physicalDefense / 2);
            return damage;
        }
        private void CheckImpactSpawn()
        {
            if (impact != null)
                Factory.CreateProjectile(impact)
                .SetPosition(hitPosition)
                .Execute();
        }

        #endregion
    }
}