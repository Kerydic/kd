using System;
using System.Collections.Generic;
using KDGame.Base;
using UnityEngine;

namespace KDGame.UI
{
	public class UIView : LogicEventSender
	{
		[ReadOnly] public ulong viewID = 0;
		public List<GameObject> holder = new List<GameObject>();

		public void OnCreateEnd(ulong viewID)
		{
			this.viewID = viewID;
			foreach (var trans in gameObject.GetComponentsInChildren<Transform>(true))
			{
				trans.gameObject.layer = LayerMask.NameToLayer("UI");
			}
		}

		public void OnViewDestroy()
		{
		}

		public virtual void OnShow()
		{
		}

		public virtual void OnHide()
		{
		}

		private void Start()
		{
			Debug.Log("UI showed: " + gameObject.name);
			foreach (var o in holder)
			{
				Debug.Log(o.name);
			}
		}
	}
}