using System;
using System.Collections;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using KDGame.Module;
using KDGame.UI;
using UnityEngine;

namespace KDGame
{
	public class MainCtrl : MonoSingleton<MainCtrl>
	{
		private List<IMgr> _mgrList = new List<IMgr>();

		public static Transform Trans => Instance.transform;

		public MainCamera MainCamera
		{
			get => _mainCamera;
			set => _mainCamera = value;
		}
		[SerializeField] private MainCamera _mainCamera;

		protected override void OnAwake()
		{
			base.OnAwake();
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			SetGameFrame(60);
			// Add basic Mgr
			AddMgr<AssetMgr>();
			AddMgr<UIMgr>();
			// Start game logic
			OnLaunch();
		}

		private IMgr AddMgr<T>() where T : MonoSingleton<T>, IMgr
		{
			var mgr = gameObject.AddComponent<T>();
			_mgrList.Add(mgr);
			return mgr;
		}

		public void Restart()
		{
			foreach (var mgr in _mgrList)
			{
				mgr.Restart();
			}
		}

		public void SetGameFrame(int frame)
		{
			Application.targetFrameRate = frame;
		}

		#region Main Logic

		private HashSet<LogicCtrl> _ctrlSet;

		// 游戏开始
		private void OnLaunch()
		{
			UIMgr.Instance.SetMainCamera(_mainCamera);
			UIMgr.Instance.CreateLayers();
			Destroy(GameObject.Find("Splash"));

			_ctrlSet = new HashSet<LogicCtrl>();
			_ctrlSet.Add(new LaunchCtrl());
			_ctrlSet.Add(new GizmosCtrl());
		}

		private void OnRelaunch()
		{
			foreach (var ctrl in _ctrlSet)
			{
				ctrl.ForceQuit();
			}
		}

		#endregion
	}
}