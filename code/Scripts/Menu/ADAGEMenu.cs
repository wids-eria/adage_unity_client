#if !UNITY_5

using UnityEngine;
using System.Collections;
using System.Threading;
using ZXing;
using ZXing.QrCode;
using System.Collections.Generic;
using System;

public abstract class ADAGEMenuPopup
{
	public abstract void Draw();
}

public delegate void ADAGEMenuClickEvent(string response);

public class ADAGEMenuYesNoPopup : ADAGEMenuPopup
{
	private Rect drawRect;
	private Rect labelRect;
	private Rect yesRect;
	private Rect noRect;
		
	private string label;

	private ADAGEMenuClickEvent onYes;
	private ADAGEMenuClickEvent onNo;

	public ADAGEMenuYesNoPopup(string label, ADAGEMenuClickEvent onYes, ADAGEMenuClickEvent onNo)
	{
		drawRect = new Rect(256f, 192f, 512f, 384f);
		labelRect = new Rect(0f, 0f, 512f, 300f);
		
		yesRect = new Rect(10,304,241,60);
		noRect = new Rect(261,304,241,60);

		this.label = label;
		this.onYes = onYes;
		this.onNo = onNo;
	}
	
	public override void Draw()
	{
		GUI.BeginGroup(drawRect, "", "box");
		{
			GUI.Label(labelRect, label, "PopupPrompt");
			
			if(GUI.Button(yesRect, "Yes", "PopupButton"))
			{
				onYes("yes");
			}
			
			if(GUI.Button(noRect, "No", "PopupButton"))
			{
				onNo("no");
			}
		}
		GUI.EndGroup();
	}
}

public class ADAGEConnectionFailurePopup : ADAGEMenuPopup
{
	private Rect drawRect;
	private Rect labelRect;
	private Rect yesRect;
	private Rect noRect;

	private int numAttempts;

	public ADAGEConnectionFailurePopup(int attempts)
	{
		drawRect = new Rect(256f, 192f, 512f, 384f);
		labelRect = new Rect(0f, 0f, 512f, 300f);

		yesRect = new Rect(10,304,241,60);
		noRect = new Rect(261,304,241,60);

		numAttempts = attempts;
	}

	public override void Draw()
	{
		GUI.BeginGroup(drawRect, "", "box");
		{
			GUI.Label(labelRect, string.Format("The system has made {0} failed attempts to connect to ADAGE. Continue in Offline Mode?", numAttempts));

			if(GUI.Button(yesRect, "Yes"))
			{
				Messenger.Broadcast(ADAGE.k_OnGameStart);
			}

			if(GUI.Button(noRect, "No"))
			{
				ADAGEMenu.ShowPanel<ADAGELoginPanel>();
			}
		}
		GUI.EndGroup();
	}
}

[Serializable]
public class ADAGEMenuButton
{
	public Rect drawRect;
	public string name;
	public bool visible;
}

public abstract class ADAGEMenuPanel
{
	protected const float buttonSpacing = 20f;

	public abstract void Draw(MonoBehaviour owner = null);
	//public abstract IEnumerator Update();
	public abstract IEnumerator Update();
	public abstract void OnEnable(MonoBehaviour owner = null);
	public abstract void OnDisable(MonoBehaviour owner = null);
	
	protected bool visible = false;

	public bool isLocked = false;

	public void Lock()
	{
		isLocked = true;
	}

	public void Unlock()
	{
		isLocked = false;
	}

	public void Enable(bool locked = false, MonoBehaviour owner = null)
	{
		isLocked = locked;
		visible = true;
		OnEnable(owner);
	}
	
	public void Disable(MonoBehaviour owner = null)
	{
		visible = false;
		OnDisable(owner);
	}

	public virtual void FixedUpdate()
	{

	}

	public virtual void OnApplicationQuit()
	{

	}

	protected void ClearFocus()
	{
		GUIUtility.keyboardControl = -1;	
	}
}

public class ADAGEMenu : MonoBehaviour 
{ 	
	public static readonly Vector2 defaultResolution = new Vector2(1024,768);
	
	private const int errorDuration = 4;
	
	public static ADAGEMenu instance;

	public static void Deactivate()
	{
		instance.gameObject.SetActive(false);
	}

	public static void SetCurrentUser(ADAGEUser user)
	{
		if ( IsNull ) {
			return;
		}
		GetPanel<ADAGELoginPanel>().SetUsername(user.email);
		//instance.loginPanel.SetUsername(user.email);
	//	instance.currentUser = user;
	}

	public static bool IsNull
	{
		get
		{
			if( instance == null || instance.gameObject == null ) {
				return true;
			}
			return !instance.gameObject.activeInHierarchy;
		}
	}

	public static bool AllowPasswordLogin
	{
		get
		{
			if(!IsNull)
				return instance.enablePasswordLogin;
			return false;
		}
	}
	
