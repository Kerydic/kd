using System;
using System.Collections;
using System.IO;
using System.Threading;
using KDGame.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace KDGame.Mgr
{
	public enum FileLoadStatus
	{
		None,
		Loading,
		LoadFailed,
		Loaded,
		Canceled,
	}

	public class FileLoaderException : Exception
	{
		public FileLoaderException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// 文件加载器，用于按需加载文件（StreamingAssets、PersistentData）
	/// 需要注意的：Android平台下，无法通过System.IO中的接口获取文件、判断文件是否存在
	///		在没法通过File.Exist判断StreamingAssets中是否存在对应文件的情况下，这里只能假定文件在PersistentData下
	/// TODO 实现断点续传
	/// </summary>
	public class FileLoader
	{
		/// <summary>
		/// 文件原始URL
		/// </summary>
		private string _url;

		/// <summary>
		/// 文件在PersistentDataPath下的相对路径
		/// </summary>
		private string _savePath;

		/// <summary>
		/// 通过_savePath获取的本地文件URL
		/// </summary>
		private string _saveUrl;

		public Action<ulong> bytesHandler;
		public Action<float> progressHandler;

		public FileLoadStatus status { get; private set; }
		public string error { get; private set; }

		protected byte[] _bytes;
		protected string _content;

		private static KDLog _logger;

		public FileLoader(string url) : this(url, string.Empty)
		{
		}

		public FileLoader(string url, string savePath)
		{
			_logger = new KDLog("FileLoader");
			_url = url;
			if (!string.IsNullOrEmpty(savePath))
			{
				_saveUrl = Path.Combine(ComUtil.GetPersistentDataUrl(), savePath);
				_savePath = Path.Combine(Application.persistentDataPath, savePath);
			}
		}

		public IEnumerator LoadAsync(int retryCount = 0, bool saveOnDownload = false,
			CancellationToken cancellationToken = default)
		{
			if (status == FileLoadStatus.Loading)
				throw new FileLoaderException("Previous load async is running!");
			if (status == FileLoadStatus.Loaded)
				throw new FileLoaderException("File is already loaded!");

			var url = _url;
			var isNeedSave = false;
			if (!string.IsNullOrEmpty(_savePath))
			{
				if (File.Exists(_savePath))
				{
					url = _saveUrl;
				}
				else if (saveOnDownload)
				{
					isNeedSave = true;
				}
			}

			if (string.IsNullOrEmpty(url))
				throw new FileLoaderException("File url is empty!");

			status = FileLoadStatus.Loading;
			error = string.Empty;
			while (retryCount >= 0)
			{
				using (var webRequest = GetWebRequest(url))
				{
					_logger.Info($"Start loading file {url}.");
					if (bytesHandler == null || progressHandler == null)
					{
						yield return webRequest.SendWebRequest();
					}
					else
					{
						webRequest.SendWebRequest();
						while (!webRequest.isDone && !cancellationToken.IsCancellationRequested)
						{
							bytesHandler?.Invoke(webRequest.downloadedBytes);
							progressHandler?.Invoke(webRequest.downloadProgress);
							yield return new WaitForEndOfFrame();
						}

						if (!webRequest.isDone)
							webRequest.Abort();
					}

					// 判断是否取消
					var isCanceled = cancellationToken.IsCancellationRequested;
					if (isCanceled)
					{
						status = FileLoadStatus.Canceled;
						break;
					}

					if (webRequest.result == UnityWebRequest.Result.Success)
					{
						ReadData(webRequest);
						if (isNeedSave) SaveDataToLocal();
						status = FileLoadStatus.Loaded;
						break;
					}

					status = FileLoadStatus.LoadFailed;
					error = webRequest.error;
				}

				retryCount--;
			}
		}

		protected virtual UnityWebRequest GetWebRequest(string url)
		{
			return UnityWebRequest.Get(url);
		}

		// 读取请求结果，如果有特殊需求（如纹理），则重写该函数
		protected virtual void ReadData(UnityWebRequest webRequest)
		{
			_bytes = webRequest.downloadHandler.data;
			_content = webRequest.downloadHandler?.text;
		}

		// 缓存到本地，如果有特殊需求（如纹理），则重写该函数
		protected virtual void SaveDataToLocal()
		{
			if (_bytes == null || string.IsNullOrEmpty(_savePath)) return;
			if (File.Exists(_savePath)) File.Delete(_savePath);
			File.WriteAllBytes(_savePath, _bytes);
		}

		public byte[] GetBytes()
		{
			return _bytes;
		}

		public string GetContent()
		{
			return _content;
		}
	}
}