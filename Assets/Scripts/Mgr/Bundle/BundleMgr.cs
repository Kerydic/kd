using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KDGame.Base;
using KDGame.Util;
using UnityEngine;

namespace KDGame.Mgr.Bundle
{
	public class BundleMgr : MonoSingleton<BundleMgr>, IMgr
	{
		private Dictionary<string, BundleManifestPairs> _manifestDict = new Dictionary<string, BundleManifestPairs>();

		public IEnumerator Initialize()
		{
			if (!ABUtil.IsUsingAB()) yield break;
			// TODO 实现远程
			// TODO 实现多套Manifest
			yield return LoadLocalManifest(ABUtil.ManifestPath);
			yield return LoadSrvManifest(ABUtil.ManifestPath);
		}

		public void Restart()
		{
		}

		private BundleManifestPairs GetManifestPairs(string manifestName)
		{
			if (_manifestDict.TryGetValue(manifestName, out var manifestPairs)) return manifestPairs;
			manifestPairs = new BundleManifestPairs();
			_manifestDict.Add(manifestName, manifestPairs);
			return manifestPairs;
		}

		// 加载本地清单文件
		private IEnumerator LoadLocalManifest(string manifestName)
		{
			var settings = BundleSettings.Instance;
			var localUrl = Path.Combine(ComUtil.GetStreamingAssetsUrl(), settings.bundleFolder, manifestName);
			var loader = new FileLoader(localUrl);
			yield return loader.LoadAsync();
			if (loader.status == FileLoadStatus.Loaded)
				GetManifestPairs(manifestName).localManifest = new BundleManifest(loader.GetContent());
			else
				Debug.LogError(loader.error);
		}

		// 加载服务器清单文件
		private IEnumerator LoadSrvManifest(string manifestName)
		{
			// TODO 获取服务器当前最新清单文件数据
			var srvManifestMd5 = string.Empty;
			if (string.IsNullOrEmpty(srvManifestMd5)) yield break;

			var localPath = GetManifestSavePath(srvManifestMd5);
			var dlUrl = GetSrvManifestUrl(srvManifestMd5);
			var loader = new FileLoader(dlUrl, localPath);
			yield return loader.LoadAsync(3, true);
			if (loader.status == FileLoadStatus.Loaded)
				GetManifestPairs(manifestName).remoteManifest = new BundleManifest(loader.GetContent());
			else
				Debug.LogError(loader.error);
		}

		public AssetBundleLoader GetBundleLoader(string bundleName)
		{
			var manifestName = ABUtil.GetManifestByBundle(bundleName);
			var pairs = GetManifestPairs(manifestName);
			var info = pairs.Parse(bundleName);
			var relativePath = Path.Combine(BundleSettings.Instance.bundleFolder, $"{bundleName}-{info.md5}");
			AssetBundleLoader loader;
			if (info.isInBuild)
			{
				// 直接从包体加载
				loader = new AssetBundleLoader(Path.Combine(ComUtil.GetStreamingAssetsUrl(), relativePath));
			}
			else
			{
				// 从远端加载、下载
				loader = new AssetBundleLoader(GetSrvBundleUrl(bundleName, info.md5), relativePath);
			}

			return loader;
		}

		// 加载指定的Bundle
		public IEnumerator LoadBundle(string bundleName, Action<bool, AssetBundle> callback)
		{
			var loader = GetBundleLoader(bundleName);
			yield return loader.LoadAsync(3, true);
			if (loader.status == FileLoadStatus.Loaded)
			{
				callback?.Invoke(true, loader.BundleData);
			}
			else
			{
				Debug.LogError(loader.error);
				callback?.Invoke(false, null);
			}
		}

		private static string GetSrvManifestUrl(string md5)
		{
			// TODO
			return md5;
		}

		private static string GetSrvBundleUrl(string bundleName, string md5)
		{
			// TODO
			return bundleName + "-" + md5;
		}

		private static string GetManifestSavePath(string md5)
		{
			var settings = BundleSettings.Instance;
			return Path.Combine(settings.bundleFolder, $"{settings.manifestTxtName}-{md5}.txt");
		}
	}
}