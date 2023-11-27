using System.IO;
using KDGame.Editor.CI;
using KDGame.Util;
using UnityEditor;

namespace KDGame.Editor.Utils
{
	public static class AssetUtil
	{
		public enum AssetType
		{
			NONE, // 非可用资源
			UI, // UI
			SCENE, // 场景
			DLL_HOTUPD, // 热更代码资源
			DLL_METADATA, // 元数据代码资源
		}

		public static AssetType GetAssetType(string path)
		{
			if (AssetDatabase.IsValidFolder(path)) return AssetType.NONE;
			if (path.StartsWith(ABUtil.UIAssetPathPrefix)) return AssetType.UI;
			if (path.StartsWith(ABUtil.SceneObjPathPrefix)) return AssetType.SCENE;
			if (path.StartsWith(Path.Combine(CIConst.TempDllPath, CIConst.HotUpdDllPath))) return AssetType.DLL_HOTUPD;
			if (path.StartsWith(Path.Combine(CIConst.TempDllPath, CIConst.MetadataDllPath))) return AssetType.DLL_METADATA;
			return AssetType.NONE;
		}
	}
}