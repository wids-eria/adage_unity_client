#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using LitJson; 

[CustomEditor(typeof(ADAGE), true)]
public class ADAGEEditor : Editor
{
	public static ADAGEGameVersionInfo VersionInfo;

	private bool loaded = false;

	private bool showingInputControl = false;
	private bool showingTypeControl = false;
	private bool showingLocalLogging = false;
	private bool showingLoginControl = false;
	private bool showingVersionControl = false;

#if(UNITY_WEBPLAYER)
	private bool showingWebPlayerOptions = false;
#endif

	private bool showingProductionSettings = false;
	private bool showingDevelopmentSettings = false;
	private bool showingStagingSettings = false;

	private int index = -1;
	private int prevIndex;
	private string[] paths;

	private bool enableGameHandlesLogin = true;
	public static bool enableGuestLogin = true;
	private bool enableFacebookLogin = true;
	private bool enableAutomaticLogin = true;
	private bool enableDefaultLogin = true;
	private bool enableLastUser = true;
	private bool enableGuestAccount = true;
	private bool enableNextLevel = true;
	private bool enableCacheVersion = true;

	//private List<string> dataTypeKeys;
	private float layoutWidth;

	private Color activeColor = Color.green;
	private Color inactiveColor = Color.red;

	private GUIStyle activeButtonStyle;

	static ADAGEEditor()
	{
	}

	static void ADAGEContextInfoToJSON(Dictionary<string, ADAGEContextInfo> info, JsonWriter writer)
	{
		writer.WriteArrayStart();
		{
			foreach(KeyValuePair<string, ADAGEContextInfo> item in info)
			{
				writer.Write (item.Key);
			}
		}
		writer.WriteArrayEnd();
	}

	/*private void CheckCompile() 
	{
		if(EditorApplication.isCompiling)
		{
			compiling = true; 
		}
		else
		{
			if(compiling)
			{
				compiling = false;
				Refresh();
			}
		}
	}*/

	public void OnEnable()
	{
		if(!loaded)
		{
			loaded = true;
		}
		LoadVersionSettings();
		//LoadDataTypes();
		SaveLevelList();

		//Refresh();
	}

	public void OnDisable()
	{
		SaveVersionSettings();
	}

	public void OnFocus()
	{
		index = (target as ADAGE).GetNextLevel();
	}

