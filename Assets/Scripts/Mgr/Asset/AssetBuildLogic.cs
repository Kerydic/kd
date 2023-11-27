using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KDGame.Util;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr.Asset
{
	internal class AssetBuildLogic : IAssetLogic
	{
		private const string ManifestName = "AssetBundleManifest";

		// 资源引用归零后多久卸载
		private static int UnloadInterval = 10;

		// 资源清单，用于加载依赖
		private AssetBundleManifest _manifest;

		// 还未被Unload的LoadCert
		private Dictionary<ulong, LoadCert> _certDict;

		// 已加载或正在加载的AssetBundle及依赖它的LoadCert信息
		private Dictionary<string, LoadInfo> _loadedInfo;

		// 当前正在异步加载的AssetBundle请求
		private Dictionary<string, AssetBundleCreateRequest> _requestMap;

		private KDLog _logger;

		public AssetBuildLogic()
		{
			_logger = new KDLog("AssetBuildLogic");

			InitManifest();
			_certDict = new Dictionary<ulong, LoadCert>();
			_loadedInfo = new Dictionary<string, LoadInfo>();
			_requestMap = new Dictionary<string, AssetBundleCreateRequest>();
		}

		/// <summary>
		/// 初始化资源清单
		/// </summary>
		private void InitManifest()
		{
			AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, ABUtil.ManifestPath));
			_manifest = ab.LoadAsset<AssetBundleManifest>(ManifestName);
			ab.Unload(true);
		}

		public void Restart()
		{
		}

		public void Unload(string bundleName, ulong certID)
		{
			if (_loadedInfo.TryGetValue(bundleName, out var info))
				info.RmCert(certID);
		}

		/// <summary>
		/// 生成LoadCert
		/// </summary>
		/// <param name="assetInfos">每个资源的路径及类型</param>
		/// <returns></returns>
		private LoadCert GetLoadCert(ToLoadAsset[] assetInfos)
		{
			if (assetInfos == null || assetInfos.Length <= 0)
				return null;
			List<string> bundles = new List<string>();
			// 解析这些资源路径所需的Bundle及依赖
			foreach (ToLoadAsset info in assetInfos)
			{
				string bundleName = ABUtil.GetBundleByPath(info.AssetPath);
				bundles.Add(bundleName);
				foreach (var bName in _manifest.GetAllDependencies(bundleName))
				{
					bundles.Add(bName);
				}
			}

			return new LoadCert(bundles.ToArray(), assetInfos);
		}

		private void LogAssetInfos(string prefix, ToLoadAsset[] infos)
		{
			StringBuilder builder = new StringBuilder(prefix);
			builder.AppendFormat(", count: {0}", infos.Length);
			foreach (var info in infos)
			{
				builder.AppendFormat("type: {0}, path: {1}\n", info.AssetType, info.AssetPath);
			}

			_logger.Info(builder.ToString());
		}

		/// <summary>
		/// 初始化Cert中每个请求加载的资源
		/// </summary>
		/// <param name="cid"></param>
		private void ExtractObjInCert(ulong cid)
		{
			if (!_certDict.TryGetValue(cid, out LoadCert cert))
				return;
			// 提取Bundle中的资源
			int count = cert.AssetInfos.Length;
			cert.Objs = new UnityObj[count];
			for (int i = 0; i < count; ++i)
			{
				string path = cert.AssetInfos[i].AssetPath;
				Type type = cert.AssetInfos[i].AssetType;
				string bundleName = ABUtil.GetBundleByPath(path);
				string goName = Path.GetFileName(path);
				if (_loadedInfo[bundleName]?.Status == LoadStatus.Success)
				{
					cert.Objs[i] = _loadedInfo[bundleName].BundleAsset.LoadAsset(goName, type);
				}
				else
				{
					cert.Status = LoadStatus.Fail;
					return;
				}
			}

			cert.Status = LoadStatus.Success;
		}

		public LoadCert LoadAsset(ToLoadAsset[] assetInfos)
		{
			LogAssetInfos("Invoke LoadAsset(Sync):", assetInfos);
			LoadCert cert = GetLoadCert(assetInfos);
			if (cert == null) return null;
			_certDict[cert.ID] = cert;

			// 根据LoadCert中的bundleName，创建/重利用ABLoadInfo
			foreach (var bName in cert.BundleNames)
			{
				if (!_loadedInfo.TryGetValue(bName, out LoadInfo info))
				{
					info = new LoadInfo(bName);
					_loadedInfo.Add(bName, info);
				}

				info.AddCert(cert.ID);
				if (info.BundleAsset == null)
				{
					AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bName));
					info.BundleAsset = bundle;
					info.Status = bundle != null ? LoadStatus.Success : LoadStatus.Fail;
					if (info.Status != LoadStatus.Success)
					{
						_logger.Error("Load Bundle Failed: " + bName);
					}
				}
			}

			ExtractObjInCert(cert.ID);

			return cert;
		}

		public LoadCert LoadAssetAsync(ToLoadAsset[] assetInfos, Action<bool, UnityObj[]> onEnd)
		{
			LogAssetInfos("Invoke LoadAssetAsync: ", assetInfos);
			LoadCert cert = GetLoadCert(assetInfos);
			if (cert == null) return null;

			_certDict[cert.ID] = cert;
			cert.OnLoadEnd = onEnd;

			// 根据LoadCert中的bundleName，创建/重利用ABLoadInfo
			foreach (var bName in cert.BundleNames)
			{
				if (!_loadedInfo.TryGetValue(bName, out LoadInfo info))
				{
					info = new LoadInfo(bName);
					_loadedInfo.Add(bName, info);
				}

				info.AddCert(cert.ID);
				if (info.BundleAsset == null && _requestMap.ContainsKey(bName))
				{
					_requestMap.Add(bName,
						AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bName)));
				}
			}

			return cert;
		}

		public void Update()
		{
			HashSet<ulong> maybeChangeCid = new HashSet<ulong>();
			// 如果有正在处理的异步加载，处理
			if (_requestMap.Count > 0)
			{
				foreach (var kv in _requestMap.Where(kv => kv.Value.isDone))
				{
					_requestMap.Remove(kv.Key);
					OnABCreateReqDone(kv.Key, kv.Value, maybeChangeCid);
				}
			}

			// 判断所有可能加载完成的LoadCert的状态
			foreach (var cid in maybeChangeCid)
			{
				CheckCertLoadStatus(cid);
			}

			// 处理卸载后超过一定时长的Bundle
			long time = TimeUtil.NowTimeStamp();
			foreach (var kv in _loadedInfo)
			{
				var info = kv.Value;
				if (time - info.MarkTime >= UnloadInterval)
				{
					info.Unload(false);
				}

				_loadedInfo.Remove(kv.Key);
			}
		}

		// 当一个异步加载流程结束
		private void OnABCreateReqDone(string bName, AssetBundleCreateRequest req, HashSet<ulong> maybeCid)
		{
			if (!req.isDone) return;
			var ab = req.assetBundle;
			// 已经被卸载掉了
			if (!_loadedInfo.TryGetValue(bName, out var info))
			{
				ab.Unload(true);
				return;
			}

			info.BundleAsset = ab;
			info.Status = ab != null ? LoadStatus.Success : LoadStatus.Fail;
			foreach (var cid in info.CertIDs)
			{
				maybeCid.Add(cid);
			}

			if (info.Status != LoadStatus.Success)
			{
				_logger.Error("Load Bundle Failed: " + bName);
			}
		}

		private void CheckCertLoadStatus(ulong cid)
		{
			if (!_certDict.TryGetValue(cid, out LoadCert cert)) return;
			LoadInfo temp;
			foreach (var bName in cert.BundleNames)
			{
				if (!_loadedInfo.TryGetValue(bName, out temp) || temp.Status == LoadStatus.Init) return;
				if (temp.Status == LoadStatus.Fail)
				{
					cert.Status = LoadStatus.Fail;
					cert.OnLoadEnd?.Invoke(false, null);
					return;
				}
			}

			ExtractObjInCert(cert.ID);
			cert.OnLoadEnd.Invoke(cert.Status == LoadStatus.Success, cert.Objs);
		}
	}
}