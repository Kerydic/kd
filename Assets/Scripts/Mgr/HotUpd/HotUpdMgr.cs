using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using KDGame.Base;
// 为了避免主Assembly引用到热更新Assembly，需要关闭GameHotUpd.asmdef中AutoReferenced选项，导致主Assembly无法引用命名空间
// using HotUpd.Core;

namespace KDGame.Mgr
{
	public class HotUpdMgr : MonoSingleton<HotUpdMgr>, IMgr
	{
		private List<Assembly> _loadedAssemblies;

		public void Restart()
		{
		}

		public void RunHotUpd()
		{
			_loadedAssemblies = new List<Assembly>();
			LoadDll();
		}

		// TODO 不在逻辑里写死，从Hybrid配置里面获取？还是从热更配置获取？
		private static string[] ASS_NAMES =
		{
			"GameHotUpd",
		};

		private void LoadDll()
		{
			foreach (var dllName in ASS_NAMES)
			{
				_loadedAssemblies.Add(LoadDllWithName(dllName));
			}
		}

		private Assembly LoadDllWithName(string dllName)
		{
#if UNITY_EDITOR
// Editor下无需加载，直接查找获得HotUpdate程序集
			var hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dllName);
#else
// TODO 后续考虑附带Md5码
			var hotUpdateAss = Assembly.LoadFile($"{Application.streamingAssetsPath}/{dllName}.dll.bytes");
#endif
			return hotUpdateAss;
		}

		public Assembly[] GetLoadedAssemblies()
		{
			return _loadedAssemblies.ToArray();
		}
	}
}