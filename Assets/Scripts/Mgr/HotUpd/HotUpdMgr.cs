using System;
using System.Reflection;
using System.Linq;
using KDGame.Base;
using UnityEngine;
// 为了避免主Assembly引用到热更新Assembly，需要关闭GameHotUpd.asmdef中AutoReferenced选项，导致主Assembly无法引用命名空间
// using HotUpd.Core;

namespace KDGame.Mgr
{
	public class HotUpdMgr : MonoSingleton<HotUpdMgr>, IMgr
	{
		public void Restart()
		{
		}

		public void RunHotUpd()
		{
#if UNITY_EDITOR
// Editor下无需加载，直接查找获得HotUpdate程序集
			Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "GameHotUpd");
#else
			Assembly hotUpdateAss = Assembly.LoadFile($"{Application.streamingAssetsPath}/GameHotUpd.dll.bytes");
#endif
			Type type = hotUpdateAss.GetType("HotUpd.Core.HotUpdCore");
			type.GetMethod("Run").Invoke(null, null);
		}
	}
}