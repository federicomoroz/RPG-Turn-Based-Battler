using UnityEngine;

namespace Skills
{
    public class CurveMovement : MovementStrategy
    {
        #region Constants
        private const int _CURVE_POINTS = 50;
        #endregion
        #region Dependencies
        private AnimationCurve _curve;
        #endregion
        #region Setup
        public CurveMovement SetCurve(AnimationCurve curve)
        {
            _curve = curve;        
            return this;
        }
        #endregion
        #region Helpers
        protected override void Move(Transform user, Vector3 origin, Vector3 target, float progress, bool rotateToTarget = false)
        {
            Vector3 newPosition = GetNewPosition(
                origin,
                target,
                progress);

            if (rotateToTarget)
                user.rotation = RotateTo(user.position, newPosition);

            user.position = newPosition;
        }
        /// <summary>
        /// Calcula la nueva posición del objeto usando la curva de animación
        /// según el tiempo transcurrido
        /// </summary>
        /// <param name="timeFraction"></param>
        /// <returns></returns>
        protected override Vector3 GetNewPosition(
            Vector3 origin, 
            Vector3 target, 
            float timeFraction)
        {
            float x = Mathf.Lerp(origin.x, target.x, timeFraction);
            float y = Mathf.Lerp(origin.y, target.y, timeFraction);

            y += _curve.Evaluate(timeFraction);

            return new Vector2(x, y); 
        }
        /// <summary>
        /// Calcula cuánto demora en llegar al punto objetivo según la curva
        /// </summary>
        /// <returns></returns>
        protected override float GetTimeToTarget(Vector3 origin, Vector3 target, float speed)
        {
            float totalDistance = 0f;
            Vector3 previousPoint = origin;

            // Integra la curva a lo largo del camino para obtener la distancia real
            for (int i = 1; i <= _CURVE_POINTS; i++)
            {
                float t = (float)i / _CURVE_POINTS;

                Vector3 currentPoint = GetNewPosition(origin, target, t);

                totalDistance += Vector3.Distance(previousPoint, currentPoint);

                previousPoint = currentPoint;
            }

            return totalDistance / speed;
        }
        #endregion
    }
}
