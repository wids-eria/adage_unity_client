using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ADAGEHomePanel : ADAGEMenuPanel
{	
	private int keyboardFieldID;
	
	private bool loading;
	private GUIStyle buttonStyle;

	private Rect loginButtonRect;
	private Rect registerButtonRect;
	
	public ADAGEHomePanel()
	{
		loginButtonRect = new Rect(212,304,290,250);
		registerButtonRect = new Rect(522,304,290,250);
		
		loading = true;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
			loading = false;
		}
		
		GUI.enabled = (ADAGE.users != null && ADAGE.users.Count < ADAGE.k_MaxUsers);
		if(GUI.Button(loginButtonRect, "Login", buttonStyle))
		{
			OnLogin();
		}
		GUI.enabled = true;
		
		if(GUI.Button(registerButtonRect, "Register", buttonStyle))
		{
			OnRegistration();
		}

		GUI.enabled = true;
	}
	
	public override IEnumerator Update()
	{
		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		if(!ADAGEMenu.AllowRegistration)
			OnLogin(true);	
	}
	
	public override void OnDisable(MonoBehaviour owner = null)
	{
		
	}
	
	private void OnLogin(bool locked = false)
	{
		ClearFocus();
		ADAGEMenu.ShowPanel<ADAGELoginOptionsPanel>(locked);
	}
	
	private void OnRegistration(bool locked = false)
	{
		ClearFocus();
		ADAGEMenu.ShowPanel<ADAGERegisterPanel>(locked);
	}
	
	private void InitStyles()
	{		
		buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		buttonStyle.fontSize = 22;
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
	}
}
