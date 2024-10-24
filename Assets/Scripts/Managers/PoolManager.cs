using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pool
{
    public class PoolManager : PersistentSingleton<PoolManager>
    {
        #region Fields
        [SerializeField] 
        private int _defaultCapacity;

        [SerializeField]
        private bool _debug;

        [SerializeField]
        private Dictionary<Type, Pool> _pools = new Dictionary<Type, Pool>();     

        [SerializeField]
        private List<PoolContainer> _poolsContainer = new List<PoolContainer>();
        #endregion
        #region Commands
        public static T GetObject<T>() where T : IPoolable<T>
        {     
            Instance.UpdateView<T>();
      
            return Instance.GetPool<T>().Pull(); 
        }
        public static T GetObject<T>(Vector3 position) where T : MonoBehaviour, IPoolable<T>
        {
            var obj = GetObject<T>();
            obj.transform.position = position;          
            return obj;
        }
        public static T GetObject<T>(Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable<T>
        {
            var obj = GetObject<T>(position);
            obj.transform.rotation = rotation;
            return obj;
        }
        public static void ReturnObject<T>(T item) where T : IPoolable<T>
        {        
            var pool = Instance.GetPool<T>();
            pool.Push(item);

            Instance.UpdateView<T>();       
        }
        #endregion
        #region Helpers / Utils
        /// <summary>
        /// Devuelve una lista con todas las clases no abstractas
        /// que implementan la interface IPoolable<T>
        /// </summary>
        /// <returns></returns>
        private List<Type> GetAllPoolableTypes()
        {
            return (from assembly in GameManager.Instance.GetAssemblies()
                    from type in assembly.GetTypes()
                    let interfaces = type.GetInterfaces()
                    where type.IsClass && !type.IsAbstract
                    from i in interfaces
                    where i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IPoolable<>)
                        && typeof(IPoolable<>).MakeGenericType(type).IsAssignableFrom(type)
                    select type).ToList();
        }
        private IPool<T> GetPool<T>() where T : IPoolable<T>
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool))                       
                return pool as IPool<T>;                
        
            Debug.LogError($"No se encontró un pool para el tipo {type}");
            return null;
        }
        private void UpdateView<T>()
        {
            if (_debug)
            {
                var poolContainer = _poolsContainer.FirstOrDefault(pool => pool.type == typeof(T));
                poolContainer.UpdateStock();
            }
        }
        #endregion
        #region Lifecycle
        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        /// <summary>
        /// Crea un pool por cada clase que implementa la interface IPoolable<T>
        /// y los agrega al dictionary utilizando su type como key.
        /// </summary>
        private void Initialize()
        {
            // Cachear todas las clases que implementan IPoolable<T>
            var poolableTypes = GetAllPoolableTypes();
        
            foreach (var type in poolableTypes)
            {
                Type poolType;

                poolType = 
                    typeof(MonoBehaviour).IsAssignableFrom(type) ? 
                    typeof(ObjectPool<>).MakeGenericType(type) : 
                    typeof(Pool<>).MakeGenericType(type);
            
                var poolInstance = Activator.CreateInstance(poolType, _defaultCapacity);
            
                _pools[type] = poolInstance as Pool;

                if(_debug) 
                    _poolsContainer.Add(new PoolContainer(poolInstance as Pool, type).UpdateStock());
            }
        }
        #endregion
    }
}
