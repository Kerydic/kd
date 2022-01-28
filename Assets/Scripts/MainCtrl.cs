using System;
using System.Collections;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using KDGame.UI;

namespace KDGame
{
	public class MainCtrl : MonoSingleton<MainCtrl>
	{
		private List<IMgr> _mgrList = new List<IMgr>();
		
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

		private IMgr AddMgr<T>() where T:IMgr
		{
			var mgr = gameObject.AddComponent<UIMgr>();
			_mgrList.Add(mgr);
			return mgr;
		}

		private void Preload()
		{
			UIMgr.Instance.PlaySplash();
			// Do load
			UIMgr.Instance.DestroySplash();
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