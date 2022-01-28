using UnityEditor;

namespace KDGame.Util
{
	public static class ABUtil
	{
		private const string UIAssetPathPrefix = "Assets/AssetUI";
		private const string SceneObjPathPrefix = "Assets/AssetScene";

		public static bool IsValidAsset(string path)
		{
			return !AssetDatabase.IsValidFolder(path) &&
			       (path.StartsWith(UIAssetPathPrefix) || path.StartsWith(SceneObjPathPrefix));
		}

		public static string GetBundleByPath(string path)
		{
			if (AssetDatabase.IsValidFolder(path))
			{
				return null;
			}
			if (path.StartsWith(UIAssetPathPrefix))
			{
				path.Replace(UIAssetPathPrefix, "ui_");
			}
			else if (path.StartsWith(SceneObjPathPrefix))
			{
				path.Replace(SceneObjPathPrefix, "scene_");
			}
			else
			{
				return null;
			}

			path.Split('/');

			return string.Join("_", path.Split('/'));
		}
	}
}