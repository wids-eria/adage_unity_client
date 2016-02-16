using System;
using UnityEngine;

public abstract class ADAGEException : Exception
{
	public int localID;

	public ADAGEException()
	{
		localID = -1;
	}

	public ADAGEException(int id)
	{
		localID = id;
	}
}

public class ADAGETrackContextException : ADAGEException
{
	public ADAGETrackContextException(int id, Type badType) : base(id)
    {
        string message = string.Format("ADAGE ERROR: Method 'ADAGE.LogData' cannot be used to track progression object '{0}'. Please use the ADAGE.LogContext method", badType.ToString());
		Messenger<int, string>.Broadcast(ADAGE.k_OnError, localID, message);
		throw new Exception(message);
    }	
}

public class ADAGEStartContextException : ADAGEException
{
	public ADAGEStartContextException(int id, string name) : base(id)
    {
		string message = string.Format("ADAGE ERROR: Cannot start tracking the progress of {0} because it is already being tracked", name);
		Messenger<int, string>.Broadcast(ADAGE.k_OnError, localID, message);
		throw new Exception(message);
    }
}

public class ADAGEEndContextException : ADAGEException
{
	public ADAGEEndContextException(int id, string name) : base(id)
    {
		string message = string.Format("ADAGE ERROR: Cannot stop tracking the progress of {0} because it isn't being tracked", name);
		Messenger<int, string>.Broadcast(ADAGE.k_OnError, localID, message);
		throw new Exception(message);
    }
}

