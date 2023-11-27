using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using KDGame.Lib;

namespace KDGame.Util
{
	/// <summary>
	/// 通用工具类，存放不知道怎么具体分类的函数
	/// </summary>
	public class ComUtil
	{
		/// <summary>
		/// 延迟一定时长执行
		/// </summary>
		/// <param name="delayFunc">被延迟执行的函数</param>
		/// <param name="delay">延迟时长，单位毫秒</param>
		public static void DelayCall(Action delayFunc, double delay)
		{
			if (delay <= 0)
			{
				delayFunc?.Invoke();
				return;
			}

			MonoTimer timer = new MonoTimer(delay);
			timer.Elapsed += delayFunc;
			timer.Start();
		}

		public static string Str2Lower(string input)
		{
			return input.ToLower(new CultureInfo("en-US", false));
		}

		public static string GetMD5(string fPath, bool shorten = true)
		{
			var md5Hash = string.Empty;
			try
			{
				FileStream fs = new FileStream(fPath, FileMode.Open);
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(fs);
				fs.Close();
				md5Hash = Str2Lower(BitConverter.ToString(retVal).Replace("-", ""));
				return shorten ? md5Hash.Substring(8, 16) : md5Hash;
			}
			catch (Exception e)
			{
				throw new Exception("Get MD5 fail, error:" + e.Message);
			}
		}
	}
}