using KDGame.PB;
using UnityEngine;

namespace KDGame.Base
{
	public class LogicEventSender : MonoBehaviour
	{
		private event LogicEventDelegate _center;

		public void AddListener(LogicEventDelegate listener)
		{
			_center += listener;
		}

		public void RmListener(LogicEventDelegate listener)
		{
			_center -= listener;
		}

		protected void Invoke(LogicEvent eventID, params object[] args)
		{
			_center?.Invoke(new LogicEventArgs(eventID, args));
		}
	}
}