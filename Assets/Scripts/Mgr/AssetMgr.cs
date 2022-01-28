using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

		// 资源清单，用于加载依赖
		private AssetBundleManifest _manifest;

		// 加载到内存中的AssetBundle
		private Dictionary<string, AssetBundle> _loadedBundles;

		// 每个Bundle的被依赖列表
		private Dictionary<string, HashSet<string>> _dependentMap;

		// 需要异步加载的AssetBundle队列
		private Queue<string> _toAsyncLoadBundles;

		// 一次性加载的资源的组合列表
		private List<GroupLoadRequest> _groupRequests;

		// 当前正在加载的AssetBundle
		private AssetBundleCreateRequest _currLoadRequest;

		private KDLog _logger;

		protected override void OnAwake()
		{
			base.OnAwake();
			AssetBundle ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, ManifestPath));
			_manifest = ab.LoadAsset<AssetBundleManifest>(ManifestName);

			_loadedBundles = new Dictionary<string, AssetBundle>();
			_dependentMap = new Dictionary<string, HashSet<string>>();

			_toAsyncLoadBundles = new Queue<string>();
			_groupRequests = new List<GroupLoadRequest>();

			_logger = new KDLog("AssetMgr");
		}

		public void Restart()
		{
		}

		public void Unload(string path)
		{
			string bundleName = ABUtil.GetBundleByPath(path);
			if (!_loadedBundles.ContainsKey(bundleName))
			{
				if (_toAsyncLoadBundles.Contains(bundleName))
				{
					if (_currLoadRequest != null && _toAsyncLoadBundles.Peek() == bundleName)
					{
						// TODO 正在异步加载的资源如何卸载
					}
					else
					{
						// TODO 移除指定BundleName
					}
				}

				return;
			}

			_loadedBundles[bundleName].Unload(true);
			RmDependent(bundleName);
			foreach (KeyValuePair<string, AssetBundle> kv in _loadedBundles)
			{
				if (!IsReferenced(kv.Key))
				{
					kv.Value.Unload(true);
					_loadedBundles[kv.Key] = null;
				}
			}
		}

		public T[] LoadAsset<T>(string path) where T : UnityObj
		{
			return LoadAsset<T>(new[] {path});
		}

		// 同步加载资源，禁止在异步加载未完成时同步加载同一个资源
		public T[] LoadAsset<T>(string[] paths) where T : UnityObj
		{
			int count = paths.Length;
			_logger.Info("Invoke LoadAsset, type: {0}, count: {1}, paths: {2}", typeof(T), count,
				string.Join(",", paths));
			if (count <= 0)
			{
				return null;
			}

			T[] resAry = new T[count];
			for (int i = 0; i < count; ++i)
			{
				string bundleName = ABUtil.GetBundleByPath(paths[i]);
				// 若该Bundle没有加载到内存，则加载其所有依赖和本体
				if (!_loadedBundles.ContainsKey(bundleName))
				{
					string[] dependencies = _manifest.GetAllDependencies(bundleName);
					foreach (string dependency in dependencies)
					{
						if (_loadedBundles.ContainsKey(dependency))
						{
							continue;
						}

						_logger.Info("Try load dependency: {0}", dependency);
						AssetBundle dependentAb =
							AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, dependency));
						if (dependentAb == null)
						{
							_logger.Error("Failed to load dependent AssetBundle: {0}", dependency);
							continue;
						}

						_logger.Info("Load dependency succeed: {0}", dependency);
						OnNewBundleLoaded(dependency, dependentAb);
					}

					AssetBundle bundle =
						AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));
					if (bundle == null)
					{
						_logger.Error("Failed to load AssetBundle! Path: {0}", paths[i]);
						return null;
					}

					OnNewBundleLoaded(bundleName, bundle);
				}

				string goName = Path.GetFileName(paths[i]);
				resAry[i] = _loadedBundles[bundleName].LoadAsset<T>(goName);
			}

			return resAry;
		}

		public void LoadAssetAsync<T>(string path, Action<bool, T> onEnd) where T : UnityObj
		{
			LoadAssetAsync<T>(new[] {path}, (success, objects) =>
			{
				if (!success)
					onEnd.Invoke(false, null);
				else
					onEnd.Invoke(true, objects[0]);
			});
		}

		// 异步加载资源
		public void LoadAssetAsync<T>(string[] paths, Action<bool, T[]> onEnd) where T : UnityObj
		{
			int count = paths.Length;
			_logger.Info("Invoke LoadAssetAsync, type: {0}, count: {1}, paths: {2}", typeof(T), count,
				string.Join(",", paths));
			if (count <= 0)
			{
				onEnd.Invoke(false, null);
				return;
			}

			HashSet<string> bundleNames = new HashSet<string>();
			bool needLoad = false;
			for (int i = 0; i < count; ++i)
			{
				string bundleName = ABUtil.GetBundleByPath(paths[i]);
				bundleNames.Add(bundleName);
				if (_loadedBundles.ContainsKey(bundleName))
				{
					continue;
				}

				// 只要任何一个Bundle仍未加载进内存，就需要走异步加载流程
				needLoad = true;

				// 已经在异步加载的资源，跳过
				if (_toAsyncLoadBundles.Contains(bundleName))
				{
					continue;
				}

				// 尚未在异步加载的资源，将其与其所有尚未加载的依赖加入加载队列
				foreach (string dependency in _manifest.GetAllDependencies(bundleName))
				{
					if (_loadedBundles.ContainsKey(dependency) || _toAsyncLoadBundles.Contains(dependency))
					{
						continue;
					}

					_logger.Info("Append dependency: {0}", dependency);
					_toAsyncLoadBundles.Enqueue(dependency);
				}

				_toAsyncLoadBundles.Enqueue(bundleName);
			}

			Action<bool> onLoadEnd = success =>
			{
				if (!success)
				{
					onEnd.Invoke(false, null);
					return;
				}

				T[] resAry = new T[count];
				for (int i = 0; i < count; ++i)
				{
					string path = paths[i];
					resAry[i] = _loadedBundles[ABUtil.GetBundleByPath(path)].LoadAsset<T>(Path.GetFileName(path));
				}

				onEnd.Invoke(true, resAry);
			};

			if (!needLoad)
			{
				onLoadEnd.Invoke(true);
				return;
			}

			_groupRequests.Add(new GroupLoadRequest
			{
				bundleNames = bundleNames,
				onLoadEnd = onLoadEnd
			});
		}

		// 新Bundle加载到内存时，为其所有依赖添加依赖关系
		private void OnNewBundleLoaded(string bundleName, AssetBundle bundle)
		{
			_loadedBundles[bundleName] = bundle;
			foreach (string dependency in _manifest.GetAllDependencies(bundleName))
			{
				if (_dependentMap[dependency] == null)
				{
					_dependentMap[dependency] = new HashSet<string>();
				}

				_dependentMap[dependency].Add(bundleName);
			}
		}

		private bool IsReferenced(string bundleName)
		{
			if (_dependentMap.TryGetValue(bundleName, out HashSet<string> dependents))
			{
				return dependents.Count > 0;
			}

			return false;
		}

		// 将一个Bundle从依赖关系列表中移除
		private void RmDependent(string bundleName)
		{
			HashSet<string> dependents;
			foreach (string dependency in _manifest.GetAllDependencies(bundleName))
			{
				if (_dependentMap.TryGetValue(dependency, out dependents))
				{
					dependents.Remove(bundleName);
				}
			}
		}

		#region ASYNC LOAD LOGIC

		private void Update()
		{
			// 这一帧刚好有Bundle加载完毕，则根据是否加载完成进行事件派出
			if (_currLoadRequest != null && _currLoadRequest.isDone)
			{
				string loadingBundleName = _toAsyncLoadBundles.Dequeue();
				bool loadSucceed = false;
				AssetBundle bundle = _currLoadRequest.assetBundle;
				if (bundle != null && bundle.name == loadingBundleName)
				{
					loadSucceed = true;
					OnNewBundleLoaded(loadingBundleName, bundle);
				}
				else
				{
					_logger.Error("Failed to load bundle async: {0}", loadingBundleName);
				}

				_currLoadRequest = null;
				List<int> removedIndex = new List<int>();
				for (int i = 0; i < _groupRequests.Count; ++i)
				{
					GroupLoadRequest req = _groupRequests[i];
					if (!req.bundleNames.Contains(loadingBundleName))
					{
						continue;
					}

					if (!loadSucceed)
					{
						req.onLoadEnd.Invoke(false);
						removedIndex.Add(i);
						continue;
					}

					bool isAllLoaded = true;
					foreach (string bundleName in req.bundleNames)
					{
						if (_loadedBundles[bundleName] == null)
						{
							isAllLoaded = false;
						}
					}

					if (isAllLoaded)
					{
						req.onLoadEnd.Invoke(true);
						removedIndex.Add(i);
					}
				}

				int index = removedIndex.Count - 1;
				for (int i = _groupRequests.Count - 1; i > -1; --i)
				{
					if (index < 0)
					{
						break;
					}

					if (i == removedIndex[index])
					{
						_groupRequests.RemoveAt(i);
						--index;
					}
				}
			}

			if (_currLoadRequest == null && _toAsyncLoadBundles.Count > 0)
			{
				string bundleName = _toAsyncLoadBundles.Peek();

				_currLoadRequest =
					AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, bundleName));
			}
		}

		#endregion
	}

	// 同时加载多个资源的请求
	public struct GroupLoadRequest
	{
		public HashSet<string> bundleNames;
		public Action<bool> onLoadEnd;
	}
}