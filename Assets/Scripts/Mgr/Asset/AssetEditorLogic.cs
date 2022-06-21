using System;
using System.IO;
using KDGame.Util;
using UnityEditor;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr.Asset
{
	internal class AssetEditorLogic : IAssetLogic
	{
		private const string AssetRoot = "Assets/";
		private readonly string[] _emptyBNames = new string[1];

		public void Restart()
		{
		}

		public void Update()
		{
		}

		public void Unload(string bundleName, ulong certID)
		{
		}

		public LoadCert LoadAsset(ToLoadAsset[] assetInfos)
		{
			LoadCert cert = new LoadCert(_emptyBNames, assetInfos);
			int count = assetInfos.Length;
			cert.Objs = new UnityObj[count];
			for (int i = 0; i < count; ++i)
			{
				string path = AssetRoot + assetInfos[i].AssetPath;
				if (!File.Exists(path))
				{
					cert.Status = LoadStatus.Fail;
					return cert;
				}

				cert.Objs[i] = AssetDatabase.LoadAssetAtPath(path, assetInfos[i].AssetType);
				if (cert.Objs[i] == null)
				{
					cert.Status = LoadStatus.Fail;
					return cert;
				}
			}

			cert.Status = LoadStatus.Success;
			return cert;
		}

		public LoadCert LoadAssetAsync(ToLoadAsset[] assetInfos, Action<bool, UnityObj[]> onEnd)
		{
			LoadCert cert = LoadAsset(assetInfos);
			ComUtil.DelayCall(() => { onEnd?.Invoke(cert.Status == LoadStatus.Success, cert.Objs); }, 17);
			return cert;
		}
	}
}