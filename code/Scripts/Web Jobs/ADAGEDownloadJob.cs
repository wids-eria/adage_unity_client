using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;
using Ionic.Zlib;

/*public class ADAGEDownloadJob<T> : WebJob
{	
	public int Status;
	public new T Output;
	public string RawOutput = "Nothing";
			
	public ADAGEDownloadJob(string url)
	{		
		if(Application.isEditor)
		{
			if(ADAGE.Staging)
			{
				this.url = ADAGE.stagingURL;	
			}
			else
			{
				this.url = ADAGE.developmentURL;	
			}
		}
		else
		{
			this.url = ADAGE.productionURL;	
		}	
		
		this.url += url;
		
		request = new HTTP.Request("Get", this.url);
		request.AddHeader("Content-Type", "application/jsonrequest");
		request.AddParameters(ADAGE.AuthenticationParameters);
	}
	
	public override void Main(WorkerPool boss = null) 
	{			
		if(ADAGE.Online)
		{
			//SendRequest();
			
			//Error Handling - Server constantly sending 404
			Status = request.response.status;
			RawOutput = request.response.Text;
			if(Status != 404)
			{
				RawOutput = request.response.Text;
				Output = JsonMapper.ToObject<T>(RawOutput);
			}	
			else
			{
				//Do Some Error Stuff	
			}
		}
		else
		{
			//not sure	
			Status = 404;
		}
		
		if(boss != null)
			boss.CompleteJob(this);
    }
}*/