using UnityEngine;
using System.Collections;
using System;

public static class WWWX : System.Object
{
	private static System.Text.UTF8Encoding m_utf8 = new System.Text.UTF8Encoding();
	
	[Obsolete]
	public static string AppendParametersToUrl(string url, System.Collections.Generic.Dictionary<string, string> parameters)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		foreach (string key in parameters.Keys) {
			sb.AppendFormat("&{0}={1}", key, parameters[key]);
		}
		return string.Format("{0}?{1}", url, sb.ToString().Substring(1));
	}
	
	[Obsolete]
	public static WWW Get(string url, System.Collections.Generic.Dictionary<string, string> parameters)
	{
		return new WWW(AppendParametersToUrl(url, parameters) );
	}
		
	[Obsolete]
	public static WWW Post(string url, System.Collections.Generic.Dictionary<string, string> parameters)
	{		
		WWWForm post = new WWWForm();
		foreach (string header in parameters.Keys) {
			post.AddField(header, parameters[header].ToString());
		}
		return new WWW(url, post);
	}	
	
	[Obsolete]
	public static WWW Post(string url, string json, System.Collections.Generic.Dictionary<string, string> otherParameters)
	{
		Hashtable postHeader = new Hashtable();
		postHeader.Add("Content-Type", "application/json");
		return new WWW(AppendParametersToUrl(url, otherParameters), m_utf8.GetBytes(json), postHeader);
	}
	
	[Obsolete]
	public static WWW Put(string url, System.Collections.Generic.Dictionary<string, string> parameters)
	{
		Hashtable putHeader = new Hashtable();
		putHeader.Add("Content-Type", "application/json");
		putHeader.Add("X-HTTP-Method-Override", "PUT");
		return new WWW(AppendParametersToUrl(url, parameters), m_utf8.GetBytes("{}"), putHeader);
	}
	
	[Obsolete]
	public static WWW Put(string url, string json, System.Collections.Generic.Dictionary<string, string> otherParameters)
	{
		Hashtable putHeader = new Hashtable();
		putHeader.Add("Content-Type", "application/json");
		putHeader.Add("X-HTTP-Method-Override", "PUT");
		return new WWW(AppendParametersToUrl(url, otherParameters), m_utf8.GetBytes(json), putHeader);
	}
	
	[Obsolete]
	public static WWW Delete(string url, string json, System.Collections.Generic.Dictionary<string, string> otherParameters)
	{
		Hashtable header = new Hashtable();
		header.Add("Content-Type", "application/json");
		header.Add("X-HTTP-Method-Override", "DELETE");
		return new WWW(AppendParametersToUrl(url, otherParameters), m_utf8.GetBytes(json), header);
	}
	
	[Obsolete]
	public static WWW Delete(string url, System.Collections.Generic.Dictionary<string, string> otherParameters)
	{
		return Delete( url, "[\"making unity play nice\"]", otherParameters );
	}
	
	[Obsolete]
	private static System.Text.RegularExpressions.Regex m_matchErrorCode = new System.Text.RegularExpressions.Regex("^[0-9]+");
	
	[Obsolete]
	public static int GetErrorCode(string wwwError)
	{
		try {
			return int.Parse(m_matchErrorCode.Match(wwwError).Value);
		}
		catch {
			return -1;
		}
	}
}