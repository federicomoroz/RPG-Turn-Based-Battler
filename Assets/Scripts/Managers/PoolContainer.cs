using System;
using UnityEngine;

namespace Pool
{
    [System.Serializable]
    public class PoolContainer
    {
        #region Fields
        [HideInInspector]
        public string name;
        public int stock;
        public Pool pool;
        public Type type;
        #endregion
        #region Constructor
        public PoolContainer(Pool pool, Type type) 
        {
            this.pool = pool;
            this.type = type;      
            this.name = this.type.ToString();           
        }
        #endregion
        #region Commands
        public PoolContainer UpdateStock()
        {
            stock = pool.stock;
            return this;
        }
        #endregion
    }
}
