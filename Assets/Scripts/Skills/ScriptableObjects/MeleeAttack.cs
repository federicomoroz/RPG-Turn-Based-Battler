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
        #region Delegates
        private event System.Action _onComplete;
        #endregion
        #region Commands
        public override IEnumerator Execute(BattleUnit user, BattleUnit target, System.Action completeCallback)
        {   
            yield return MoveTo(
                user, 
                new Vector3(
                    target.unitBase.attackerBase.position.x,
                    target.unitBase.attackerBase.position.y - 0.016f,
                    0)
                );

            yield return PerformAttack(user, target);

            yield return MoveTo(user, user.unitBase.transform.position);

            completeCallback?.Invoke();
            user.ChangeAnimationState(user.Data.Motions.Idle);            

            yield break;
        }
        public override void Trigger()
        {
            SoundManager.PlaySound(sfx, _sfxVolume);
            CheckImpactSpawn();
            //hacerle daño al enemigo
        }
        #endregion
        #region Helpers
        private IEnumerator PerformAttack(BattleUnit user, BattleUnit target)
        {
            Debug.Log($"{user} perform {this.Name}");

            var targetTransform = target.transform;

            hitPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + 1, 0);
            //view        

            AnimationClip anim = user.Data.Motions.Melee1;
            
            if(animationVariant > 1)
                anim = user.Data.Motions.Melee2;         

            user.ChangeAnimationState(anim);        
            
            while (!user.IsAnimationFinished(anim.name))
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