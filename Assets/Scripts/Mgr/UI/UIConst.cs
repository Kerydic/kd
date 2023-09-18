namespace KDGame.UI
{
	public static class UIConst
	{
	}

	public static class Forms
	{
		public static readonly ViewForm GizmosView = new ViewForm("AssetUI/Utils/GizmosView", UILayerNames.Util);
		public static readonly ViewForm LaunchView = new ViewForm("AssetUI/Launch/LaunchView", UILayerNames.Content);
		public static readonly ViewForm MainView = new ViewForm("AssetUI/Main/MainView", UILayerNames.Content);
	}

	public static class UILayerNames
	{
		public const string Util = "Util";
		public const string Popup = "Popup";
		public const string Content = "Content";

		public static readonly string[] Layers = { Util, Popup, Content };
	}

	public struct ViewForm
	{
		public string Path;
		public string LayerName;

		public ViewForm(string path, string layerName)
		{
			Path = path;
			LayerName = layerName;
		}
	}
}