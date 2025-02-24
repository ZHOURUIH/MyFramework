﻿using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static MathUtility;

// RawImage的序列帧
public class myUGUIRawImageAnim : myUGUIRawImage, IUIAnimation
{
	protected List<TextureAnimCallback> mPlayEndCallback;	// 一个序列播放完时的回调函数,只在非循环播放状态下有效
	protected List<TextureAnimCallback> mPlayingCallback;	// 一个序列正在播放时的回调函数
	protected List<Vector2> mTexturePosList;				// 每一帧的位置偏移列表
	protected List<Texture> mTextureList = new();			// 序列帧列表
	protected OnPlayEndCallback mThisPlayEnd;				// 避免GC的委托
	protected OnPlayingCallback mThisPlaying;				// 避免GC的委托
	protected AnimControl mControl = new();					// 序列帧播放控制器
	protected string mTexturePreName = EMPTY;				// 序列帧图片的前缀名
	protected string mTexturePath = EMPTY;					// 序列帧图片的路径
	protected bool mUseTextureSize;							// 是否使用图片的大小改变当前窗口大小
	public myUGUIRawImageAnim()
	{
		mNeedUpdate = true;
		mThisPlayEnd = onPlayEnd;
		mThisPlaying = onPlaying;
	}
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent<RawImageAnimPath>(out var animPath))
		{
			logError("myUGUIRawImageAnim的节点必须带有RawImageAnimPath组件");
			return;
		}
		setTexturePath(animPath.mTexturePath, animPath.mTextureName, animPath.mImageCount);
		mControl.setObject(this);
		mControl.setPlayEndCallback(mThisPlayEnd);
		mControl.setPlayingCallback(mThisPlaying);
	}
	public override void destroy()
	{
		base.destroy();
		foreach (Texture tex in mTextureList)
		{
			Texture temp = tex;
			mResourceManager.unload(ref temp);
		}
		mTextureList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTextureList.Count == 0)
		{
			setTexture(null, false);
		}
		mControl.update(elapsedTime);
	}
	public string getTextureSet() { return mTexturePath; }
	public int getTextureFrameCount() { return mTextureList.Count; }
	public void setUseTextureSize(bool useSize) { mUseTextureSize = useSize; }
	public void setTexturePosList(List<Vector2> posList) { mTexturePosList = posList; }
	public List<Vector2> getTexturePosList() { return mTexturePosList; }
	// texturePath是GameResource下的相对路径,以/结尾,不支持读取Resources中的资源
	// texturePreName是图片序列的前缀名,不带_
	public void setTexturePath(string texturePath, string texturePreName, int textureCount)
	{
		if (mTexturePath == texturePath && mTexturePreName == texturePreName)
		{
			return;
		}
		mTextureList.Clear();
		mTexturePath = texturePath;
		mTexturePreName = texturePreName;
		string preName = mTexturePath + mTexturePreName + "_";
		for (int i = 0; i < textureCount; ++i)
		{
			mTextureList.Add(mResourceManager.loadGameResource<Texture>(preName + IToS(i) + ".png"));
		}
		mControl.setFrameCount(getTextureFrameCount());
		if (mTextureList.Count == 0)
		{
			logError("无效的图片序列帧! mTexturePath: " + mTexturePath + ", mTexturePreName: " + mTexturePreName);
		}
	}
	public LOOP_MODE getLoop() { return mControl.getLoop(); }
	public float getInterval() { return mControl.getInterval(); }
	public float getSpeed() { return mControl.getSpeed(); }
	public int getStartIndex() { return mControl.getStartIndex(); }
	public PLAY_STATE getPlayState() { return mControl.getPlayState(); }
	public bool getPlayDirection() { return mControl.getPlayDirection(); }
	public int getEndIndex() { return mControl.getEndIndex(); }
	public bool isAutoHide() { return mControl.isAutoResetIndex(); }
	// 获得实际的终止下标,如果是自动获得,则返回最后一张的下标
	public int getRealEndIndex() { return mControl.getRealEndIndex(); }
	public void setLoop(LOOP_MODE loop) { mControl.setLoop(loop); }
	public void setInterval(float interval) { mControl.setInterval(interval); }
	public void setSpeed(float speed) { mControl.setSpeed(speed); }
	public void setPlayDirection(bool direction) { mControl.setPlayDirection(direction); }
	public void setAutoHide(bool autoHide) { mControl.setAutoHide(autoHide); }
	public void setStartIndex(int startIndex) { mControl.setStartIndex(startIndex); }
	public void setEndIndex(int endIndex) { mControl.setEndIndex(endIndex); }
	public void stop(bool resetStartIndex = true, bool callback = true, bool isBreak = true) { mControl.stop(resetStartIndex, callback, isBreak); }
	public void play() { mControl.play(); }
	public void pause() { mControl.pause(); }
	public int getCurFrameIndex() { return mControl.getCurFrameIndex(); }
	public void setCurFrameIndex(int index) { mControl.setCurFrameIndex(index); }
	public void addPlayEndCallback(TextureAnimCallback callback, bool clear = true)
	{
		if (clear && !mPlayEndCallback.isEmpty())
		{
			using var a = new ListScope<TextureAnimCallback>(out var tempList);
			// 如果回调函数当前不为空,则是中断了更新
			foreach (TextureAnimCallback item in tempList.move(mPlayEndCallback))
			{
				item(this, true);
			}
		}
		if (callback != null)
		{
			mPlayEndCallback ??= new();
			mPlayEndCallback.Add(callback);
		}
	}
	public void addPlayingCallback(TextureAnimCallback callback, bool clear = true)
	{
		if (clear)
		{
			mPlayingCallback?.Clear();
		}
		if (callback != null)
		{
			mPlayingCallback ??= new();
			mPlayingCallback.Add(callback);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onPlaying(AnimControl control, int frame, bool isPlaying)
	{
		if (mControl.getCurFrameIndex() >= mTextureList.Count)
		{
			return;
		}
		setTexture(mTextureList[mControl.getCurFrameIndex()], mUseTextureSize);
		if (!mTexturePosList.isEmpty())
		{
			setPosition(mTexturePosList[round(divide(frame, mTextureList.Count * mTexturePosList.Count))]);
		}
		foreach (TextureAnimCallback item in mPlayingCallback.safe())
		{
			item(this, false);
		}
	}
	protected void onPlayEnd(AnimControl control, bool callback, bool isBreak)
	{
		// 正常播放完毕后根据是否重置下标来判断是否自动隐藏
		if (!isBreak && mControl.isAutoResetIndex())
		{
			setActive(false);
		}
		if (mPlayEndCallback.isEmpty())
		{
			return;
		}
		if (callback)
		{
			using var a = new ListScope<TextureAnimCallback>(out var tempList);
			foreach (TextureAnimCallback item in tempList.move(mPlayEndCallback))
			{
				item(this, isBreak);
			}
		}
		else
		{
			mPlayEndCallback.Clear();
		}
	}
}