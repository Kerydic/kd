namespace KDGame.UI
{
	public static class UIConst
	{
		public const string LayerPath = "AssetUI/Utils/UILayer";
	}

	public static class Forms
	{
		public static readonly ViewForm GizmosView = new ViewForm("AssetUI/Utils/GizmosView", UIDepth.Util);
	}

	public enum UIDepth
	{
		Content = 100,
		Util = 999,
	}

	public struct ViewForm
	{
		public string Path;
		public UIDepth Depth;

		public ViewForm(string path, UIDepth depth)
		{
			Path = path;
			Depth = depth;
		}
	}
}