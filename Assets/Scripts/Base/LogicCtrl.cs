using System;
using KDGame.Lib;
using UnityEngine;

namespace KDGame.Base
{
	public class LogicCtrl
	{
		protected string _name;

		public LogicStatus Status = LogicStatus.Init;

		public event Action<LogicStatus> OnStatusChange;

		public LogicCtrl()
		{
			OnEnter();
		}

		~LogicCtrl()
		{
			Debug.Log(_name + "Ctrl Destroy");
		}

		/// <summary>
		/// 逻辑开始
		/// </summary>
		protected virtual void OnEnter()
		{
			OnStatusChange?.Invoke(LogicStatus.Running);
		}

		/// <summary>
		/// 逻辑结束
		/// </summary>
		protected virtual void OnQuit()
		{
			OnStatusChange?.Invoke(LogicStatus.Dead);
		}

		/// <summary>
		/// 强制结束逻辑
		/// </summary>
		public virtual void ForceQuit()
		{
			
		}


		#region Promise Like

		public static void All(Action onAllDead, params LogicCtrl[] ctrlList)
		{
			int count = ctrlList.Length;
			foreach (var logicCtrl in ctrlList)
			{
				logicCtrl.OnStatusChange += (status) =>
				{
					if (status == LogicStatus.Dead) count--;
					if (count <= 0)
					{
						// TODO 移除这个匿名函数？
						onAllDead.Invoke();
					}
				};
			}
		}

		public static void Any(Action onAnyDead, params LogicCtrl[] ctrlList)
		{
			
		}

		#endregion
	}

	public enum LogicStatus
	{
		Init = 0, // 默认状态
		Running = 1, // 运行状态
		Suspend = 2, // 挂起状态
		Dead = 3 // 结束状态
	}
}