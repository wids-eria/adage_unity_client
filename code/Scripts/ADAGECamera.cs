using UnityEngine;
using System.Collections;

public class ADAGECamera : MonoBehaviour 
{
	public string cameraName;
	
	// Use this for initialization
	void Start () 
	{
		ADAGE.AddCamera(this);
	}
	
	
	public void TakeScreenShot()
	{
			
	}
}
