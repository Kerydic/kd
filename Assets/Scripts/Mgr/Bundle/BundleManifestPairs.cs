using System;

namespace KDGame.Mgr.Bundle
{
	public class BundleManifestPairs
	{
		public string groupName;
		public BundleManifest localManifest;
		public BundleManifest remoteManifest;

		private BundleEntry _localEntry, _remoteEntry;

		public BundleInfo Parse(string bundleName, bool forceLocal = false)
		{
			if (localManifest == null)
				throw new Exception($"Local bundle manifest {groupName} is null");

			var info = new BundleInfo();
			var isLocalExist = localManifest.entryDict.TryGetValue(bundleName, out _localEntry);
			// 不强制使用本地资源，且远程清单里存在对应条目时，尝试使用远程Entry
			if (!forceLocal && remoteManifest != null &&
			    remoteManifest.entryDict.TryGetValue(bundleName, out _remoteEntry))
			{
				_remoteEntry = remoteManifest.entryDict[bundleName];
				if (isLocalExist && _localEntry.hash.Equals(_remoteEntry.hash))
				{
					info.md5 = _localEntry.hash;
					info.isInBuild = _localEntry.IsInBuild();
				}
				else
				{
					info.md5 = _remoteEntry.hash;
					info.isInBuild = false;
				}
			}
			else if (isLocalExist)
			{
				info.md5 = _localEntry.hash;
				info.isInBuild = _localEntry.IsInBuild();
			}
			else
			{
				throw new Exception($"Local bundle manifest {groupName}'s entry {bundleName} is null");
			}

			return info;
		}
	}

	public struct BundleInfo
	{
		/// <summary>
		/// 对应Bundle的资源md5
		/// </summary>
		public string md5;

		/// <summary>
		/// 对应Bundle是否打在包内
		/// </summary>
		public bool isInBuild;
	}
}