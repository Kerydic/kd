using System;

namespace KDGame.Base
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class ModuleAttribute : Attribute
	{
		private string _name;
		public ModuleAttribute(string name)
		{
			_name = name;
		}

		public string GetName()
		{
			return _name;
		}
	}
}