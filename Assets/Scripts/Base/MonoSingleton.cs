using UnityEngine;

namespace KDGame.Base
{
    // 通用单例类
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    return null;
                return _instance;
            }
        }

        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
        }
    }
}