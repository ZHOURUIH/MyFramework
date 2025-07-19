using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static MathUtility;

// Image的序列帧
public class myUGUIImageAnim : myUGUIImage, IUIAnimation
{
	protected List<TextureAnimCallback> mPlayEndCallbackList;   // 一个序列播放完时的回调函数,只在非循环播放状态下有效
	protected List<TextureAnimCallback> mPlayingCallbackList;   // 一个序列正在播放时的回调函数
	protected List<Vector2> mTexturePosList;                    // 每一帧的位置偏移列表
	protected List<Sprite> mSpriteList = new();                 // 序列帧图片列表
	protected OnPlayEndCallback mPlayEndCallback;               // 播放完成时的回调
	protected OnPlayingCallback mPlayingCallback;               // 正在播放的回调
	protected AnimControl mControl = new();                     // 序列帧控制器
	protected string mTextureSetName;                           // 序列帧名字
	protected bool mUseTextureSize;                             // 是否使用图片的大小改变当前窗口大小
	protected EFFECT_ALIGN mEffectAlign;                        // 图片的位置对齐方式
	public myUGUIImageAnim()
	{
		mNeedUpdate = true;
		mPlayEndCallback = onPlayEnd;
		mPlayingCallback = onPlaying;
	}
	public override void init()
	{
		base.init();
		string spriteName = getSpriteName();
		if (!spriteName.isEmpty() && spriteName.Contains('_'))
		{
			setTextureSet(spriteName.rangeToLast('_'));
		}
		mControl.setObject(this);
		mControl.setPlayEndCallback(mPlayEndCallback);
		mControl.setPlayingCallback(mPlayingCallback);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isCulled())
		{
			return;
		}
		if (mSpriteList.Count == 0)
		{
			setSpriteName(null);
		}
		mControl.update(elapsedTime);
	}
	public override void setAtlas(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
	{
		if (!force && atlas?.getAtlas() == getAtlas()?.getAtlas())
		{
			return;
		}
		// 改变图集时先停止播放
		stop();
		base.setAtlas(atlas, clearSprite, force);
		// 图集改变后自动设置默认的序列帧
		setTextureSet(null);
	}
	// 设置图集,并且设置将第一张图片设置为要播放的序列帧
	public void setAtlasWithFirstSprite(UGUIAtlasPtr atlas, bool clearSprite = false, bool force = false)
	{
		if (!force && atlas?.getAtlas() == getAtlas()?.getAtlas())
		{
			return;
		}
		// 改变图集时先停止播放
		stop();
		base.setAtlas(atlas, clearSprite, force);
		// 图集改变后自动设置默认的序列帧
		setTextureSet(atlas.getFirstSpriteName().rangeToLast('_'));
	}
	public string getTextureSet() { return mTextureSetName; }
	public int getTextureFrameCount() { return mSpriteList.Count; }
	public void setUseTextureSize(bool useSize) { mUseTextureSize = useSize; }
	public void setTexturePosList(List<Vector2> posList)
	{
		mTexturePosList = posList;
		if (mTexturePosList != null)
		{
			setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
		}
	}
	public List<Vector2> getTexturePosList() { return mTexturePosList; }
	public void setEffectAlign(EFFECT_ALIGN align) { mEffectAlign = align; }
	public void setTextureSet(string textureSetName)
	{
		if (mTextureSetName == textureSetName)
		{
			return;
		}
		mSpriteList.Clear();
		mTextureSetName = textureSetName;
		if (!mTextureSetName.isEmpty())
		{
			int index = 0;
			while (mSpriteList.addNotNull(getSpriteInAtlas(mTextureSetName + "_" + IToS(index++)))){}
			if (getTextureFrameCount() == 0)
			{
				logError("invalid sprite anim! atlas : " + (getAtlas()?.getFilePath() ?? EMPTY) + ", anim set : " + textureSetName);
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
		if (clear && !mPlayEndCallbackList.isEmpty())
		{
			using var a = new ListScope<TextureAnimCallback>(out var tempList);
			// 如果回调函数当前不为空,则是中断了更新
			foreach (TextureAnimCallback item in tempList.move(mPlayEndCallbackList))
			{
				item(this, true);
			}
		}
		if (callback != null)
		{
			mPlayEndCallbackList ??= new();
			mPlayEndCallbackList.Add(callback);
		}
	}
	public void addPlayingCallback(TextureAnimCallback callback, bool clear = true)
	{
		if (clear)
		{
			mPlayingCallbackList?.Clear();
		}
		if (callback != null)
		{
			mPlayingCallbackList ??= new();
			mPlayingCallbackList.Add(callback);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onPlaying(AnimControl control, int frame, bool isPlaying)
	{
		if (mControl.getCurFrameIndex() >= mSpriteList.Count)
		{
			return;
		}
		setSprite(mSpriteList[mControl.getCurFrameIndex()], mUseTextureSize);
		// 使用位置列表进行校正
		if (mEffectAlign == EFFECT_ALIGN.POSITION_LIST)
		{
			if (!mTexturePosList.isEmpty())
			{
				setPosition(mTexturePosList[round(divide(frame, mSpriteList.Count * mTexturePosList.Count))]);
			}
		}
		// 对齐父节点的底部
		else if (mEffectAlign == EFFECT_ALIGN.PARENT_BOTTOM)
		{
			myUIObject parent = getParent();
			if (parent != null)
			{
				setPositionY((getWindowSize().y - parent.getWindowSize().y) * 0.5f);
			}
		}
		foreach (TextureAnimCallback item in mPlayingCallbackList.safe())
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
		if (mPlayEndCallbackList.isEmpty())
		{
			return;
		}
		if (callback)
		{
			using var a = new ListScope<TextureAnimCallback>(out var tempList);
			foreach (TextureAnimCallback item in tempList.move(mPlayEndCallbackList))
			{
				item(this, isBreak);
			}
		}
		else
		{
			mPlayEndCallbackList.Clear();
		}
	}
}