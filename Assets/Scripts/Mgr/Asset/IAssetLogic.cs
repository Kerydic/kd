using System;
using KDGame.Base;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr.Asset
{
	internal interface IAssetLogic : IMgr
	{
		public void Update();
		public void Unload(string bundleName, ulong certID);
		public LoadCert LoadAsset(ToLoadAsset[] assetInfos);
		public LoadCert LoadAssetAsync(ToLoadAsset[] assetInfos, Action<bool, UnityObj[]> onEnd);
	}
}