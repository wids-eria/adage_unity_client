using UnityEngine;
using System.Collections;
using LitJson;

namespace HTTP.ContentType.Application
{
	public class Json : Content
	{
		public override void AddHeaderTag(Request request)
		{
			request.AddHeader("Content-Type", "application/json");
		}
		
		public override void BuildBody(Request request, object obj)
		{
			request.bytes = System.Text.Encoding.ASCII.GetBytes(JsonMapper.ToJson(obj));
		}
	} 
}