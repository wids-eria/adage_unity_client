using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJson;
using System.IO;
using System.Net;

public delegate void ADAGEKeyboardEventDelegate(int[] ASCII);
public delegate void ADAGEMouseEventDelegate(Vector3 Position, string Button);

public delegate ADAGEUser ADAGECreateUserDelegate();
public delegate void ADAGELoginDelegate(int id, string token);

public class ADAGE : MonoBehaviour
{
    public static event Action<ADAGEUser> OnUserLoggedIn;

    public static event Action OnRegistrationSuccess;
    public static event Action<string[]> OnRegistrationFailed;
    public static event Action<string[]> OnLoginFailed;

	public class BaseClass : System.Attribute{}

#if (UNITY_EDITOR)
	public static bool ForceProduction
	{
		get
		{
			return instance.forceProduction;
		}
	}

	public static bool ForceDevelopment
	{
		get
		{
			return instance.forceDevelopment;
		}
	}

	public static bool ForceStaging
	{
		get
		{
			return instance.forceStaging;
		}
	}

	[HideInInspector]
	public bool forceProduction = false;
	[HideInInspector]
	public bool forceDevelopment = false;
	[HideInInspector]
	public bool forceStaging = false;
#endif

	public const string k_OnLoggingIn = "ADAGE.OnLoggingIn(void)";
	public const string k_OnCachedUserLoaded = "ADAGE.OnCachedUserLoaded(ADAGEUser)";
	public const string k_OnConnectionTimeout = "ADAGE.OnConnectionTimeout(string)";
	public const string k_OnError = "ADAGE.OnError(int, string)";
	public const string k_OnGameStart = "ADAGE.OnGameStart(void)";
	public const string k_OnJobTimeout = "ADAGE.OnJobTimeout(string)";
	public const string k_OnLoginComplete = "ADAGE.OnLoginComplete(int, string)";
	public const string k_OnQuit = "ADAGE.OnQuit(void)";
	public const string k_OnUserReceived = "ADAGE.OnUserReceived(string)";

	private const int k_LocalWrapperMax = 25;

	#region Static Fields and Properties
	public static string VERSION = "electric_eel";

	public const int k_MaxUsers = 4;

	public static string productionURL
	{
		get
		{
			if(!IsNull)
				return instance.productionServer;
			return "";
		}
		set
		{
			if(!IsNull)
				instance.productionServer = value;
		}
	}
	
	public static string developmentURL
	{
		get
		{
			if(!IsNull)
				return instance.developmentServer;
			return "";
		}
		set
		{
			if(!IsNull)
				instance.developmentServer = value;
		}
	}
	
	public static string stagingURL
	{
		get
		{
			if(!IsNull)
				return instance.stagingServer;
			return "";
		}
		set
		{
			if(!IsNull)
				instance.stagingServer = value;
		}
	}
		
	//public static List<ADAGEUser> users = new List<ADAGEUser>();
	public static Dictionary<int, ADAGEUser> users = new Dictionary<int, ADAGEUser>();

	protected static ADAGE instance; 

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

	public static string GameID
	{
		get
		{
			if(!IsNull)
				return instance.gameID;
			return "";
		}
		
		set
		{
			if(!IsNull)
				instance.gameID = value;
		}
	}
	
	public static bool AllowGuestLogin
	{
		get
		{
			if(!IsNull)
				return instance.allowGuestLogin;
			return false;
		}

		set
		{
			if(!IsNull)
				instance.allowGuestLogin = value;
		}
	}

	public static bool AutoGuestLogin
	{
		get
		{
			if(!IsNull)
				return instance.autoGuestLogin;
			return false;
		}
	}

	public static bool GameHandlesLogin
	{
		get
		{
			if(!IsNull)
				return instance.gameHandlesLogin;
			return false;
		}
	}

	public static bool LogLocal
	{
		get
		{
			if(!IsNull)
				return instance.logLocal;
			return false;
		}
	}

	public static bool AllowFacebook
	{
		get
		{
			if(!IsNull)
				return instance.allowFacebook;
			return false;
		}
	}

	public static bool Staging
    {
		get
		{
			if(!IsNull)
				return instance.staging;
			return false;
		}
    }	
	
	public static bool Online
    {
		get
		{
			if(!IsNull)
				return instance.isOnline;
			return false;
		}
    }

	//For turning off logging while the game is running, this is needed for things like Makescape where the game has an attract mode.
	public static bool DisableLogging
	{
		get
		{
			if(!IsNull)
				return instance.isLoggingDisabled;
			return true;
		}

		set
		{
			if(!IsNull)
				instance.isLoggingDisabled = value;

		}
	}
	
	public static bool UserIsValid(int id)
    {
		if(!IsNull) 
        {
            if(users.Count == 0 || id > users.Count - 1) { return false; }
            
		    return users[id].valid();
        }
        
		return false;

    }	

	public static string ApplicationName
	{
		get
		{
			if(!IsNull)
				return instance.applicationName;
			return "";
		}
	}

	public static string CurrentSession
	{
		get
		{
			if(IsNull)
				return "";
			return instance.currentSession;
		}
	}

	public static DateTime BaseTime
	{
		get
		{
			if(IsNull)
				return DateTime.UtcNow;
			return instance.baseTime;
		}

		set
		{
			if(IsNull)
				return;

			instance.baseTime = value;
			instance.timeDuration = 0f;
		}
	}

	public static float TimeDuration
	{
		get
		{
			if(IsNull)
				return 0f;

			return instance.timeDuration;
		}
	}

	public static string AppToken
	{
		get
		{
			if(!IsNull)
				return instance.appToken;
			return "";
		}
	}


	public static ADAGEKeyboardEventDelegate OnKeyboardEvent
	{
		get
		{
			if(!IsNull)
				return instance.onKeyboardEvent;
			return null;
		}

		set
		{
			if(!IsNull)
				instance.onKeyboardEvent = value;
		}
	}
	
	public static ADAGEMouseEventDelegate OnMouseEvent
	{
		get
		{
			if(!IsNull)
				return instance.onMouseEvent;
			return null;
		}
		
		set
		{
			if(!IsNull)
				instance.onMouseEvent = value;
		}
	}
	#endregion Static Fields and Properties

	#region Instance Fields and Properties
	private ADAGEKeyboardEventDelegate onKeyboardEvent;
	private ADAGEMouseEventDelegate onMouseEvent;
	
	[HideInInspector]
	public string productionServer = "";
	[HideInInspector]
	public string developmentServer = "";
	[HideInInspector]
	public string stagingServer = "";
	
	[HideInInspector]
	public bool useDefaultMenu = true;
	[HideInInspector]
	public bool allowGuestLogin = true;
	[HideInInspector]
	public bool autoGuestLogin = true;

