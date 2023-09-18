using KDGame.Base;
using KDGame.PB;
using KDGame.UI;
using KDGame.View;

namespace KDGame.Module
{
	[Module("Game")]
	public class GameModule : Base.Module
	{
		private GameView _gameView;

		protected override void OnEnter()
		{
		}

		public void ShowMain()
		{
			UIMgr.Instance.ShowView(Forms.MainView, view =>
			{
				_gameView = view as GameView;
				view.AddListener(OnViewEvent);
			});
		}

		private void OnViewEvent(LogicEventArgs args)
		{
			// if (args.EventID == LogicEvent.LaunchBtnClick)
			// {
			// 	if (_view != null)
			// 	{
			// 		UIMgr.Instance.HideView(_view.viewID);
			// 		_view = null;
			// 	}
			// 	// 结束Launch逻辑，退出
			// }
		}
	}
}