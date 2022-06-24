using System;
using System.Collections.Generic;
using KDGame.Base;
using UnityEngine;

namespace KDGame.UI
{
	public class UIView : LogicEventSender
    {
	    [ReadOnly]
	    public ulong viewID = 0;
        public List<GameObject> holder = new List<GameObject>();

        public void OnCreateEnd(ulong viewID)
        {
	        this.viewID = viewID;
        }

        public void OnViewDestroy()
        {
	        
        }

        protected virtual void OnShow()
        {
	        
        }

        protected virtual void OnHide()
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
