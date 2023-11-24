using KDGame.Util;
using UnityEditor;

namespace KDGame.Editor.Utils
{
	public static class AssetUtil
	{
		public static bool IsValidAsset(string path)
		{
			return !AssetDatabase.IsValidFolder(path) &&
			       (path.StartsWith(ABUtil.UIAssetPathPrefix) || path.StartsWith(ABUtil.SceneObjPathPrefix));
		}
	}
}