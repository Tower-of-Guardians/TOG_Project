using System.Collections.Generic;
using UnityEngine;

namespace JxModule
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [BigHeader("Pool configurations")] 
        [SerializeField] private List<JxObjectPoolConfig> poolConfigs = new();

        private readonly Dictionary<GameObject, JxObjectPool> _poolDict = new();
        private readonly Dictionary<GameObject, GameObject> _instanceDict = new();
        private Transform _poolParent;

        protected override void Awake()
        {
            base.Awake();

            _poolParent = new GameObject("Object Pools").transform;
            _poolParent.SetParent(transform);

            InitializePools();
        }
        
        private void InitializePools()
        {
            foreach (var config in poolConfigs)
            {
                if (config.prefab == null)
                {
                    Debug.LogWarning("ObjectPoolManager: Pool config has null prefab, skipping.", this);
                    continue;
                }

                CreatePool(config);
            }
        }
        
        private void CreatePool(JxObjectPoolConfig config)
        {
            var poolObject = new GameObject($"Pool_{config.prefab.name}");
            poolObject.transform.SetParent(_poolParent);

            var pool = poolObject.AddComponent<JxObjectPool>();
            pool.Initialize(config.prefab, config.initialPoolSize, config.maxPoolSize, config.isExpandable);

            _poolDict[config.prefab] = pool;
        }
        
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("ObjectPoolManager: Prefab is null.", this);
                return null;
            }

            if (!_poolDict.TryGetValue(prefab, out var pool))
            {
                Debug.LogWarning($"ObjectPoolManager: Pool for prefab '{prefab.name}' not found. Creating default pool.", this);
                pool = CreateDefaultPool(prefab);
            }

            var instance = pool.Get();
            if (instance != null)
            {
                _instanceDict[instance] = prefab;
            }

            return instance;
        }
        
        public void Return(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!_instanceDict.TryGetValue(obj, out var prefab))
            {
                Debug.LogWarning("ObjectPoolManager: Cannot determine prefab from instance. Destroying object.", this);
                Destroy(obj);
                return;
            }

            if (_poolDict.TryGetValue(prefab, out var pool))
            {
                pool.Return(obj);
                _instanceDict.Remove(obj);
            }
            else
            {
                Debug.LogWarning($"ObjectPoolManager: Pool for prefab '{prefab.name}' not found. Destroying object.", this);
                _instanceDict.Remove(obj);
                Destroy(obj);
            }
        }
        
        public void ReturnSpecificPoolAll(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (_poolDict.TryGetValue(prefab, out var pool))
            {
                pool.ReturnAll();
            }
        }

        public void ReturnAll()
        {
            foreach (var pool in _poolDict.Values)
            {
                pool.ReturnAll();
            }
        }
        
        public void Clear()
        {
            foreach (var pool in _poolDict.Values)
            {
                pool.Clear();
            }

            _instanceDict.Clear();
        }
        
        public bool HasPool(GameObject prefab)
        {
            return prefab != null && _poolDict.ContainsKey(prefab);
        }

        public int GetPooledCount(GameObject prefab)
        {
            if (prefab != null && _poolDict.TryGetValue(prefab, out var pool))
            {
                return pool.PooledCount;
            }
            return 0;
        }

        public int GetActiveCount(GameObject prefab)
        {
            if (prefab != null && _poolDict.TryGetValue(prefab, out var pool))
            {
                return pool.ActiveCount;
            }
            return 0;
        }

        private JxObjectPool CreateDefaultPool(GameObject prefab)
        {
            var defaultConfig = new JxObjectPoolConfig()
            {
                prefab = prefab,
                initialPoolSize = 5,
                maxPoolSize = 25,
                isExpandable = true
            };

            CreatePool(defaultConfig);
            return _poolDict[prefab];
        }
        
        private void OnDestroy()
        {
            Clear();
        }
    }
}