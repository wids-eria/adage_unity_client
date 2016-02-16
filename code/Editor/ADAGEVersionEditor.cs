using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

/*public class ADAGEVersionEditor : EditorWindow 
{
	public static string ADAGEBasePath = "";
	public static float defaultWidth = 0f;
	public static float defaultHeight = 0f;

	private static ADAGE curTarget;

	private bool refreshing = false;

	private Vector2 eventsScrollPosition = Vector2.zero;
	private Vector2 contextScrollPosition = Vector2.zero;

	private GUIStyle labelStyle = null;

	public static void Init (ADAGE target)
	{
		curTarget = target;
		// Get existing open window or if none, make a new one:
		ADAGEVersionEditor newWindow = (ADAGEVersionEditor) EditorWindow.GetWindow(typeof(ADAGEVersionEditor), false, "ADAGE");
		defaultWidth = Screen.currentResolution.width * 0.5f;
		defaultHeight = Screen.currentResolution.height * 0.5f;

		newWindow.labelStyle = new GUIStyle("label");
		newWindow.labelStyle.richText = true;

		newWindow.minSize = new Vector2(defaultWidth, defaultHeight);
		newWindow.maxSize = new Vector2(Screen.currentResolution.width * 0.8f, Screen.currentResolution.height * 0.8f);

		newWindow.position = new Rect(Screen.currentResolution.width * 0.1f,Screen.currentResolution.height * 0.1f,defaultWidth,defaultHeight);

		newWindow.Refresh();
	}
	
	void OnGUI () 
	{
		EditorGUIUtility.labelWidth = 145f;
		EditorGUIUtility.fieldWidth = 150f;

		float buttonWidth = position.width / 3f;

		GUILayout.BeginVertical();
		{
			GUILayout.BeginHorizontal("box", GUILayout.Width(position.width), GUILayout.Height(50f)); 
			{	
				GUILayout.Space(buttonWidth * 2f - 25f);

				GUILayout.BeginHorizontal(GUILayout.Width(buttonWidth)); 
				{				
					if(GUILayout.Button ("Refresh"))
					{
						Refresh ();					
					}
					
					if(GUILayout.Button ("Submit"))
					{
						Debug.Log (LitJson.JsonMapper.ToJson(curTarget.versionInfo));					
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				GUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2f - 2.5f), GUILayout.MinHeight(position.height - 70f));
				{
					GUILayout.Label("<b>ADAGE Events</b>", labelStyle);

					eventsScrollPosition = GUILayout.BeginScrollView(eventsScrollPosition,false,true);
					{
						if(!refreshing && curTarget.versionInfo != null && curTarget.versionInfo.events != null && curTarget.versionInfo.events.Count > 0)
						{
							List<string> events = new List<string>(curTarget.versionInfo.events.Keys);
							for(int i = 0; i < events.Count; i++)
							{
								string eventName = events[i];
								ADAGEEventInfo eventInfo = curTarget.versionInfo.events[eventName];
								
								EditorGUI.indentLevel++;
								EditorGUILayout.BeginHorizontal();
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
								EditorGUILayout.EndHorizontal();
								EditorGUI.indentLevel--;
							}
						}
						else if(refreshing)
						{
							GUILayout.Label("Refreshing...");
						}
						else if(curTarget.versionInfo == null)
						{
							GUILayout.Label("No version information was found. Please refresh.");
						}
						else if(curTarget.versionInfo.events == null || curTarget.versionInfo.events.Count == 0)
						{
							GUILayout.Label("No events from ADAGEData were found. Please refresh.");
						}
					}
					GUILayout.EndScrollView();
				}
				GUILayout.EndVertical();

				GUILayout.Space(5f);

				GUILayout.BeginVertical("box", GUILayout.MaxWidth(position.width / 2f - 2.5f), GUILayout.MinHeight(position.height - 70f));
				{
					GUILayout.Label("<b>ADAGE Context</b>", labelStyle);
				
					contextScrollPosition = GUILayout.BeginScrollView(contextScrollPosition,false,true);
					{
						if(!refreshing && curTarget.versionInfo != null && curTarget.versionInfo.context != null && curTarget.versionInfo.context.Count > 0)
						{
							List<string> contexts = new List<string>(curTarget.versionInfo.context.Keys);

							for(int i = 0; i < contexts.Count; i++)
							{
								string context = contexts[i];

								ADAGEEventInfo contextInfo = curTarget.versionInfo.context[context];
								
								EditorGUI.indentLevel = 1;
								EditorGUILayout.BeginHorizontal();
								{
									EditorGUILayout.BeginVertical();
									{
										contextInfo.showing = EditorGUILayout.Foldout(contextInfo.showing, context);
										
										if(contextInfo.showing)
										{
											foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in contextInfo.properties)
											{
												EditorGUI.indentLevel = 6;
												prop.Value.DrawLabel(prop.Key);
											}
										}
									}
									EditorGUILayout.EndVertical();
								}
								EditorGUILayout.EndHorizontal();
							}
						}
						else if(refreshing)
						{
							GUILayout.Label("Refreshing...");
						}
						else if(curTarget.versionInfo == null)
						{
							GUILayout.Label("No version information was found. Please refresh.");
						}
						else if(curTarget.versionInfo.context == null || curTarget.versionInfo.context.Count == 0)
						{
							GUILayout.Label("No context objects from ADAGEContext were found. Please refresh.");
						}				
					}
					GUILayout.EndScrollView();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	private void Refresh()
	{
		refreshing = true;
		//List<string> currentEvents = new List<string>(curTarget.versionInfo.eventTypes.Keys);

		if(curTarget.versionInfo == null)
			curTarget.versionInfo = new ADAGEGameVersionInfo();

		Dictionary<string, Type> eventTypes = ReflectionUtils.GetChildTypes(typeof(ADAGEData));
		foreach(KeyValuePair<string, Type> e in eventTypes)
		{
			if(!IsADAGEBase(e.Value))
				curTarget.versionInfo.AddEvent(e.Key, GetEventInfo(e.Value));
		}

		Dictionary<string, Type> contexts = ReflectionUtils.GetChildTypes(typeof(ADAGEContext));
		foreach(KeyValuePair<string, Type> c in contexts)
		{
			if(!IsADAGEBase(c.Value))
				curTarget.versionInfo.AddContext(c.Key, GetEventInfo(c.Value));
		}

		refreshing = false;
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
}*/