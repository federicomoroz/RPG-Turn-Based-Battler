using System.Collections;
using UnityEngine;
using Managers;
using BattleUnits;

namespace Skills
{
    [CreateAssetMenu(fileName = "newJumpData", menuName = "Data/Movement Data/Jump Data")]
    public class Jump : SO_Movement
    {
        #region Fields
        [SerializeField] private AudioClip _sfxEnd;
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private int speed = 10;
        private CurveMovement _movement = new CurveMovement();
        #endregion
        #region Commands
        public override IEnumerator Execute(BattleUnit user, Vector3 target)
        {
            _movement.SetCurve(_curve);

            //Anticipation
            user.ChangeAnimationState(user.Data.Motions.JumpStart);

            while (!user.IsAnimationFinished(user.Data.Motions.JumpStart.name))       
                yield return null;                              
        
            if(sfxCast != null) 
                SoundManager.PlaySound(sfxCast);

            //Jump
             user.ChangeAnimationState(user.Data.Motions.JumpLoop);

            _movement.Execute(
                user.transform,
                target,
                speed,
                false,
                () => 
                {
                    if(_sfxEnd != null)
                        SoundManager.PlaySound(_sfxEnd);

                    user.ChangeAnimationState(user.Data.Motions.JumpEnd); 
                }
                );

            //Land
            while (!user.IsAnimationFinished(user.Data.Motions.JumpEnd.name))
                yield return null;
        }
        #endregion
    }
}