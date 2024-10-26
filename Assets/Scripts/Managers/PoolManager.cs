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
        private bool _useObjectsContainer;
        [SerializeField]
        private string _objectContainerName = "Pool Objects";

        private Transform _objectContainer;

        private Dictionary<Type, Pool> _pools = new Dictionary<Type, Pool>();     

        private Dictionary<Type, Dictionary<string, Pool>> _prefabPools = 
            new Dictionary<Type, Dictionary<string, Pool>>();

        #endregion
        #region Commands
        public static T GetObject<T>() where T : IPoolable<T> => Instance.GetPool<T>().Pull(); 
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
        public static void ReturnObject<T>(T item) where T : IPoolable<T> => Instance.GetPool<T>().Push(item);     
        public static void CreatePrefabPool<T>(GameObject prefab, int capacity) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab
        {      
            var type = typeof(T);

            if (!Instance._prefabPools.ContainsKey(type))            
                Instance._prefabPools[type] = new Dictionary<string, Pool>();                      
              
            if (Instance._prefabPools[type].ContainsKey(prefab.name))
            {
                Debug.LogWarning($"El prefab pool para {prefab.name} ya existe.");
                return;
            }           

            var prefabPool = new PrefabPool<T>(prefab, capacity);

            if(Instance._useObjectsContainer)
                prefabPool.SetObjectContainer(Instance._objectContainer);

            prefabPool.Initialize();

            Instance._prefabPools[type].Add(prefab.name, prefabPool);
        }
        public static T GetPrefabObject<T>(string name) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab 
            => Instance.GetPrefabPool<T>(name).Pull();
        public static T GetPrefabObject<T>(string name, Vector3 position) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab
        {
            var obj = GetPrefabObject<T>(name);
            obj.transform.position = position;
            return obj;
        }
        public static T GetPrefabObject<T>(string name, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab
        {
            var obj = GetPrefabObject<T>(name, position);
            obj.transform.rotation = rotation;
            return obj;
        }
        public static void ReturnPrefabObject<T>(T item) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab 
            => Instance.GetPrefabPool<T>(item.name).Push(item);
        
        #endregion
        #region Helpers / Utils
        private void CreatePools()
        {
            var poolableTypes = GetAllPoolableTypes();

            foreach (var type in poolableTypes)
            {
                if (typeof(IPoolablePrefab).IsAssignableFrom(type))
                    continue;                

                var poolType =
                    typeof(MonoBehaviour).IsAssignableFrom(type) ?
                    typeof(ObjectPool<>).MakeGenericType(type) :
                    typeof(ClassPool<>).MakeGenericType(type);                            
           
                _pools[type] = (Pool)Activator.CreateInstance(poolType, _defaultCapacity);

                if (_useObjectsContainer)
                    _pools[type].SetObjectContainer(Instance._objectContainer);

                _pools[type].Initialize();                
            }
        }
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
                return (IPool<T>)pool;                
        
            Debug.LogError($"No se encontró un pool para el tipo {type}");
            return null;
        }
        private IPool<T> GetPrefabPool<T>(string prefabName) where T : MonoBehaviour, IPoolable<T>, IPoolablePrefab
        {
            var type = typeof(T);

            if (_prefabPools.TryGetValue(type, out var prefabDict) &&
                    prefabDict.TryGetValue(prefabName, out var pool))
                return (pool as IPool<T>);         

            Debug.LogError($"No se encontró un pool para el prefab {prefabName} de tipo {type}");
            return null;
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
        /// deja de lado las que implementan IPoolablePrefab y los agrega al dictionary 
        /// utilizando su type como key.
        /// </summary>
        private void Initialize()
        {
            if (_useObjectsContainer) 
                _objectContainer = new GameObject(_objectContainerName).transform;         
            
            CreatePools();
        }
        #endregion
    }
}