	public bool cacheLastUser = false;
	[HideInInspector]
	public bool autoLoginLastUser = false;
	[HideInInspector]
	public bool allowFacebook = true;
	[HideInInspector]
	public bool automaticLogin = true;
	[HideInInspector]
	public bool automaticStartGame = true;
	[HideInInspector]
	public bool gameHandlesLogin = false;
	
	[HideInInspector]
	public bool staging = false;
	[HideInInspector]
	private bool logLocal = false;
	[HideInInspector]
	public bool enableLogLocal = true;
	public int pushRate = 5;
	public string applicationName = "";
	[HideInInspector]
	public string appToken;
	[HideInInspector]
	public string appSecret;
	[HideInInspector]
	public string devToken = "foo";
	[HideInInspector]
	public string devSecret = "bar";

	[HideInInspector]
	public bool forceLogin = false;
	[HideInInspector]
	public string forcePlayer;
	[HideInInspector]
	public string forcePassword;

	[HideInInspector]
	public bool enableKeyboardCapture = true;
	[HideInInspector]
	public bool autoCaptureKeyboard = true;
	[HideInInspector]
	public bool enableMouseCapture = true;
	[HideInInspector]
	public bool autoCaptureMouse = true;

	[HideInInspector]
	public string dataPath = "";

	[HideInInspector]
	public DateTime baseTime;
	[HideInInspector]
	public float timeDuration = 0f;

	[HideInInspector]
	public ADAGEGameInformation gameInformation;

	[SerializeField, HideInInspector]
	public ADAGEGameVersionInfo versionInfo;

	[HideInInspector]
	public bool enableCacheVersion = true;

	[HideInInspector]
	public bool enableValidation = true;

#if(UNITY_WEBPLAYER)
	[HideInInspector]
	public int socketPort = 50000;
#endif

	private bool   isOnline = false;
	private bool   isLoggingDisabled = false;
	private string currentSession;
	private string gameID = "";
	private string statusMessage = "Offline";
	private float  lastPush = 0;  
	
	[SerializeField]
	[HideInInspector]
	private int nextLevel;

	private WorkerPool 			   threads;	
	
	private Dictionary<string, ADAGECamera> cameras;

	[HideInInspector]
	public List<string> dataTypes;
	[HideInInspector]
	public List<bool> isDataTypeActive;

	private Dictionary<string, ADAGEData> dataObjectCache;
	#endregion Instance Fields and Properties
		
	#region Static Methods
	public static T GetCachedMessage<T>() where T:ADAGEData
	{
		return (T)GetCachedMessage(typeof(T));
	}
	
	public static ADAGEData GetCachedMessage(Type dataType)
	{
		if(IsNull) return null;
		
		if(!ReflectionUtils.CompareType(dataType, typeof(ADAGEData))) return null;

		if(instance.dataObjectCache == null)
			instance.dataObjectCache = new Dictionary<string, ADAGEData>();
		
		if(!instance.dataObjectCache.ContainsKey(dataType.ToString()))
			instance.addInstanceToCache((ADAGEData)Activator.CreateInstance(dataType));
		
		return instance.dataObjectCache[dataType.ToString()];
	}

	public static void LogData<T>(int localId = 0) where T:ADAGEData
	{
		LogData<T>(GetCachedMessage<T>(), localId);
		//LogData<T>((T)GetCachedMessage(typeof(T)), localId);
	}

	public static void LogData<T>(T data, int localId = 0) where T:ADAGEData
	{
		if(IsNull)
			return;

		if(DisableLogging)
			return;

		if(ReflectionUtils.CompareType(data.GetType(), (typeof(ADAGEContext))))
		{
			string message = string.Format("ADAGE WARNING: Method 'ADAGE.LogData' should not be used to track progression object '{0}'. Please use the ADAGE.LogContext method", data.GetType().ToString());
			Debug.LogWarning(message);	
		}
		else if(ReflectionUtils.CompareType(data.GetType(), (typeof(ADAGEScreenshot))))
		{
			ADAGEScreenshot shotData = data as ADAGEScreenshot;
			if(instance.cameras != null && instance.cameras.ContainsKey(shotData.cameraName))
			{
				ADAGECamera cam = instance.cameras[shotData.cameraName];
				if(cam.GetComponent<Camera>() != null)
				{
					shotData.shot = instance.TakeScreenshot(cam.GetComponent<Camera>());
				}
				else
				{
					string message = string.Format("ADAGE WARNING: Cannot log screenshot from source '{0}' because no Unity3D camera is present.", shotData.cameraName);
					Debug.LogWarning(message);	
				}
			}
			else
			{
				string message = string.Format("ADAGE WARNING: Cannot log screenshot from source '{0}' because the camera has not been registered with ADAGE. Did you add the ADAGECamera object to the camera?", shotData.cameraName);
				Debug.LogWarning(message);	
			}
		}

		instance.AddData<T>(data, localId);
	}
	
	public static void GetData<T>(T trackerInfo, int localId = 0) where T:ADAGETracker
	{
		if(IsNull)
			return;

		instance.AddTrackerJob(trackerInfo, localId);
	}

	public static void GetRequest<T>(T requestInfo, int localId = 0) where T:ADAGEGetRequest
	{
		if(IsNull)
			return;
		
		instance.AddGetDataJob(requestInfo, localId);
	}

    public static void GetRequest(string targetURL, ADAGEDataReceived callback = null, int localID = 0)
    {
        if (IsNull)
            return;

        ADAGEGetRequest request = new ADAGEGetRequest(targetURL, callback);

        instance.AddGetDataJob(request, localID);
    }

	public static void LogContext<T>(int localId = 0) where T:ADAGEContext
	{
		LogContext<T>((T)GetCachedMessage<T>(), localId);
	}

	public static void LogContext<T>(T data, int localId=0) where T:ADAGEContext
	{
		if(IsNull)
			return;

		if(DisableLogging)
			return;

		instance.AddContext<T>(data, localId);	
	}
	
	public static void UpdatePositionalContext(Transform transform, int localId=0)
	{
		if(IsNull)
			return;

		UpdatePositionalContext(transform.position, transform.eulerAngles, localId);
	}

	public static void UpdatePositionalContext(Vector3 pos, Vector3 rot = new Vector3(), int localId=0)
	{
		if(IsNull)
			return;

		users[localId].pContext.setPosition(pos.x, pos.y, pos.z);
		users[localId].pContext.setRotation(rot.x, rot.y, rot.z);
	}

	public static bool IsTypeActive<T>() where T:ADAGEData
	{
		return IsTypeActive(typeof(T).ToString());
	}

