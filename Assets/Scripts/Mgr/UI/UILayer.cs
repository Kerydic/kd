using System;
using UnityEngine;
using UnityEngine.UI;

namespace KDGame.UI
{
	public class UILayer : MonoBehaviour
	{
		private Canvas _canvas;

		private void Awake()
		{
			_canvas = gameObject.AddComponent<Canvas>();
			_canvas.renderMode = RenderMode.ScreenSpaceCamera;
			var canvasScaler = gameObject.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referenceResolution = new Vector2(1920, 1080);
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			gameObject.AddComponent<GraphicRaycaster>();
		}

		public void Setup(Camera camera, string layerName)
		{
			_canvas.worldCamera = camera;
			_canvas.sortingLayerName = layerName;
		}

		public Transform GetRoot()
		{
			return _canvas.transform;
		}
	}
}