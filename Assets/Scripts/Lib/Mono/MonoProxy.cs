using System;
using System.Collections;
using UnityEngine;

namespace KDGame.Lib
{
	public class MonoProxy : MonoBehaviour
	{
		private static MonoProxy s_proxy;

		private static MonoProxy proxy
		{
			get
			{
				if (s_proxy == null)
				{
					GameObject go = new GameObject("MonoProxy");
					go.transform.parent = MainCtrl.Trans;
					s_proxy = go.AddComponent<MonoProxy>();
				}

				return s_proxy;
			}
		}

		private MonoProxy()
		{
		}

		#region UpdateProxy

		private event Action UpdateEvtHandler;

		public static void AddUpdate(Action evt)
		{
			proxy.UpdateEvtHandler += evt;
		}

		public static void RmUpdate(Action evt)
		{
			proxy.UpdateEvtHandler -= evt;
		}

		void Update()
		{
			UpdateEvtHandler?.Invoke();
		}

		#endregion

		#region CoroutineProxy

		public static Coroutine StartCo(IEnumerator coroutine)
		{
			return proxy.StartCoroutine(coroutine);
		}

		public static void StopCo(Coroutine coroutine)
		{
			if (coroutine != null)
				proxy.StopCoroutine(coroutine);
		}

		public static void StopAllCo()
		{
			proxy.StopAllCoroutines();
		}

		#endregion
	}
}