	public static bool IsTypeActive(string type)
	{
		if(IsNull)
			return false;

		if(instance.dataTypes == null || instance.isDataTypeActive == null)
		{
			instance.dataTypes = new List<string>();
			instance.isDataTypeActive = new List<bool>();
			return false;
		}
		else
		{
			int index = instance.dataTypes.IndexOf(type);
			if(index > -1 && index < instance.isDataTypeActive.Count)
				return instance.isDataTypeActive[instance.dataTypes.IndexOf(type)];
			return false;
		}
	}

//All player login and registration functions will return the local player id that should be used to reference this player for data logging. 
	public static int LoginPlayer(string playerName, string password)
	{
		if(IsNull)
			return -1;

		int user_id = AddUser();
		if(user_id == -1)
			return -1;

		Messenger.Broadcast(k_OnLoggingIn);

		//Returns even if this errors out...TODO
		instance.AddLoginJob(playerName, password, user_id);
		return user_id;
	}
	 
	public static int AddUser(ADAGEUser newUser = null)
	{
		if(IsNull)
			return -1;

		if(users == null)
			users = new Dictionary<int, ADAGEUser>();

		if(users.Count >= k_MaxUsers)
			return -1;

		if(newUser == null)
		{
			newUser = instance.createNewUser(newUser);
		}

		users.Add(users.Count, newUser);

		instance.StartSession(users.Count - 1);

		return users.Count - 1;
	}

	public static int RegisterPlayer(string playerName, string email, string password, string passwordConfirm, bool loginWhenComplete = true)
	{
		if(IsNull)
			return -1;

		int user_id = AddUser();
		if(user_id == -1)
			return -1;

		instance.AddRegistrationJob(playerName, email, password, passwordConfirm, user_id);
		return user_id;
	}

	public static void StartGame()
	{
		StartGame(string.Format("{0}:{1}", SystemInfo.deviceUniqueIdentifier, DateTime.Now.ToString("yyyy-MM-dd_HHmmss")));
	}

	public static void StartGame(string game, int localId)
	{
		if(IsNull)
			return;

		ADAGE.GameID = game;
		instance.StartGame(localId);
	}

	public static void StartGame(string game)
	{
		if(IsNull)
			return;

		if(users == null)
		{
			users = new Dictionary<int, ADAGEUser>();
			return;
		}

		if(users.Count == 0)
			return;

		ADAGE.GameID = game;

		foreach(KeyValuePair<int, ADAGEUser> user in users)
		{
			instance.StartGame(user.Key);
		}
	}

	//This will create an anonymous yet unique guest login
	public static int ConnectAsGuest()
	{
		if(IsNull)
			return -1;

		int user_id = AddUser();
		if(user_id == -1)
			return -1;

		Messenger.Broadcast(ADAGE.k_OnLoggingIn);

		instance.AddGuestConnectionJob(user_id);
		return user_id;
	}

	public static int ConnectWithQR(string group)
	{
		if(IsNull)
			return -1;
		
		ADAGEUser newUser = new ADAGEUser();
		newUser.guest = true;
		
		int user_id = AddUser(newUser);
		if(user_id == -1)
			return -1;

		instance.AddQRJob(group, user_id);
		return user_id;
	}

	public static string GetStatusMessage()
	{
		if(IsNull)
			return "";

		return instance.statusMessage;	
	}

	public static bool FindPreviousPlayer()
	{
		if(IsNull)
			return false;

		Debug.Log("*************************Finding Previous Player");
		return instance.LoadUserInfo();
	}
	
	public static void Signout(int localId)
	{
		if(IsNull)
			return;

		//forget everything
		instance.LogoutUser(localId);
	}

	public static void ConnectToFacebook()
	{
		if(IsNull)
			return;

		instance.BeginFacebookAuth();	
	}	

	public static void SaveGame(string savegame, int localId=0)
	{
		if(IsNull)
			return;

		instance.AddSaveUtilityJob("/save_game", savegame, localId);
	}

	public static void SaveConfig(string config, int localId=0)
	{
		if(IsNull)
			return;
		
		instance.AddSaveUtilityJob("/save_config", config, localId);
	}

	public static void LoadGame(ADAGEUtilityResponseCallback callback, int localId=0)
	{
		if(IsNull)
			return;
		
		instance.AddLoadUtilityJob("/load_game.json", callback, localId);
	}

	public static void LoadConfig(ADAGEUtilityResponseCallback callback, int localId=0)
	{
		if(IsNull)
			return;
		
		instance.AddLoadUtilityJob("/load_config.json", callback, localId);
	}
	
	public static void AddCamera(ADAGECamera camera)
	{
		if(IsNull)
			return;
		
		if(instance.cameras == null)
			instance.cameras = new Dictionary<string, ADAGECamera>();
		
		instance.cameras[camera.cameraName] = camera;
	}
	
	public static long convertDateTimeToEpoch(DateTime time)
	{
		DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		return (long) time.Subtract(epoch).TotalMilliseconds;
	}

	public static void ChangeSession(string newSession)
	{
		if(newSession.Trim() != "")
		{
			instance.currentSession = newSession.Trim();
		}
	}

	#endregion Static Methods

	#region Facebook Classes
	//Structure returned by the FB login call
	public class FBLoginResponse
	{
		public bool is_logged_in;
		public string user_id;
		public string access_token;
	}
	
	//Structure returned by the call to get /me from the FB graph API
	public class FBProfileInfo
	{
		public string id;
		public int timezone;
		public string username;
		public string link;
		public string locale;
		public string last_name;
		public string email;
		public bool verified;
		public string gender;
		public string name;
		public string first_name;
		public DateTime updated_time;
	}
	
	//Since currently I can't get the real full auth response from the FB SDK
	//construct a fake one!
	public class FakebookInfo
	{
		public string username;
		public string email;
	}

	public class FakebookCredentials
	{
		public string token;
		public string expires_at;
	}

	public class FakebookAuthResponse
	{
		public FakebookInfo raw_info;
		public FakebookInfo info;
		public string uid;
		public string provider = "facebook";
		public FakebookCredentials credentials;
		
		public FakebookAuthResponse()
		{
			raw_info = new FakebookInfo();
			info = new FakebookInfo();
			credentials = new FakebookCredentials();
		}
	}

	public class FakebookCookie
	{
		public FakebookAuthResponse omniauth;
	}
	#endregion Facebook Classes
	
