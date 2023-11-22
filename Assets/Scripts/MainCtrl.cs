using System;
using System.Collections.Generic;
using System.Reflection;
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
			AddMgr<HotUpdMgr>();
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

		private Dictionary<string, Base.Module> _moduleDict;
		private Dictionary<Type, string> _moduleTypeNameDict;

		// 游戏开始
		private void OnLaunch()
		{
			UIMgr.Instance.SetMainCamera(_mainCamera);
			UIMgr.Instance.CreateLayers();
			HotUpdMgr.Instance.RunHotUpd();
			CreateModules();
			Destroy(GameObject.Find("Splash"));
		}

		private void CreateModules()
		{
			_moduleDict = new Dictionary<string, Base.Module>();
			_moduleTypeNameDict = new Dictionary<Type, string>();
			foreach (var assembly in HotUpdMgr.Instance.GetLoadedAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					var moduleAttr = type.GetCustomAttribute<ModuleAttribute>();
					if (moduleAttr == null) continue;
					var mName = moduleAttr.GetName();
					_moduleDict[mName] = Activator.CreateInstance(type) as Base.Module;
					_moduleTypeNameDict[type] = mName;
				}
			}
		}

		public T GetModule<T>() where T : Base.Module
		{
			var mName = _moduleTypeNameDict?[typeof(T)];
			if (string.IsNullOrEmpty(mName)) return null;
			return _moduleDict?[mName] as T;
		}

		private void OnRelaunch()
		{
			foreach (var ctrl in _moduleDict)
			{
				ctrl.Value.Quit();
			}
		}

		#endregion
	}
}