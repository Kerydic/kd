using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using HybridCLR;
using KDGame.Base;
using UnityEngine;

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
			LoadMetadataForAOTAssembly();
#if UNITY_EDITOR
			// Editor下无需加载，直接查找获得HotUpdate程序集
			var hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dllName);
#else
			// TODO 换成从AssetBundle加载
			var bytes = File.ReadAllBytes($"{Application.streamingAssetsPath}/{dllName}.dll.bytes");
			var hotUpdateAss = Assembly.Load(bytes);
#endif
			return hotUpdateAss;
		}

		// 加载补充元数据Dll
		private static void LoadMetadataForAOTAssembly()
		{
#if !UNITY_EDITOR
			foreach (var dllName in GetMetadataDllNames())
			{
				// TODO 换成从AssetBundle加载
				var bytes = File.ReadAllBytes($"{Application.streamingAssetsPath}/{dllName}.dll.bytes");
				var err = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);
				Debug.Log($"LoadMetadataForAOTAssembly:{dllName}, result:{err}");
			}
#endif
		}

		// 解析HybridCLR生成的文件，并获取需要补充元数据的Dll名
		public static string[] GetMetadataDllNames()
		{
			// 用反射，防止没有生成报错
			var aotGenRefClass = AppDomain.CurrentDomain.GetAssemblies()
				.First(a => a.GetName().Name == "Assembly-CSharp").GetType("AOTGenericReferences");
			if (aotGenRefClass == null)
			{
				Debug.LogError("Class AOTGenericReferences is not found! Please run HybridCLR/Generate All first!");
				return new string[] { };
			}

			var nameList = aotGenRefClass.GetField("PatchedAOTAssemblyList").GetValue(null) as List<string>;
			return nameList.ToArray();
		}

		public Assembly[] GetLoadedAssemblies()
		{
			return _loadedAssemblies.ToArray();
		}
	}
}