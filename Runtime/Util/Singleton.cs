using UnityEngine;

namespace SpeedrunSim
{
    public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T instance { get; private set; }

        protected virtual void Awake()
        {
            instance = this as T;
        }

        protected virtual void OnApplicationQuit()
        {
            instance = null;
            Destroy(gameObject);
        }
    }


    public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
        
            base.Awake();
        }
    }

    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
    }
}