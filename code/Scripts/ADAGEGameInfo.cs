using System;
using System.Collections.Generic;
using Thrift.Protocol;
using System.IO;
using Thrift.Transport;
using System.Reflection;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ADAGEPlayerInformation
{
	public string identifier;
	public Color color;

	public ADAGEPlayerInformation(){}

	public ADAGEPlayerInformation(string sID, Color color)
	{
		this.identifier = sID;
		this.color = color;
	}
}

[System.Serializable, ADAGE.BaseClass]
public class ADAGEGameInformation : ADAGEData
{
	public Dictionary<string, ADAGEPlayerInformation> players;
}

//[System.Serializable]
public class ADAGEEventInfoDictionary : Dictionary<string, ADAGEEventInfo>{} 

//[System.Serializable]
public class ADAGEGameVersionInfo
{
	//public Dictionary<string, ADAGEContextInfo> virtualContext = new Dictionary<string, ADAGEContextInfo>();
	[LitJson.SkipSerialization]
	public bool dirty;

	protected MemoryStream stream;
	protected TStreamTransport transport;
	protected TCompactProtocol protocol;

	private TStruct struc;
	protected TField fieldWriter;
	protected TField fieldReader;

	//[SerializeField]
	public ADAGEEventInfoDictionary context = new ADAGEEventInfoDictionary();

	//[SerializeField]
	public ADAGEEventInfoDictionary events = new ADAGEEventInfoDictionary();

	public ADAGEGameVersionInfo()
	{
		struc = new TStruct(GetType().ToString());
		fieldWriter = new TField();
		fieldReader = new TField();
	}

	public byte[] Pack()
	{
		stream = new MemoryStream();
		if(transport == null)
			transport = new TStreamTransport(null, stream);
		else
			transport.OutputStream = stream;
		
		if(protocol == null)
			protocol = new TCompactProtocol(transport);
		else
		{
			protocol.reset();
			protocol.Transport = transport;
		}
		
		protocol.WriteStructBegin(struc);
		
		fieldWriter.Name = "context";
		fieldWriter.Type = TType.Map;
		fieldWriter.ID = 1;
		protocol.WriteFieldBegin(fieldWriter);
		{		
			protocol.WriteMapBegin(new TMap(TType.String, TType.Struct, context.Count));
			foreach (string _iter44 in context.Keys)
			{
				protocol.WriteString(_iter44);
				context[_iter44].Write(fieldWriter, protocol);
			}
			protocol.WriteMapEnd();
		}
		protocol.WriteFieldEnd();

		fieldWriter.Name = "events";
		fieldWriter.Type = TType.Map;
		fieldWriter.ID = 2;
		protocol.WriteFieldBegin(fieldWriter);
		{		
			protocol.WriteMapBegin(new TMap(TType.String, TType.Struct, events.Count));
			foreach (string _iter44 in events.Keys)
			{
				protocol.WriteString(_iter44);
				events[_iter44].Write(fieldWriter, protocol);
			}
			protocol.WriteMapEnd();
		}
		protocol.WriteFieldEnd();
		
		protocol.WriteFieldStop();
		protocol.WriteStructEnd();
		
		return stream.ToArray();
	}

	private string tempKey;
	private ADAGEEventInfo tempValue;

	public void Unpack(byte[] data)
	{
		stream = new MemoryStream(data);
		if(transport == null)
			transport = new TStreamTransport(stream, null);
		else
			transport.InputStream = stream;
		
		if(protocol == null)
			protocol = new TCompactProtocol(transport);
		else
		{
			protocol.reset();
			protocol.Transport = transport;
		}
		
		protocol.ReadStructBegin();
		while (true)
		{
			fieldReader = protocol.ReadFieldBegin();
			if (fieldReader.Type == TType.Stop) { 
				break;
			}
			
			switch (fieldReader.ID)
			{
			case 1:
				if (fieldReader.Type == TType.Map) {
					if(context == null)
						context = new ADAGEEventInfoDictionary();
					else
						context.Clear();
					
					TMap _map17 = protocol.ReadMapBegin();
					for( int _i18 = 0; _i18 < _map17.Count; ++_i18)
					{
						tempKey = protocol.ReadString();
						tempValue = new ADAGEEventInfo();
						tempValue.Read(fieldReader, protocol);
						context[tempKey] = tempValue;
					}
					protocol.ReadMapEnd();
				} else { 
					TProtocolUtil.Skip(protocol, fieldReader.Type);
				}
				break;
			case 2:
				if (fieldReader.Type == TType.Struct) {
					if(events == null)
						events = new ADAGEEventInfoDictionary();
					else
						events.Clear();
					
					TMap _map17 = protocol.ReadMapBegin();
					for( int _i18 = 0; _i18 < _map17.Count; ++_i18)
					{
						tempKey = protocol.ReadString();
						tempValue = new ADAGEEventInfo();
						tempValue.Read(fieldReader, protocol);
						events[tempKey] = tempValue;
					}
					protocol.ReadMapEnd();
				} else { 
					TProtocolUtil.Skip(protocol, fieldReader.Type);
				}
				break;
			default: 
				TProtocolUtil.Skip(protocol, fieldReader.Type);
				break;
			}
			
			protocol.ReadFieldEnd();
		}
		protocol.ReadStructEnd();		
	}

