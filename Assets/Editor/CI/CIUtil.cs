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
		[MenuItem(EditorMenuConst.CICD + "Build With Param", false, 2)]
		public static void BuildWithParam()
		{
			Application.logMessageReceivedThreaded += CaptureLogThread;
			var buildTarget = EditorUserBuildSettings.activeBuildTarget;
			CIHybridCLRUtil.OnPreBuild();
			CIABUtil.BuildAllAssetBundles(buildTarget);
			// TODO 异常依赖检测
			Application.logMessageReceivedThreaded -= CaptureLogThread;
			// 构建
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

		private static void BuildPlayer(BuildParam param)
		{
			var sceneNameList = new List<string>();
			foreach (var scene in EditorBuildSettings.scenes)
				if (scene != null && scene.enabled)
					sceneNameList.Add(scene.path);
			var sceneNameAry = sceneNameList.ToArray();
			var outputDir = Path.GetDirectoryName(param.outputPath);
			if (!Directory.Exists(outputDir))
				Directory.CreateDirectory(outputDir);
			BuildOptions options = BuildOptions.None;
			EditorUserBuildSettings.development = param.isDev;
			EditorUserBuildSettings.allowDebugging = param.isDev;
			EditorUserBuildSettings.connectProfiler = param.isDev;
			EditorUserBuildSettings.buildAppBundle = param.isAppBundle;
			EditorUserBuildSettings.androidCreateSymbolsZip = param.createSymbol;
			if(param.isDev)
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
				Debug.Log("OutputPath:" + param.outputPath);
				if (param.Target == BuildTarget.Android && param.isAppBundle)
					// 打AAB
					BuildAAB(param, options);
				else
					BuildPipeline.BuildPlayer(sceneNameAry, param.outputPath, param.Target, options);
				Debug.Log("Build Player Success");
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
			}
			finally
			{
				
			}
			// TODO 上传/缓存AssetBundle
		}

		private static void BuildAAB(BuildParam param, BuildOptions options)
		{
			var buildPlayerOptions = AndroidBuildHelper.CreateBuildPlayerOptions(param.outputPath);
			buildPlayerOptions.options = options;
			var assetPackConfig = new AssetPackConfig();
			assetPackConfig.SplitBaseModuleAssets = true;
			Bundletool.BuildBundle(buildPlayerOptions, assetPackConfig);
		}
	}

	public class BuildParam
	{
		public BuildTarget Target;

		public string versionStr;
		public int buildNum;
		public int CompareBuildNum;
		
		public string outputPath;
		public bool isDev;
		public bool isAppBundle;
		public bool createSymbol;
	}
}