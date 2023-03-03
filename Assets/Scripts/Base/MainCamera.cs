using KDGame.Util;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KDGame.Base
{
	[RequireComponent(typeof(Camera))]
	public class MainCamera : MonoBehaviour
	{
		private UniversalAdditionalCameraData _cameraData;
		private KDLog _logger;

		void Awake()
		{
			_logger = new KDLog("MainCamera", LogColor.Error);
			// 先判定当前相机的渲染类型，如果不是Base则报错
			_cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
			if (_cameraData == null || _cameraData.renderType != CameraRenderType.Base)
			{
				_logger.Error("Not a URP camera or render type is not set to Base!");
			}
		}

		/// <summary>
		/// 想其CameraStack中添加新的OverlayCamera
		/// </summary>
		/// <param name="subCamera">要添加的OverlayCamera</param>
		public void AddOverlayCamera(Camera subCamera)
		{
			if (!_cameraData) return;
			_cameraData.cameraStack.Add(subCamera);
		}

		/// <summary>
		/// 清除CameraStack中的所有OverlayCamera
		/// </summary>
		public void ClearCameraStack()
		{
			if (!_cameraData) return;
			_cameraData.cameraStack.Clear();
		}
	}
}