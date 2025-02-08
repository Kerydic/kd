using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace KDGame.Util
{
	public static class ABUtil
	{
		public const string UIAssetPathPrefix = "Assets/Game/AssetUI";
		public const string SceneObjPathPrefix = "Assets/Game/AssetScene";

		public const string HotUpdDllABName = "dll_hotupd";
		public const string MetadataDllABName = "dll_metadata";
		
		public const string ManifestPath = "assetbundles";
		public const string ManifestExt = ".manifest";

		/// <summary>
		/// 
		/// </summary>
		/// <returns>当前是否正在使用AssetBundle</returns>
		public static bool IsUsingAB()
		{
			return !Application.isEditor;
		}

		public static string GetBundleByPath(string path)
		{
			if (path.StartsWith(UIAssetPathPrefix))
			{
				path = "ui_" + path.Substring(UIAssetPathPrefix.Length + 1);
			}
			else if (path.StartsWith(SceneObjPathPrefix))
			{
				path = "scene_" + path.Substring(SceneObjPathPrefix.Length + 1);
			}
			else
			{
				return "";
			}

			// TODO 拓展资源分bundle策略
			return ComUtil.Str2Lower(path.Split('/')[0]);
		}

		public static string GetManifestByBundle(string bundleName)
		{
			// TODO 根据Bundle名获取manifest
			return ManifestPath;
		}
	}
}