using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_ANDROID
using Google.Android.AppBundle.Editor;
#endif

namespace KDGame.Editor.CI
{
	public class CIUtil
	{
		[MenuItem(EditorMenuConst.CICD + "Build iOS Test", false, 2)]
		public static void BuildIOSTest()
		{
			BuildWithParam(new CIParam{
				Target = BuildTarget.iOS,
				TargetGroup = BuildTargetGroup.iOS,
				PkgID = "com.kerydic.kdexp.ios",
				Version = "0.0.1",
				BuildNum = 1,
				CompareBuildNum = 1,
				OutputPath = "/Users/kerydic/Downloads/kdexp_xcode_output",
				IsDev = false,
				IsAab = false,
				CreateSymbol = false
			});
		}

		public static void BuildWithParam(CIParam param)
		{
			Application.logMessageReceivedThreaded += CaptureLogThread;
			if (EditorUserBuildSettings.activeBuildTarget != param.Target){
				EditorUserBuildSettings.SwitchActiveBuildTarget(param.TargetGroup, param.Target);
			}
			// 版本号等配置
			PlayerSettings.SetApplicationIdentifier(param.TargetGroup, param.PkgID);
			PlayerSettings.bundleVersion = param.Version;
			PlayerSettings.Android.bundleVersionCode = param.BuildNum;
			PlayerSettings.iOS.buildNumber = param.BuildNum.ToString();
			// Android 特定配置
			PlayerSettings.Android.useCustomKeystore = false; //true;
			// PlayerSettings.Android.keystoreName = setting.keystorePath;
			// PlayerSettings.Android.keystorePass = setting.keystorePass;
			// PlayerSettings.Android.keyaliasName = setting.keyAliasName;
			// PlayerSettings.Android.keyaliasPass = setting.keyAliasPass;
			// iOS特定配置
			PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
			PlayerSettings.stripEngineCode = false;
			AssetDatabase.SaveAssets();
			CIHybridCLRUtil.OnPreBuild();
			CIABUtil.BuildAllAssetBundles(param.Target);
			// TODO 异常依赖检测
			// 构建
			BuildPlayer(param);
			Application.logMessageReceivedThreaded -= CaptureLogThread;
		}

		private static void CaptureLogThread(string condition, string stacktrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
					Console.WriteLine("CI-ERR:" + condition);
					break;
				case LogType.Exception:
					Console.WriteLine("CI-EXC:" + string.Join("@", condition.Split('\n')));
					break;
			}
		}

		private static void BuildPlayer(CIParam param)
		{
			var sceneNameList = new List<string>();
			foreach (var scene in EditorBuildSettings.scenes)
				if (scene != null && scene.enabled)
					sceneNameList.Add(scene.path);
			var sceneNameAry = sceneNameList.ToArray();
			var outputDir = Path.GetDirectoryName(param.OutputPath);
			if (!Directory.Exists(outputDir))
				Directory.CreateDirectory(outputDir);
			BuildOptions options = BuildOptions.None;
			EditorUserBuildSettings.development = param.IsDev;
			EditorUserBuildSettings.allowDebugging = param.IsDev;
			EditorUserBuildSettings.connectProfiler = param.IsDev;
			EditorUserBuildSettings.buildAppBundle = param.IsAab;
			EditorUserBuildSettings.androidCreateSymbols = param.CreateSymbol ? AndroidCreateSymbols.Public : AndroidCreateSymbols.Disabled;
			if(param.IsDev)
			{
				options |= BuildOptions.Development;
				options |= BuildOptions.ConnectWithProfiler;
				options |= BuildOptions.AllowDebugging;
			}
			else
			{
				options &= ~BuildOptions.Development;
				options &= ~BuildOptions.ConnectWithProfiler;
				options &= ~BuildOptions.AllowDebugging;
			}

			try
			{
				Debug.Log("OutputPath:" + param.OutputPath);
				if (param.Target == BuildTarget.Android && param.IsAab)
					// 打AAB
					BuildAAB(param, options);
				else
					BuildPipeline.BuildPlayer(sceneNameAry, param.OutputPath, param.Target, options);
				Debug.Log("Build Player Success");
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
			}
			finally
			{
				Debug.Log("Build Player Exit.");
			}
			// TODO 上传/缓存AssetBundle
		}

		private static void BuildAAB(CIParam param, BuildOptions options)
		{
			#if UNITY_ANDROID
			var buildPlayerOptions = AndroidBuildHelper.CreateBuildPlayerOptions(param.OutputPath);
			buildPlayerOptions.options = options;
			var assetPackConfig = new AssetPackConfig();
			assetPackConfig.SplitBaseModuleAssets = true;
			Bundletool.BuildBundle(buildPlayerOptions, assetPackConfig);
			#endif
		}
	}

	public class CIParam
	{
		public BuildTarget Target;
		public BuildTargetGroup TargetGroup;
		public string PkgID;

		public string Version;
		public int BuildNum;
		public int CompareBuildNum;
		public string OutputPath;
		public bool IsDev;
		public bool IsAab;
		public bool CreateSymbol;
	}
}