	public void AddContext(string newContext, ADAGEEventInfo info)
	{
		dirty = true;
		if(context == null)
			context = new ADAGEEventInfoDictionary();
		context[newContext] = info;
	}

	public void RemoveContext(string badContext)
	{
		if(context == null)
			context = new ADAGEEventInfoDictionary();

		if(context.ContainsKey(badContext))
		{
			context.Remove(badContext);
			dirty = true;
		}
	}

	public void AddEvent(string newEvent, ADAGEEventInfo info)
	{
		dirty = true;
		if(events == null)
			events = new ADAGEEventInfoDictionary();

		if(!events.ContainsKey(newEvent))
			events.Add(newEvent, info);
		else
			events[newEvent] = info;
	}

	public void RemoveEvent(string badEvent)
	{
		if(events == null)
			events = new ADAGEEventInfoDictionary();
		
		if(events.ContainsKey(badEvent))
		{
			events.Remove(badEvent);
			dirty = true;
		}
	}	

	public bool Validate(ADAGEData data)
	{
		string type = data.GetType().ToString();
		ADAGEEventInfo dataInfo = null;
		if(events != null && events.ContainsKey(type))
			dataInfo = events[type];
		else if(context != null && context.ContainsKey(type))
			dataInfo = context[type];

		if(dataInfo == null)
			return false;

		FieldInfo field;
		PropertyInfo property;

		foreach(KeyValuePair<string, ADAGEDataPropertyInfo> prop in dataInfo.properties)
		{
			field = data.GetType().GetField(prop.Key);
			if(field != null)
			{
				if(!prop.Value.IsValid(field.GetValue(data)))
					return false;
			}
			else
			{
				property = data.GetType().GetProperty(prop.Key);
				if(property == null)
					return false; //it's neither and shouldn't be here

				if(!prop.Value.IsValid(property.GetValue(data, null)))
					return false;
			}
		}

		return true;
	}
}

[System.Serializable]
public class ADAGEContextInfo
{
	public string name;
	public bool selected;

	public ADAGEContextInfo(string name)
	{
		this.name = name;
		selected = false;
	}
}

[System.Serializable]
public class ADAGEDataPropertyInfo
{             
	public static ADAGEDataPropertyInfo Build(Type propertyType)
	{
		if(propertyType.IsArray)
		{
			return new ADAGEArrayInfo(propertyType.GetElementType());
		}
		else if(propertyType == typeof(int))
		{
			return new ADAGEIntegerInfo();
		}
		else if(propertyType == typeof(float))
		{
			return new ADAGEFloatInfo();
		}
		else if(propertyType == typeof(bool))
		{
			return new ADAGEBooleanInfo();
		}
		else
		{
			return new ADAGEStringInfo();
		}
	}

	[SerializeField]
	public string type;

	[LitJson.SkipSerialization]
	public System.Type Type
	{
		get
		{
			return m_type;
		}

		set
		{
			m_type = value;
			type = m_type.ToString();
		}
	}
	
	#if UNITY_EDITOR
		[LitJson.SkipSerialization]
		protected bool showing = false;
	#endif

	[LitJson.SkipSerialization]
	private System.Type m_type;

	public virtual void DrawLabel(string name)
	{
		#if UNITY_EDITOR
		showing = EditorGUILayout.Foldout(showing, name + " : " + Type.ToString());

		if(showing)
		{
			DrawContents();
		}
		#endif // UNITY_EDITOR
	}

