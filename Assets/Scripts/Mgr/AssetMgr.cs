using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KDGame.Base;
using KDGame.Util;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr
{
	public class AssetMgr : MonoSingleton<AssetMgr>, IMgr
	{
		private const string ManifestPath = "assetbundles";
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

		protected override void OnAwake()
		{
			base.OnAwake();
			_logger = new KDLog("AssetMgr");

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
			AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, ManifestPath));
			_manifest = ab.LoadAsset<AssetBundleManifest>(ManifestName);
			ab.Unload(true);
		}

		public void Restart()
		{
		}

		internal void Unload(string bundleName, ulong certID)
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

		/// <summary>
		/// 将同类型的资源路径转化为ToLoadAsset数组
		/// </summary>
		/// <param name="paths">资源路径列表</param>
		/// <typeparam name="T">资源类型</typeparam>
		/// <returns></returns>
		private ToLoadAsset[] GenAssetInfos<T>(string[] paths)
		{
			if (paths == null || paths.Length <= 0) return null;
			int count = paths.Length;
			ToLoadAsset[] infos = new ToLoadAsset[count];
			Type type = typeof(T);
			for (int i = 0; i < count; ++i)
			{
				infos[i] = new ToLoadAsset
				{
					AssetPath = paths[i],
					AssetType = type
				};
			}

			return infos;
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
				cert.Objs[i] = _loadedInfo[bundleName]?.BundleAsset.LoadAsset(goName, type);
			}
		}

		#region Synchronize Load

		public LoadCert LoadAsset<T>(string path) where T : UnityObj
		{
			return LoadAsset<T>(new[] {path});
		}

		/// <summary>
		/// 同步加载资源，禁止在异步加载未完成时同步加载同一个资源
		/// </summary>
		/// <param name="paths">需要加载的资源的路径</param>
		/// <typeparam name="T">这些资源的类型</typeparam>
		/// <returns></returns>
		public LoadCert LoadAsset<T>(string[] paths) where T : UnityObj
		{
			return LoadAsset(GenAssetInfos<T>(paths));
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

		#endregion

		#region Asynchronize Load

		public LoadCert LoadAssetAsync<T>(string path, Action<bool, T> onEnd) where T : UnityObj
		{
			return LoadAssetAsync<T>(new[] {path}, (success, objects) =>
			{
				if (!success)
					onEnd.Invoke(false, null);
				else
					onEnd.Invoke(true, objects[0]);
			});
		}

		// 异步加载资源
		public LoadCert LoadAssetAsync<T>(string[] paths, Action<bool, T[]> onEnd) where T : UnityObj
		{
			return LoadAssetAsync(GenAssetInfos<T>(paths), (success, objects) =>
			{
				
				// TODO
				// onEnd.Invoke(success, objects);
			});
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

		#endregion

		private void Update()
		{
			// 如果有正在处理的异步加载，处理
			if (_requestMap.Count > 0)
			{
				foreach (var kv in _requestMap.Where(kv => kv.Value.isDone))
				{
					_requestMap.Remove(kv.Key);
					OnABCreateReqDone(kv.Key, kv.Value);
				}
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
		private void OnABCreateReqDone(string bName, AssetBundleCreateRequest req)
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
			if (info.Status != LoadStatus.Success)
			{
				_logger.Error("Load Bundle Failed: " + bName);
			}
		}
	}

	internal enum LoadStatus
	{
		Init,
		Success,
		Fail
	}

	/// <summary>
	/// 保存BundleName及所有引用这个Bundle的LoadCert的ID，当LoadCert数量降为0，即进入卸载流程
	/// </summary>
	internal class LoadInfo
	{
		public static bool UnloadAllLoadedObjects = true;
		public string BundleName;
		public LoadStatus Status = LoadStatus.Init;
		public AssetBundle BundleAsset;
		private readonly HashSet<ulong> _certIDs;

		/// <summary>
		/// 被标记为可卸载的时间，时间戳，为-1时表示不能卸载
		/// </summary>
		public long MarkTime = -1;

		public LoadInfo(string bundleName)
		{
			BundleName = bundleName;
			_certIDs = new HashSet<ulong>();
		}

		public void Unload(bool force)
		{
			if (force || _certIDs.Count <= 0)
			{
				BundleAsset.Unload(UnloadAllLoadedObjects);
			}
		}

		public void AddCert(ulong cid)
		{
			_certIDs.Add(cid);
			MarkTime = -1;
		}

		/// <summary>
		/// 移除某个LoadCert对此Bundle的引用
		/// </summary>
		/// <param name="cid"></param>
		public void RmCert(ulong cid)
		{
			if (_certIDs.Contains(cid))
				_certIDs.Remove(cid);
			if (_certIDs.Count <= 0)
				MarkTime = TimeUtil.NowTimeStamp();
		}
	}

	/// <summary>
	/// 请求一个/一组资源的证明，该证明由AssetMgr创建，有且仅有持有该证明时，外部才能显式卸载该资源
	/// </summary>
	public class LoadCert
	{
		// 自增的证明ID，用来唯一标记证明
		private static ulong _certID;

		public readonly ulong ID;
		private bool _unloaded;

		public string[] BundleNames { get; }
		public ToLoadAsset[] AssetInfos { get; }

		public UnityObj[] Objs;
		public Action<bool, UnityObj[]> OnLoadEnd;

		public LoadCert(string[] bundleNames, ToLoadAsset[] toLoads)
		{
			ID = ++_certID;
			BundleNames = bundleNames;
			AssetInfos = toLoads;
		}

		public void Unload()
		{
			if (_unloaded)
				return;
			foreach (var name in BundleNames)
				AssetMgr.Instance.Unload(name, ID);
			_unloaded = true;
		}
	}

	/// <summary>
	/// 业务意图加载的资源，包含资源路径和资源类型
	/// </summary>
	public struct ToLoadAsset
	{
		public Type AssetType;
		public string AssetPath;
	}
}