using UnityEngine;

namespace Pool
{
    public class PrefabPool<T> : ObjectPool<T>, IPool<T> where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab
    {
        #region Dependencies
        private GameObject _prefab;
        #endregion
        #region Constructor
        public PrefabPool(GameObject prefab, int capacity) : base(capacity)
        {                                  
            _prefab = prefab;         
        }
        #endregion
        #region Helpers / Utils
        protected override T Create()
        {    
            GameObject go = GameObject.Instantiate(_prefab);

            if (parent != null)
                go.transform.parent = parent;

            go.name = $"o_{_prefab.name}";
            return go.GetComponent<T>(); 
        }
        #endregion
    }
}
