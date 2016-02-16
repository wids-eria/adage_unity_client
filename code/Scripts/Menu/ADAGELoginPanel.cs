using UnityEngine;
using System.Collections;

public class ADAGELoginPanel : ADAGEMenuPanel
{
	private const string kDefaultUsernameText = "Please Enter Username";
	private const string kDefaultPasswordText = "Please Enter Password";
	
	private string username;
	private string password;
	
	private int keyboardFieldID;
	
	private bool loading;
	private int usernameFieldID;
	private int passwordFieldID;
	private GUIStyle usernameStyle;
	private GUIStyle passwordStyle;
	private GUIStyle buttonStyle;
	private GUIStyle imageButtonStyle;
	
	private Texture2D facebookButton;
	
	private Rect usernameFieldRect;
	private Rect passwordFieldRect;
	private Rect backButtonRect;
	private Rect loginButtonRect;
	private Rect registerButtonRect;
	private Rect facebookButtonRect;
	private Rect qrButtonRect;
	
	private int loginAttempts = 0;
	private readonly int maxLoginAttempts = 3;
	
	public ADAGELoginPanel()
	{
		username = "";
		password = "";	
		
		usernameFieldRect = new Rect(212,284,600,50);
		passwordFieldRect = new Rect(212,344,600,50);
				
		float buttonWidth = 300 - (buttonSpacing / 2.0f);

		if(ADAGE.AllowFacebook)
		{
			loginButtonRect = new Rect(212,474,buttonWidth,60);
			facebookButtonRect = new Rect(212 + buttonWidth + buttonSpacing,474,buttonWidth,60);
		}
		else
		{
			loginButtonRect = new Rect(212,474,600,60);
		}


		backButtonRect = new Rect(418,554,188,60);
				
		facebookButton = Resources.Load("Images/facebook-button") as Texture2D;
		
		Messenger<int, string>.AddListener(ADAGE.k_OnLoginComplete, OnLoginComplete);
		
		loading = true;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
			CheckFields();
			loading = false;
		}
		
		usernameFieldID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
		username = GUI.TextField(usernameFieldRect, username, usernameStyle);
		
		passwordFieldID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
		if(keyboardFieldID == passwordFieldID || password != kDefaultPasswordText)
		{
			password = GUI.PasswordField(passwordFieldRect, password, '*', passwordStyle);
		}
		else
		{
			password = GUI.TextField(passwordFieldRect, password, passwordStyle);	
		}
		
		GUI.enabled = (password != kDefaultPasswordText && username != kDefaultUsernameText && password.Trim().Length != 0 && username.Trim().Length != 0);
		if(GUI.Button(loginButtonRect, "Login", buttonStyle))
		{
			ClearFocus();
			loginAttempts++;
			ADAGEMenu.ShowPanel<ADAGESplashPanel>();
			ADAGE.LoginPlayer(username.Trim(), password.Trim());
		}
		GUI.enabled = true;

		if(!isLocked)
		{
			if(GUI.Button(backButtonRect, "Back", buttonStyle))
			{
				ClearFocus();
				ADAGEMenu.ShowLast();
			}
		}

		if(ADAGE.AllowFacebook)
		{
			if(GUI.Button(facebookButtonRect, "", imageButtonStyle))
			{
				ClearFocus();				
			}
		}
				
		CheckFields();
	}
	
	public override IEnumerator Update()
	{
		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		
	}
	
	public override void OnDisable(MonoBehaviour owner = null)
	{
		
	}
	
	private void InitStyles()
	{
		usernameStyle = new GUIStyle(GUI.skin.GetStyle("textfield"));	
		usernameStyle.fontSize = 22;
		usernameStyle.fontStyle = FontStyle.BoldAndItalic;
		usernameStyle.alignment = TextAnchor.MiddleCenter;
		
		passwordStyle = new GUIStyle(GUI.skin.GetStyle("textfield"));	
		passwordStyle.fontSize = 22;
		passwordStyle.fontStyle = FontStyle.BoldAndItalic;
		passwordStyle.alignment = TextAnchor.MiddleCenter;
		
		buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		buttonStyle.fontSize = 22;
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
		
		imageButtonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		imageButtonStyle.alignment = TextAnchor.MiddleCenter;
		imageButtonStyle.normal.background = facebookButton;
		imageButtonStyle.hover.background = facebookButton;
		imageButtonStyle.focused.background = facebookButton;
		imageButtonStyle.active.background = facebookButton;
		imageButtonStyle.imagePosition = ImagePosition.ImageOnly;
		imageButtonStyle.contentOffset = Vector2.zero;
		imageButtonStyle.padding = new RectOffset(0,0,0,0);
	}
	
	private void CheckFields()
	{		
		keyboardFieldID = GUIUtility.keyboardControl;
		
		if(username.Trim() == "" && keyboardFieldID != usernameFieldID)
		{
			username = kDefaultUsernameText;	
			usernameStyle.fontStyle = FontStyle.BoldAndItalic;
		}
		else if(username == kDefaultUsernameText && keyboardFieldID == usernameFieldID)
		{
			username = "";
			usernameStyle.fontStyle = FontStyle.Bold;	
		}
		
		if(password.Trim() == "" && keyboardFieldID != passwordFieldID)
		{
			password = kDefaultPasswordText;	
			passwordStyle.fontStyle = FontStyle.BoldAndItalic;
		}
		else if(password == kDefaultPasswordText && keyboardFieldID == passwordFieldID)
		{
			password = "";
			passwordStyle.fontStyle = FontStyle.Bold;			
		}
	}
	
	private void OnLoginComplete(int id, string token)
	{
		username = "";
		password = "";
	}
	
	private void OnADAGELoginFailed()
	{
		if(loginAttempts >= maxLoginAttempts)
		{
			ADAGEMenu.ShowPopup(new ADAGEConnectionFailurePopup(loginAttempts));
		}
	}
	
	public void SetUsername(string username)
	{
		this.username = username;
	}
}
