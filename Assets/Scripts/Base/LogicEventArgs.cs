using System;

namespace KDGame.Base
{
	public class LogicEventArgs : EventArgs
	{
		public int EventID;
		public object[] EventArgs;

		public LogicEventArgs(int id, object[] args)
		{
			this.EventID = id;
			this.EventArgs = args;
		}
	}

	public delegate void LogicEventDelegate(LogicEventArgs args);
}