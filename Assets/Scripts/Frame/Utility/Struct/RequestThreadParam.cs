using LitJson;
using System;
using System.Net;
using System.Threading;

public struct RequestThreadParam
{
	public HttpWebRequest mRequest;
	public Action<string, object> mCallback;
	public Thread mThread;
	public byte[] mByteArray;
	public object mUserData;
	public string mFullURL;
	public bool mLogError;
}