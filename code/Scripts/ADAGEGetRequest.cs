using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using LitJson;

public class ADAGEGame
{
	public class ADAGEGameVersionInfo
	{
		public string name = "";
		public string token = "";
	}

	public string game = "";
	public List<ADAGEGameVersionInfo> versions;
}

public class ADAGEGetUserRequest : ADAGEGetRequest
{
	public string app_token = "";

	public ADAGEGetUserRequest(string adageId, string endPoint) : base(string.Format("/users/{0}{1}", adageId, endPoint)){}

	public ADAGEGetUserRequest(string adageId, string endPoint, ADAGEDataReceived OnComplete) : base(string.Format("/users/{0}{1}", adageId, endPoint), OnComplete){}
}

public class ADAGEGetRequest 
{	
	private string target;
	private ADAGEDataReceived OnComplete;
	
	public ADAGEGetRequest(string targetURL) : this(targetURL, null){}
		
	public ADAGEGetRequest(string targetURL, ADAGEDataReceived OnComplete)
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

public class ADAGEGetDataJob : ADAGEGetRequestJob<HTTP.ContentType.Application.JsonRequest>
{
	public ADAGEGetRequest data;
	public string accessToken;

	public ADAGEGetDataJob(ADAGEGetRequest request, string token) : base(request.GetTargetURL())
	{
		this.data = request;
		this.accessToken = token;		
	}

	protected override void BuildRequest()
	{
		base.BuildRequest();
		
		request.AddHeader("Authorization", "Bearer " + accessToken);

		request = data.AddRequestInfo(request);
	}
}

public class ADAGEGetUserStatsRequest : ADAGEGetRequest
{
    public ADAGEGetUserStatsRequest(string access_token, ADAGEDataReceived callback) : base("/stats/get_stats.json?access_token=" + access_token, callback)
    {

    }
}

public class ADAGEGetUserStatRequest : ADAGEGetRequest
{
    public ADAGEGetUserStatRequest(string key, string access_token, ADAGEDataReceived callback) : base("/stats/get_stat.json?key=" + key + "&access_token=" + access_token, callback)
    {

    }
}

public class ADAGESaveUserStatsJob : ADAGEPostRequestJob<HTTP.ContentType.Application.Json>
{
    public string access_token;
    public Dictionary<string, string> stats;

    public ADAGESaveUserStatsJob(string access_token, Dictionary<string, string> data) : base("/stats/save_stats", 0)
    {
        this.access_token = access_token;
        this.stats = data;
    }

	protected override void BuildRequest()
    {
		base.BuildRequest();

        request.SetHeader("Accept", "application/json");
        request.AddHeader("Authorization", "Bearer " + access_token);

        JsonData statData = new JsonData();

        var keys = this.stats.Keys;

        foreach(string key in keys)
        {
            statData[key] = stats[key];
        }

        JsonData data = new JsonData();
        data["access_token"] = access_token;
        data["stats"] = statData;

        request.SetBody(data);
    }
}
