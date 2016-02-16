using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ADAGEResponse
{
	public string text;
	
	public ADAGEResponse()
	{
		this.text = "";
	}
	
	public ADAGEResponse(string text)
	{
		this.text = text;
	}
}

public class ADAGEErrorResponse : ADAGEResponse
{
	public string error
	{
		get
		{
			if(errors != null && errors.Count > 0)
				return errors[0];
			else
				return "";
		}

		set
		{
			if(errors == null)
				errors = new List<string>();
			errors.Add(value);
		}
	}
	public List<string> errors;
	
	public ADAGEErrorResponse()
	{
		errors = new List<string>();
	}
	
	public ADAGEErrorResponse(string errorText) : this()
	{
		errors.Add(errorText);
	}
}

public class ADAGEConnectionError : ADAGEErrorResponse
{
	public ADAGEConnectionError(string errorText) : base(errorText){}
}

public class ADAGEHostError : ADAGEConnectionError
{
	public ADAGEHostError(string errorText) : base(errorText){}
}

public class ADAGEAccessTokenResponse : ADAGEResponse
{
	public string access_token;
}

public class ADAGEUserResponse : ADAGEResponse
{
	public string provider;
	public string uid;
	public string player_name;
	public string email;
	public bool guest;
}

public class ADAGEJsonFileResponse: ADAGEResponse
{
	public string json;
}