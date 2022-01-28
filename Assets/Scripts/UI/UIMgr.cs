using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using KDGame.Base;
using KDGame.Mgr;
using UnityEngine;

namespace KDGame.UI
{
	public class UIMgr : MonoSingleton<UIMgr>, IMgr
	{
		private Transform _uiRoot;
		private Dictionary<int, UILayer> _layerDict;

		private void Awake()
		{
			_uiRoot = GameObject.Find("UIRoot").transform;
		}

		public void Restart()
		{
		}

		public void CreateLayers()
		{
			AssetMgr.Instance.LoadAssetAsync<GameObject>(UIPath.UILayer, (success, obj) =>
			{
				foreach (UIDepth uiDepth in Enum.GetValues(typeof(UIDepth)))
				{
					Debug.Log($"Create UI layer, name: {uiDepth}, depth: {(int) uiDepth}");
					var layer = GameObject.Instantiate(obj, _uiRoot);
				}
			});
		}

		public void PlaySplash()
		{
		}

		public void DestroySplash()
		{
		}
	}
}