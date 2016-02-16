using UnityEngine;
using System.Collections;

namespace HTTP.ContentType.Application
{
	public class JPEG : Content
	{
		public override void AddHeaderTag(Request request)
		{
			request.AddHeader("Content-Type", "image/jpeg");
		}
		
		public override void BuildBody(Request request, object obj)
		{
			
		}
	} 
}

