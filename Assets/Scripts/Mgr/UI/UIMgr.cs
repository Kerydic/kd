using System;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using UnityEngine;

namespace KDGame.UI
{
	public class UIMgr : MonoSingleton<UIMgr>, IMgr
	{
		private Transform _uiRoot;
		private Dictionary<UIDepth, UILayer> _layerDict;

		private void Awake()
		{
			_uiRoot = GameObject.Find("UIRoot").transform;
			_layerDict = new Dictionary<UIDepth, UILayer>();
		}

		public void Restart()
		{
			DestroyLayers();
			CreateLayers();
		}

		public void CreateLayers()
		{
			// AssetMgr.Instance.LoadAssetAsync<GameObject>(UIPath.UILayer, (success, obj) =>
			// {
			// 	foreach (UIDepth uiDepth in Enum.GetValues(typeof(UIDepth)))
			// 	{
			// 		Debug.Log($"Create UI layer, name: {uiDepth}, depth: {(int) uiDepth}");
			// 		var layer = GameObject.Instantiate(obj, _uiRoot);
			// 		layer.name = Enum.GetName(typeof(UIDepth), uiDepth) ?? "Unknown"[;
			// 		layer.GetComponent<UILayer>().InitDepth((int) uiDepth);
			// 	}
			// });
		}

		public void DestroyLayers()
		{
			// TODO 清除所有当前显示界面的资源引用
			foreach (var kv in _layerDict)
			{
				Debug.Log($"Destroy UI Layer, depth: {(int) kv.Key}, name: {kv.Value.name}");
				GameObject.Destroy(kv.Value);
			}

			_layerDict.Clear();
		}

		#region 界面显示

		public void ShowView(ViewForm vf)
		{
			ShowViewInternal(vf.Path, vf.Depth);
		}

		/// <summary>
		/// 使用指定的深度显示UI界面
		/// </summary>
		/// <param name="vf">页面参数</param>
		/// <param name="depth">期望的页面深度</param>
		public void ShowView(ViewForm vf, UIDepth depth)
		{
			ShowViewInternal(vf.Path, depth);
		}

		private void ShowViewInternal(string assetPath, UIDepth depth)
		{
			
		}

		#endregion
	}
}