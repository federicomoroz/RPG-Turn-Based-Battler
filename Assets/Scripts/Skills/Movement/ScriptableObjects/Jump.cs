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
            var animation = user.Data.Motions.Jump;
            _movement.SetCurve(_curve);

            //Anticipation
            user.ChangeAnimationState(animation.Start);

            while (!user.IsAnimationFinished(animation.Start.name))       
                yield return null;                              
        
            if(sfxCast != null) 
                SoundManager.PlaySound(sfxCast);

            //Jump
             user.ChangeAnimationState(animation.Loop);

            _movement.Execute(
                user.transform,
                target,
                speed,
                false,
                () => 
                {
                    if(_sfxEnd != null)
                        SoundManager.PlaySound(_sfxEnd);

                    user.ChangeAnimationState(animation.End); 
                }
                );

            //Land
            while (!user.IsAnimationFinished(animation.End.name))
                yield return null;
        }
        #endregion
    }
}