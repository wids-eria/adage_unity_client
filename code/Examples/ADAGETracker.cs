using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Reflection;

public delegate void ADAGEDataReceived(string json);

public class ADAGETrackerByGroup : ADAGETracker
{
	public string group = "";

	public ADAGETrackerByGroup(string targetURL) : base(targetURL){}
	
	public ADAGETrackerByGroup(string targetURL, bool repeat) : base(targetURL, repeat){}
	
	public ADAGETrackerByGroup(string targetURL, ADAGEDataReceived OnComplete) : base(targetURL, false, OnComplete){}
	
	public ADAGETrackerByGroup(string targetURL, bool repeat, ADAGEDataReceived OnComplete) : base(targetURL, repeat, OnComplete){}
}

public class ADAGESortedTracker : ADAGETracker {
	public ADAGESortedTracker(string targetURL, ADAGEDataReceived OnComplete) : base(targetURL, OnComplete){}
	public string key; //The class to get data from
	public string field_name; //The field to sort by
	public int start; //Int of start pos
	public int limit; //Int of number to retrieve
}

public class ADAGETracker
{
	public string app_token = "";
	public string time_range = "";
	public List<string> events_list = new List<string>();
	public string game_id = "";

	private string target;
	private ADAGEDataReceived OnComplete;

	public ADAGETracker(string targetURL) : this(targetURL, false, null){}

	public ADAGETracker(string targetURL, bool repeat) : this(targetURL, repeat, null){}

	public ADAGETracker(string targetURL, ADAGEDataReceived OnComplete) : this(targetURL, false, OnComplete){}

	public ADAGETracker(string targetURL, bool repeat, ADAGEDataReceived OnComplete)
	{
		this.target = targetURL;
		this.OnComplete = OnComplete;
	}

	public string GetTargetURL()
	{
		return this.target;
	}

	public void Complete(string json)
	{
		if(OnComplete != null)
			OnComplete(json);
	}

	public virtual HTTP.Request AddRequestInfo(HTTP.Request request)
	{
		Dictionary<string, object> requestBody = new Dictionary<string, object>();
		Type currentType = this.GetType();
		
		foreach(FieldInfo field in currentType.GetFields(BindingFlags.Public | BindingFlags.Instance)) 
		{
			object temp = field.GetValue(this);
			if(temp != null)
			{
				if(temp is ICollection)
				{
					requestBody[field.Name] = temp;
				}
				else
				{
					request.AddParameter(field.Name, System.Uri.EscapeDataString(temp.ToString()));
				}
				continue;
			}
			request.AddParameter(field.Name, "");
		}
		
		foreach(PropertyInfo prop in currentType.GetProperties(BindingFlags.Public)) 
		{
			MethodInfo getter = prop.GetGetMethod();
			if(getter != null)
			{
				object temp = getter.Invoke(this,null);
				if(temp != null)
				{
					if(temp is ICollection)
					{
						requestBody[prop.Name] = temp;
					}
					else
					{
						request.AddParameter(prop.Name, temp.ToString());
					}
					continue;
				}
			}
			request.AddParameter(prop.Name, "");
		}

		if(requestBody.Count > 0)
			request.SetBody(requestBody);

		return request;
	}

	public virtual Dictionary<string, object> BuildParameters()
	{
		Dictionary<string, object> output = new Dictionary<string, object>();
		Type currentType = this.GetType();

		foreach(FieldInfo field in currentType.GetFields(BindingFlags.Public | BindingFlags.Instance)) 
		{
			object temp = field.GetValue(this);
			if(temp != null)
			{
				output[field.Name] = temp;
				continue;
			}
			output[field.Name] = "";
		}

		foreach(PropertyInfo prop in currentType.GetProperties(BindingFlags.Public)) 
		{
			MethodInfo getter = prop.GetGetMethod();
			if(getter != null)
			{
				object temp = getter.Invoke(this,null);
				if(temp != null)
				{
					output[prop.Name] = temp.ToString();
					continue;
				}
			}
			output[prop.Name] = "";
		}

		return output;
	}
}

public class ADAGETrackerResult
{
	public List<ADAGEData> data;
}

public class ADAGETrackerJob : ADAGEGetRequestJob<HTTP.ContentType.Application.JsonRequest>
{
	public ADAGETracker data;
	public string accessToken;
	
	public ADAGETrackerJob(ADAGETracker tracker, string token) : base(tracker.GetTargetURL())
	{
		this.data = tracker;
		this.accessToken = token;		
	}

	protected override void BuildRequest()
	{
		base.BuildRequest();

		request.AddHeader("Authorization", "Bearer " + accessToken);

		request = data.AddRequestInfo(request);
	}
}
