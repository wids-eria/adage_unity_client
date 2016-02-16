using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class WorkerPool 
{	
	private BlockingQueue jobs;
	public Queue completedJobs;
	private List<Thread> workers;
	
	private bool Stop;
		
	public WorkerPool()
	{
		Init(1);
	}
	
	public WorkerPool(int WorkerNum)
	{
		Init(WorkerNum);
	}
	
	private void Init(int WorkerNum)
	{
		jobs = new BlockingQueue();
		completedJobs = new Queue();
		workers = new List<Thread>();
		
		Stop = false;
		for (int i=0; i<WorkerNum; i++){
			Thread worker = new Thread(delegate(){
				while(!Stop)
				{
					try
					{
						Job job = jobs.Dequeue() as Job;
						job.Main(this);
					}
					catch(Exception e)
					{
						if(e.GetType() != typeof(System.Threading.ThreadAbortException))
							Debug.Log(e);	
					}
				}
			});
			worker.Start();
			workers.Add(worker);
		}			
	}
	
	public void FireWorkers()
	{
		Stop = true;
	}
		
	public void AddJob(Job newJob)
	{
		jobs.Enqueue(newJob);
	}
	
	public List<Job> GetRemainingJobs()
	{
		Job[] leftovers = jobs.ToArray() as Job[];
		
		List<Job> output;
		if(leftovers != null && leftovers.Length > 0)
			output = new List<Job>(leftovers);
		else
			output = new List<Job>();
		
		return output;	
	}
	
	public void CompleteJob(Job job)
	{
		Queue.Synchronized(completedJobs).Enqueue(job);
	}
	
	public bool jobsComplete()
	{
		return (Queue.Synchronized(completedJobs).Count > 0);
	}
}
