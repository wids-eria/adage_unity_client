using UnityEngine;
using System.Collections;

namespace HTTP.ContentType.Application
{
	public class XWWWFormURLEncoded : Content
	{
		public override void AddHeaderTag(Request request)
		{
			request.AddHeader ("Content-Type", "application/x-www-form-urlencoded");
		}
		
		public override void BuildBody(Request request, object obj)
		{
			request.bytes = System.Text.Encoding.ASCII.GetBytes(obj.ToString());
		}
	} 
}