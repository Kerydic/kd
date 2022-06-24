using KDGame.Base;
using KDGame.UI;

namespace KDGame.Module
{
	public class GizmosCtrl : LogicCtrl
	{
		protected override void OnEnter()
		{
			UIMgr.Instance.ShowView(Forms.GizmosView);
		}

		protected override void OnQuit()
		{
			
		}
	}
}