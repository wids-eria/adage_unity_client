using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Globalization;
using System.Threading;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace HTTP
{
	public abstract class Content
	{
		public abstract void AddHeaderTag(Request request);
		public abstract void BuildBody(Request request, object obj);
	}

	public class HTTPException : Exception
    {
    	public HTTPException (string message) : base(message)
        {
        }
    }
	
	public struct JSON {
		public JSON( string data ) {
			Data = data;
		}
		
		public string	Data;
	}
    
    public enum RequestState {
    	Waiting, Reading, Done  
    }

	public class GetRequest<T> : TypedRequest<T> where T : Content
	{
		public GetRequest(string uri) : base(uri)
		{
			method = "GET";
		}

		protected override string BuildPath ()
		{
			string pathAndQuery;
			string builtQuery = BuildQuery();
			if( uri.Query.Length <= 0 && builtQuery.Length > 0 ) {
				pathAndQuery = uri.AbsolutePath + "?" + builtQuery;
			} else if( uri.Query.Length > 0 && builtQuery.Length > 0 ) {
				pathAndQuery = uri.PathAndQuery + "&" + builtQuery;
			} else {
				pathAndQuery = uri.PathAndQuery;
			}
			return pathAndQuery;
		}
	}

	public class PostRequest<T> : TypedRequest<T> where T : Content
	{
		public PostRequest(string uri) : base(uri)
		{
			method = "POST";			
		}	
				
		protected override string BuildPath ()
		{
			return uri.PathAndQuery;
		}
	}

	public abstract class Request
	{
		public static bool ValidateServerCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			Debug.LogWarning ("SSL Cert Error:" + sslPolicyErrors.ToString ());
			return true;
		}

		static Dictionary<string, string> etags = new Dictionary<string, string> ();

		public string method;
		public string protocol = "HTTP/1.1";
		public Uri uri;
		public bool isDone = false;
		public int maximumRetryCount = 8;
		public bool acceptGzip = true;
		public bool useCache = false;
		public Exception exception = null;
		public RequestState state = RequestState.Waiting;
		public static byte[] EOL = { (byte)'\r', (byte)'\n' };
		public byte[] bytes;
		public Response response = null;

		protected Dictionary<string, List<string>> headers = new Dictionary<string, List<string>> ();
		protected Dictionary<string, string> parameters = new Dictionary<string, string>();
		
		public bool	ProducedError {
			get {
				if( exception != null ) {
					return true;
				}
				
				if( response == null ) {
					return true;
				}
				
				if( response.Text.Length <= 0 ) {
					return true;
				}
				if( response.status < 200 || response.status >= 300 ) {
					return true;
				}
				
				return false;
			}
		}
		
		public string	Error {
			get {
				if( !ProducedError ) {
					return "";
				}
				
				if( exception != null ) {
					return exception.ToString();
				}
				
				if( response.status < 200 || response.status >= 300 ) {
					return string.Format( "Response Error: {0}", response.status );
				}
				
				return "Unknowmn Error Occured";
			}
		}

		public Request (string uri)
		{
			this.uri = new Uri (uri);
		}
		
		public Request (string uri, bool useCache) : this(uri)
		{
			this.useCache = useCache;
		}
		
		public Request (string uri, byte[] bytes) : this(uri, false)
		{
			this.bytes = bytes;
		}

		public Dictionary<string, string> GetParameters()
		{
			return parameters;
		}

		public void AddParameter( string name, string value ) {
			parameters.Add( name, value );
		}
		
		public void AddParameter( string name, object value ) {
			parameters.Add( name, value.ToString() );
		}
		
		public void AddParameters( Dictionary<string, string> parms ) {
			foreach( KeyValuePair<string, string> kv in parms ) {
				parameters.Add( kv.Key, kv.Value );
			}
		}
		
		public void AddParameters( Dictionary<string, object> parms ) {
			foreach( KeyValuePair<string, object> kv in parms ) {
				parameters.Add( kv.Key, kv.Value.ToString() );
			}
		}
		
		public void SetParameter( string name, string value ) {
			parameters.Clear();
			AddParameter( name, value );
		}
		
		public void SetParameters( Dictionary<string, string> parms ) {
			parameters.Clear();
			AddParameters( parms );
		} 

		public void AddHeader(string name, string value)
		{
			name = name.ToLower().Trim();
			value = value.Trim();
			if(!headers.ContainsKey(name)) {
				headers[name] = new List<string>();
			}
			headers[name].Add(value);
		}
		
		public string GetHeader (string name)
		{
			name = name.ToLower().Trim();
			if (!headers.ContainsKey(name)) {
				return "";
			}
			return headers[name][0];
		}
		
		public List<string> GetHeaders(string name)
		{
			name = name.ToLower ().Trim();
			if( !headers.ContainsKey(name) ) {
				headers[name] = new List<string>();
			}
			return headers[name];
		}
		
		public void SetHeader(string name, string value)
		{
			name = name.ToLower().Trim();
			value = value.Trim();
			if( !headers.ContainsKey(name) ) {
				headers[name] = new List<string>();
			}
			headers[name].Clear ();
			headers[name].Add(value);
		}

		public void Send ()
		{
			isDone = false;
			state = RequestState.Waiting;
			if (acceptGzip) {
				SetHeader( "Accept-Encoding", "gzip" );
			}
			//ThreadPool.QueueUserWorkItem (new WaitCallback (delegate(object t) {
			try {
				var retry = 0;
				while (++retry < maximumRetryCount) {
					if (useCache) {
						string etag = "";
						if (etags.TryGetValue (uri.AbsoluteUri, out etag)) {
							SetHeader( "If-None-Match", etag );
						}
					}
					SetHeader("Host", uri.Host);

					/*Debug.Log (uri.Host);
					IPAddress[] addresses = Dns.GetHostAddresses(uri.Host);
					foreach (IPAddress theaddress in addresses)
					{
						Debug.Log(theaddress.ToString());
					}*/

					var client = new TcpClient ();
					client.Connect (uri.Host, uri.Port);

					using (var stream = client.GetStream ()) {
						var ostream = stream as Stream;
						if(uri.Scheme.ToLower() == "https") {
							ostream = new SslStream (stream, false, new RemoteCertificateValidationCallback (ValidateServerCertificate));
							try {
								var ssl = ostream as SslStream;
								ssl.AuthenticateAsClient (uri.Host);
							} catch (Exception e) {
								Debug.LogError("Exception: " + e.Message);
								return;
							}
						}
						WriteToStream (ostream);
						response = new Response ();
						state = RequestState.Reading;
						response.ReadFromStream(ostream);
					}
					client.Close ();
					switch (response.status) {
					case 307:
					case 302:
					case 301:
						uri = new Uri( response.GetHeader("Location") );
						continue;
					default:
						retry = maximumRetryCount;
						break;
					}
				}
				if (useCache) {
					string etag = response.GetHeader("etag");
					if (etag.Length > 0) {
						etags[uri.AbsoluteUri] = etag;
					}
				}
			} catch(Exception e) {
				Console.WriteLine("Unhandled Exception, aborting request.");
				Console.WriteLine(e);
				exception = e;
				//GLS BEGIN - ams
				//response = null;
				Debug.Log("Unhandled Exception, aborting request: " + e.Message);
				response = new Response();
				response.message = e.Message;
				response.status = 0;
				
				//GLS END
			}
			state = RequestState.Done;
			isDone = true;
			//	}));
		}
		
		protected string pathAndQuery;
		void WriteToStream( Stream outputStream )
		{
			BinaryWriter stream = new BinaryWriter( outputStream );
			
			pathAndQuery = BuildPath();
			
			stream.Write( ASCIIEncoding.ASCII.GetBytes(method.ToUpper() + " " + pathAndQuery + " " + protocol) );
			stream.Write( EOL );
			foreach( string name in headers.Keys ) {
				foreach( string value in headers[name] ) {
					stream.Write( ASCIIEncoding.ASCII.GetBytes(name) );
					stream.Write(':');
					stream.Write( ASCIIEncoding.ASCII.GetBytes(value) );
					stream.Write( EOL );
				}
			}
			
			//bytes = BuildBody();

			if( bytes != null && bytes.Length > 0 ) {
				if( GetHeader("Content-Length") == "" ) {
					stream.Write( ASCIIEncoding.ASCII.GetBytes("content-length: " + bytes.Length.ToString()) );
					stream.Write( EOL );
					stream.Write( EOL );
				}
				stream.Write( bytes );
			} else {
				stream.Write( EOL );
			}
		}  
		
		public string BuildQuery() {
			if( parameters.Count <= 0 ) {
				return "";
			}
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (string key in parameters.Keys) {
				sb.AppendFormat("&{0}={1}", key, parameters[key].ToString());
			}
			return string.Format("{0}", sb.ToString().Substring(1));
		} 
		
		protected abstract string BuildPath();

		public abstract void SetBody(object obj);
	}

	public abstract class TypedRequest<T> : Request where T : Content
    {            
		protected Content contentTypeHandler;

		public TypedRequest (string uri) : base(uri)
		{
			SetContentType();
		}

		public TypedRequest (string uri, bool useCache) : base(uri, useCache)
		{
			SetContentType();
		}

		public TypedRequest (string uri, byte[] bytes) : base(uri, bytes)
		{
			SetContentType();
		}

		private void SetContentType()
		{
			SetContentType(Activator.CreateInstance<T>());
		}

		private void SetContentType(T obj)
		{
			contentTypeHandler = obj;
			contentTypeHandler.AddHeaderTag(this);
		}

		public override void SetBody(object obj)
		{
			contentTypeHandler.BuildBody(this, obj);
		}
    }   
}