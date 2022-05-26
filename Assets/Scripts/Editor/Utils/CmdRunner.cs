namespace KDGame.Editor.Utils
{
	public class CmdRunner
	{
		/// <summary>
		/// 运行一条简单命令，适用于大部分情形
		/// </summary>
		/// <param name="cmd">命令路径</param>
		/// <param name="args">命令参数</param>
		/// <param name="workDir">可选，工作目录</param>
		/// <returns>返回长度为2的数组，第一位存储运行命令后的标准输出，第二位为标准错误</returns>
		public static string[] RunCmd(string cmd, string args, string workDir = null)
		{
			var output = new string[2];
			var process = CreateCmdProcess(cmd, args, workDir);
			output[0] = process.StandardOutput.ReadToEnd();
			output[1] = process.StandardError.ReadToEnd();
			process.Close();
			return output;
		}

		private static System.Diagnostics.Process CreateCmdProcess(string cmd, string args, string workDir = null)
		{
			var info = new System.Diagnostics.ProcessStartInfo(cmd, args)
			{
				CreateNoWindow = false,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				StandardErrorEncoding = System.Text.Encoding.UTF8,
				StandardOutputEncoding = System.Text.Encoding.UTF8
			};
			if (!string.IsNullOrEmpty(workDir))
			{
				info.WorkingDirectory = workDir;
			}

			return System.Diagnostics.Process.Start(info);
		}
	}
}