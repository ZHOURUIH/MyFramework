using System;
using System.Net;
using System.Threading;

public struct RequestThreadParam
{
	public HttpWebRequest mRequest;
	public Action<string> mCallback;
	public Thread mThread;
	public string mFullURL;
}