using System.Collections.Generic;
using UnityEngine;

namespace JxModule
{
    public class JxObjectPool : MonoBehaviour
    {
        private GameObject _objectPrefab;
        private int _initialPoolSize;
        private int _maxPoolSize;
        private bool _isExpandable = true;
        
        private Queue<GameObject> _objectPool = new();
        private HashSet<GameObject> _activeObjects = new();
        private HashSet<GameObject> _allObjects = new();
        
        public int PooledCount => _objectPool.Count;
        public int ActiveCount => _activeObjects.Count;
        public int TotalCount => _allObjects.Count;

        public void Initialize(GameObject prefab, int initialPoolSize, int maxPoolSize, bool isExpandable)
        {
            _objectPrefab = prefab;
            _initialPoolSize = initialPoolSize;
            _maxPoolSize = maxPoolSize;
            _isExpandable = isExpandable;

            if (prefab == null)
            {
                Debug.LogError("Object Pool: Prefab is not assigned", this);
                return;
            }

            InitializePool();
        }

        public GameObject Get()
        {
            GameObject obj;

            if (PooledCount > 0)
            {
                obj = _objectPool.Dequeue();
            }
            else if (_isExpandable)
            {
                obj = Instantiate(_objectPrefab, transform);
                _allObjects.Add(obj);
            }
            else
            {
                return null;
            }
            
            obj.SetActive(true);
            _activeObjects.Add(obj);
            return obj;
        }

        public void Return(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (!_activeObjects.Contains(obj))
            {
                return;
            }
            
            _activeObjects.Remove(obj);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            if (PooledCount < _maxPoolSize)
            {
                _objectPool.Enqueue(obj);
            }
            else
            {
                _allObjects.Remove(obj);
                Destroy(obj);
            }
        }

        public void ReturnAll()
        {
            var objs = new List<GameObject>(_activeObjects);
            foreach (var obj in objs)
            {
                Return(obj);
            }
        }

        public void Clear()
        {
            ReturnAll();

            while (PooledCount > 0)
            {
                var obj = _objectPool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            
            _objectPool.Clear();
            _activeObjects.Clear();
            _allObjects.Clear();
        }

        private void InitializePool()
        {
            for (var i = 0; i < _initialPoolSize; i++)
            {
                CreatePooledObject();
            }
        }

        private GameObject CreatePooledObject()
        {
            var obj = Instantiate(_objectPrefab, transform);
            obj.SetActive(false);
            
            _allObjects.Add(obj);
            _objectPool.Enqueue(obj);
            
            return obj;
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}