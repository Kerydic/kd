using System;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr;
using KDGame.Util;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KDGame.UI
{
	public class UIMgr : MonoSingleton<UIMgr>, IMgr
	{
		private Transform _uiRoot;
		private MainCamera _mainCamera;
		private Dictionary<string, UILayer> _layerDict;
		private KDLog _logger;

		protected override void OnAwake()
		{
			base.OnAwake();
			_uiRoot = GameObject.Find("UIRoot").transform;
			_layerDict = new Dictionary<string, UILayer>();
			_logger = new KDLog("UIMgr", "FF0000");
		}

		public void Restart()
		{
			DestroyLayers();
			CreateLayers();
		}

		private Camera _uiCamera;
		private UniversalAdditionalCameraData _uiCameraData;

		private void CreateUICamera()
		{
			if (_uiCamera != null) return;
			var uiGo = new GameObject("UICamera");
			uiGo.transform.SetParent(_uiRoot);
			uiGo.layer = LayerMask.NameToLayer("UI");
			_uiCamera = uiGo.AddComponent<Camera>();
			_uiCameraData = uiGo.AddComponent<UniversalAdditionalCameraData>();
			_uiCameraData.renderType = CameraRenderType.Overlay;
			_uiCamera.orthographic = true;
			// 只显示UI层
			// 常用指令：
			// camera.cullingMask = ~(1 << x); 除x外的所有层
			// camera.cullingMask &= ~(1 << x); 关闭x层
			// camera.cullingMask != (1 << x); 打开x层
			// camera.cullingMask = 1 << x + 1 << y + 1 << z; 只显示xyz层
			_uiCamera.cullingMask |= 1 << LayerMask.NameToLayer("UI");
			_uiCamera.cullingMask |= 1 << LayerMask.NameToLayer("Default");
			if (_mainCamera)
			{
				_mainCamera.AddOverlayCamera(_uiCamera);
			}
		}

		/// <summary>
		/// 设置当前的主相机，并将所有UI层全部移到其CameraStack中
		/// </summary>
		/// <param name="mainCamera">主相机</param>
		public void SetMainCamera(MainCamera mainCamera)
		{
			if (_mainCamera)
			{
				_mainCamera.ClearCameraStack();
			}

			_mainCamera = mainCamera;
			if (_uiCamera)
			{
				_mainCamera.AddOverlayCamera(_uiCamera);
			}
		}

		public void CreateLayers()
		{
			CreateUICamera();
			foreach (var layerName in UILayerNames.Layers)
			{
				_logger.Info($"Create UI layer, name: {layerName}, layer: {layerName}");
				var layer = new GameObject(layerName);
				layer.layer = LayerMask.NameToLayer("UI");
				layer.transform.SetParent(_uiRoot);
				var uiLayer = layer.AddComponent<UILayer>();
				_layerDict[layerName] = uiLayer;
				uiLayer.Setup(_uiCamera, layerName);
			}
		}

		public void DestroyLayers()
		{
			foreach (var kv in _layerDict)
			{
				_logger.Info($"Destroy UI Layer, layer: {kv.Key}, name: {kv.Value.name}");
				Destroy(kv.Value.gameObject);
			}

			_layerDict.Clear();
		}

		#region View Show

		/// <summary>
		/// 使用指定的深度显示UI界面
		/// </summary>
		/// <param name="vf">页面参数</param>
		/// <param name="onShowEnd">页面显示结束回调</param>
		/// <param name="vParams">传给View的参数</param>
		public void ShowView(ViewForm vf, Action<UIView> onShowEnd = null, params object[] vParams)
		{
			var vid = ShowViewInternal(vf.Path, vf.LayerName, vParams);
			if (vid <= 0) return;

			if (!_showingForms.ContainsKey(vf))
				_showingForms[vf] = new List<ulong>();
			_showingForms[vf].Add(vid);
			onShowEnd?.Invoke(_showingViews[vid].View);
		}

		private ulong _uniqViewID = 0;
		private Dictionary<ulong, ShowingUIView> _showingViews = new Dictionary<ulong, ShowingUIView>();
		private Dictionary<ViewForm, List<ulong>> _showingForms = new Dictionary<ViewForm, List<ulong>>();

		private ulong ShowViewInternal(string assetPath, string layerName, params object[] vParams)
		{
			LoadCert cert = AssetMgr.LoadAsset<GameObject>(assetPath);
			if (cert.Status != LoadStatus.Success)
			{
				_logger.Error("ShowView fail, due to LoadAssetFail.");
				return 0;
			}

			if (!_layerDict.TryGetValue(layerName, out UILayer layer))
			{
				_logger.Error("ShowView fail, due to invalid UILayer.");
				cert.Unload();
				return 0;
			}

			ulong viewID = ++_uniqViewID;

			GameObject viewGo = GameObject.Instantiate((GameObject)cert.Objs[0], layer.GetRoot());
			UIView view = viewGo.GetComponent<UIView>();
			view.OnCreateEnd(viewID);

			ShowingUIView curr = new ShowingUIView();
			curr.ID = viewID;
			curr.View = view;
			curr.AssetCert = cert;
			curr.Params = vParams;
			_showingViews[curr.ID] = curr;

			return viewID;
		}

		#endregion

		#region View Hide/Destroy

		/// <summary>
		/// 隐藏该ViewForm的对应数量页面
		/// </summary>
		/// <param name="vf"></param>
		/// <param name="count">删除多少个，若不填或小于0，全部删除</param>
		public void HideView(ViewForm vf, int count = -1)
		{
			if (!_showingForms.TryGetValue(vf, out List<ulong> vids))
			{
				_logger.Error("No corresponding view found!");
				return;
			}

			var vCount = vids.Count;
			if (count < 0) count = vCount;
			for (var i = 1; i <= Mathf.Min(vCount, count); ++i)
			{
				var index = vCount - i + 1;
				DestroyView(vids[index]);
				vids.RemoveAt(index);
			}
		}

		/// <summary>
		/// 精确隐藏某个ID的页面
		/// </summary>
		/// <param name="viewID"></param>
		public void HideView(ulong viewID)
		{
			DestroyView(viewID);
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
		public object[] Params;
	}
}