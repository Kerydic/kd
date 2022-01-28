using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KDGame.UI
{
    public class UIBase : MonoBehaviour
    {
        public List<GameObject> holder = new List<GameObject>();

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
