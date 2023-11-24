using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using KDGame.Mgr;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace KDGame.Editor.CI
{
	public class CIUtil
	{
		[MenuItem(EditorMenuConst.CICD + "Check Before Build", false, 1)]
		public static void CheckBeforeBuild()
		{
			// Application.logMessageReceivedThreaded += CaptureLogThread;
			// 检查是否需要安装HybridCLR
			var controller = new InstallerController();
			if (!controller.HasInstalledHybridCLR())
			{
				Debug.Log("Start install default HybridCLR...");
				controller.InstallDefaultHybridCLR();
				AssetDatabase.Refresh();
				Debug.Log("Install default HybridCLR success!");
			}
			else
			{
				Debug.Log("Check HybridCLR end, no need to install...");
			}
			// 运行HybridCLR/Generate All
			Debug.Log("Start run HybridCLR.GenerateAll...");
			HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
			AssetDatabase.Refresh();
			Debug.Log("HybridCLR.GenerateAll success!");

			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			var hybridCLRSettings = HybridCLRSettings.LoadOrCreate();
			// 移动热更新Dll到StreamingAssets
			MoveHotUpdDll(hybridCLRSettings, buildTarget);
			// 移动补充元数据Dll到StreamingAssets
			MoveMetadataDll();
		}

		private const string HotUpdDllLibPath = "HybridCLRData/HotUpdateDlls/";
		private const string HotUpdDllPath = "HotUpdDll";
		private const string MetadataDllPath = "MetadataDll";

		private static void MoveHotUpdDll(HybridCLRSettings settings, BuildTarget buildTarget)
		{
			var hotUpdDllNames = new HashSet<string>();
			foreach (var assName in settings.hotUpdateAssemblies)
				hotUpdDllNames.Add(assName);
			foreach (var def in settings.hotUpdateAssemblyDefinitions)
				hotUpdDllNames.Add(def.name);
			var libPath = HotUpdDllLibPath + buildTarget;
			if (!Directory.Exists(libPath))
			{
				Debug.LogWarning("HotUpdDll folder not exist!");
				return;
			}

			var finalLibPath = $"{Application.streamingAssetsPath}/{HotUpdDllPath}";
			if (!Directory.Exists(finalLibPath))
			{
				AssetDatabase.CreateFolder("Assets/StreamingAssets", HotUpdDllPath);
			}
			foreach (var dllName in hotUpdDllNames)
			{
				var filePath = $"{libPath}/{dllName}.dll";
				if (!File.Exists(filePath))
				{
					Debug.LogError("HotUpdDll not found!" + dllName);
					continue;
				}
				// TODO md5
				var tarPath = $"{finalLibPath}/{dllName}.dll.bytes";
				if (File.Exists(tarPath))
				{
					File.Delete(tarPath);
				}
				File.Copy(filePath, tarPath);
			}
			AssetDatabase.Refresh();
		}

		private static void MoveMetadataDll()
		{
			foreach (var name in HotUpdMgr.GetMetadataDllNames())
			{
				Debug.LogError(name);
			}
		}

		[MenuItem(EditorMenuConst.CICD + "Build With Param", false, 2)]
		public static void BuildWithParam()
		{
			// Debug.Log(1);
			// Debug.LogWarning(2);
			// Debug.LogError(3);
			// var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			// var hybridCLRSettings = HybridCLRSettings.LoadOrCreate();
			// MoveHotUpdDll(hybridCLRSettings, buildTarget);
			MoveMetadataDll();
		}
		
		private static void CaptureLogThread(string condition, string stacktrace, LogType type)
		{
			if(type == LogType.Error || type == LogType.Assert)
			{
				Console.WriteLine("CI-ERR:" + condition);
			}
			else if(type == LogType.Exception)
			{
				Console.WriteLine("CI-EXC:" + string.Join("@", condition.Split('\n')));
			}
		}
	}
}