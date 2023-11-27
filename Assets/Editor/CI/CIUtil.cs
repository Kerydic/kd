using System;
using UnityEditor;
using UnityEngine;

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
			// TODO
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
	}
}