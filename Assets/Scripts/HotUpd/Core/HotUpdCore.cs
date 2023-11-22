using KDGame.UI;
using UnityEngine;

namespace HotUpd.Core
{
	public class HotUpdCore
	{
		public static void Run()
		{
			Debug.Log("HotUpd Running! Found UIMgr:" + UIMgr.Instance.name);
		}
	}
}