	public virtual void DrawContents()
	{
		#if UNITY_EDITOR
		EditorGUILayout.LabelField("There are no properties to define.");
		#endif // UNITY_EDITOR
	}

	
	public void Write(TField writer, TCompactProtocol protocol)
	{
		protocol.WriteStructBegin(new TStruct(GetType().ToString()));
		
		writer.Name = "type";
		writer.Type = TType.String;
		writer.ID = 1;
		protocol.WriteFieldBegin(writer);
		protocol.WriteString(type);
		protocol.WriteFieldEnd();
		
		WriteFields(writer, protocol);

		protocol.WriteFieldStop();
		protocol.WriteStructEnd();
	}	

	public bool Read(TField reader, TCompactProtocol protocol)
	{
		return ReadFields(reader, protocol);
	}

	protected virtual void WriteFields(TField writer, TCompactProtocol protocol){}	
	protected virtual bool ReadFields(TField reader, TCompactProtocol protocol){return false;}

	public virtual bool IsValid(object obj){return true;}
}

[System.Serializable]
public class ADAGEBooleanInfo : ADAGEDataPropertyInfo 
{
	public ADAGEBooleanInfo()
	{
		this.Type = typeof(bool);
	}
}

[System.Serializable]
public class ADAGEStringInfo : ADAGEDataPropertyInfo
{
	[SerializeField]
	public List<string> acceptableValues = new List<string>();

	private int acceptableValuesCount
	{
		get
		{
			if(acceptableValues != null)
				return acceptableValues.Count;
			return 0;
		}

		set
		{
			int size = value;

			if(size < 0)
				size = 0;
			else if(size > 9999)
				size = 9999;

			if(acceptableValues == null)
				acceptableValues = new List<string>(size);
			else
			{
				if(acceptableValues.Count < size)
				{
					while(acceptableValues.Count < size)
					{
						acceptableValues.Add ("");
					}
				}
				else if(acceptableValues.Count > size)
				{
					while(acceptableValues.Count > size)
					{
						acceptableValues.RemoveAt(acceptableValues.Count - 1);
					}
				}
			}
		}
	}

	[LitJson.SkipSerialization]
	string editingValue;

	[LitJson.SkipSerialization]
	string lastFocusedControl;
	
#if UNITY_EDITOR
	[LitJson.SkipSerialization]
	private bool showingValues = false;
#endif

	public ADAGEStringInfo()
	{
		this.Type = typeof(string);
	}

	public override void DrawContents()
	{
		#if UNITY_EDITOR
		{
			int currentIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;

			showingValues = EditorGUILayout.Foldout(showingValues, "Possible Values");

			if(showingValues)
			{
				EditorGUI.indentLevel++;
				acceptableValuesCount = EditorGUILayout.IntField("Size", acceptableValuesCount, GUILayout.ExpandWidth(false));

				for(int i = 0; i < acceptableValues.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					{
						acceptableValues[i] = EditorGUILayout.TextField(string.Format("Element {0}", i), acceptableValues[i], GUILayout.ExpandWidth(false)); 
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUI.indentLevel = currentIndent;
		}
		#endif // UNITY_EDITOR
	}

	public override bool IsValid (object obj)
	{
		if(acceptableValues != null)
			return acceptableValues.Contains(obj as String);

		return true;
	}
}

[System.Serializable]
public class ADAGEArrayInfo : ADAGEDataPropertyInfo
{
	[SerializeField]
	public int maxSize = -1;

	public ADAGEDataPropertyInfo elementInfo;

	public ADAGEArrayInfo(Type elementType)
	{
		Type = elementType;
		elementInfo = ADAGEDataPropertyInfo.Build(elementType);
	}

	public override void DrawLabel(string name)
	{
		#if UNITY_EDITOR
		showing = EditorGUILayout.Foldout(showing, name + " : " + Type.ToString() + "[]");
		
		if(showing)
		{
			DrawContents();
		}
		#endif // UNITY_EDITOR
	}

	public override void DrawContents()
	{
		#if UNITY_EDITOR
		
			EditorGUI.indentLevel++;
				maxSize = EditorGUILayout.IntField("Max Size", maxSize, GUILayout.ExpandWidth(false));
				maxSize = Mathf.Clamp(maxSize, -1, int.MaxValue);
			EditorGUI.indentLevel--;
			
			elementInfo.DrawContents();

		#endif // UNITY_EDITOR
	}
	
	public override bool IsValid(object obj)
	{
		int count = 0;
		IEnumerable enumerable = obj as IEnumerable;
		if (enumerable != null)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				count++;
			}
		}
		return count <= maxSize;
	}
}

[System.Serializable]
public class ADAGEFloatInfo : ADAGEDataPropertyInfo
{
	[SerializeField]
	public float minValue = 0.0f;
	