	#region Monobehaviour Events
	public virtual void Awake()
	{
		if(instance != null) 
		{
			Debug.LogWarning("You have multiple copies of the ADAGE object running. Overriding...");
		}
		DontDestroyOnLoad(this);
		instance = this;

		ADAGEJsonConversion.Init();

		threads = new WorkerPool(8);
		cameras = new Dictionary<string, ADAGECamera>();

		if(autoCaptureKeyboard)
			onKeyboardEvent = LogKeyboardEvent;

		if(autoCaptureMouse)
			onMouseEvent = LogMouseEvent;

		if(dataPath == "")
		{
			dataPath = "/ADAGE/Data/";	
		}
		else
		{
			if(dataPath.Substring(dataPath.Length-1) != "/")
			{
				dataPath += "/";
			}
		}

		if(productionServer != "")
		{
			productionURL = productionServer;
		}

		if(developmentServer != "")
		{
			developmentURL = developmentServer;
		}

		if(stagingServer != "")
		{
			stagingURL = stagingServer;
		}

		//If this is a production build we will attempt to load and use the default url and user settings from local
		/*if(!Debug.isDebugBuild && !Application.isEditor)
		{
			LoadOverrideSettings();
		}*/

#if (UNITY_WEBPLAYER)
		string url = "";
		if(Application.isEditor || Debug.isDebugBuild)
		{
			if(ADAGE.Staging)
			{
				url = stagingURL;
			}
			else
			{
				url = developmentURL;
			}
		}
		else
		{
			url = productionURL;
		}

#if (UNITY_EDITOR)
		if(ADAGE.ForceProduction)
			url = productionURL;	
		
		if(ADAGE.ForceDevelopment)
			url = developmentURL;	
		
		if(ADAGE.ForceStaging)
			url = stagingURL;	
#endif

		Debug.Log (url);

		Uri target = new Uri(url);
		Debug.Log (Dns.GetHostAddresses(target.Host)[0].ToString());
		Debug.Log (Security.PrefetchSocketPolicy(Dns.GetHostAddresses(target.Host)[0].ToString(), socketPort, 10000));
#endif

		SaveOverrideSettings();

		SyncToServer();

		
#if (UNITY_EDITOR)
		if(!staging && !forceProduction && !forceStaging)
		{
			appToken = devToken;
			appSecret = devSecret;
		}
#else
		if(Debug.isDebugBuild && !staging)
		{
			appToken = devToken;
			appSecret = devSecret;
		}
#endif

		appToken.Trim();
		appSecret.Trim();

		Messenger.AddListener(k_OnQuit, OnQuit);
		Messenger.AddListener(k_OnGameStart, OnStartGame);

		if(automaticLogin && automaticStartGame)
			Messenger.AddListener(k_OnGameStart, OnAutoStartGame);
	}
	
