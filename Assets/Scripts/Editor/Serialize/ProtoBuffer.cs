using System;
using System.IO;
using KDGame.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace KDGame.Editor.Serialize
{
	public class ProtoBuffer
	{
		private const string SrcPath = "Proto";
		private const string DstPath = "Assets/Scripts/PB";
		private const string PkgPath = "Packages";
		private const string ProtoCFName = "Google.Protobuf.Tools";

		[MenuItem(EditorMenuConst.Serialize + "Generate PB", false, 1)]
		public static void GenCsPb()
		{
			var exePath = GetExePath();
			if (string.IsNullOrEmpty(exePath))
			{
				return;
			}

			foreach (var file in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, SrcPath)))
			{
				if (Path.GetExtension(file) != ".proto" || !IsNeedGen(Path.GetFileNameWithoutExtension(file)))
				{
					continue;
				}

				var args = GenArgs(Path.GetFileName(file));
				Debug.Log("Gen Cmd: " + exePath + " " + args);
				var res = CmdRunner.RunCmd(exePath, args);

				if (!string.IsNullOrEmpty(res[0]))
				{
					Debug.Log("$\t" + res[0]);
				}

				if (!string.IsNullOrEmpty(res[1]))
				{
					Debug.LogError("$!\t" + res[1]);
				}
			}

			Debug.Log("Generate End.");
		}

		/// <returns>当前可用的可执行文件相对路径</returns>
		private static string GetExePath()
		{
			var root = Path.Combine(Environment.CurrentDirectory, PkgPath);
			var packagePath = "";
			foreach (var dic in Directory.GetDirectories(root))
			{
				var dictName = Path.GetFileName(dic);
				if (dictName != null && dictName.StartsWith(ProtoCFName))
				{
					packagePath = dic;
				}
			}
			Debug.Log(Path.GetDirectoryName(packagePath)+ " "+  Path.GetFileName(packagePath));

			if (string.IsNullOrEmpty(packagePath))
			{
				Debug.LogError("No NuGet Google.Protobuf.Tools found! Please import first!");
				return null;
			}

			var platID = Environment.OSVersion.Platform;
			string platName, exeName;
			if (platID == PlatformID.MacOSX || platID == PlatformID.Unix)
			{
				platName = "macosx_x64";
				exeName = "protoc";
			}
			else
			{
				platName = Environment.Is64BitOperatingSystem ? "windows_x64" : "windows_x86";
				exeName = "protoc.exe";
			}

			return Path.Combine(PkgPath, Path.GetFileName(packagePath), "tools", platName, exeName);
		}

		/// <summary>
		/// 拼接生成CS PB文件的参数
		/// </summary>
		/// <param name="pbName">PB名，含后缀</param>
		/// <returns>拼好的参数</returns>
		private static string GenArgs(string pbName)
		{
			return string.Format("-I={0} --csharp_out={1} {0}/{2}", SrcPath, DstPath, pbName);
		}

		/// <summary>
		/// 根据名称判断一个PB是否需要生成CS文件
		/// </summary>
		/// <param name="pbName">PB名，不含后缀</param>
		/// <returns>该PB是否需要生成CS文件</returns>
		private static bool IsNeedGen(string pbName)
		{
			return true;
		}
	}
}