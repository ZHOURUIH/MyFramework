using UnityEngine;
using System.Collections.Generic;

public class HeadLoadInfo
{
	public List<HeadDownloadCallback> mCallbackList;
	public Texture mTexture;
	public string mOpenID;
	public string mURL;
	public LOAD_STATE mState;
	public HeadLoadInfo()
	{
		mCallbackList = new List<HeadDownloadCallback>();
	}
}