	public static bool AllowQRLogin
	{
		get
		{
			if(!IsNull)
				return instance.enableQRLogin;
			return false;
		}
	}
	
	public static bool AllowRegistration
	{
		get
		{
			if(!IsNull)
				return instance.enableRegistration;
			return false;
		}
	}

	private static void InitPanel<T>() where T : ADAGEMenuPanel
	{
		if(instance.panels == null)
			instance.panels = new Dictionary<Type, ADAGEMenuPanel>();
		
		if(!instance.panels.ContainsKey(typeof(T)))
			instance.panels.Add(typeof(T), Activator.CreateInstance<T>());
	}

	public static T GetPanel<T>() where T : ADAGEMenuPanel
	{
		if(IsNull)
			return null;
		
		InitPanel<T>();

		return (T)instance.panels[typeof(T)];
	}

	public static void ShowPanel<T>(bool locked = false) where T : ADAGEMenuPanel
	{
		if(IsNull)
			return;

		InitPanel<T>();

		if(instance.currentPanel != instance.panels[typeof(T)])
		{
			TurnOffCurrentPanel();
			instance.currentPanel = instance.panels[typeof(T)];
			instance.currentPanel.Enable(locked);
		}

		instance.popup = null;
	}

	public static void ShowLast()
	{
		if(IsNull)
			return;

		//There is no previous panel, so there's nowhere to go
		if(instance.previousPanel == null)
		{
			instance.previousPanel = instance.currentPanel;
			return;
		}

		//If the previous panel is the current panel, there's nowhere to go 
		if(instance.currentPanel != instance.previousPanel)
		{
			if(instance.currentPanel != null)
				instance.currentPanel.Disable();

			ADAGEMenuPanel temp = instance.previousPanel;
			instance.previousPanel = instance.currentPanel;
			instance.currentPanel = temp;
			instance.currentPanel.Enable(temp.isLocked);
		}

		instance.popup = null;
	}

	private static void TurnOffCurrentPanel()
	{
		if(IsNull)
			return;

		if(instance.currentPanel != null)
		{
			instance.currentPanel.Disable();
			instance.previousPanel = instance.currentPanel;
		}
	}

	public static void ShowError(int id, string error)
	{
		ShowLast();
		instance.error = error;		
	}

	public void MShowError(int id, string error)
	{
		ShowLast();
		instance.error = error;		
	}
		
	public static void ShowPopup(ADAGEMenuPopup popup)
	{
		instance.popup = popup;
	}

	//setting force to true allows you to load the level as long as there is a level to load, regardless of the level you are currently on
	/*public static void LoadLevel(bool force = false)
	{
		if(instance.OnLoginLoadLevel)
		{
			if(instance.nextLevel != -1 && (force || (Application.loadedLevel != instance.nextLevel && Application.loadedLevel == 0)))
			{
				Application.LoadLevel(instance.nextLevel);
			}
			else
			{
				Debug.LogWarning("ADAGEMenu object is set to load a level upon successful login, but no level was provided in the inspector.");
			}
		}
		
		instance.gameObject.SetActive(false);
	}*/
	
	public Texture2D backgroundImage;
	public Texture2D overlay;
	public bool stretchBackgroundAspect;
	
	public GUISkin skin;
		
	private Texture2D logo;
	private GUIStyle invisibleButtonStyle;
	
	private Rect screenRect;
	private GUIStyle screenStyle;
	
	private Rect backgroundRect;
	private Rect overlayRect;
	private Rect menuRect;
	
	private Rect logoRect;
	private Rect errorRect;

	private bool loading;
	private string error;
	private GUIStyle errorStyle;
	private float errorTime;

	private Dictionary<Type, ADAGEMenuPanel> panels;

	public bool enablePasswordLogin = true;
	public bool enableQRLogin = false;
	public bool enableRegistration = true;

	private ADAGEMenuPanel previousPanel;
	private ADAGEMenuPanel currentPanel;

	private ADAGEMenuPopup popup;

	public void Awake()
	{
		if(instance != null) 
		{
			Debug.LogWarning("You have multiple copies of the ADAGEMenu object running. Overriding...");
			DestroyImmediate(instance.gameObject);
		}
		DontDestroyOnLoad(this);
		instance = this;

		Messenger.AddListener(ADAGE.k_OnGameStart, OnGameStart);
		Messenger<int, string>.AddListener(ADAGE.k_OnError, ShowError);
		Messenger<string>.AddListener(ADAGE.k_OnConnectionTimeout, OnConnectionTimeout);
		Messenger<int, string>.AddListener(ADAGE.k_OnLoginComplete, OnLogin);

		//Messenger.AddListener(ADAGE.k_OnLoggingIn, ShowPanel<ADAGESplashPanel>);

		popup = null;
		menuRect = new Rect(112,84,800,600);
		logoRect = new Rect(312,104,400,150);
		errorRect = new Rect(212,614,600,50);
		screenRect = new Rect(0,0,defaultResolution.x,defaultResolution.y);
		backgroundRect = overlayRect = screenRect;
		
		logo = Resources.Load("Images/GLS_Logo_Dark") as Texture2D;
		
		error = "";
		errorTime = 0f;
		
		loading = true;
	}

