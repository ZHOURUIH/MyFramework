using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class myNGUISpriteAnim : myNGUISprite, IUIAnimation
{
	private Dictionary<string, UISpriteData> mTempSpriteDataList;
	protected List<TextureAnimCallBack> mPlayEndCallback;  // 一个序列播放完时的回调函数,只在非循环播放状态下有效
	protected List<TextureAnimCallBack> mPlayingCallback;  // 一个序列正在播放时的回调函数
	protected List<string> mTextureNameList;
	protected List<Vector2> mTexturePosList;
	protected AnimControl mControl;
	protected string mTextureSetName;
	protected bool mUseTextureSize;
	public myNGUISpriteAnim()
	{
		mControl = new AnimControl();
		mTextureNameList = new List<string>();
		mPlayEndCallback = new List<TextureAnimCallBack>();
		mPlayingCallback = new List<TextureAnimCallBack>();
		mTempSpriteDataList = new Dictionary<string, UISpriteData>();
		mUseTextureSize = false;
	}
	public override void init()
	{
		base.init();
		string spriteName = getSpriteName();
		if (!isEmpty(spriteName))
		{
			int index = spriteName.LastIndexOf('_');
			if (index >= 0)
			{
				string textureSetName = spriteName.Substring(0, index);
				setTextureSet(textureSetName);
			}
		}
		mControl.setObject(this);
		mControl.setPlayEndCallback(onPlayEnd);
		mControl.setPlayingCallback(onPlaying);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTextureNameList.Count == 0)
		{
			setSpriteName(EMPTY_STRING, false);
		}
		mControl.update(elapsedTime);
	}
	public override void setAtlas(UIAtlas atlas, bool clearSprite = false)
	{
		if(atlas == getAtlas())
		{
			return;
		}
		// 改变图集时先停止播放
		stop();
		base.setAtlas(atlas, clearSprite);
		// 图集改变后清空当前序列帧列表
		setTextureSet(EMPTY_STRING);
	}
	public string getTextureSet() { return mTextureSetName; }
	public int getTextureFrameCount() { return mTextureNameList.Count; }
	public void setUseTextureSize(bool useSize){mUseTextureSize = useSize;}
	public void setTexturePosList(List<Vector2> posList) { mTexturePosList = posList; }
	public List<Vector2> getTexturePosList() { return mTexturePosList; }
	public void setTextureSet(string textureSetName)
	{
		if (mTextureSetName != textureSetName)
		{
			setTextureSet(textureSetName, EMPTY_STRING);
		}
	}
	public void setTextureSet(string textureSetName, string subPath)
	{
		if(mTextureSetName == textureSetName)
		{
			return;
		}
		mTextureNameList.Clear();
		mTextureSetName = textureSetName;
		if (mSprite != null && mSprite.atlas != null && !isEmpty(mTextureSetName))
		{
			mTempSpriteDataList.Clear();
			var sprites = mSprite.atlas.spriteList;
			int count = sprites.Count;
			for (int i = 0; i < count; ++i)
			{
				if (startWith(sprites[i].name, mTextureSetName))
				{
					mTempSpriteDataList.Add(sprites[i].name, sprites[i]);
				}
			}
			int index = 0;
			while(true)
			{
				string name = mTextureSetName + "_" + intToString(index++);
				if (!mTempSpriteDataList.ContainsKey(name))
				{
					break;
				}
				mTextureNameList.Add(name);
			}
			if(getTextureFrameCount() == 0)
			{
				logError("invalid sprite anim! atlas : " + mSprite.atlas.name + ", anim set : " + textureSetName);
			}
		}
		mControl.setFrameCount(getTextureFrameCount());
	}
	public LOOP_MODE getLoop() { return mControl.getLoop(); }
	public float getInterval() { return mControl.getInterval(); }
	public float getSpeed() { return mControl.getSpeed(); }
	public int getStartIndex() { return mControl.getStartIndex(); }
	public float getPlayedTime() { return mControl.getPlayedTime(); }
	public float getLength() { return mControl.getLength(); }
	public PLAY_STATE getPlayState() { return mControl.getPlayState(); }
	public bool getPlayDirection() { return mControl.getPlayDirection(); }
	public int getEndIndex() { return mControl.getEndIndex(); }
	public bool isAutoHide() { return mControl.getAutoResetIndex(); }
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
	public void addPlayEndCallback(TextureAnimCallBack callback, bool clear = true)
	{
		if (clear)
		{
			List<TextureAnimCallBack> tempList = newList(out tempList);
			tempList.AddRange(mPlayEndCallback);
			mPlayEndCallback.Clear();
			// 如果回调函数当前不为空,则是中断了更新
			foreach (var item in tempList)
			{
				item(this, true);
			}
			destroyList(tempList);
		}
		mPlayEndCallback.Add(callback);
	}
	public void addPlayingCallback(TextureAnimCallBack callback)
	{
		mPlayingCallback.Add(callback);
	}
	//--------------------------------------------------------------------------------------------------------
	protected void onPlaying(AnimControl control, int frame, bool isPlaying)
	{
		if(mControl.getCurFrameIndex() >= mTextureNameList.Count)
		{
			return;
		}
		setSpriteName(mTextureNameList[mControl.getCurFrameIndex()], mUseTextureSize);
		if (mTexturePosList != null && mTexturePosList.Count > 0)
		{
			int positionIndex = (int)(frame / (float)mTextureNameList.Count * mTexturePosList.Count + 0.5f);
			Vector3 texPos = mTexturePosList[positionIndex];
			setLocalPosition(ref texPos);
		}
		int count = mPlayingCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mPlayingCallback[i](this, false);
		}
	}
	protected void onPlayEnd(AnimControl control, bool callback, bool isBreak)
	{
		// 正常播放完毕后根据是否重置下标来判断是否自动隐藏
		if (!isBreak && mControl.getAutoResetIndex())
		{
			setActive(false);
		}
		if(callback)
		{
			List<TextureAnimCallBack> tempList = newList(out tempList);
			tempList.AddRange(mPlayEndCallback);
			mPlayEndCallback.Clear();
			foreach (var item in tempList)
			{
				item(this, isBreak);
			}
			destroyList(tempList);
		}
		else
		{
			mPlayEndCallback.Clear();
		}
	}
}

#endif