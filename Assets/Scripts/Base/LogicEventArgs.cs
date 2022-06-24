using System;
using KDGame.PB;

namespace KDGame.Base
{
	public class LogicEventArgs : EventArgs
	{
		public LogicEvent EventID;
		public object[] EventArgs;

		public LogicEventArgs(LogicEvent id, object[] args)
		{
			this.EventID = id;
			this.EventArgs = args;
		}
	}

	public delegate void LogicEventDelegate(LogicEventArgs args);
}