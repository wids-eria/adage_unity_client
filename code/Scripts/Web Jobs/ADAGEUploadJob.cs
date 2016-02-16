using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;
using Ionic.Zlib;

public class ADAGEUploadFileJob : ADAGEUploadRequestJob
{
	private string accessToken;
	private string data;

	public string Path{ get; protected set; }
		
	public ADAGEUploadFileJob(string token, string filepath, string text) : base("/data_collector.json", 0)
	{
		this.Path = filepath;
		this.data = text;
		this.accessToken = token;
	}
	
	protected override void BuildRequest()
	{
		base.BuildRequest();

		request.AddHeader("Authorization", "Bearer " + accessToken);
		request.SetBody(data);
	}
}

public class ADAGEUploadJob : ADAGEUploadRequestJob
{
	private ADAGEUploadWrapper data;

	public string accessToken;
		
	public ADAGEUploadJob(ADAGEUploadWrapper data, string token, int localId) : base("/data_collector.json", localId)
	{
		//this.data = data;	
		this.data = new ADAGEUploadWrapper(data);
		this.accessToken = token;	
	}

	protected override void BuildRequest()
	{
		base.BuildRequest();
		
		request.AddHeader("Authorization", "Bearer " + accessToken);

		string json = JsonMapper.ToJson(this.data);
		request.SetBody(json);
	}

	public ADAGEUploadWrapper GetData()
	{
		return data;	
	}
}