using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;

// 微信头像下载管理器,以后可以扩展,不仅仅是用于下载微信头像
public class HeadTextureManager : FrameSystem
{
	protected Dictionary<string, HeadLoadInfo> mHeadTextureList = new();        // 正在下载或者已下载的头像信息列表
	public override void destroy()
	{
		foreach (HeadLoadInfo item in mHeadTextureList.Values)
		{
			destroyUnityObject(ref item.mTexture);
		}
		mHeadTextureList.Clear();
		base.destroy();
	}
	public Texture getHead(string openID) { return mHeadTextureList.get(openID)?.mTexture; }
	public void requestLoadTexture(string url, string openID, HeadDownloadCallback doneCallback)
	{
		if (url.isEmpty() || openID.isEmpty())
		{
			doneCallback?.Invoke(null, openID);
			return;
		}
		if (mHeadTextureList.TryGetValue(openID, out HeadLoadInfo info))
		{
			if (info.mURL == url)
			{
				if (doneCallback != null)
				{
					// 已经下载过了,则直接调用回调
					if (info.mState == LOAD_STATE.LOADED)
					{
						doneCallback(info.mTexture, openID);
					}
					// 正在下载,则将回调添加到列表中
					else if (info.mState == LOAD_STATE.LOADING)
					{
						info.mCallbackList.addUnique(doneCallback);
					}
				}
			}
			else
			{
				// 头像链接修改了,则需要重新下载
				if (info.mState == LOAD_STATE.LOADED)
				{
					// 先销毁旧头像
					destroyUnityObject(ref info.mTexture);
					// 下载新头像
					info.mState = LOAD_STATE.LOADING;
					info.mURL = url;
					info.mCallbackList.addNotNull(doneCallback);
					ResourceManager.loadAssetsFromUrl(url, (Texture tex) => { onLoadWechatHead(tex, openID); });
				}
				// 如果头像正在下载,则只能等待头像下载完毕
				else if (info.mState == LOAD_STATE.LOADING)
				{
					info.mCallbackList.addUnique(doneCallback);
				}
			}
		}
		else
		{
			info = mHeadTextureList.add(openID, new());
			info.mOpenID = openID;
			info.mTexture = null;
			info.mState = LOAD_STATE.LOADING;
			info.mURL = url;
			info.mCallbackList.addNotNull(doneCallback);
			ResourceManager.loadAssetsFromUrl(url, (Texture tex) => { onLoadWechatHead(tex, openID); });
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLoadWechatHead(Texture head, string openID)
	{
		HeadLoadInfo info = mHeadTextureList.get(openID);
		if (head != null)
		{
			info.mTexture = head;
			info.mState = LOAD_STATE.LOADED;
		}
		foreach (HeadDownloadCallback callback in info.mCallbackList)
		{
			callback(head, openID);
		}
		info.mCallbackList.Clear();
		// 头像下载失败,删除下载信息,当有再次请求头像时再重新下载
		if (head == null)
		{
			mHeadTextureList.Remove(openID);
		}
	}
}