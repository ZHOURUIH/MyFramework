using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadLoadInfo
{
	public string mOpenID;
	public string mURL;
	public Texture mTexture;
	public LOAD_STATE mState;
	public List<HeadDownloadCallback> mCallbackList;
	public HeadLoadInfo()
	{
		mCallbackList = new List<HeadDownloadCallback>();
	}
}