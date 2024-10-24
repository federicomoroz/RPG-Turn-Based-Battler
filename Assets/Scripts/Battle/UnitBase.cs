using UnityEngine;

namespace BattleUnits
{
    public class UnitBase : MonoBehaviour
    {
        public Transform attackerBase { get; private set; }
        public BattleUnit host;
        private void Awake()
        {
            attackerBase = transform.GetChild(0);
            host.SetUnitBase(this);
        }

        public UnitBase SetHost(BattleUnit host)
        {
            this.host = host;
            return this;
        }
}
}
