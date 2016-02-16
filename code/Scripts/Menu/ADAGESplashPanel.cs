using UnityEngine;
using System.Collections;

public class ADAGESplashPanel : ADAGEMenuPanel
{			
	private Rect drawRect;
	private Texture2D splash;
	
	public ADAGESplashPanel()
	{
		drawRect = new Rect(0f,0f,1024f,769f);
		splash = Resources.Load("Images/GLS_Splash") as Texture2D;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{	
		GUI.DrawTexture(drawRect, splash);
	}
	
	public override IEnumerator Update()
	{
		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		
	}
	
	public override void OnDisable(MonoBehaviour owner = null)
	{
		
	}
}
