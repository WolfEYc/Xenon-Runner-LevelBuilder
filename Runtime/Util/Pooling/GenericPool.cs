using UnityEngine;
using UnityEngine.Pool;

namespace SpeedrunSim
{
    public abstract class GenericPool<T> : Singleton<GenericPool<T>> where T : PooledObject<T>
    {
        [SerializeField] T prefab;
        public Transform Transform { get; private set; }

        ObjectPool<T> _objectPool;

        protected override void Awake()
        {
            base.Awake();
            Transform = transform;
            _objectPool = new ObjectPool<T>(CreatePooledObject, ActionOnGet, ActionOnRelease);
        }
        
        void ActionOnRelease(T obj)
        {
            obj.GameObject.SetActive(false);
        }

        void ActionOnGet(T obj)
        {
            obj.GameObject.SetActive(true);
        }

        T CreatePooledObject()
        {
            return Instantiate(prefab, Transform);
        }
        
        public T Get()
        {
            return _objectPool.Get();
        }
        
        public void Release(T element)
        {
            _objectPool.Release(element);
        }
        
        public int CountInactive => _objectPool.CountInactive;
        public int CountActive => _objectPool.CountActive;
        public int CountAll => _objectPool.CountAll;
    }
}
