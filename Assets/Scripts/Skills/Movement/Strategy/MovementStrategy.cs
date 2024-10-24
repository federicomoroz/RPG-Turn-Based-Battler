using UnityEngine;
using Pool;

namespace Skills
{
    public abstract class MovementStrategy
    {
        #region Commands
        /// <summary>
        /// El método es el encargado de recibir y ejecutar la orden de moverse
        /// </summary>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <param name="onArriveCallback"></param>
        /// <returns></returns>    
        public virtual MovementStrategy Execute(
            Transform user,
            Vector3 target,
            int speed,
            bool rotateToTarget = false,
            System.Action onArriveCallback = null,
            System.Action stopAction = null
            )
        {
            var origin = user.position;

            var timer = PoolManager.GetObject<Timer>();

            timer.SetTime(GetTimeToTarget(origin, target, speed))
                .SetCompleteCallback(onArriveCallback)
                .SetStopAction(ref stopAction)
                .SetChangeCallback(() => Move(user, origin, target, timer.Ratio, rotateToTarget));

            return this;
        }
        #endregion        
        #region Helpers / Utils
        /// <summary>
        /// "¿Cuál es el origen? ¿Cuál es el destino? ¿Cuál es el progreso del trayecto? ¿Debo rotar?
        /// </summary>
        /// <param name="user"></param>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="progress"></param>
        /// <param name="rotateToTarget"></param>
        protected abstract void Move(Transform user, Vector3 origin, Vector3 target, float progress, bool rotateToTarget = false);

        /// <summary>
        /// Calcula la nueva posición según el timer
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="timeFraction"></param>
        /// <returns></returns>
        protected abstract Vector3 GetNewPosition(
            Vector3 origin,
            Vector3 target,
            float timeFraction
            );
        /// <summary>
        /// Calcula la rotación hacia el target
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        protected Quaternion RotateTo(
            Vector3 currentPosition,
            Vector3 newPosition
            )
        {
            Vector3 direction = newPosition - currentPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            return Quaternion.Euler(0, 0, angle);
        }
        /// <summary>
        /// Calcula cuánto demora en llegar a la posición objetivo
        /// </summary>
        /// <returns></returns>
        protected virtual float GetTimeToTarget(
            Vector3 origin,
            Vector3 target,
            float speed
            )
        {
            float distance = Vector3.Distance(origin, target);
            return distance / speed;
        }
        #endregion     
    }
}
