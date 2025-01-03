using UnityEngine;
using System.Collections.Generic;

// 微信头像信息
public class HeadLoadInfo
{
	public List<HeadDownloadCallback> mCallbackList = new();	// 回调列表
	public Texture mTexture;									// 头像图片
	public string mOpenID;										// 微信OpenID
	public string mURL;											// 图片链接
	public LOAD_STATE mState;									// 下载状态
}