using System;

public abstract class Job {
	//public int id;
	public Action<Job> OnComplete = null;
	public abstract void Main(WorkerPool boss = null);
}

//T is the type of request you are making
//U is the content type of the request you are making
public abstract class WebJob : Job
{
	public string url {get; set;}
	
	protected HTTP.Request request;

	public WebJob()
	{

	}
}