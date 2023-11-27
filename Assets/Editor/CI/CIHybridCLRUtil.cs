using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using KDGame.Mgr;
using UnityEditor;
using UnityEngine;

namespace KDGame.Editor.CI
{
	public static class CIHybridCLRUtil
	{
		[MenuItem(EditorMenuConst.CICD + "HybridCLR Pre-Build", false, 1)]
		public static void OnPreBuild()
		{
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

			// 检查生成的outputAOTGenericReferenceFile文件后缀，若为.cs
			// 为了避免生成时Refresh打断打包流程，修改为.bytes
			var outputAOTGenericReferenceFile = HybridCLRSettings.Instance.outputAOTGenericReferenceFile;
			if (outputAOTGenericReferenceFile.EndsWith(".cs"))
			{
				outputAOTGenericReferenceFile += ".bytes";
				HybridCLRSettings.Instance.outputAOTGenericReferenceFile = outputAOTGenericReferenceFile;
				HybridCLRSettings.Save();
				AssetDatabase.Refresh();
			}

			// 运行HybridCLR/Generate All
			Debug.Log("Start run HybridCLR.GenerateAll...");
			HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
			// 将outputAOTGenericReferenceFile改名移动到StreamingAssets中供后续使用
			File.Copy("Assets/" + outputAOTGenericReferenceFile, $"{Application.streamingAssetsPath}/{HotUpdMgr.MetadataDllConfig}",
				true);
			AssetDatabase.Refresh();
			Debug.Log("HybridCLR.GenerateAll success!");

			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			// 移动热更新Dll到StreamingAssets
			MoveHotUpdDll(buildTarget);
			// 移动补充元数据Dll到StreamingAssets
			MoveMetadataDll(buildTarget);
		}

		private static void MoveHotUpdDll(BuildTarget buildTarget)
		{
			MoveDllFiles(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget),
				$"{CIConst.TempDllPath}/{CIConst.HotUpdDllPath}",
				SettingsUtil.HotUpdateAssemblyFilesExcludePreserved);
		}

		private static void MoveMetadataDll(BuildTarget buildTarget)
		{
			// 文件名列表其实最好使用这个，但是这里不会自动设置
			// SettingsUtil.AOTAssemblyNames
			MoveDllFiles(SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget),
				$"{CIConst.TempDllPath}/{CIConst.MetadataDllPath}",
				HotUpdMgr.GetMetadataDllNames());
		}

		private static void MoveDllFiles(string srcPath, string dstPath, List<string> fNames)
		{
			if (!Directory.Exists(srcPath))
			{
				Debug.LogWarning("Dll source folder not exist!");
				return;
			}

			if (!Directory.Exists(dstPath))
			{
				Directory.CreateDirectory(dstPath);
				AssetDatabase.Refresh();
			}

			foreach (var fName in fNames)
			{
				var filePath = $"{srcPath}/{fName}";
				if (!File.Exists(filePath))
				{
					Debug.LogError($"Dll file not found! {srcPath}/{fName}");
					continue;
				}

				var tarPath = $"{dstPath}/{fName}.bytes";
				File.Copy(filePath, tarPath, true);
			}

			AssetDatabase.Refresh();
		}
	}
}