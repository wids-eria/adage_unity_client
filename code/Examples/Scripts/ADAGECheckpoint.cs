using UnityEngine;
using System.Collections;

public class ADAGECheckpoint : MonoBehaviour
{
	public Transform player;
	
	///Event occurs when Trigger Collider 
	///on this GameObject collides with 
	///another Collider on another GameObject
	void OnTriggerEnter(Collider other)
	{
		if(other.transform == player)
		{
			///Create a new ADAGE object based on the
			///type of event you want to log. In this
			///example, the default ADAGEGameEvent 
			///object is used. You will more than 
			///likely want to create an object that 
			///inherits from an ADAGE base class and 
			///it to log more specific information.
			ADAGEGameEvent newEvent = new ADAGEGameEvent();
			ADAGE.LogData<ADAGEGameEvent>(newEvent);
		}
	}
}