	[SerializeField]
	public float maxValue = 0.0f;
	
	public ADAGEFloatInfo()
	{
		Type = typeof(float);
	}
	
	public override void DrawContents()
	{
		#if UNITY_EDITOR
		{
			int currentIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(150f));
				{
					minValue = Mathf.Clamp(EditorGUILayout.FloatField("Minimum", minValue, GUILayout.ExpandWidth(false)), float.MinValue, float.MaxValue);
					maxValue = Mathf.Clamp(EditorGUILayout.FloatField("Maximum", maxValue, GUILayout.ExpandWidth(false)), float.MinValue, float.MaxValue);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUI.indentLevel = currentIndent;
		}
		#endif // UNITY_EDITOR
	}
	
	public override bool IsValid(object obj)
	{
		return ((float)obj >= minValue && (float)obj <= maxValue);
	}
}

[System.Serializable]
public class ADAGEIntegerInfo : ADAGEDataPropertyInfo
{
	[SerializeField]
	public int minValue = 0;

	[SerializeField]
	public int maxValue = 0;
	
	public ADAGEIntegerInfo()
	{
		Type = typeof(int);
	}
	
	public override void DrawContents()
	{
		#if UNITY_EDITOR
		{
			int currentIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(150f));
				{
					minValue = EditorGUILayout.IntField("Minimum", minValue, GUILayout.ExpandWidth(false));
					maxValue = EditorGUILayout.IntField("Maximum", maxValue, GUILayout.ExpandWidth(false));

					Mathf.Clamp(minValue, int.MinValue, int.MaxValue);
					Mathf.Clamp(maxValue, int.MinValue, int.MaxValue);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUI.indentLevel = currentIndent;
		}
		#endif // UNITY_EDITOR
	}

	public override bool IsValid(object obj)
	{
		return ((int)obj >= minValue && (int)obj <= maxValue);
	}
}

[System.Serializable]
public class ADAGEDataPropertyInfoDictionary : Dictionary<string, ADAGEDataPropertyInfo>{}

[System.Serializable]
public class ADAGEEventInfo
{
	public static TStruct struc = new TStruct("ADAGEEventInfo");

	[LitJson.SkipSerialization]
	public bool showing = false;

	[LitJson.SkipSerialization]
	public bool selected = false;

	[SerializeField]
	public ADAGEDataPropertyInfoDictionary properties = new ADAGEDataPropertyInfoDictionary();

	public void Write(TField writer, TCompactProtocol protocol)
	{
		protocol.WriteStructBegin(struc);
		
		writer.Name = "properties";
		writer.Type = TType.Map;
		writer.ID = 1;
		protocol.WriteFieldBegin(writer);
		{		
			protocol.WriteMapBegin(new TMap(TType.String, TType.Struct, properties.Count));
			foreach (string _iter44 in properties.Keys)
			{
				protocol.WriteString(_iter44);
				properties[_iter44].Write(writer, protocol);
			}
			protocol.WriteMapEnd();
		}
		protocol.WriteFieldEnd();
		
		protocol.WriteFieldStop();
		protocol.WriteStructEnd();
	}

	private string tempKey;
	private ADAGEDataPropertyInfo tempValue;

	public void Read(TField reader, TCompactProtocol protocol)
	{
		protocol.ReadStructBegin();
		while (true)
		{
			reader = protocol.ReadFieldBegin();
			if (reader.Type == TType.Stop) { 
				break;
			}
			
			switch (reader.ID)
			{
			case 1:
				if (reader.Type == TType.Map) 
				{
					if(properties == null)
						properties = new ADAGEDataPropertyInfoDictionary();
					else
						properties.Clear();

					TMap _map17 = protocol.ReadMapBegin();
					for( int _i18 = 0; _i18 < _map17.Count; ++_i18)
					{
						tempKey = protocol.ReadString();
						tempValue = new ADAGEDataPropertyInfo();
						tempValue.Read(reader, protocol);
						properties[tempKey] = tempValue;
					}
					protocol.ReadMapEnd();
				} 
				else
				{ 
					TProtocolUtil.Skip(protocol, reader.Type);
				}
				break;
			default: 
				TProtocolUtil.Skip(protocol, reader.Type);
				break;
			}
			protocol.ReadFieldEnd();
		}
		protocol.ReadStructEnd();
	}
}