using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using KDGame.Editor.Utils;
using KDGame.Util;
using UnityEngine;

namespace KDGame.Editor.CI
{
	public static class CIABUtil
	{
		public static void BuildAllAssetBundles(BuildTarget target)
		{
			SetAllAssetBundleNames();
			var libPath = Path.Combine(CIConst.ABLibPath, target.ToString());
			if (!Directory.Exists(libPath))
				Directory.CreateDirectory(libPath);
			ClearUnusedBundle(libPath);
			// 使用LZ4压缩
			var manifest = BuildPipeline.BuildAssetBundles(libPath, BuildAssetBundleOptions.ChunkBasedCompression, target);
			// 重命名Manifest文件
			RenameManifestAB(libPath, target);
			// 生成文件清淡文本文件
			GenManifestTxt(manifest, libPath);
			AssetDatabase.Refresh();
			Debug.Log("Build all assetBundle end!");
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build Curr", false, 1)]
		public static void BuildCurrAB()
		{
			BuildAllAssetBundles(EditorUserBuildSettings.activeBuildTarget);
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build Android", false, 2)]
		public static void BuildAndroidAB()
		{
			BuildAllAssetBundles(BuildTarget.Android);
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build iOS", false, 3)]
		public static void BuildIosAB()
		{
			BuildAllAssetBundles(BuildTarget.iOS);
		}

		public static void SetAllAssetBundleNames()
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			HashSet<string> bundleNameSet = new HashSet<string>();
			// 最简单的以路径打Bundle
			foreach (string path in assetPaths)
			{
				switch (AssetUtil.GetAssetType(path))
				{
					case AssetUtil.AssetType.UI:
					case AssetUtil.AssetType.SCENE:
						var bundleName = ABUtil.GetBundleByPath(path);
						SetAssetBundleName(bundleName, path);
						bundleNameSet.Add(bundleName);
						break;
					case AssetUtil.AssetType.DLL_HOTUPD:
						SetAssetBundleName(ABUtil.HotUpdDllABName, path);
						bundleNameSet.Add(ABUtil.HotUpdDllABName);
						break;
					case AssetUtil.AssetType.DLL_METADATA:
						SetAssetBundleName(ABUtil.MetadataDllABName, path);
						bundleNameSet.Add(ABUtil.MetadataDllABName);
						break;
					default: continue;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.RemoveUnusedAssetBundleNames();
			AssetDatabase.Refresh();

			foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
			{
				if (bundleNameSet.Contains(bundleName)) continue;
				if (AssetDatabase.RemoveAssetBundleName(bundleName, true))
					Debug.Log("Force remove AssetBundle name: " + bundleName);
				else
					Debug.LogError("Force remove AssetBundle name failed: " + bundleName);
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

		private static void ClearUnusedBundle(string libPath)
		{
			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
			string[] files = Directory.GetFiles(libPath);
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

		private static void GenManifestTxt(AssetBundleManifest manifest, string libPath)
		{
			var dict = new Dictionary<string, string>();
			foreach (var abName in manifest.GetAllAssetBundles())
			{
				dict[abName] = manifest.GetAssetBundleHash(abName).ToString();
			}
			dict[ABUtil.ManifestPath] = ComUtil.GetMD5(Path.Combine(libPath, ABUtil.ManifestPath));
			var content = new StringBuilder();
			foreach (var kv in dict)
			{
				content.AppendLine($"{kv.Key} {kv.Value}");
			}

			var fPath = Path.Combine(libPath, CIConst.ManifestTxtName);
			if (File.Exists(fPath))
				File.Delete(fPath);
			File.WriteAllText(fPath, content.ToString());
		}
		
		private static void RenameManifestAB(string libPath ,BuildTarget target)
		{
			var srcName = Path.Combine(libPath, target.ToString());
			var dstName = Path.Combine(libPath, ABUtil.ManifestPath);
			File.Move(srcName, dstName);
			File.Move(srcName + ABUtil.ManifestExt, dstName + ABUtil.ManifestExt);
		}
	}
}