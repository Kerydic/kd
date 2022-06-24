using KDGame.Base;
using KDGame.PB;
using KDGame.UI;

namespace KDGame.Module
{
	public class LaunchCtrl : LogicCtrl
	{
		private LaunchView _view;
		
		protected override void OnEnter()
		{
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
				// 结束Launch逻辑，退出
			}
		}
	}
}