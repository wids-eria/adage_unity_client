using UnityEngine;
using System.Collections;
using LitJson;

namespace HTTP.ContentType.Application
{
	public class JsonRequest : Content
	{
		public override void AddHeaderTag(Request request)
		{
			request.AddHeader("Content-Type", "application/jsonrequest");
		}

		public override void BuildBody(Request request, object obj)
		{
			request.bytes = System.Text.Encoding.ASCII.GetBytes(JsonMapper.ToJson(obj));
		}
	} 
}
