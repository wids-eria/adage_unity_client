using UnityEngine;
using System.Collections;

public class ADAGELoginOptionsPanel : ADAGEMenuPanel
{
	private int keyboardFieldID;
	
	private bool loading;
	private GUIStyle buttonStyle;

	private Rect backButtonRect;

	private Rect buttonAreaRect;
	private float buttonWidth;

	public ADAGELoginOptionsPanel()
	{
		buttonAreaRect = new Rect(212,304,600,250);

		backButtonRect = new Rect(419,574,186,75);

		loading = true;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
			loading = false;
		}

		GUILayout.BeginArea(buttonAreaRect);
		{
			GUILayout.BeginHorizontal();
			{
				if(ADAGEMenu.AllowPasswordLogin)
				{
					if(GUILayout.Button("Password", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.ExpandHeight(true)))
					{
						OnPassword();
					}

					if(ADAGE.AllowGuestLogin || ADAGEMenu.AllowQRLogin)
						GUILayout.Space(buttonSpacing);
				}
					
				if(ADAGEMenu.AllowQRLogin)
				{
					if(GUILayout.Button("QR", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.ExpandHeight(true)))
					{
						OnQR();
					}	
					if(ADAGE.AllowGuestLogin)
						GUILayout.Space(buttonSpacing);		
				}
			
				if(ADAGE.AllowGuestLogin)
				{
					if(GUILayout.Button("Guest", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.ExpandHeight(true)))
					{
						OnGuest();
					}				
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();

		if(!isLocked)
		{
			if(GUI.Button(backButtonRect, "Back", buttonStyle))
			{
				ADAGEMenu.ShowPanel<ADAGEHomePanel>();
			}
		}
		
		GUI.enabled = true;
	}
	
	public override IEnumerator Update()
	{
		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		int buttons = 0;
		if(ADAGEMenu.AllowPasswordLogin)
			buttons++;
		if(ADAGEMenu.AllowQRLogin)
			buttons++;
		if(ADAGE.AllowGuestLogin)
			buttons++;

		if(buttons > 1)
		{
			buttonWidth = (buttonAreaRect.width - (buttonSpacing * (buttons - 1))) / buttons;
		}
		else if(buttons == 1)
		{
			if(ADAGEMenu.AllowPasswordLogin)
				OnPassword(true);
			else if(ADAGEMenu.AllowQRLogin)
				OnQR(true);
			else if(ADAGE.AllowGuestLogin)
				OnGuest();
		}
		else //for some reason...
			buttonWidth = buttonAreaRect.width;
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

	private void OnPassword(bool locked = false)
	{
		ADAGEMenu.ShowPanel<ADAGELoginPanel>(locked);
	}

	private void OnQR(bool locked = false)
	{
		ADAGEMenu.ShowPanel<ADAGEQRPanel>(locked);
	}

	private void OnGuest()
	{
		ADAGE.ConnectAsGuest();
	}
}
