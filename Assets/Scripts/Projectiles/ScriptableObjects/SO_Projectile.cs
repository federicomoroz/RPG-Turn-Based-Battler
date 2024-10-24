using UnityEngine;
using Skills;

namespace Projectiles
{
    [CreateAssetMenu(fileName = "newProjectileData", menuName = "Data/Projectile Data/Projectile Data")]
    public class SO_Projectile : ScriptableObject
    {
        #region Fields
        public string Name { get; private set; } = "Projectile";
        public DestructionCondition destructionCondition;
        public MovementData movementData = new MovementData();
        public float lifeTime;
        #endregion
        #region Dependencies
        public SO_ObjectView viewData;
        public SO_Projectile[] impacts;
        #endregion
    }
}