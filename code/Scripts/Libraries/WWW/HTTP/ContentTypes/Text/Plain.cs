using UnityEngine;
using System.Collections;

namespace HTTP.ContentType.Text
{
	public class Plain : Content
	{		
		public override void AddHeaderTag(Request request)
		{
			request.AddHeader("Content-Type", "text/plain");
		}
		
		public override void BuildBody(Request request, object obj)
		{
			request.bytes = System.Text.Encoding.ASCII.GetBytes(obj.ToString());
		}
	} 
}