	public virtual void Start()
	{			
		//Start the session
		string deviceInfo;
		
		#if (UNITY_EDITOR)
			deviceInfo = "Editor Mode - No Device Info Found";
		#else
			deviceInfo = SystemInfo.deviceUniqueIdentifier;
		#endif

		this.currentSession = string.Format("{0}:{1}", deviceInfo, DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
				
		//Temp
		isOnline = false;

		if(forceLogin)
		{
			LoginPlayer(forcePlayer, forcePassword);
		}
		else if(!gameHandlesLogin)
		{
			#if(!UNITY_WEBPLAYER)
			if(cacheLastUser)
			{
				if(File.Exists(Application.persistentDataPath + "/session.info"))
				{
					string sessionInfo = File.ReadAllText(Application.persistentDataPath + "/session.info");	
					ADAGEUser user = JsonMapper.ToObject<ADAGEUser>(sessionInfo);
					
					if(user.valid())
					{
						if(autoLoginLastUser)
						{
							if(Application.internetReachability != NetworkReachability.NotReachable)
							{
								Messenger.Broadcast(ADAGE.k_OnLoggingIn);

								int user_id = AddUser(user);
								if(user_id == -1)
									return;
								AddUserRequestJob(user_id);
								return;
							}
						}
						else if(useDefaultMenu)
						{
							//Call ADAGEMenu static methods
                            #if !UNITY_5
							ADAGEMenu.ShowPanel<ADAGEHomePanel>();
							ADAGEMenu.SetCurrentUser(user);
                            #endif
							return;
						}
						else
						{
							Messenger<ADAGEUser>.Broadcast(k_OnCachedUserLoaded, user);
						}
					}
					else
					{
						Debug.Log ("invalid");
						ConnectAsGuest (); //1/7/15 connect as guest to prevent total failure
					}
				}
			}
			#endif

			if(autoGuestLogin)
			{
                #if !UNITY_5
				ConnectAsGuest();
                #endif
            }
			else
			{
				#if !UNITY_5
				ADAGEMenu.ShowPanel<ADAGEHomePanel>();
                #endif
			}
		}
	}

	//Temp
	private void StartGame(int localId)
	{
		UpdatePositionalContext(Vector3.zero,Vector3.zero, localId);
		ADAGEStartGame log_start = new ADAGEStartGame();
		
		LogData<ADAGEStartGame>(log_start, localId);
	}

	private void StartSession(int localId)
	{
		//this.currentSession = string.Format("{0}:{1}", SystemInfo.deviceUniqueIdentifier, DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
		users[localId].StartSession(baseTime.AddSeconds(timeDuration));
		LogData<ADAGEStartSession>(GetCachedMessage<ADAGEStartSession>(), localId);
	}

	private void SyncToServer()
	{
		baseTime = DateTime.UtcNow;
		timeDuration = 0f;
	}

	public virtual void Update()
	{
		/*if(logLocal)
		{
			isOnline = false;
		}*/

		timeDuration += Time.deltaTime;

		foreach(ADAGEUser user in users.Values)
			user.duration += Time.deltaTime;

		CheckThreads();	

		//Keyboard Processing
		if(enableKeyboardCapture && Input.inputString != "" && onKeyboardEvent != null)
		{
			char[] inputChars = Input.inputString.ToCharArray();
			List<int> inputCodes = new List<int>();
			foreach(char input in inputChars)
			{
				inputCodes.Add((int)input);
			}
			onKeyboardEvent(inputCodes.ToArray());
		}

		if(enableMouseCapture && onMouseEvent != null)
		{
			if(Input.GetMouseButtonDown(0))
				onMouseEvent(Input.mousePosition, "left");
			else if(Input.GetMouseButtonDown(1))
				onMouseEvent(Input.mousePosition, "right");
			else if(Input.GetMouseButtonDown(2))
				onMouseEvent(Input.mousePosition, "middle");
		}
	}

	public virtual void FixedUpdate()
	{
		float elapsedTime = Time.time - lastPush;

		if(elapsedTime > pushRate)
		{
			lastPush = Time.time;
			AddLogJob();
		}
	}
		
	public void OnLevelWasLoaded(int level) 
	{
		foreach(ADAGEUser u in users.Values)
		{
			u.vContext.level = Application.loadedLevelName;
		}
	}
	
	public virtual void OnApplicationQuit() 
	{
		Messenger.Broadcast(k_OnQuit);
	}
	#endregion Monobehaviour Events

	#region Instance Methods
	private void OnQuit()
	{    
		ADAGEQuitGame log_quit = new ADAGEQuitGame();
		LogData<ADAGEQuitGame>(log_quit);

		AddLogJob();

		//Stops workers from handling threads
		threads.FireWorkers();
		List<Job> remainingJobs = threads.GetRemainingJobs();
		Debug.Log ("Remaining Jobs: " + remainingJobs.Count);
		foreach(Job job in remainingJobs)
		{
			job.Main();	
			OnADAGEWebJobComplete(job);
		}
		
		WriteDataLocal();

		Messenger.RemoveListener(k_OnQuit, OnQuit);
	}

	private void CheckThreads()
	{
		if(threads.jobsComplete())
		{
			Job output = Queue.Synchronized(threads.completedJobs).Dequeue() as Job;
			OnADAGEWebJobComplete(output);
		}
	}

	private void LogKeyboardEvent(int[] ASCII)
	{
		ADAGEKeyboardEvent keyPressEvent = new ADAGEKeyboardEvent(ASCII);
		ADAGE.LogData<ADAGEKeyboardEvent>(keyPressEvent);
	}

	private void LogMouseEvent(Vector3 Position, string Button)
	{
		ADAGEMouseEvent mouseEvent = new ADAGEMouseEvent(Position, Button);
		ADAGE.LogData<ADAGEMouseEvent>(mouseEvent);
	}

	private void WriteDataLocal()
	{
#if(!UNITY_WEBPLAYER)
		if(enableLogLocal)
		{
			foreach (int id in ADAGE.users.Keys)
			{
				WriteDataLocal(id);
			}
		}
#endif
	}

	private void WriteDataLocal(int userID)
	{
#if(!UNITY_WEBPLAYER)
		if(ADAGE.users == null)
			ADAGE.users = new Dictionary<int, ADAGEUser>();

		if(!ADAGE.users.ContainsKey(userID) || !enableLogLocal)
			return;

		if(ADAGE.users[userID].localWrapper != null && ADAGE.users[userID].localWrapper.Count > 0)
		{
			//Do local write
			string outgoingData = JsonMapper.ToJson(ADAGE.users[userID].localWrapper);
			string stripedData = outgoingData.Substring(9, outgoingData.Length - 11) + ",";
			string path = Application.persistentDataPath + dataPath + ADAGE.users[userID].adageAccessToken + "/";
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			StreamWriter writer = new StreamWriter(path +  currentSession + ".data");
			writer.Write(stripedData);
			writer.Close();

			//System.IO.File.WriteAllText(path +  currentSession + ".data", stripedData);
			Debug.Log ("local data file write " + path  +  currentSession + ".data");
			ADAGE.users[userID].ClearLocalData();
		}
#endif
	}

	private void addInstanceToCache(ADAGEData dataObj)
	{
		if(dataObjectCache == null)
			dataObjectCache = new Dictionary<string, ADAGEData>();

		if(!dataObjectCache.ContainsKey(dataObj.GetType().ToString()))
			dataObjectCache.Add(dataObj.GetType().ToString(), dataObj);
	}

	private void AddData<T>(T data, int id) where T:ADAGEData
	{
		if(!IsTypeActive<T>()) 
		{
			Debug.LogWarning(string.Format("ADAGEWarning : ADAGE attempted to log an event of type '{0}' but that event has been disabled.", typeof(T).ToString()));
			return;
		}

		if(data.GetType().IsSubclassOf(typeof(ADAGEEventData)))
			(data as ADAGEEventData).Update(id);

		data.application_name = ADAGE.ApplicationName;
		data.application_version = appToken;
		if(data.timestamp == null || data.timestamp == "") //if we don't have a timestamp, let's make one. Networked players might be keeping track of their own times
			data.timestamp = convertDateTimeToEpoch(users[id].GetLocalTime()).ToString();
		//data.timestamp = convertDateTimeToEpoch(System.DateTime.Now.ToUniversalTime()).ToString();
		data.session_token = currentSession;
		data.game_id = gameID;
		data.key = data.GetType().ToString();
		
		data.ada_base_types = new List<string>();
		Type curType = data.GetType().BaseType;
		while(curType != typeof(System.Object))
		{
			data.ada_base_types.Add(curType.ToString());
			curType = curType.BaseType;
		}		
		
        if(users.Count == 0 || id > users.Count - 1) { return; }

		if(enableValidation && versionInfo != null)
		{
			if(versionInfo.Validate(data))
				users[id].dataWrapper.Add(data);
		}
		else
		{
			users[id].dataWrapper.Add(data);
		}
	}
	
	//This looks to see if a player was previously logged in and will try to reconnect them if 
	private void SearchForPreviousPlayer()
	{
		//if this is a web build then look for credentials passed in from the website
		if(Application.isWebPlayer)
		{
			Application.ExternalCall("GetAccessToken");
		}
		else
		{
			LoadUserInfo();
		}
	}

	private void OnStartGame()
	{
		if(ADAGE.users == null || ADAGE.users.Count == 0)
		{
			if(allowGuestLogin)
			{
				Debug.Log ("On Start, Connecting as Guest");
				ConnectAsGuest();
			}
		}
	}

	private void OnAutoStartGame()
	{
		if(nextLevel != -1 && (Application.loadedLevel != nextLevel && Application.loadedLevel == 0))
		{
			Application.LoadLevel(nextLevel);
		}
		else
		{
			Debug.LogWarning("ADAGE object is set to load a level upon successful login, but no level was provided in the inspector.");
		}
	}
	
	//FIX ME - Currently we are only saving the user info for the first player. Should extend this to save and restore multiple users
	private void SaveUserInfo()
	{
#if(!UNITY_WEBPLAYER)
		string userData = JsonMapper.ToJson(users[0]);
		string path = Application.persistentDataPath + "/session.info";
		System.IO.File.WriteAllText(path, userData);
		Debug.Log ("user file write complete:" + path);
#endif
	}
	
	private bool LoadUserInfo()
	{
#if(!UNITY_WEBPLAYER)
		if(File.Exists(Application.persistentDataPath + "/session.info"))
		{
			Debug.Log ("loading previous user info");
			string sessionInfo = File.ReadAllText(Application.persistentDataPath + "/session.info");	
			ADAGEUser user = JsonMapper.ToObject<ADAGEUser>(sessionInfo);
			//request user info to make sure we actually can connect
			users[0] = user;
			if(Application.internetReachability != NetworkReachability.NotReachable)
			{
				AddUserRequestJob(0);

				/*StartGame();
				
				//Had to do this to allow Zeness to log in still.  -greg
				if (ADAGEMenu.instance != null)
				{
					ADAGEMenu.LoadLevel();
				}*/
			}
			return true;
		}
#endif
		return false;		
	}

    public static void AddJob(Job job)
    {
        instance.threads.AddJob(job);
    }


	//If the override settings file does not exist we will write out the file.
	//After that if this is a production build the override file settings will be used instead of the baked in settings.
	//This will provide a way to override the server URL and the force user without redeploying the build.
	private void SaveOverrideSettings()
	{

#if(!UNITY_WEBPLAYER)
		ADAGEDefaultOverride defaults = new ADAGEDefaultOverride();
		defaults.url = productionURL;
		defaults.playername = forcePlayer;
		defaults.password = forcePassword;
		string defaultString = JsonMapper.ToJson(defaults);
		string path = Application.persistentDataPath + "/override.info";
//		Debug.Log (path);
		if(!System.IO.File.Exists(path))
		{
			Debug.Log("writing override settings file.");
			System.IO.File.WriteAllText(path, defaultString);	
		}
#endif
	}

	private void LoadOverrideSettings()
	{
#if !UNITY_WEBPLAYER
		string path = Application.persistentDataPath + "/override.info";
		if(System.IO.File.Exists(path) && !Application.isEditor && !Debug.isDebugBuild)
		{
			string defaultString = File.ReadAllText(path);
			ADAGEDefaultOverride defaults = JsonMapper.ToObject<ADAGEDefaultOverride>(defaultString);
			productionURL = defaults.url;
			forcePlayer = defaults.playername;
			forcePassword = defaults.password;
		}
#endif
	}

	//This call will log out everyone on the device
	public void ClearUserInfo()
	{
		isOnline = false;
		users.Clear();
		if (File.Exists(Application.persistentDataPath + "/session.info")){
			File.Delete(Application.persistentDataPath + "/session.info");
		}

		if(FB.IsLoggedIn)
		{
			FB.Logout();
		}
	}

	public void LogoutUser(int id)
	{
		users.Remove(id);
		if(users.Count == 0)
		{
			ClearUserInfo();
		}
	}

	public int GetNextLevel()
	{
		return nextLevel;
	}

	public void SetNextLevel(int level)
	{
		nextLevel = level;	
	}

	protected virtual ADAGEUser createNewUser(ADAGEUser newUser = null)
	{
		if(newUser == null)
			newUser = new ADAGEUser();
		newUser.guest = true;
		newUser.adageId = users.Count.ToString();
		newUser.playerName = string.Format("{0}-{1}", instance.currentSession, newUser.adageId);
		newUser.email = string.Format("{0}@{1}.com", newUser.playerName, instance.applicationName);
		return newUser;
	}

	private void ListenForAccessToken(string incoming){
		if(incoming != "invalid")
		{
			ADAGEUser user = new ADAGEUser();
			user.adageAccessToken = incoming;
			
			int user_id = AddUser(user);
			if(user_id == -1)
				return;
			//try to connect and get user info
			AddUserRequestJob(user_id);
			
		}
	}
	
	private void AddLoginJob(string name, string password, int localId)
	{
		statusMessage = "Connecting...";
		ADAGELoginJob job = new ADAGELoginJob(appToken, appSecret, name, password, localId);
		job.OnComplete = OnLoginComplete;
		threads.AddJob(job);
	}
	
	private void AddGuestConnectionJob(int localId)
	{
		statusMessage = "Connecting as guest...";
		ADAGEGuestConnectionJob job = new ADAGEGuestConnectionJob(appToken, appSecret, localId);
		job.OnComplete = OnLoginComplete;
		threads.AddJob(job);
	}

	private void AddQRJob(string group, int localId)
	{
		statusMessage = "Processing QR code...";
		ADAGE.ChangeSession(group);
		ADAGEGuestConnectionJob job = new ADAGEGuestConnectionJob(appToken, appSecret, localId, group);
		job.OnComplete = OnLoginComplete;
		threads.AddJob(job);
	}

	private void AddRegistrationJob(string name, string email, string password, string passwordConfirm, int localId)
	{
		statusMessage = "Registering User...";
		ADAGERegistrationJob job = new ADAGERegistrationJob(appToken, appSecret, name, email, password, passwordConfirm, localId);
		job.OnComplete = OnLoginComplete;
		threads.AddJob(job);
	}
		
	private void AddUserRequestJob(int localId)
	{
		statusMessage = "Requesting player info...";

		ADAGERequestUserJob rjob = new ADAGERequestUserJob(users[localId].adageAccessToken, true, localId);
		rjob.OnComplete = OnRequestUserComplete;
		threads.AddJob(rjob);
	}

	private void AddSaveUtilityJob(string endpoint, string json, int localId)
	{

		ADAGESaveUtilityJob job = new ADAGESaveUtilityJob(endpoint, json, users[localId].adageAccessToken, appToken, localId);
		job.OnComplete = OnUtilityUploadComplete;
		threads.AddJob(job);
	}



	private void AddLoadUtilityJob(string endpoint, ADAGEUtilityResponseCallback callback, int localId)
	{
			
		ADAGELoadUtilityJob job = new ADAGELoadUtilityJob(endpoint, users[localId].adageAccessToken, appToken, callback, localId);
		job.OnComplete = OnUtilityDownloadComplete;
		threads.AddJob(job);
	}


	
	private void AddLogJob()
	{
		for(int i=0; i < ADAGE.users.Count; i++)
		{
			ADAGEUser u = ADAGE.users[i];
			if(u.valid() && u.dataWrapper.Count > 0)
			{
				ADAGEUploadJob job = new ADAGEUploadJob(u.dataWrapper, u.adageAccessToken, i);
				if(ADAGE.Online)
				{
					//job.OnComplete = OnADAGEWebJobComplete;
					threads.AddJob(job);
				}
				else
				{			
					u.localWrapper.Add(job.GetData());
			
					if(u.localWrapper.Count > k_LocalWrapperMax)
						WriteDataLocal(i);
				}
				u.dataWrapper.Clear();
			}
		}
	}

	private void AddTrackerJob(ADAGETracker tracker, int localId)
	{
		if(ADAGE.Online)
		{
			ADAGETrackerJob job = new ADAGETrackerJob(tracker, ADAGE.users[localId].adageAccessToken);
			job.OnComplete = OnTrackerComplete;
			threads.AddJob(job);
		}
	}

	private void OnTrackerComplete(Job job)
	{
		ADAGETrackerJob tracker = (job as ADAGETrackerJob);
		tracker.data.Complete(tracker.response.text);
	}

	private void AddGetDataJob(ADAGEGetRequest request, int localId)
	{
		if(ADAGE.Online)
		{
			ADAGEGetDataJob job = new ADAGEGetDataJob(request, ADAGE.users[localId].adageAccessToken);
			job.OnComplete = OnGetDataComplete;
			threads.AddJob(job);
		}
	}
	
	private void OnGetDataComplete(Job job)
	{
		ADAGEGetDataJob request = (job as ADAGEGetDataJob);
		request.data.Complete(request.response.text);
	}

	private void OnADAGEWebJobComplete(Job job)
	{
		ADAGEWebJob aJob = null;

		try
		{
			aJob = (job as ADAGEWebJob);
			Type errorType = aJob.response.GetType();
			ADAGEUser u = users[aJob.localId];

			if(ReflectionUtils.CompareType(errorType, typeof(ADAGEErrorResponse)))
			{
				//If it was an upload that threw the error, we need to do something with the data
				if(ReflectionUtils.CompareType(job.GetType(), typeof(ADAGEUploadJob)))
				{
					u.localWrapper.Add((job as ADAGEUploadJob).GetData());
				}

				/*** Alert the user/developers ***/
				string statusMessage = "";

				List<string> errors = (aJob.response as ADAGEErrorResponse).errors;
				
				if (errors == null)
				{
					statusMessage = string.Format("There was an error trying to process {0}, but no message was returned.", job.GetType());
				}
				else
				{
					Debug.Log ("The number of errors is : " + errors.Count);

					if(errors.Count > 0)
					{
						statusMessage = errors[0];  //Let's just take one for now
						Debug.Log (statusMessage);
					}

                    if(job.GetType() == typeof(ADAGELoginJob))
                    {
                        if(OnLoginFailed != null)
                        {
                            OnLoginFailed(errors.ToArray());
                        }
                    }

                    if(job.GetType() == typeof(ADAGERegistrationJob))
                    {
                        if (OnRegistrationFailed != null)
                        {
                            OnRegistrationFailed(errors.ToArray());
                        }
                    }
				}

				Messenger<int, string>.Broadcast(k_OnError, aJob.localId, statusMessage);
				
				aJob.numAttempts++;
				//If the connection didn't work, take us offline
				if(ReflectionUtils.CompareType(errorType, typeof(ADAGEConnectionError)))
				{
					isOnline = false;
					
					/*if(ReflectionUtils.CompareType(job.GetType(), typeof(ADAGEGuestConnectionJob)))
					{
						if(aJob.numAttempts < ADAGEWebJob.maxAttempts)
						{
							threads.AddJob(aJob);
						}
						else
						{
							Messenger<string>.Broadcast(k_OnConnectionTimeout, job.GetType().ToString());
						}
					}*/
				}

				//If we are totally screwed and can't resolve the host, load the level
				if(ReflectionUtils.CompareType(errorType, typeof(ADAGEHostError)) || statusMessage.Trim() == "Could not find application.")
				{
					if(enableLogLocal) //if we are good to log locally, just start the game
					{
						Messenger<int, string>.Broadcast(k_OnLoginComplete, (job as ADAGEWebJob).localId, "");

						Messenger.Broadcast(k_OnGameStart);
					}
					else //if we aren't, we need to display an error
					{
                        #if !UNITY_5
						//Heavy processing, but needed in very few cases
						if(!gameHandlesLogin && ADAGEMenu.IsNull)
						{
							GameObject newMenu = Instantiate(Resources.Load<GameObject>("Prefabs/ADAGEMenu")) as GameObject;
							newMenu.transform.parent = transform;
							ADAGEMenu.ShowPanel<ADAGEHomePanel>();
						}
						else
						{
							Messenger<int, string>.Broadcast(k_OnError, aJob.localId, "Unable to resolve host");
						}
                        #endif
					}
				}

				return;
			}
		}
		catch(Exception e)
		{
            #if !UNITY_5
			if (ADAGEMenu.instance != null)
			{
				ADAGEMenu.ShowError((job as ADAGEWebJob).localId, e.ToString());
			}
            #endif

			Debug.Log (e.ToString());
			return;
		}

		if(aJob != null && aJob.OnComplete != null)
		{
			aJob.OnComplete(aJob);
		}
	}
	
	/*private void OnComplete(Job job)
	{
		ADAGEUploadJob upload = (job as ADAGEUploadJob);

		if(upload.status != 201) 
		{	
			ADAGEUser u = users[upload.localId];
		
			u.localWrapper.Add(upload.GetData());
		}	
	}*/

	/*** This works for logging in, registering, or guest access ***/
	private void OnLoginComplete(Job job)
	{
		//ADAGEClientJob<HTTP.ContentType.Application.JsonRequest> connection = (job as ADAGEClientJob<HTTP.ContentType.Application.JsonRequest>);
        
		ADAGEWebJob connection = job as ADAGEWebJob;

		ADAGEUser u = users[connection.localId];

		ADAGEAccessTokenResponse accessResponse = (connection.response as ADAGEAccessTokenResponse);
		u.adageAccessToken = accessResponse.access_token;

		//Messenger<int, string>.Broadcast(k_OnLoginComplete, connection.localId, accessResponse.access_token);

		//temp
		AddUserRequestJob(connection.localId);

		if(automaticLogin)
			Messenger.Broadcast(k_OnGameStart);

		/*StartGame();
		//StartSession();
		
		//Had to do this to allow Zeness to log in still.  -greg
		if (ADAGEMenu.instance != null)
		{
			ADAGEMenu.LoadLevel();
		}*/

        if (job.GetType() == typeof(ADAGERegistrationJob) && OnRegistrationSuccess != null)
        {
            OnRegistrationSuccess();
        }

	}

	private void OnRequestUserComplete(Job job)
	{
		ADAGERequestUserJob connection = (job as ADAGERequestUserJob);
		ADAGEUserResponse userResponse = (connection.response as ADAGEUserResponse);

		ADAGEUser u = users[connection.localId];

		u.playerName = userResponse.player_name;
		u.adageId = userResponse.uid;
		u.guest = userResponse.guest;
		u.email = userResponse.email;

		isOnline = true;
		statusMessage = u.playerName;
		SaveUserInfo();
		//PushLocalToOnline();
		Debug.Log("Completed User request for: " + u.playerName);

		Messenger<int, string>.Broadcast(k_OnLoginComplete, connection.localId, u.adageAccessToken);

		/*if(connection.onCompleteLoad)
		{

			StartSession(connection.localId);

			//Had to do this to allow Zeness to log in still.  -greg
			if (ADAGEMenu.instance != null)
			{
				ADAGEMenu.LoadLevel();
			}
		}*/

        if (OnUserLoggedIn != null)
        {
            OnUserLoggedIn(u);
        }
	}

	private void OnLocalUploadComplete(Job job)
	{
		ADAGEUploadFileJob upload = (job as ADAGEUploadFileJob);
		Debug.Log ("Local upload status " + upload.status);
		if(upload.status == 201)
		{
			Debug.Log("Upload complete deleting file " + upload.Path);
			File.Delete(upload.Path);	
		}	
	}

	private void OnUtilityUploadComplete(Job job)
	{
		ADAGESaveUtilityJob utility = (job as ADAGESaveUtilityJob);
		
		if(utility.status == 201) 
		{	
			Debug.Log("Utility file upload complete");
		}	
	}

	private void OnUtilityDownloadComplete(Job job)
	{
		ADAGELoadUtilityJob utility = (job as ADAGELoadUtilityJob);
		
		if(utility.status == 200) 
		{	
			Debug.Log("Utility file download complete");
		}	
	}
	
	private void AddContext<T>(T data, int localId) where T:ADAGEContext
	{
		if(!users[localId].vContext.IsTracking(data.name))
		{
			if(!users[localId].vContext.Add(data.name))
			{
				throw new ADAGEStartContextException(localId, data.name);
			}
		}
		else
		{
			if(!users[localId].vContext.Remove(data.name))
			{
				throw new ADAGEEndContextException(localId, data.name);	
			}
		}

		AddData<T>(data, localId);
	}
	
	private void PushLocalToOnline()
	{	
		//Debug.Log("Push local to online.");
#if(!UNITY_WEBPLAYER)
		if(!Directory.Exists(Application.persistentDataPath + dataPath))
		{
			Directory.CreateDirectory(Application.persistentDataPath + dataPath);	
		}
        DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath  + dataPath);
        DirectoryInfo[] diArr = di.GetDirectories();
		
		foreach(DirectoryInfo token in diArr)
		{
			string[] files = System.IO.Directory.GetFiles(token.ToString(), "*.data");
			//Debug.Log("Local File Count For " + token.Name + ": " + files.Length);
			for(int i=files.Length-1; i >= 0; i--)
			{
				//Debug.Log("Reading File: " + files[i]);

				string outgoingData = "{\"data\":[";
				outgoingData += System.IO.File.ReadAllText(files[i]);
				outgoingData = outgoingData.Substring(0, outgoingData.Length - 1);  //remove trailing comma
				outgoingData += "]}";  //add closing brackets.

				//Debug.Log("All Data: " + outgoingData);
				ADAGEUploadFileJob job = new ADAGEUploadFileJob(token.Name, files[i], outgoingData);
				job.OnComplete = OnLocalUploadComplete;
				threads.AddJob(job);		
			}
		}
	#endif
	}
	
