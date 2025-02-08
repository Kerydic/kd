using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using KDGame.Editor.Utils;
using KDGame.Mgr.Bundle;
using KDGame.Util;
using UnityEngine;

namespace KDGame.Editor.CI
{
	public static class CIABUtil
	{
		/// <summary>
		/// 返回AssetBundle的存储文件夹路径
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static string BuildAllAssetBundles(BuildTarget target)
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
			// 给生成的AB添加Hash后缀，并生成清单文本文件，将他们移动到特殊文件夹
			var signedPath =  libPath + "_Signed";
			if (Directory.Exists(signedPath))
				Directory.Delete(signedPath, true);
			Directory.CreateDirectory(signedPath);
			AddHashSuffixAndGenManifestTxt(manifest, libPath, signedPath);
			AssetDatabase.Refresh();
			// TODO 异常依赖检测
			Debug.Log("Build all assetBundle end!");
			return signedPath;
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Build Active Target", false, 1)]
		public static void BuildActiveTarget()
		{
			BuildAllAssetBundles(EditorUserBuildSettings.activeBuildTarget);
		}

		#region AB NAME
		[MenuItem(EditorMenuConst.AssetBundle + "Set All AssetBundle Names", false, 101)]
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
			Debug.Log("Set All AssetBundle Names End!");
		}

		public static void ClearAllAssetBundleNames(bool rename)
		{
			foreach (var path in AssetDatabase.GetAllAssetPaths())
			{
				if (AssetDatabase.IsValidFolder(path) || AssetUtil.GetAssetType(path) == AssetUtil.AssetType.NONE) continue;
				AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant("", "");
			}
			Debug.Log("Clear All AssetBundle Names End!");
			if (rename)
				SetAllAssetBundleNames();
			else
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.RemoveUnusedAssetBundleNames();
				AssetDatabase.Refresh();
			}
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Clear All AssetBundle Names", false, 102)]
		public static void ClearAllAssetBundleNames()
		{
			ClearAllAssetBundleNames(false);
		}

		[MenuItem(EditorMenuConst.AssetBundle + "Clear And Rename All AssetBundle Names", false, 103)]
		public static void ClearAndRenameAllAssetBundleNames()
		{
			ClearAllAssetBundleNames(true);
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
		#endregion

		private static void AddHashSuffixAndGenManifestTxt(AssetBundleManifest manifest, string libPath, string signedPath)
		{
			// TODO 拿ScriptableObject
			var myManifest = new BundleManifest(string.Empty);
			// 清单Bundle
			var manifestBundleName = Path.Combine(libPath, ABUtil.ManifestPath);
			var fileInfo = new FileInfo(manifestBundleName);
			var manifestBundleHash = ComUtil.GetMD5(manifestBundleName, false);
			myManifest.SetManifestEntry(ABUtil.ManifestPath, manifestBundleHash, fileInfo.Length);
			File.Copy(manifestBundleName, Path.Combine(signedPath, ABUtil.ManifestPath + "-" + manifestBundleHash));
			// 其他Bundle
			foreach (var abName in manifest.GetAllAssetBundles())
			{
				var fName = Path.Combine(libPath, abName);
				fileInfo = new FileInfo(fName);
				var abHash = manifest.GetAssetBundleHash(abName);
				myManifest.SetOrAddEntry(abName, abHash.ToString(), fileInfo.Length);
				File.Copy(fName, Path.Combine(signedPath, abName + "-" + abHash));
			}
			// 写文件
			var fPath = Path.Combine(signedPath, CIConst.ManifestTxtName);
			if (File.Exists(fPath))
				File.Delete(fPath);
			File.WriteAllText(fPath, myManifest.Serialize());
			File.Move(fPath, Path.Combine(signedPath, CIConst.ManifestTxtName + "-" + ComUtil.GetMD5(fPath, false)));
		}

		private static void RenameManifestAB(string libPath, BuildTarget target)
		{
			var srcName = Path.Combine(libPath, target.ToString());
			var dstName = Path.Combine(libPath, ABUtil.ManifestPath);
			File.Move(srcName, dstName);
			File.Move(srcName + ABUtil.ManifestExt, dstName + ABUtil.ManifestExt);
		}
	}
}