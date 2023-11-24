using System.Globalization;
using UnityEditor;

namespace KDGame.Util
{
	public static class ABUtil
	{
		public const string UIAssetPathPrefix = "Assets/AssetUI";
		public const string SceneObjPathPrefix = "Assets/AssetScene";

		private static CultureInfo _enCulture = null;

		public static string GetBundleByPath(string path)
		{
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

			_enCulture ??= new CultureInfo("en-US");

			return string.Join("_", path.Split('/')).ToLower(_enCulture);
		}
	}
}