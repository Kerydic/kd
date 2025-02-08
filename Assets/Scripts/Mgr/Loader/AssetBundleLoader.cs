using UnityEngine;
using UnityEngine.Networking;

namespace KDGame.Mgr
{
	public class AssetBundleLoader : FileLoader
	{
		public AssetBundle BundleData { get; private set; }

		public AssetBundleLoader(string url) : base(url)
		{
		}

		public AssetBundleLoader(string url, string savePath) : base(url, savePath)
		{
		}

		protected override UnityWebRequest GetWebRequest(string url)
		{
			return UnityWebRequestAssetBundle.GetAssetBundle(url);
		}

		protected override void ReadData(UnityWebRequest webRequest)
		{
			var abDownloadHandler = webRequest.downloadHandler as DownloadHandlerAssetBundle;
			_bytes = abDownloadHandler?.data;
			_content = abDownloadHandler?.text;
			BundleData = abDownloadHandler?.assetBundle;
		}
	}
}