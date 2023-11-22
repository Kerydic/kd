using KDGame.Base;
using KDGame.UI;

namespace KDGame.Module
{
	[Module("Gizmos")]
	public class GizmosModule : Base.Module
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