using UnityEngine;

namespace KDGame.Base
{
    public interface IMgr
    {
        void Restart();
    }

    public class MgrBase : MonoSingleton<MgrBase>, IMgr
    {
        protected override void OnAwake()
        {
            base.OnAwake();
            Debug.Log($"Manager is awaken, name: {nameof(MgrBase)}");
        }

        public void Restart()
        {
            Debug.Log($"Manager is restarted, name: {nameof(MgrBase)}");
        }
    }
}