	void OnGUI()
	{		
		Vector3 scale = new Vector3(Screen.width/(defaultResolution.x * 1.0f), Screen.height/(defaultResolution.y * 1.0f), 1f);
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

		if(loading)
		{
			InitStyles();
			loading = false;
		}		
			
		GUI.skin = skin;

		if(currentPanel != null)
			screenRect = GUI.Window (0, screenRect, Draw, "");
	}
		
	void Draw(int windowID)
	{	
		if(backgroundImage != null)
			GUI.DrawTexture(backgroundRect, backgroundImage);
		
		if(overlay != null)
			GUI.DrawTexture(overlayRect, overlay);

		GUI.Box(menuRect, "");

		GUI.DrawTexture(logoRect, logo);

		if(GUI.Button(logoRect, "", invisibleButtonStyle))
			ADAGEMenu.ShowPanel<ADAGEHomePanel>();

		GUI.enabled = (popup == null);
			currentPanel.Draw();
		GUI.enabled = true;

		GUI.Label(errorRect, instance.error, errorStyle);

		if(popup != null)
		{
			if(overlay != null)
				GUI.DrawTexture(overlayRect, overlay);

			popup.Draw();
		}
	}
	
	void Update()
	{		
		if(error.Trim() != "" && error != null)
		{
			errorTime += Time.deltaTime;
			
			if(errorTime >= errorDuration)
			{
				error = "";	
				errorTime = 0f;
			}
		}

		if(currentPanel != null)
			currentPanel.Update();	
	}
		
	void OnApplicationQuit()
	{
		if(currentPanel != null)
			currentPanel.OnApplicationQuit();
	}

	public void OnLevelWasLoaded(int level) 
	{
		if(Application.loadedLevelName != "ADAGELogin")
			gameObject.SetActive(false);
		else
			gameObject.SetActive(true);
	}

	private void OnGameStart()
	{
		//gameObject.SetActive(false);
	}

	private void OnLogin(int id, string token)
	{
		ShowPanel<ADAGESplashPanel>();
		//ShowHome();
	}

	private void OnConnectionTimeout(string connectionType)
	{
		popup = new ADAGEConnectionFailurePopup(ADAGEWebJob.maxAttempts);
	}

	private void InitStyles()
	{		
		invisibleButtonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		invisibleButtonStyle.normal.background = null;
		invisibleButtonStyle.hover.background = null;
		invisibleButtonStyle.focused.background = null;
		invisibleButtonStyle.active.background = null;

		errorStyle = new GUIStyle(GUI.skin.GetStyle("label"));	
		errorStyle.fontSize = 22;
		errorStyle.fontStyle = FontStyle.BoldAndItalic;
		errorStyle.alignment = TextAnchor.MiddleLeft;
		errorStyle.normal.textColor = new Color(0.8f,0.2f,0.2f,1f);		
	}
	
	private void CheckResolution()
	{
		int screenHeight = Screen.height;
		int screenWidth = Screen.width;
		
		if(stretchBackgroundAspect)
			backgroundRect = new Rect(0,0,screenWidth,screenHeight);
		else if(backgroundImage != null)
		{
			float width = backgroundImage.width;
			float height = backgroundImage.height;
			
			float screenAspect = screenWidth / (screenHeight * 1.0f);
			float imageAspect = width / (height * 1.0f);
					
			if(screenAspect == imageAspect)
			{
				backgroundRect = new Rect(0,0,screenWidth,screenHeight);	
			}
			else
			{
				float multiplier;
				float diff;
				float modDimension;
				float modPosition;
								
				float widthRatio = width / (screenWidth * 1.0f);
				float heightRatio = height / (screenHeight * 1.0f);
				
				if(widthRatio < heightRatio)
				{
					//the heights are closer together
					multiplier = screenHeight / (height * 1.0f);		
					modDimension = width * multiplier;	
					diff = screenWidth - modDimension;
					modPosition = diff / 2.0f;	
					backgroundRect = new Rect(modPosition,0,modDimension,screenHeight);
				}
				else
				{
					multiplier = screenWidth / (width * 1.0f);	
					modDimension = height * multiplier;
					diff = screenHeight - modDimension;
					modPosition = diff / 2.0f;
					backgroundRect = new Rect(0,modPosition,screenWidth,modDimension);								
				}
			}
		}
			
		overlayRect = new Rect(0,0,screenWidth,screenHeight);
	}
}

#endif