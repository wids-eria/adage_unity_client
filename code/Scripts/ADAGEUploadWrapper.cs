using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class ADAGEUploadWrapper
{
	[SkipSerialization]
	public int Count
	{
		get
		{
			if(data == null)
				data = new List<object>();
			
			return data.Count;		
		}
	}
	
	[SkipSerialization]
	public object[] Items
	{
		get
		{
			if(data == null)
				data = new List<object>();
			
			return data.ToArray();					
		}
	}	
	
	public List<object> data;
	
	public ADAGEUploadWrapper()
	{
		data = new List<object>();
	}	
	
	public ADAGEUploadWrapper(ADAGEUploadWrapper copy)
	{
		data = new List<object>();
		
		foreach(object element in copy.Items)
		{
			Add(element);	
		}
	}
	
	public void Add(ADAGEData newData)
	{		
		if(data == null)
			data = new List<object>();
		
		Add(newData.ToJson());
		//if(newData.GetType() == typeof(PollutionState))
		//	Debug.Log (data[data.Count - 1].ToString());
	}
	
	private void Add(object jsonObject)
	{
		if(data.Count == 0 || (data[data.Count - 1] != jsonObject))
		{
			data.Add(jsonObject);
		}
	}
	
	public void Add(ADAGEUploadWrapper copy)
	{
		foreach(string element in copy.Items)
		{
			Add(element);	
		}
	}
	
	public void Clear()
	{
		if(data == null)
			data = new List<object>();
		else
			data.Clear();
	}
}
