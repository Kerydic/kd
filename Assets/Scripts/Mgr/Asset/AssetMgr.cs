using System;
using System.Collections.Generic;
using KDGame.Base;
using KDGame.Mgr.Asset;
using KDGame.Util;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace KDGame.Mgr
{
	public class AssetMgr : MonoSingleton<AssetMgr>, IMgr
	{
		private IAssetLogic _logic;

		protected override void OnAwake()
		{
#if UNITY_EDITOR
			_logic = new AssetEditorLogic();
#else
			_logic = new AssetBuildMgr();
#endif
		}

		protected void Update()
		{
			_logic.Update();
		}

		public void Restart()
		{
			_logic.Restart();
		}

		public static void Unload(string bundleName, ulong certID)
		{
			Instance._logic.Unload(bundleName, certID);
		}

		/// <summary>
		/// 同步加载资源，禁止在异步加载未完成时同步加载同一个资源
		/// </summary>
		/// <param name="path">需要加载的资源的路径</param>
		/// <typeparam name="T">资源的类型</typeparam>
		/// <returns></returns>
		public static LoadCert LoadAsset<T>(string path) where T : UnityObj
		{
			return LoadAsset<T>(new[] {path});
		}

		public static LoadCert LoadAsset<T>(string[] paths) where T : UnityObj
		{
			return LoadAsset(GenAssetInfos<T>(paths));
		}

		public static LoadCert LoadAsset(ToLoadAsset[] assetInfos)
		{
			return Instance._logic.LoadAsset(assetInfos);
		}

		/// <summary>
		/// 异步加载资源，调用后不允许对同一资源进行同步加载
		/// </summary>
		/// <param name="path">需要加载的资源的路径</param>
		/// <param name="onEnd">加载完成回调</param>
		/// <typeparam name="T">资源的类型</typeparam>
		/// <returns></returns>
		public static LoadCert LoadAssetAsync<T>(string path, Action<bool, T> onEnd) where T : UnityObj
		{
			return LoadAssetAsync<T>(new[] {path},
				(success, objects) => { onEnd.Invoke(success, success ? objects[0] : null); });
		}

		public static LoadCert LoadAssetAsync<T>(string[] paths, Action<bool, T[]> onEnd) where T : UnityObj
		{
			return LoadAssetAsync(GenAssetInfos<T>(paths), (success, objects) =>
			{
				if (!success)
				{
					onEnd.Invoke(false, null);
					return;
				}

				T[] res = new T[objects.Length];
				for (int i = 0; i < objects.Length; ++i)
				{
					res[i] = objects as T;
				}

				onEnd.Invoke(true, res);
			});
		}

		public static LoadCert LoadAssetAsync(ToLoadAsset[] assetInfos, Action<bool, UnityObj[]> onEnd)
		{
			return Instance._logic.LoadAssetAsync(assetInfos, onEnd);
		}

		/// <summary>
		/// 将同类型的资源路径转化为ToLoadAsset数组
		/// </summary>
		/// <param name="paths">资源路径列表</param>
		/// <typeparam name="T">资源类型</typeparam>
		/// <returns></returns>
		private static ToLoadAsset[] GenAssetInfos<T>(string[] paths)
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
	}

	public enum LoadStatus
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
		public readonly HashSet<ulong> CertIDs;

		/// <summary>
		/// 被标记为可卸载的时间，时间戳，为-1时表示不能卸载
		/// </summary>
		public long MarkTime = -1;

		public LoadInfo(string bundleName)
		{
			BundleName = bundleName;
			CertIDs = new HashSet<ulong>();
		}

		public void Unload(bool force)
		{
			if (force || CertIDs.Count <= 0)
			{
				BundleAsset.Unload(UnloadAllLoadedObjects);
			}
		}

		public void AddCert(ulong cid)
		{
			CertIDs.Add(cid);
			MarkTime = -1;
		}

		/// <summary>
		/// 移除某个LoadCert对此Bundle的引用
		/// </summary>
		/// <param name="cid"></param>
		public void RmCert(ulong cid)
		{
			if (CertIDs.Contains(cid))
				CertIDs.Remove(cid);
			if (CertIDs.Count <= 0)
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

		public LoadStatus Status = LoadStatus.Init;
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
				AssetMgr.Unload(name, ID);
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