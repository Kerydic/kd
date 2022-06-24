using System;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using KDGame.Util;
using UnityEngine;

namespace KDGame.UI
{
	public class UIMgr : MonoSingleton<UIMgr>, IMgr
	{
		private Transform _uiRoot;
		private Dictionary<UIDepth, UILayer> _layerDict;
		private KDLog _logger;

		protected override void OnAwake()
		{
			base.OnAwake();
			_uiRoot = GameObject.Find("UIRoot").transform;
			_layerDict = new Dictionary<UIDepth, UILayer>();
			_logger = new KDLog("UIMgr", "FF0000");
		}

		public void Restart()
		{
			DestroyLayers();
			CreateLayers();
		}

		private LoadCert _layerCert;

		public void CreateLayers()
		{
			_layerCert = AssetMgr.LoadAsset<GameObject>(UIConst.LayerPath);
			foreach (UIDepth uiDepth in Enum.GetValues(typeof(UIDepth)))
			{
				_logger.Info($"Create UI layer, name: {uiDepth}, depth: {(int) uiDepth}");
				var layer = GameObject.Instantiate(_layerCert.Objs[0] as GameObject, _uiRoot);
				layer.name = Enum.GetName(typeof(UIDepth), uiDepth) ?? "Unknown";
				var uiLayer = layer.GetComponent<UILayer>();
				uiLayer.InitDepth((int) uiDepth);

				_layerDict[uiDepth] = uiLayer;
			}
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
			if (_layerCert != null)
			{
				_layerCert.Unload();
				_layerCert = null;
			}
		}

		#region View Show

		public void ShowView(ViewForm vf, Action<UIView> onShowEnd = null)
		{
			ShowViewInternal(vf.Path, vf.Depth, onShowEnd);
		}

		/// <summary>
		/// 使用指定的深度显示UI界面
		/// </summary>
		/// <param name="vf">页面参数</param>
		/// <param name="depth">期望的页面深度</param>
		/// <param name="onShowEnd">页面显示结束回调</param>
		public void ShowView(ViewForm vf, UIDepth depth, Action<UIView> onShowEnd = null)
		{
			ShowViewInternal(vf.Path, depth, onShowEnd);
		}

		private ulong _uniqViewID = 0;
		private Dictionary<ulong, ShowingUIView> _showingViews = new Dictionary<ulong, ShowingUIView>();

		private void ShowViewInternal(string assetPath, UIDepth depth, Action<UIView> onShowEnd = null)
		{
			LoadCert cert = AssetMgr.LoadAsset<GameObject>(assetPath);
			if (cert.Status != LoadStatus.Success)
			{
				_logger.Error("ShowView fail, due to LoadAssetFail.");
				return;
			}

			if (!_layerDict.TryGetValue(depth, out UILayer layer))
			{
				_logger.Error("ShowView fail, due to invalid UILayer.");
				cert.Unload();
				return;
			}

			ulong viewID = ++_uniqViewID;

			GameObject viewGo = GameObject.Instantiate((GameObject) cert.Objs[0], layer.GetRoot());
			UIView view = viewGo.GetComponent<UIView>();
			view.OnCreateEnd(viewID);

			ShowingUIView curr = new ShowingUIView();
			curr.ID = viewID;
			curr.View = view;
			curr.AssetCert = cert;
			_showingViews[curr.ID] = curr;

			onShowEnd?.Invoke(view);
		}

		#endregion

		#region View Hide/Destroy

		public void HideView(ulong viewID)
		{
		}

		private void DestroyView(ulong viewID)
		{
			if (_showingViews.TryGetValue(viewID, out ShowingUIView showing))
			{
				showing.View.OnViewDestroy();
				GameObject.Destroy(showing.View.gameObject);
				showing.AssetCert.Unload();
				_showingViews[viewID] = null;
			}
		}

		#endregion
	}

	public class ShowingUIView
	{
		public ulong ID;
		public UIView View;
		public LoadCert AssetCert;
	}
}