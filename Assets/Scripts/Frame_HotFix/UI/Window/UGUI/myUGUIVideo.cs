#if USE_AVPRO_VIDEO
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.Events;
using static UnityUtility;
using static MathUtility;
using static StringUtility;
using static FrameUtility;
using static FileUtility;
using static FrameDefine;
using static FrameDefineBase;

// 用于播放视频的窗口
public class myUGUIVideo : myUGUIRawImage
{
	protected UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> mVideoEventCallback;	// 避免GC的委托
	protected VideoErrorCallback mErrorCallback;		// 错误回调
	protected VideoCallback mVideoReadyCallback;		// 视频准备完毕的回调
	protected VideoCallback mVideoEndCallback;			// 视频播放结束的回调
	protected MediaPlayer mMediaPlayer;					// 视频播放组件
	protected PLAY_STATE mNextState;					// 缓存的视频播放状态
	protected string mFileName;							// 视频文件名
	protected float mNextRate;							// 缓存的视频播放速度
	protected float mNextSeekTime;						// 缓存的视频播放点
	protected bool mAutoShowOrHide;						// 是否播放完毕时自动隐藏,开始播放时自动显示
	protected bool mIsRequires;							// 是否旋转180°
	protected bool mNextLoop;							// 刚设置视频文件,还未加载时,要设置播放状态就需要先保存状态,然后等到视频准备完毕后再设置
	protected bool mReady;								// 视频是否已准备完毕
	public myUGUIVideo()
	{
		mVideoEventCallback = onVideoEvent;
		mAutoShowOrHide = true;
		mNextState = PLAY_STATE.NONE;
		mNextRate = 1.0f;
		mFileName = null;
		mNeedUpdate = true;
	}
	public override void init()
	{
		base.init();
		mIsRequires = false;
		mMediaPlayer = getOrAddUnityComponent<MediaPlayer>();
		mMediaPlayer.Events.AddListener(mVideoEventCallback);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (!mReady || mMediaPlayer.Control == null || !mMediaPlayer.Control.IsPlaying())
		{
			return;
		}
		if (mMediaPlayer == null)
		{
			mRawImage.texture = null;
		}
		if (mMediaPlayer.TextureProducer == null)
		{
			return;
		}
		Texture texture = mMediaPlayer.TextureProducer.GetTexture();
		if (texture == null)
		{
			return;
		}
		if (mMediaPlayer.TextureProducer.RequiresVerticalFlip() && !mIsRequires)
		{
			mIsRequires = true;
			Vector3 rot = getRotation();
			float rotX = rot.x + PI_DEGREE;
			adjustAngle180(ref rotX);
			rot.x = rotX;
			// 旋转180 
			setRotation(rot);
		}
		mRawImage.texture = texture;
		// 只有当真正开始渲染时才认为是准备完毕
		mVideoReadyCallback?.Invoke(mFileName, false);
		mVideoReadyCallback = null;
	}
	public override void destroy()
	{
		mMediaPlayer.Events.RemoveAllListeners();
		base.destroy();
	}
	public void setPlayState(PLAY_STATE state, bool autoShowOrHide = true)
	{
		if (mFileName.isEmpty())
		{
			return;
		}
		if (mReady)
		{
			if (state == PLAY_STATE.PLAY)
			{
				play(autoShowOrHide);
			}
			else if (state == PLAY_STATE.PAUSE)
			{
				pause();
			}
			else if (state == PLAY_STATE.STOP)
			{
				stop(autoShowOrHide);
			}
		}
		else
		{
			mNextState = state;
			mAutoShowOrHide = autoShowOrHide;
		}
	}
	public bool setFileNameInVideoPath(string videoName)
	{
		string fullPath = availableReadPath(SA_VIDEO_PATH + videoName);
		// 如果是PersistentDataPath的资源
		if (fullPath.startWith(F_PERSISTENT_ASSETS_PATH))
		{
			return setFileURL(fullPath, MediaPlayer.FileLocation.AbsolutePathOrURL);
		}
		// 否则就是StreamingAssets的资源
		else
		{
			// 因为内部会对StreamingAssets的路径进行重新处理,跟FrameDefine.F_STREAMING_ASSETS_PATH中的处理重复了
			// 所以只能将相对路径传进去,否则在Android平台下会出现路径错误
			string relativePath = fullPath.removeStartString(F_STREAMING_ASSETS_PATH);
			return setFileURL(Application.streamingAssetsPath + "/" + relativePath, MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder);
		}
	}
	public bool setFileName(string fullPath)
	{
		if (!isFileExist(fullPath))
		{
			logError("找不到视频文件 : " + fullPath);
			return false;
		}
		return setFileURL(fullPath, MediaPlayer.FileLocation.AbsolutePathOrURL);
	}
	public bool setFileURL(string url, MediaPlayer.FileLocation localtion)
	{
		if (mFileName == url)
		{
			return true;
		}
		setVideoEndCallback(null);
		notifyVideoReady(false);
		mFileName = url;
		mRawImage.texture = null;
		return mMediaPlayer.OpenVideoFromFile(localtion, url, false);
	}
	public string getFileName() { return mFileName; }
	public void setLoop(bool loop)
	{
		if (mReady)
		{
			mMediaPlayer.Control.SetLooping(loop);
		}
		else
		{
			mNextLoop = loop;
		}
	}
	public bool isLoop() { return mMediaPlayer.m_Loop; }
	public void setRate(float rate)
	{
		if (mReady)
		{
			clamp(ref rate, 0.0f, 4.0f);
			if (!isFloatEqual(rate, getRate()))
			{
				mMediaPlayer.Control.SetPlaybackRate(rate);
			}
		}
		else
		{
			mNextRate = rate;
		}
	}
	public float getRate()
	{
		if (mMediaPlayer.Control == null)
		{
			return 0.0f;
		}
		return mMediaPlayer.Control.GetPlaybackRate();
	}
	public float getVideoLength()
	{
		if (mMediaPlayer.Info == null)
		{
			return 0.0f;
		}
		return mMediaPlayer.Info.GetDurationMs() * 0.001f;
	}
	public float getVideoPlayTime()
	{
		return mMediaPlayer.Control.GetCurrentTimeMs();
	}
	public void setVideoPlayTime(float timeMS)
	{
		if (mReady)
		{
			mMediaPlayer.Control.SeekFast(timeMS);
		}
		else
		{
			mNextSeekTime = timeMS;
		}
	}
	public void setVideoEndCallback(VideoCallback callback)
	{
		// 重新设置回调之前,先调用之前的回调
		clearAndCallEvent(ref mVideoEndCallback, true);
		mVideoEndCallback = callback;
	}
	public void closeVideo()
	{
		mMediaPlayer.CloseVideo();
	}
	public bool isPlaying()
	{
		return mMediaPlayer.Control.IsPlaying();
	}
	public void setVideoReadyCallback(VideoCallback callback) { mVideoReadyCallback = callback; }
	public void setErrorCallback(VideoErrorCallback callback) { mErrorCallback = callback; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void notifyVideoReady(bool ready)
	{
		mReady = ready;
		if (mReady)
		{
			if (mNextState != PLAY_STATE.NONE)
			{
				setPlayState(mNextState, mAutoShowOrHide);
			}
			setLoop(mNextLoop);
			setRate(mNextRate);
			setVideoPlayTime(mNextSeekTime);
		}
		else
		{
			mNextState = PLAY_STATE.NONE;
			mNextRate = 1.0f;
			mNextLoop = false;
			mNextSeekTime = 0.0f;
		}
	}
	protected void clearAndCallEvent(ref VideoCallback callback, bool isBreak)
	{
		VideoCallback temp = callback;
		callback = null;
		temp?.Invoke(mFileName, isBreak);
	}
	protected void onVideoEvent(MediaPlayer player, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
	{
		log("video event : " + eventType, LOG_LEVEL.HIGH);
		if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
		{
			// 播放完后设置为停止状态
			clearAndCallEvent(ref mVideoEndCallback, false);
		}
		else if (eventType == MediaPlayerEvent.EventType.ReadyToPlay)
		{
			// 视频准备完毕时,设置实际的状态
			if (mMediaPlayer.Control == null)
			{
				logError("video is ready, but MediaPlayer.Control is null!");
			}
			notifyVideoReady(true);
		}
		else if (eventType == MediaPlayerEvent.EventType.Error)
		{
			log("video error code : " + errorCode);
			mErrorCallback?.Invoke(errorCode);
		}
	}
	protected void play(bool autoShow = true)
	{
		if (mMediaPlayer.Control != null)
		{
			if (autoShow)
			{
				mRawImage.enabled = true;
			}
			if (!mMediaPlayer.Control.IsPlaying())
			{
				mMediaPlayer.Play();
			}
		}
	}
	protected void pause()
	{
		if (mMediaPlayer.Control != null && !mMediaPlayer.Control.IsPaused())
		{
			mMediaPlayer.Pause();
		}
	}
	protected void stop(bool autoHide = true)
	{
		// 停止并不是真正地停止视频,只是将视频暂停,并且移到视频开始位置
		if (mMediaPlayer.Control != null)
		{
			mMediaPlayer.Rewind(true);
			if (autoHide)
			{
				mRawImage.enabled = false;
			}
			clearAndCallEvent(ref mVideoEndCallback, true);
		}
	}
}
#endif