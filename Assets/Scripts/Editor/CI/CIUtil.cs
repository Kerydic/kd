using HybridCLR.Editor.Installer;
using UnityEditor;

namespace KDGame.Editor.CI
{
	public class CIUtil
	{
		public static void CheckBeforeBuild()
		{
			var controller = new InstallerController();
			if (!controller.HasInstalledHybridCLR())
			{
				controller.InstallDefaultHybridCLR();
				AssetDatabase.Refresh();
			}
		}

		public static void BuildWithParam()
		{
			
		}
	}
}