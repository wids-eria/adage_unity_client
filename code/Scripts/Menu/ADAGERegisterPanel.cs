using UnityEngine;
using System.Collections;

public class ADAGERegisterPanel : ADAGEMenuPanel
{
	private const string kDefaultUsernameText = "Please Enter Username";
	private const string kDefaultPasswordText = "Please Enter Password";
	private const string kDefaultConfirmText = "Please Confirm Password";
	private const string kDefaultEmailText = "Please Enter Email (Optional)";
	
	private string username;
	private string password;
	private string confirm;
	private string email;
	
	private int keyboardFieldID;
	
	private bool loading;
	private int usernameFieldID;
	private int passwordFieldID;
	private int confirmFieldID;
	private int emailFieldID;
	
	private GUIStyle usernameStyle;
	private GUIStyle passwordStyle;
	private GUIStyle confirmStyle;
	private GUIStyle emailStyle;
	
	private GUIStyle buttonStyle;
	
	private Rect usernameFieldRect;
	private Rect passwordFieldRect;
	private Rect confirmFieldRect;
	private Rect emailFieldRect;
	private Rect submitButtonRect;
	private Rect backButtonRect;
	
	public ADAGERegisterPanel()
	{
		username = "";
		password = "";
		confirm = "";
		email = "";
		
		usernameFieldRect = new Rect(212,284,600,50);
		passwordFieldRect = new Rect(212,344,600,50);
		confirmFieldRect = new Rect(212,404,600,50);
		emailFieldRect = new Rect(212,464,600,50);
		submitButtonRect = new Rect(212,554,290,60);
		backButtonRect = new Rect(522,554,290,60);
		
		loading = true;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
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
		
		confirmFieldID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
		if(keyboardFieldID == confirmFieldID || confirm != kDefaultConfirmText)
		{
			confirm = GUI.PasswordField(confirmFieldRect, confirm, '*', confirmStyle);
		}
		else
		{
			confirm = GUI.TextField(confirmFieldRect, confirm, confirmStyle);	
		}
		
		emailFieldID = GUIUtility.GetControlID(FocusType.Keyboard) + 1;
		email = GUI.TextField(emailFieldRect, email, emailStyle);
		
		if(GUI.Button(submitButtonRect, "Submit", buttonStyle))
		{
			ClearFocus();
			ADAGEMenu.ShowPanel<ADAGESplashPanel>();
			if(IsValid())
			{
				ADAGE.RegisterPlayer(username,email,password,confirm);
			}
		}
		
		if(GUI.Button(backButtonRect, "Back", buttonStyle))
		{
			ClearFocus();
			ADAGEMenu.ShowLast();
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
	
	private bool IsValid()
	{
		if(username.Trim() == "" || username == kDefaultUsernameText)
		{
			ADAGEMenu.ShowError(-1, "Username cannot be blank");
			return false;
		}
		
		if(password.Trim() == "" || password == kDefaultPasswordText)
		{
			ADAGEMenu.ShowError(-1, "Password cannot be blank");
			return false;
		}
		
		if(email.Trim() == "" || email == kDefaultEmailText)
		{
			ADAGEMenu.ShowError(-1, "Email cannot be blank");
			return false;
		}
		
		if(password != confirm || confirm == kDefaultConfirmText)
		{
			ADAGEMenu.ShowError(-1, "Passwords do not match");
			return false;
		}
		
		return true;
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
		
		confirmStyle = new GUIStyle(GUI.skin.GetStyle("textfield"));	
		confirmStyle.fontSize = 22;
		confirmStyle.fontStyle = FontStyle.BoldAndItalic;
		confirmStyle.alignment = TextAnchor.MiddleCenter;
		
		emailStyle = new GUIStyle(GUI.skin.GetStyle("textfield"));	
		emailStyle.fontSize = 22;
		emailStyle.fontStyle = FontStyle.BoldAndItalic;
		emailStyle.alignment = TextAnchor.MiddleCenter;
		
		buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		buttonStyle.fontSize = 22;
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
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
		
		if(confirm.Trim() == "" && keyboardFieldID != confirmFieldID)
		{
			confirm = kDefaultConfirmText;	
			confirmStyle.fontStyle = FontStyle.BoldAndItalic;
		}
		else if(confirm == kDefaultConfirmText && keyboardFieldID == confirmFieldID)
		{
			confirm = "";
			confirmStyle.fontStyle = FontStyle.Bold;			
		}
		
		if(email.Trim() == "" && keyboardFieldID != emailFieldID)
		{
			email = kDefaultEmailText;	
			emailStyle.fontStyle = FontStyle.BoldAndItalic;
		}
		else if(email == kDefaultEmailText && keyboardFieldID == emailFieldID)
		{
			email = "";
			emailStyle.fontStyle = FontStyle.Bold;			
		}
	}
}
