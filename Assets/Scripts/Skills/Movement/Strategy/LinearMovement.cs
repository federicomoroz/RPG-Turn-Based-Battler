using UnityEngine;

namespace Skills
{
    public class LinearMovement : MovementStrategy
    {
        #region Helpers / Utils
        protected override void Move(Transform user, Vector3 origin, Vector3 target, float progress, bool rotateToTarget = false)
        {
            var newPosition = GetNewPosition(
                origin,
                target,
                progress
                );

            if (rotateToTarget)
                user.rotation = RotateTo(user.position, newPosition);
        
            user.position = newPosition;
        }
        protected override Vector3 GetNewPosition(Vector3 origin, Vector3 target, float timeFraction) 
            => Vector3.Lerp(origin, target, timeFraction);
        #endregion
    }
}
