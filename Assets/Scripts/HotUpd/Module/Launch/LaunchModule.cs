using KDGame.Base;
using KDGame.PB;
using KDGame.UI;

namespace KDGame.Module
{
	[Module("Launch")]
	public class LaunchModule : Base.Module
	{
		private LaunchView _view;

		protected override void OnEnter()
		{
			base.OnEnter();
			UIMgr.Instance.ShowView(Forms.LaunchView, view =>
			{
				_view = view as LaunchView;
				view.AddListener(OnViewEvent);
			});
		}

		private void OnViewEvent(LogicEventArgs args)
		{
			if (args.EventID == LogicEvent.LaunchBtnClick)
			{
				if (_view != null)
				{
					UIMgr.Instance.HideView(_view.viewID);
					_view = null;
				}

				Quit();
			}
		}

		protected override void OnQuit()
		{
			base.OnQuit();
			GetModule<GameModule>().ShowMain();
		}
	}
}