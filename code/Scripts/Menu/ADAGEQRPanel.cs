using UnityEngine;
using System.Collections;
using System.Threading;
using ZXing;

public class ADAGEQRGroup
{
	public string group = "";
}

public class ADAGEQRPanel : ADAGEMenuPanel
{
	private bool loading;
	private bool decoding;
	
	private WebCamTexture cameraTexture;
	private Thread qrThread;
	
	private Rect imagePanelRect;
	private Rect labelRect;
	private Rect backButtonRect;
	
	private GUIStyle buttonStyle;
	
	private Color32[] c;
	private int W, H;
	
	private string qrResult = "";
	private string lastQrResult = "";
	
	public ADAGEQRPanel()
	{
		imagePanelRect = new Rect(362,284,320,240);
		labelRect = new Rect(212,554,290,60);
		backButtonRect = new Rect(418,554,188,60);
		
		loading = true;
		decoding = false;
	}
	
	public override void Draw(MonoBehaviour owner = null)
	{
		if(loading)
		{
			InitStyles();
			loading = false;
		}
		
		if(cameraTexture != null)
			GUI.DrawTexture(imagePanelRect, cameraTexture, ScaleMode.StretchToFill);
		
		GUI.Label(labelRect, "Point your camera at the QR code");
		
		if(!isLocked)
		{
			if(GUI.Button(backButtonRect, "Back", buttonStyle))
			{
				ADAGEMenu.ShowLast();
			}	
		}
	}
	
	public override void OnApplicationQuit()
	{
		KillThread();
	}
	
	public override IEnumerator Update()
	{			
		if(cameraTexture == null)
		{		
			#if UNITY_WEBPLAYER
			yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
			
			if(Application.HasUserAuthorization(UserAuthorization.WebCam))
			{
				InitCamera();
				yield return null;
			}
			else
			{
				ADAGEMenu.ShowPanel<ADAGELoginPanel>();
			}	
			#else
			InitCamera();
			#endif
		}
		else
		{
			if(!cameraTexture.isPlaying)
				cameraTexture.Play();
			else
			{
				if(W != cameraTexture.width)
					W = cameraTexture.width;
				if(H != cameraTexture.height)
					H = cameraTexture.height;
			}
		}
		
		if (c == null)
		{			
			c = cameraTexture.GetPixels32();
		}
		
		if(qrResult != "" && !decoding)
		{
			lastQrResult = qrResult;
			
			try
			{
				ADAGEQRGroup qrData = LitJson.JsonMapper.ToObject<ADAGEQRGroup>(qrResult);
				ADAGEMenu.ShowPanel<ADAGESplashPanel>();
				ADAGE.ConnectWithQR(qrData.group);
			}
			catch
			{
				if (ADAGEMenu.instance != null)
				{
					ADAGEMenu.ShowError(-1, "Invalid QR Code");
					StartThread();
				}
			}
			qrResult = "";
		}
		yield return null;
	}
	
	public override void OnEnable(MonoBehaviour owner = null)
	{
		InitCamera();	
	}
	
	public override void OnDisable(MonoBehaviour owner = null)
	{
		if (cameraTexture != null)
		{
			cameraTexture.Pause();
		}
		
		KillThread();
		ClearFocus();
	}
	
	private void InitCamera()
	{
		cameraTexture = new WebCamTexture();
		cameraTexture.requestedHeight = 480;
		cameraTexture.requestedWidth = 640;
		
		if (cameraTexture != null)
		{
			cameraTexture.Play();
			W = cameraTexture.width;
			H = cameraTexture.height;
		}
		
		StartThread();	
	}
	
	private void StartThread()
	{
		decoding = true;
		
		qrThread = new Thread(DecodeQR);
		qrThread.Start();
	}
	
	private void KillThread()
	{
		decoding = false;
		qrThread.Abort();
	}
	
	private void InitStyles()
	{		
		buttonStyle = new GUIStyle(GUI.skin.GetStyle("button"));	
		buttonStyle.fontSize = 22;
		buttonStyle.fontStyle = FontStyle.Bold;
		buttonStyle.alignment = TextAnchor.MiddleCenter;
	}
	
	private void DecodeQR()
	{
		// create a reader with a custom luminance source
		BarcodeReader barcodeReader = new BarcodeReader {AutoRotate = false, TryHarder = false};
		
		while (decoding)
		{		
			try
			{
				// decode the current frame
				if(c != null)
				{
					Result result = barcodeReader.Decode(c, W, H);
					if (result != null)
					{
						if(result.Text != lastQrResult)
						{
							decoding = false;
							qrResult = result.Text;
						}
					}
					
					// Sleep a little bit and set the signal to get the next frame
					Thread.Sleep(200);
					c = null;
				}
			}
			catch(System.Exception e)
			{
				Debug.Log (e.ToString());
			}
		}
	}
}