using System.Collections;
using System.Collections.Generic;
using KDGame.UI;
using UnityEditor;
using UnityEngine;

namespace KDGame.Editor.Inspector.UI
{
	[CustomEditor(typeof(UIView))]
	public class UIBaseInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}