using System;
using System.Collections.Generic;
using System.Linq;
using KDGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KDGame.Module
{
	public class GizmosView : UIView
	{
		[SerializeField] private Text _fpsLbl;

		public int frameRange = 30; // 用于计算平均帧率时，取多少个帧
		private Queue<float> _deltaTimeQueue;

		private void Awake()
		{
			_deltaTimeQueue = new Queue<float>();
		}

		void Update()
		{
			var time = Time.deltaTime;
			_deltaTimeQueue.Enqueue(time);
			if (_deltaTimeQueue.Count > frameRange)
				_deltaTimeQueue.Dequeue();
			_fpsLbl.text = $"FPS: {Mathf.Round(1 * _deltaTimeQueue.Count / _deltaTimeQueue.Sum())}Hz";
		}
	}
}