	private byte[] TakeScreenshot(Camera cam)
	{
		Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		// Initialize and render
		RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		
		// Read pixels
		tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height), 0, 0);
		
		// Clean up
		cam.targetTexture = null;
		RenderTexture.active = null; 
		DestroyImmediate(rt);
		
		return tex.EncodeToPNG();
	}
	#endregion Instance Methods


	private void BeginFacebookAuth()
	{
		Debug.Log("called FB Init");
		FB.Init(OnFBInitComplete);	
	}
	
	private void OnFBInitComplete()
	{
		Debug.Log("FB Init complete...Login starting");
		FB.Login("email", OnFBLoginComplete);
	}
	
	private void OnFBLoginComplete(FBResult response)
	{
		Debug.Log("FB Access token: " + FB.AccessToken);
		Debug.Log("FB Uid: " + FB.UserId);
		Debug.Log("FB response: " + response.Text);
		
		FBLoginResponse info =  JsonMapper.ToObject<FBLoginResponse>(response.Text);
		users[0].fbAccessToken = info.access_token;
		
		FB.API("/me", Facebook.HttpMethod.GET, OnFBUserInfo);
		//FB.GetAuthResponse(OnAuthResponse);
	}

	//Facebook right now will always be the zero users - potentially might want to change this but currently the facebook SDK really only
	//supports one user per device.
	private void OnFBUserInfo(FBResult response)
	{
		Debug.Log("FB User Info... ");
		Debug.Log("FB response: " + response.Text);
		
		FBProfileInfo info = JsonMapper.ToObject<FBProfileInfo>(response.Text);
		Debug.Log("Look a valid email account! " + info.email);
		users[0].email = info.email;
		users[0].playerName = info.name;
		users[0].username = info.username;
		users[0].facebookId = info.id;
		//user.adageExpiresAt = 
		FBAuthWithAdage();
	}
	
	private void OnFBConnectionComplete(Job job)
	{
		ADAGEFacebookConnectionJob connection = (job as ADAGEFacebookConnectionJob);
		Debug.Log(connection.status);
		
		if(connection.status != 200) 
		{
			Debug.Log("What we have here is a FAILURE to authenticate!");
			statusMessage = "Could not connect";
			return;
		}
		
		ADAGEAccessTokenResponse accessResponse = JsonMapper.ToObject<ADAGEAccessTokenResponse>(connection.response.text);
		users[0].adageAccessToken = accessResponse.access_token;
		Debug.Log (accessResponse.access_token);
		//DebugEx.Log("Successfully authenticated with ADAGE."); 

		
		AddUserRequestJob(0);
	}
	
	private void OnAuthResponse(FBResult response)
	{
		Debug.Log("FB auth response " + response.Text);	
	}

	//This function takes info supplied by the iOS FB Oauth and constructs an Oauth like response 
	//that is sent to ADAGE to authenticate the user. This path will create an ADAGE user for 
	//the FB email if none already exists.
	private void FBAuthWithAdage()
	{
		FakebookAuthResponse cookie = new FakebookAuthResponse();
		cookie.credentials.token = users[0].fbAccessToken;
		cookie.credentials.expires_at = "";
		cookie.info.email = users[0].email;
		cookie.raw_info.email = users[0].email;
		cookie.info.username = users[0].username;
		cookie.raw_info.username = users[0].username;
		cookie.uid = users[0].facebookId;
		statusMessage = "Connection...";
		Debug.Log("Starting Auth with ADAGE");
		FakebookCookie cookieHolder = new FakebookCookie();
		cookieHolder.omniauth = cookie;
		string c = JsonMapper.ToJson(cookieHolder);
		Debug.Log("Cookie: " + c);
		ADAGEFacebookConnectionJob job = new ADAGEFacebookConnectionJob(appToken, appSecret, c);
		job.OnComplete = OnFBConnectionComplete;
		threads.AddJob(job);
	}

	/*public static long convertDateTimeToEpoch(DateTime time)
	{
   		DateTime epoch = new DateTime(1970, 1, 1);
		
    	TimeSpan ts = time - epoch;
    	return (long) ts.Ticks/ 10;
	}*/
}
