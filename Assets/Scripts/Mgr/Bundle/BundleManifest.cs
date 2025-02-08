using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace KDGame.Mgr.Bundle
{
	[Flags]
	public enum BundleOption
	{
		None = 0, // 0000
		Remote = 1, // 0001 是否是远程资源
		Sealed = 2, // 0010 是否不可热更新（仅包内类型有效）
		Needed = 4, // 0100 是否为必须资源（必须资源启动时下载）
	}

	[Serializable]
	public class BundleEntry : IComparable<BundleEntry>
	{
		/// <summary>
		/// AssetBundle名
		/// </summary>
		public string name;
		/// <summary>
		/// 哈希值
		/// </summary>
		public string hash;
		/// <summary>
		/// 大小
		/// </summary>
		public long size;
		/// <summary>
		/// 选项设置，若该设置不同于批次的设置，优先使用自身配置
		/// </summary>
		public BundleOption option = BundleOption.None;

		/// <summary>
		/// 根路径，对于文件夹类型的AssetBundle，用该值来判断某个资源是否处于当前Bundle内
		/// </summary>
		public string rootPath;

		/// <summary>
		/// 特殊文件路径，用于一些只打特殊文件的AssetBundle（如代码）
		/// </summary>
		public string[] uniquePaths;

		public bool IsInBuild()
		{
			return (option & BundleOption.Remote) == 0;
		}

		public int CompareTo(BundleEntry other)
		{
			return string.Compare(name, other.name, StringComparison.Ordinal);
		}
	}

	// TODO 实现基于ScriptableObject的BundleManifest
	// 本框架使用的清单文件格式
	public class BundleManifest
	{
		public BundleManifest(string content)
		{
			entryList = new List<BundleEntry>();
			entryDict = new Dictionary<string, BundleEntry>();
			if (!string.IsNullOrEmpty(content)) Deserialize(content);
		}

		// 该清单Md5值
		public string md5 { get; private set; }

		// 清单Bundle数据
		public BundleEntry manifestEntry;

		// 除清单外所有Bundle数据的列表
		public List<BundleEntry> entryList { get; private set; }

		// 除清单外所有Bundle的字典
		[NonSerialized] public Dictionary<string, BundleEntry> entryDict;

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
		}

		public string Serialize()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"{manifestEntry.name}|{manifestEntry.hash}|{manifestEntry.size}");
			entryList.Sort();
			foreach (var entry in entryList) builder.AppendLine($"{entry.name}|{entry.hash}|{entry.size}");
			return builder.ToString();
		}

		public void Deserialize(string content)
		{
			var reader = new StringReader(content);
			var lineIndex = 0;
			string lineTxt;
			string[] lineVal;
			long size;
			while ((lineTxt = reader.ReadLine()) != null)
			{
				lineIndex++;
				lineVal = lineTxt.Trim().Split("!");
				if (lineVal.Length < 3) continue;
				long.TryParse(lineVal[2], out size);
				if (lineIndex == 1)
					SetManifestEntry(lineVal[0], lineVal[1], size);
				else
					SetOrAddEntry(lineVal[0], lineVal[1], size);
			}

			OnAfterDeserialize();
		}

		public void SetManifestEntry(string name, string hash, long size)
		{
			if (manifestEntry == null) manifestEntry = new BundleEntry();
			manifestEntry.name = name;
			manifestEntry.hash = hash;
			manifestEntry.size = size;
		}

		public void SetOrAddEntry(string name, string hash, long size)
		{
			BundleEntry entry;
			if (!entryDict.TryGetValue(name, out entry))
			{
				entry = new BundleEntry();
				entryList.Add(entry);
				entryDict.Add(name, entry);
			}
			entry.name = name;
			entry.hash = hash;
			entry.size = size;
		}
	}
}