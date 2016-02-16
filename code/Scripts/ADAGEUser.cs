using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using System.Linq;

public class ADAGEUser 
{
    [SkipSerialization]
    public event Action OnStatsUpdated;

    [SkipSerialization]
    public static ADAGEUser main
    {
        get
        {
            if(ADAGE.users.Count > 0)
            {
                return ADAGE.users[0];
            }

            return null;
        }
    }

	public ADAGEUser(){
		playerName = "EMPTY";
		username = "EMPTY";
		email = "EMPTY@EMPTY.COM";
		adageId = "EMPTY";
		adageAccessToken = "EMPTY";
		adageRefreshToken = "EMPTY";
		fbAccessToken = "EMPTY";
		facebookId = "EMPTY";
		fbExpiresAt = new DateTime();
		adageExpiresAt = new DateTime();
		guest = false;
		dataWrapper = new ADAGEUploadWrapper();
		localWrapper = new ADAGEUploadWrapper();
		pContext = new ADAGEPositionalContext();
		vContext = new ADAGEVirtualContext(Application.loadedLevelName);
		duration = 0f;
	}
	public string playerName {get; set;} //what to display on the UI
	public string username {get; set; } //what the unique user name is. With FB username can be different from playerName
	public string email {get; set;}
	public string adageId {get; set;}
    public string adageAccessToken { get; set; }
	public string adageRefreshToken { get; set; }
	public string fbAccessToken {get; set; }
	public string facebookId {get; set;}
	public bool guest {get; set;}

	[SkipSerialization]
	public DateTime sessionStart {get; set;}
	[SkipSerialization]
	public float duration {get; set;}

	public DateTime fbExpiresAt {get; set; }
	public DateTime adageExpiresAt {get; set; }

	[SkipSerialization]
	//data buffers for this user
	public ADAGEUploadWrapper		   dataWrapper;
	[SkipSerialization]
	public ADAGEUploadWrapper 		   localWrapper;

	[SkipSerialization]
	public ADAGEPositionalContext pContext;
	[SkipSerialization]
	public ADAGEVirtualContext    vContext;

    public Dictionary<string, string> stats = new Dictionary<string, string>();

	
	//Is this a valid user
	public bool valid()
	{
		return !playerName.Equals("EMPTY") && !adageAccessToken.Equals("EMPTY");	
	}
	
	public void Clear()
	{
		playerName = "EMPTY";
		username = "EMPTY";
		email = "EMPTY@EMPTY.COM";
		adageId = "EMPTY";
		adageAccessToken = "EMPTY";
		adageRefreshToken = "EMPTY";
		fbAccessToken = "EMPTY";
		facebookId = "EMPTY";
		fbExpiresAt = new DateTime();
		adageExpiresAt = new DateTime();
		guest = false;
		duration = 0f;
	}

	public void ClearLocalData()
	{
		localWrapper.Clear();
	}

	public void ClearUploadData()
	{
		dataWrapper.Clear();
	}

	public void StartSession(DateTime adageTime)
	{
		sessionStart = adageTime;
		duration = 0f;
	}

	public DateTime GetLocalTime()
	{
		return sessionStart.AddSeconds(duration);
	}

    /// <summary>
    /// Requests the dictionary of user stats from the ADAGE server.
    /// </summary>
    public void UpdateStats()
    {
        ADAGEGetUserStatsRequest request = new ADAGEGetUserStatsRequest(this.adageAccessToken, this.OnParseStats);

        ADAGE.GetRequest<ADAGEGetUserStatsRequest>(request);
    }

    /// <summary>
    /// Saves the dictionary of user stats back to the server.
    /// </summary>
    public void SaveStats()
    {
        ADAGESaveUserStatsJob job = new ADAGESaveUserStatsJob(this.adageAccessToken, this.stats);

        ADAGE.AddJob(job);
    }

    private void OnParseStats(string json)
    {
        JsonData response = JsonMapper.ToObject(json);

        if(response["errors"] != null)
        {
            if(response["errors"].IsArray && response["errors"].Count > 0)
            {
                Debug.LogError("Stats Update Error: " + response["errors"].ToJson());
            }
        }

        if(response["data"] != null)
        {
            this.stats = JsonMapper.ToObject <Dictionary<string, string>>(response["data"].ToJson());

            if(this.OnStatsUpdated != null)
            {
                this.OnStatsUpdated();
            }
        }
    }

}
