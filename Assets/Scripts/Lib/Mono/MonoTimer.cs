using System;
using UnityEngine;

namespace KDGame.Lib
{
	/// <summary>
	/// 基于MonoBehaviour实现的Timer，其注册的事件函数会运行在主线程上
	/// </summary>
	public class MonoTimer
	{
		public MonoTimer()
		{
		}

		public MonoTimer(double interval)
		{
			Interval = interval;
		}

		/// <summary>
		/// 此Timer在一次生命周期内抛出几次事件
		/// </summary>
		public int Times = 1;

		/// <summary>
		/// 此Timer在生命周期结束后是否自动重置，一般用于无限循环
		/// </summary>
		public bool AutoReset = false;

		/// <summary>
		/// 该Timer每次抛出事件的间隔，单位毫秒
		/// </summary>
		public double Interval = 100;

		/// <summary>
		/// 每次抛出的事件，可以在一个Timer内注册多个
		/// </summary>
		public event Action Elapsed;

		private bool _isRunning = false;
		private int _runTimes = 0;
		private double _sinceLastEvt = 0;

		/// <summary>
		/// 启动该Timer。若已处于启动状态，无效
		/// </summary>
		public void Start()
		{
			if (!_isRunning)
			{
				MonoProxy.AddUpdate(Update);
				_isRunning = true;
			}
		}

		/// <summary>
		/// 停止该Timer。注意如果Timer.AutoReset为false，则在其最后一次抛出事件前会自动调用该函数，外部无需显示调用
		/// </summary>
		public void Stop()
		{
			if (_isRunning)
			{
				MonoProxy.RmUpdate(Update);
				_isRunning = false;
			}
		}

		private void Update()
		{
			if (!_isRunning) return;
			_sinceLastEvt += Time.deltaTime * 1000;
			if (_sinceLastEvt < Interval) return;
			_sinceLastEvt %= Interval;
			if (++_runTimes >= Times)
			{
				if (AutoReset)
					_runTimes = 0;
				else
					Stop();
			}

			Elapsed?.Invoke();
		}
	}
}