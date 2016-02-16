#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using LitJson; 

[CustomEditor(typeof(ADAGEMenu), true)]
public class ADAGEMenuEditor : Editor
{
	private bool loaded = false;
	
	static ADAGEMenuEditor()
	{
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}
	
	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		ADAGEMenu curTarget = (target as ADAGEMenu);
		
		//GUI.enabled = ADAGEEditor.enableGuestLogin;
		//ADAGE.AllowGuestLogin = EditorGUILayout.Toggle("Enable Guest Login", ADAGE.AllowGuestLogin);

		//curTarget.qrPanel.isLocked = EditorGUILayout.Toggle("Lock QR Panel", curTarget.qrPanel.isLocked);

		if(GUI.changed)
		{
			EditorUtility.SetDirty(curTarget);
		}
	}
		
	private void IndentGUI(int amount)
	{
		GUILayout.Space (33 * amount);
	}
}
#endif