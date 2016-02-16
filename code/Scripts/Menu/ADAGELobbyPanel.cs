using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ADAGELobbyPanel : ADAGEMenuPanel
{	
	private int keyboardFieldID;
	
	private bool loading;
	private GUIStyle buttonStyle;
	
	private Rect currentUsersRect;
	private Rect loginButtonRect;
	private Rect registerButtonRect;
	private Rect startButtonRect;
	
	private string startButtonText;
	
	public ADAGELobbyPanel()
	{
		loginButtonRect = new Rect(522,284,290,125);
		registerButtonRect = new Rect(522,419,290,125);
		startButtonRect = new Rect(212,614,600,50);
		
		currentUsersRect = new Rect(212,284,290,260);
		
		loading = true;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
			loading = false;
		}
		
		GUILayout.BeginArea(currentUsersRect, "", "box");
		{
			GUILayout.BeginVertical();
			{
				int badUser = -1;
				foreach(KeyValuePair<int, ADAGEUser> user in ADAGE.users)
				{
					if(GUILayout.Button(user.Value.playerName, buttonStyle, GUILayout.Height(60)))
					{
						badUser = user.Key;
					}
				}
				
				if(badUser > -1)
				{
					ADAGE.users.Remove(badUser);
					badUser = -1;
				}
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndArea();
		
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
		
		GUI.enabled = (ADAGE.AllowGuestLogin || (ADAGE.users != null && ADAGE.users.Count > 0)); 
		if(GUI.Button(startButtonRect, startButtonText, buttonStyle))
		{
			Messenger.Broadcast(ADAGE.k_OnGameStart);
		}
		GUI.enabled = true;
	}
	
	public override IEnumerator Update()
	{
		if(ADAGE.AllowGuestLogin && ADAGE.users != null && ADAGE.users.Count == 0)
			startButtonText = "Start Game as Guest";
		else
			startButtonText = "Start Game";

		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		if(!ADAGEMenu.AllowRegistration)
			OnLogin();
	}
	
	public override void OnDisable(MonoBehaviour owner = null)
	{
		
	}
	
	private void InitStyles()
	{		
		buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		buttonStyle.fontSize = 22;
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
	}

	private void OnLogin()
	{
		ClearFocus();
		ADAGEMenu.ShowPanel<ADAGELoginPanel>();
	}

	private void OnRegistration()
	{
		ClearFocus();
		ADAGEMenu.ShowPanel<ADAGERegisterPanel>();
	}
}
