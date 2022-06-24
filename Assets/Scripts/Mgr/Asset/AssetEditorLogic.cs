using System;
using System.IO;
using KDGame.Util;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr.Asset
{
	internal class AssetEditorLogic : IAssetLogic
	{
		private const string AssetRoot = "Assets/Game/";
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
				Type assetType = assetInfos[i].AssetType;
				string path = GetEditorPath(assetInfos[i].AssetPath, assetType);
				if (!File.Exists(path))
				{
					cert.Status = LoadStatus.Fail;
					return cert;
				}

				cert.Objs[i] = AssetDatabase.LoadAssetAtPath(path, assetType);
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


		private static string[] _supportedExtensions =
		{
			".prefab", ".png", ".jpg", ".jpeg", ".txt", ".bytes", ".json",
			".tga", ".mat", ".asset", ".spriteatlas", ".spriteAtlas", ".mp3", ".ogg", ".wav", ".otf", ".ttf", ".ttc"
		};

		/// <summary>
		/// 根据传入类型，给传入路径添加后缀，以便于在Editor模式下加载
		/// </summary>
		/// <param name="runtimePath">运行时传入路径，通常没有后缀</param>
		/// <param name="assetType">传入的资源类型</param>
		/// <returns>拼合了后缀的路径</returns>
		private static string GetEditorPath(string runtimePath, Type assetType)
		{
			string fullPath = Path.Combine(AssetRoot, runtimePath);
			if (assetType == typeof(GameObject))
			{
				return fullPath + ".prefab";
			}

			foreach (var extension in _supportedExtensions)
			{
				string tempPath = fullPath + extension;
				if (File.Exists(tempPath))
				{
					return tempPath;
				}
			}

			return fullPath;
		}
	}
}