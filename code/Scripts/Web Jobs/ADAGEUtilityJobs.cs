using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;
using Ionic.Zlib;

//Delegate for games to hook into to get their requested json utility files like save games and configs
public delegate void ADAGEUtilityResponseCallback(bool error, string json_response);


public class ADAGESaveUtilityJob : ADAGEUploadRequestJob
{
	private string data;
	private string app_token;
	public string accessToken;
	
	public ADAGESaveUtilityJob(string endpoint, string save_data, string token, string app_token, int localId) : base(endpoint, localId)
	{
	
		this.data = save_data;
		this.accessToken = token;	
		this.app_token = app_token;
	}
	
	protected override void BuildRequest()
	{
		Debug.Log (this.data);
		base.BuildRequest();
		
		request.AddHeader("Authorization", "Bearer " + accessToken);
		request.AddParameter("app_token", this.app_token);
		request.SetBody(this.data);
	}
	
	public string GetData()
	{
		return data;	
	}
}


public class ADAGELoadUtilityJob : ADAGEGetRequestJob<HTTP.ContentType.Application.JsonRequest>
{	

	private string access_token; 
	private string app_token;
	private ADAGEUtilityResponseCallback callOnComplete;
	
	
	/*public ADAGERequestUserJob(string access_token, int localId) : base("/auth/adage_user.json", localId)
	{
		this.access_token = access_token;
	}	*/
	
	public ADAGELoadUtilityJob(string endpoint, string access_token, string app_token, ADAGEUtilityResponseCallback callback, int localId) : base(endpoint, localId)
	{
		this.access_token = access_token;
		this.callOnComplete = callback;
		this.app_token = app_token;
	}
	
	protected override void BuildRequest()
	{
		base.BuildRequest();
		request.AddParameter("app_token", this.app_token);
		request.AddHeader("Authorization", "Bearer " + access_token);
	}
	
	protected override void HandleResponse()
	{
		response = JsonMapper.ToObject<ADAGEJsonFileResponse>(request.response.Text);
		bool error = (status != 200);
		this.callOnComplete(error, request.response.Text);
	}
}

