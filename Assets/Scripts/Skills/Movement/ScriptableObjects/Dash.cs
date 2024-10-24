using Managers;
using System.Collections;
using UnityEngine;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newDashData", menuName = "Data/Movement Data/Dash Data")]
    public class Dash : SO_Movement
    {
        #region Fields
        [SerializeField]
        private int speed = 20; 
        private LinearMovement _movement = new LinearMovement();
        #endregion
        #region Commands
        public override IEnumerator Execute(BattleUnit user, Vector3 target)
        {
            user.ChangeAnimationState(user.Data.Motions.DashStart);    
        
            while(!user.IsAnimationFinished(user.Data.Motions.DashStart.name))
                yield return null;

            user.ChangeAnimationState(user.Data.Motions.DashLoop);
            SoundManager.PlaySound(sfxCast);

            _movement.Execute(
                user.transform, 
                target, 
                speed, 
                false, 
                () => 
                {
                    user.ChangeAnimationState(user.Data.Motions.DashEnd);
                    OnComplete?.Invoke(); 
                }
                );
 
            while (!user.IsAnimationFinished(user.Data.Motions.DashEnd.name))
                yield return null;   
        }
        #endregion
    }
}
