using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using KDGame.Util;
using UnityEngine;

namespace KDGame.Editor.AssetBundle
{
	public static class AssetBundleBuilder
	{
		private const string abDirectory = "Assets/AssetBundles";

		private static void BuildAllAssetBundles(BuildTarget target)
		{
			SetAllAssetBundleNames();
			if (!Directory.Exists(abDirectory))
			{
				Directory.CreateDirectory(abDirectory);
			}

			ClearUnusedBundle();
			// 使用LZ4压缩
			BuildPipeline.BuildAssetBundles(abDirectory, BuildAssetBundleOptions.ChunkBasedCompression, target);
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build Android", false, 1)]
		public static void BuildAndroidAB()
		{
			BuildAllAssetBundles(BuildTarget.Android);
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build iOS", false, 2)]
		public static void BuildIosAB()
		{
			BuildAllAssetBundles(BuildTarget.iOS);
		}

		public static void SetAllAssetBundleNames()
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			HashSet<string> bundleNameSet = new HashSet<string>();
			// TODO 最简单的以路径打Bundle
			foreach (string path in assetPaths)
			{
				if (!ABUtil.IsValidAsset(path))
				{
					continue;
				}

				string bundleName = ABUtil.GetBundleByPath(path);
				SetAssetBundleName(bundleName, path);
				bundleNameSet.Add(bundleName);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.RemoveUnusedAssetBundleNames();
			AssetDatabase.Refresh();

			foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
			{
				if (bundleNameSet.Contains(bundleName))
				{
					continue;
				}

				if (AssetDatabase.RemoveAssetBundleName(bundleName, true))
				{
					Debug.Log("Force remove AssetBundle name: " + bundleName);
				}
				else
				{
					Debug.LogError("Force remove AssetBundle name failed: " + bundleName);
				}
			}
		}

		// TODO 处理AssetBundle变体
		private static void SetAssetBundleName(string bundleName, params string[] paths)
		{
			foreach (string path in paths)
			{
				AssetImporter importer = AssetImporter.GetAtPath(path);
				if (importer.assetBundleName != bundleName || !string.IsNullOrEmpty(importer.assetBundleVariant))
				{
					importer.SetAssetBundleNameAndVariant(bundleName, "");
				}
			}
		}

		private static void ClearUnusedBundle()
		{
			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
			string[] files = Directory.GetFiles(abDirectory);
			foreach (string path in files)
			{
				string fileName = Path.GetFileName(path);
				if (!fileName.EndsWith(".manifest") && Array.IndexOf(bundleNames, fileName) < 0)
				{
					File.Delete(path);
					string manifestPath = path + ".manifest";
					if (File.Exists(manifestPath))
					{
						File.Delete(manifestPath);
					}
				}
			}
		}
	}
}