using System;
using System.Collections;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using KDGame.UI;
using UnityEngine;

namespace KDGame
{
	public class MainCtrl : MonoSingleton<MainCtrl>
	{
		private List<IMgr> _mgrList = new List<IMgr>();

		public static Transform Trans => Instance.transform;

		protected override void OnAwake()
		{
			base.OnAwake();
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			AddMgr<AssetMgr>();
			AddMgr<UIMgr>();
			Preload();
		}

		private IMgr AddMgr<T>() where T : MonoSingleton<T>, IMgr
		{
			var mgr = gameObject.AddComponent<T>();
			_mgrList.Add(mgr);
			return mgr;
		}

		private void Preload()
		{
			// Do load, maybe async?
			// Destroy the splash screen
			// GameObject.Destroy(GameObject.Find("Splash"));
		}

		public void Restart()
		{
			foreach (var mgr in _mgrList)
			{
				mgr.Restart();
			}
		}
	}
}