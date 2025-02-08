using UnityEngine;
using UnityEngine.Serialization;

namespace KDGame.Mgr.Bundle
{
	public class BundleSettings : ScriptableObject
	{
		public static BundleSettings Instance;
		// 最终构建/运行时加载用
		public string bundleFolder = "AssetBundles";
		public string manifestTxtName = "ManifestTxt";
	}
}