	public override void OnInspectorGUI()
	{
		if(activeButtonStyle == null)
			CreateGUIStyles();

		base.DrawDefaultInspector();
		ADAGE curTarget = (target as ADAGE);

		showingProductionSettings = EditorGUILayout.Foldout(showingProductionSettings, "Production Settings");
		if(showingProductionSettings)
		{
			EditorGUI.indentLevel += 2;
			{
				curTarget.productionServer = EditorGUILayout.TextField("Server", curTarget.productionServer.Trim());
				curTarget.appToken = EditorGUILayout.TextField("Token", curTarget.appToken.Trim());
				curTarget.appSecret = EditorGUILayout.TextField("Secret", curTarget.appSecret.Trim());
				GUI.enabled = !curTarget.forceDevelopment && !curTarget.forceStaging;
					curTarget.forceProduction = EditorGUILayout.Toggle("Force Connect", curTarget.forceProduction);
				GUI.enabled = true;
			}
			EditorGUI.indentLevel -= 2;
		}

		showingDevelopmentSettings = EditorGUILayout.Foldout(showingDevelopmentSettings, "Development Settings");
		if(showingDevelopmentSettings)
		{
			EditorGUI.indentLevel += 2;
			{
				curTarget.developmentServer = EditorGUILayout.TextField("Server", curTarget.developmentServer.Trim());
				curTarget.devToken = EditorGUILayout.TextField("Token", curTarget.devToken.Trim());
				curTarget.devSecret = EditorGUILayout.TextField("Secret", curTarget.devSecret.Trim());
				GUI.enabled = !curTarget.forceProduction && !curTarget.forceStaging;
					curTarget.forceDevelopment = EditorGUILayout.Toggle("Force Connect", curTarget.forceDevelopment);
				GUI.enabled = true;
			}
			EditorGUI.indentLevel -= 2;
		}
		
		showingStagingSettings = EditorGUILayout.Foldout(showingStagingSettings, "Staging Settings");
		if(showingStagingSettings)
		{
			EditorGUI.indentLevel += 2;
			{
				curTarget.staging = EditorGUILayout.Toggle("Enable", curTarget.staging);
				curTarget.stagingServer = EditorGUILayout.TextField("Server", curTarget.stagingServer.Trim());
				curTarget.appToken = EditorGUILayout.TextField("Token", curTarget.appToken.Trim());
				curTarget.appSecret = EditorGUILayout.TextField("Secret", curTarget.appSecret.Trim());
				GUI.enabled = !curTarget.forceProduction && !curTarget.forceDevelopment;
					curTarget.forceStaging = EditorGUILayout.Toggle("Force Connect", curTarget.forceStaging);
				GUI.enabled = true;
			}
			EditorGUI.indentLevel -= 2;
		}

		showingLoginControl = EditorGUILayout.Foldout(showingLoginControl, "Login Settings");
		
		if(showingLoginControl)
		{
			EditorGUI.indentLevel += 2;

			GUI.enabled = enableGameHandlesLogin;
				curTarget.gameHandlesLogin = EditorGUILayout.Toggle("Game Handles Login", curTarget.gameHandlesLogin);
			GUI.enabled = enableGuestLogin;
				curTarget.allowGuestLogin = EditorGUILayout.Toggle("Enable Guest Login", curTarget.allowGuestLogin);
			GUI.enabled = enableFacebookLogin;
				curTarget.allowFacebook = EditorGUILayout.Toggle("Enable Facebook Login", curTarget.allowFacebook);

			GUI.enabled = enableAutomaticLogin;
			curTarget.automaticLogin = EditorGUILayout.Toggle("Automatically Login", curTarget.automaticLogin);

			EditorGUI.indentLevel += 2;
			{
				GUI.enabled = enableDefaultLogin;
				curTarget.forceLogin = EditorGUILayout.Toggle("Use Default Login", curTarget.forceLogin);
				GUI.enabled = curTarget.forceLogin;
				EditorGUI.indentLevel += 2;
					curTarget.forcePlayer = EditorGUILayout.TextField("Username", curTarget.forcePlayer);
					curTarget.forcePassword = EditorGUILayout.TextField("Password", curTarget.forcePassword);
				EditorGUI.indentLevel -= 2;

				GUI.enabled = enableLastUser;
				curTarget.autoLoginLastUser = EditorGUILayout.Toggle("Last User", curTarget.autoLoginLastUser);

				GUI.enabled = enableGuestAccount;
				curTarget.autoGuestLogin = EditorGUILayout.Toggle("Guest", curTarget.autoGuestLogin);

				GUI.enabled = enableNextLevel;
				curTarget.automaticStartGame = EditorGUILayout.Toggle("Auto Start Level", curTarget.automaticStartGame); 
				EditorGUI.BeginDisabledGroup(!curTarget.automaticStartGame);
				index = EditorGUILayout.Popup("Next Level", index, paths);
				EditorGUI.EndDisabledGroup();
				if(index != -1 && index != prevIndex)
				{
					curTarget.SetNextLevel(index);
					prevIndex = index;
				}
			}
			EditorGUI.indentLevel -= 2;
			GUI.enabled = true;

			EditorGUI.indentLevel -= 2;
		}

		showingTypeControl = EditorGUILayout.Foldout(showingTypeControl, "Manage Data Types");
		
		if(showingTypeControl)
		{
			EditorGUI.indentLevel += 2;
			{
				EditorGUILayout.BeginHorizontal();
				{
					IndentGUI(1);
					if(GUILayout.Button("Enable All"))
					{
						for(int i = 0; i < curTarget.isDataTypeActive.Count; i++)
						{
							curTarget.isDataTypeActive[i] = true;
						}
					}

					if(Event.current.type == EventType.Repaint)
						layoutWidth = GUILayoutUtility.GetLastRect().width;

					if(GUILayout.Button("Disable All"))
					{
						for(int i = 0; i < curTarget.isDataTypeActive.Count; i++)
						{
							curTarget.isDataTypeActive[i] = false;
						}
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.BeginVertical();
					{
						for(int i = 0; i < curTarget.dataTypes.Count; i++)
						{
							EditorGUILayout.BeginHorizontal();
							{
								ADAGEEventInfo eventInfo = null;
								
								if(curTarget.versionInfo != null)
								{
									if(curTarget.versionInfo.events.ContainsKey(curTarget.dataTypes[i]))
										eventInfo = curTarget.versionInfo.events[curTarget.dataTypes[i]];
									else if(curTarget.versionInfo.context.ContainsKey(curTarget.dataTypes[i]))
										eventInfo = curTarget.versionInfo.context[curTarget.dataTypes[i]];

								}
								else
								{
									EditorGUILayout.LabelField("no version");
								}

								string eventName = curTarget.dataTypes[i];

								EditorGUI.indentLevel++;
								EditorGUILayout.BeginHorizontal();
								{
									if(eventInfo != null)
									{
										EditorGUILayout.BeginVertical();
										{
											eventInfo.showing = EditorGUILayout.Foldout(eventInfo.showing, eventName);
											
											if(eventInfo.showing)
											{
												foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in eventInfo.properties)
												{
													EditorGUI.indentLevel++; 
													prop.Value.DrawLabel(prop.Key);
													EditorGUI.indentLevel--;
												}
											}
										}
										EditorGUILayout.EndVertical();
									}
									else
									{
										EditorGUILayout.LabelField(eventName);
									}
								}
								EditorGUILayout.EndHorizontal();
								EditorGUI.indentLevel--;
					
								EditorGUILayout.Space();
								
								EditorGUILayout.BeginVertical(GUILayout.Width(layoutWidth));
								{
									string buttonText;
									if(curTarget.isDataTypeActive[i])
										buttonText = "ACTIVE";
									else
										buttonText = "INACTIVE";

									curTarget.isDataTypeActive[i] = GUILayout.Toggle(curTarget.isDataTypeActive[i], buttonText, activeButtonStyle, GUILayout.Width(layoutWidth));
								}
								EditorGUILayout.EndVertical();
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					EditorGUILayout.EndVertical();
	
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel -= 2;
		}

		showingInputControl = EditorGUILayout.Foldout(showingInputControl, "Input Logging");

		if(showingInputControl)
		{
			EditorGUI.indentLevel += 2;
			curTarget.enableKeyboardCapture = EditorGUILayout.Toggle("Enable Keyboard Capture", curTarget.enableKeyboardCapture);

			GUI.enabled = curTarget.enableKeyboardCapture;
			EditorGUI.indentLevel += 2;
			curTarget.autoCaptureKeyboard = EditorGUILayout.Toggle("Auto Capture Keyboard", curTarget.autoCaptureKeyboard);
			EditorGUI.indentLevel -= 2;
			GUI.enabled = true;
			
			curTarget.enableMouseCapture = EditorGUILayout.Toggle("Enable Mouse Capture", curTarget.enableMouseCapture);
			
			GUI.enabled = curTarget.enableMouseCapture;
			EditorGUI.indentLevel += 2;
			curTarget.autoCaptureMouse = EditorGUILayout.Toggle("Auto Capture Mouse", curTarget.autoCaptureMouse);
			EditorGUI.indentLevel -= 2;
			GUI.enabled = true;
			EditorGUI.indentLevel -= 2;
		}

		/*if(ADAGEEditor.VersionInfo != null && ADAGEEditor.VersionInfo.dirty)
		{
			ADAGEEditor.VersionInfo.dirty = false;
			Debug.Log ("merging");
			MergeVersionInfo(target);
			EditorUtility.SetDirty(curTarget);
		}*/

		showingLocalLogging = EditorGUILayout.Foldout(showingLocalLogging, "Local Logging");
		if(showingLocalLogging)
		{
			EditorGUI.indentLevel += 2;
			curTarget.enableLogLocal = EditorGUILayout.Toggle("Enable", curTarget.enableLogLocal);
			GUI.enabled = curTarget.enableLogLocal;
				GUILayout.BeginHorizontal();
				{
					EditorGUILayout.TextField("Data Path", (target as ADAGE).dataPath);
					if(GUILayout.Button("Select"))
					{
						string oldPath = curTarget.dataPath;
						curTarget.dataPath = EditorUtility.OpenFolderPanel("Select ADAGE Data Directory", "", "");
						
						string appDataPath = Application.dataPath;
						bool longer = (curTarget.dataPath.Length >= appDataPath.Length);
						if(!longer || (longer && curTarget.dataPath.Substring(0,appDataPath.Length) != appDataPath))
						{
							EditorUtility.DisplayDialog("ADAGE Data Path Error", "You must select a path that is in the project path", "OK");
							curTarget.dataPath = oldPath;
						}
						else
						{
							curTarget.dataPath = curTarget.dataPath.Substring(appDataPath.Length);	
						}
					}
				}
				GUILayout.EndHorizontal();
			GUI.enabled = true;
			EditorGUI.indentLevel -= 2;
		}

		showingVersionControl = EditorGUILayout.Foldout(showingVersionControl, "Version Control");
		if(showingVersionControl)
		{
			EditorGUI.indentLevel += 2;
			enableCacheVersion = EditorGUILayout.Toggle("Compile Version Info", enableCacheVersion);
			GUI.enabled = enableCacheVersion;
			{
				EditorGUI.indentLevel += 2;
				curTarget.enableValidation = EditorGUILayout.Toggle("Validate Data", curTarget.enableValidation);
				EditorGUI.indentLevel -= 2;
			}
			GUI.enabled = true;
			EditorGUI.indentLevel -= 2;
		}

#if(UNITY_WEBPLAYER)
		showingWebPlayerOptions = EditorGUILayout.Foldout(showingWebPlayerOptions, "Web Player Options");
		if(showingWebPlayerOptions)
		{
			EditorGUI.indentLevel += 2;
			curTarget.socketPort = EditorGUILayout.IntField("Socket Policy Port", curTarget.socketPort);
			EditorGUI.indentLevel -= 2;
		}
#endif

		GUILayout.Space(15);

		/*if(GUILayout.Button("Version Control"))
		{
			ADAGEVersionEditor.Init(curTarget);
			//ADAGEVersionEditor myCustomWindow = (ADAGEVersionEditor) EditorWindow.GetWindow(typeof(ADAGEVersionEditor) ,false, "ADAGE Version");
		}
		
		GUILayout.Space(15);*/

		if(GUI.changed)
		{
			CheckSettings(curTarget);
			EditorUtility.SetDirty(curTarget);
		}
	}

	private void LoadDataTypes()
	{
		ADAGE tar = (target as ADAGE);
		if(tar.dataTypes == null)
		{
			Debug.Log ("resetting active types");
			tar.dataTypes = new List<string>();
			tar.isDataTypeActive = new List<bool>();
		}

		Dictionary<string, Type> types = ReflectionUtils.GetChildTypes(typeof(ADAGEData));
		if(types.Count > 0)
		{
			foreach(KeyValuePair<string, Type> type in types)
			{
				if(!type.Value.IsDefined(typeof(ADAGE.BaseClass), false))
				{
					if(!tar.dataTypes.Contains(type.Key))
					{
						tar.dataTypes.Add(type.Key);
						tar.isDataTypeActive.Add(true);
					}
				}
			}

			List<string> badKeys = new List<string>();
			foreach(string type in tar.dataTypes)
			{
				if(!types.ContainsKey(type))
					badKeys.Add(type);
			}

			foreach(string key in badKeys)
			{
				int index = tar.dataTypes.IndexOf(key);
				tar.dataTypes.RemoveAt(index);
				tar.isDataTypeActive.RemoveAt(index);
			}
		}
	}

	private void SaveLevelList()
	{
		ADAGE tar = (target as ADAGE);
		int next = tar.GetNextLevel();

		int sceneCount = 0;
		for(int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			if(EditorBuildSettings.scenes[i].enabled)
				sceneCount++;
		}

		paths = new string[sceneCount];

		int pathIndex = 0;
		for(int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			if(EditorBuildSettings.scenes[i].enabled)
			{
				paths[pathIndex] = EditorBuildSettings.scenes[i].path;
				pathIndex++;
			}
		}

		if(paths.Length - 1 > next)
		{
			index = next;
		}
		
		prevIndex = index;
	}

	private void CreateGUIStyles()
	{
		activeButtonStyle = new GUIStyle(GUI.skin.button);

		activeButtonStyle.onNormal.textColor = activeColor;
		activeButtonStyle.normal.textColor = inactiveColor;
	}

	private void SaveVersionSettings()
	{
	#if(!UNITY_WEBPLAYER)
		if((target as ADAGE).versionInfo != null)
		{
			string path = Application.dataPath + "/ADAGE/";
			if(!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			File.WriteAllBytes(path + "schema.info", (target as ADAGE).versionInfo.Pack());
		}
	#endif
	}

	private void LoadVersionSettings()
	{
	#if(!UNITY_WEBPLAYER)
		ADAGE tar = (target as ADAGE);
		if(tar.enableCacheVersion)
			tar.versionInfo = new ADAGEGameVersionInfo();

		if(File.Exists(Application.dataPath + "/ADAGE/schema.info"))
		{
			tar.versionInfo.Unpack(File.ReadAllBytes(Application.dataPath + "/ADAGE/schema.info"));
		}
		/*else
		{*/
			tar.dataTypes = null;
			if(tar.dataTypes == null)
			{
				tar.dataTypes = new List<string>();
				tar.isDataTypeActive = new List<bool>();
			}
			
			Dictionary<string, Type> types = ReflectionUtils.GetChildTypes(typeof(ADAGEData));

			if(types.Count > 0)
			{
				foreach(KeyValuePair<string, Type> type in types)
				{
					if(!type.Value.IsDefined(typeof(ADAGE.BaseClass), false))
					{
						if(!tar.dataTypes.Contains(type.Key))
						{
							tar.dataTypes.Add(type.Key);
							//if(tar.enableCacheVersion)
								tar.versionInfo.AddEvent(type.Key, GetEventInfo(type.Value));
							tar.isDataTypeActive.Add(true);
						}
					}
				}
				
				List<string> badKeys = new List<string>();
				foreach(string type in tar.dataTypes)
				{
					if(!types.ContainsKey(type))
						badKeys.Add(type);
				}
				
				foreach(string key in badKeys)
				{
					int index = tar.dataTypes.IndexOf(key);
					tar.dataTypes.RemoveAt(index);
					tar.isDataTypeActive.RemoveAt(index);
				}
			}
		//}
	#endif
	}

	private void CheckSettings(ADAGE curTarget)
	{
		if(curTarget.allowGuestLogin || curTarget.allowFacebook || curTarget.autoLoginLastUser || curTarget.autoGuestLogin)
			curTarget.forceLogin = false;

		if(!curTarget.automaticLogin)
		{
			curTarget.forceLogin = false;
			curTarget.autoLoginLastUser = false;
			curTarget.autoGuestLogin = false;
		}

		if(curTarget.forceLogin)
		{
			curTarget.allowGuestLogin = false;
			curTarget.allowFacebook = false;
			curTarget.autoLoginLastUser = false;
		}

		//Enable and disable fields
		enableGameHandlesLogin = !curTarget.automaticLogin;
		enableGuestLogin = !curTarget.forceLogin;
		enableFacebookLogin = !curTarget.forceLogin && !curTarget.autoGuestLogin;
		enableAutomaticLogin = !curTarget.gameHandlesLogin;
		enableDefaultLogin = curTarget.automaticLogin && !curTarget.allowGuestLogin;
		enableLastUser = curTarget.automaticLogin && !curTarget.forceLogin && !curTarget.autoGuestLogin;
		enableGuestAccount = curTarget.automaticLogin && !curTarget.forceLogin && curTarget.allowGuestLogin;
		enableNextLevel = curTarget.automaticLogin;

		if(curTarget.productionServer.Trim() == "")
			curTarget.productionServer = ADAGE.productionURL;
		if(curTarget.developmentServer.Trim() == "")
			curTarget.developmentServer = ADAGE.developmentURL;
		if(curTarget.stagingServer.Trim() == "")
			curTarget.stagingServer = ADAGE.stagingURL;
	}

	void MergeVersionInfo(UnityEngine.Object target)
	{
		ADAGE currentTarget = (target as ADAGE);

		if(currentTarget.versionInfo == null)
		{
			Debug.Log ("Clearing version info");  
			currentTarget.versionInfo = ADAGEEditor.VersionInfo;
			return;
		}

		List<string> currentObjects = new List<string>();

		//Start Context Merge
		foreach(KeyValuePair<string, ADAGEEventInfo> item in ADAGEEditor.VersionInfo.context)
		{
			currentObjects.Add(item.Key);

			if(!currentTarget.versionInfo.context.ContainsKey(item.Key)) //If this event is new
			{
				currentTarget.versionInfo.AddContext(item.Key, item.Value);
			}
			else //If we already have an event like this
			{
				ADAGEEventInfo curEvent = item.Value;
				foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in curEvent.properties)
				{
					if(!currentTarget.versionInfo.context[item.Key].properties.ContainsKey(prop.Key))
					{
						currentTarget.versionInfo.context[item.Key].properties.Add(prop.Key, prop.Value);
					}
				}
				
				foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in currentTarget.versionInfo.context[item.Key].properties)
				{
					if(!curEvent.properties.ContainsKey(prop.Key))
					{
						currentTarget.versionInfo.context[item.Key].properties.Remove(prop.Key);
					}
				}
			}
		}

		//Look for any outdated context, as in they are no longer present in the folder
		foreach(KeyValuePair<string, ADAGEEventInfo> savedEvent in currentTarget.versionInfo.context)
		{
			if(!currentObjects.Contains(savedEvent.Key))
			{
				currentTarget.versionInfo.RemoveContext(savedEvent.Key);
			}
		}

		currentObjects = new List<string>();

		//Start Event Merge
		foreach(KeyValuePair<string, ADAGEEventInfo> item in ADAGEEditor.VersionInfo.events)
		{
			currentObjects.Add(item.Key);
			
			if(!currentTarget.versionInfo.events.ContainsKey(item.Key)) //If this event is new
			{
				currentTarget.versionInfo.AddEvent(item.Key, item.Value);
			}
			else //If we already have an event like this
			{
				ADAGEEventInfo curEvent = item.Value;
				foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in curEvent.properties)
				{
					if(!currentTarget.versionInfo.events[item.Key].properties.ContainsKey(prop.Key))
					{
						currentTarget.versionInfo.events[item.Key].properties.Add(prop.Key, prop.Value);
					}
				}
				
				foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in currentTarget.versionInfo.events[item.Key].properties)
				{
					if(!curEvent.properties.ContainsKey(prop.Key))
					{
						currentTarget.versionInfo.events[item.Key].properties.Remove(prop.Key);
					}
				}
			}
		}
		
		//Look for any outdated context, as in they are no longer present in the folder
		foreach(KeyValuePair<string, ADAGEEventInfo> savedEvent in currentTarget.versionInfo.events)
		{
			if(!currentObjects.Contains(savedEvent.Key))
			{
				currentTarget.versionInfo.RemoveEvent(savedEvent.Key);
			}
		}
	}

	private void Refresh()
	{		
		if(VersionInfo == null)
			VersionInfo = new ADAGEGameVersionInfo();
		
		Dictionary<string, Type> eventTypes = ReflectionUtils.GetChildTypes(typeof(ADAGEData));
		foreach(KeyValuePair<string, Type> e in eventTypes)
		{
			if(!IsADAGEBase(e.Value))
				VersionInfo.AddEvent(e.Key, GetEventInfo(e.Value));
		}
		
		Dictionary<string, Type> contexts = ReflectionUtils.GetChildTypes(typeof(ADAGEContext));
		foreach(KeyValuePair<string, Type> c in contexts)
		{
			if(!IsADAGEBase(c.Value))
				VersionInfo.AddContext(c.Key, GetEventInfo(c.Value));
		}
	}
	
	private bool IsADAGEBase(Type curType)
	{
		System.Object[] attrs = curType.GetCustomAttributes(false);
		if(attrs.Length > 0)
		{
			for(int j = 0; j < attrs.Length; j++)
			{
				if(attrs[j].GetType() == typeof(ADAGE.BaseClass))
				{
					return true;
				}
			}
		}
		return false;
	}
	
	private ADAGEEventInfo GetEventInfo(Type type)
	{
		ADAGEEventInfo newEvent = new ADAGEEventInfo();
		
		bool inherit = false;
		
		//Get properties for current type
		PropertyInfo[] propsInfo;
		if(inherit)
			propsInfo = type.GetProperties();
		else
			propsInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		
		foreach (PropertyInfo p_info in propsInfo) 
		{	
			bool skip = false;
			System.Object[] attrs = p_info.GetCustomAttributes(false);
			if(attrs.Length > 0)
			{
				for(int j = 0; (j < attrs.Length && !skip); j++)
				{
					skip = (attrs[j].GetType() == typeof(LitJson.SkipSerialization));
				}
			}
			
			if (p_info.Name == "Item")
				continue;
			
			ADAGEDataPropertyInfo propertyInfo = ADAGEDataPropertyInfo.Build(p_info.PropertyType);
			
			if(propertyInfo == null)
				continue;
			
			newEvent.properties.Add(p_info.Name, propertyInfo);
		}
		
		//Get fields for current type
		FieldInfo[] fieldsInfo;
		if(inherit)
			fieldsInfo = type.GetFields();
		else
			fieldsInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		
		foreach (FieldInfo f_info in fieldsInfo)
		{		
			bool skip = false;
			System.Object[] attrs = f_info.GetCustomAttributes(false);
			if(attrs.Length > 0)
			{
				for(int j = 0; (j < attrs.Length && !skip); j++)
				{
					skip = (attrs[j].GetType() == typeof(LitJson.SkipSerialization));
				}
			}
			
			if(skip)
				continue;
			
			if(!f_info.Name.Contains("<"))
			{
				ADAGEDataPropertyInfo propertyInfo = ADAGEDataPropertyInfo.Build(f_info.FieldType);
				
				if(propertyInfo == null)
					continue;
				
				newEvent.properties.Add(f_info.Name, propertyInfo);
			}
		}
		
		return newEvent;
	}

	private void IndentGUI(int amount)
	{
		GUILayout.Space (33 * amount);
	}
}
#endif