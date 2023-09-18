using UnityEngine;

namespace KDGame.Base
{
	public class Module
	{
		public string Name;

		public LogicStatus Status = LogicStatus.Init;

		public Module()
		{
			Status = LogicStatus.Running;
			OnEnter();
		}

		~Module()
		{
			// Debug.Log(Name + "Ctrl Destroy");
		}

		/// <summary>
		/// 逻辑开始事件
		/// </summary>
		protected virtual void OnEnter()
		{
		}

		/// <summary>
		/// 结束逻辑
		/// </summary>
		public virtual void Quit()
		{
			Status = LogicStatus.Dead;
			OnQuit();
		}

		/// <summary>
		/// 逻辑结束事件
		/// </summary>
		protected virtual void OnQuit()
		{
		}

		protected T GetModule<T>() where T : Module
		{
			return MainCtrl.Instance.GetModule<T>();
		}
	}

	public enum LogicStatus
	{
		Init = 0, // 默认状态
		Running = 1, // 运行状态
		Suspend = 2, // 挂起状态
		Dead = 3 // 结束状态
	}
}