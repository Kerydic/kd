using KDGame.PB;
using KDGame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KDGame.Module
{
	public class LaunchView : UIView
	{
		[SerializeField] private Button _btnStart;

		protected override void OnShow()
		{
			if (_btnStart != null)
			{
				_btnStart.onClick.AddListener(OnStartClick);
			}
		}

		private void OnStartClick()
		{
			Invoke(LogicEvent.LaunchBtnClick);
		}
	}
}