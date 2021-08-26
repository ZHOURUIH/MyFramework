using UnityEngine;
using System.Collections.Generic;

// 音频管理器
public class AudioManager : FrameSystem
{
	protected Dictionary<string, AudioInfo> mAudioClipList; // 音效资源列表,Key是音频文件的相对路径,不带后缀,相对于Sound,或者是一个通过url加载的音效
	protected Dictionary<int, string> mSoundDefineMap;      // 音效定义与音效名的映射
	protected Dictionary<int, float> mVolumeScale;          // 音量缩放表,记录每个音效的音量缩放值
	protected SafeHashSet<AudioHelper> mHelperList;         // 音频播放的辅助物体列表,value表示销毁的剩余时间,有时候音效播放找不到合适的对象进行播放,就需要提供一个辅助播放的物体
	protected AssetLoadDoneCallback mAudioLoadCallback;     // 单个音效文件加载完毕的回调,为了避免GC
	protected AudioHelper mMusicHelper;                     // 全局的背景音乐播放辅助物体,认为同一时间只有一个背景音乐在播放
	protected int mLoadedCount;                             // 音效已加载数量
	protected bool mAutoLoad;                               // 是否在资源可用时自动加载所有已注册的音效
	public AudioManager()
	{
		mAudioClipList = new Dictionary<string, AudioInfo>();
		mSoundDefineMap = new Dictionary<int, string>();
		mVolumeScale = new Dictionary<int, float>();
		mHelperList = new SafeHashSet<AudioHelper>();
		mAudioLoadCallback = onAudioLoaded;
		mAutoLoad = true;
	}
	public override void init()
	{
		base.init();
	}
	public override void lateInit()
	{
		base.lateInit();
		mMusicHelper = getOneUnusedHelper(-1.0f);
	}
	public override void destroy()
	{
		foreach (var item in mAudioClipList)
		{
			// 只销毁通过链接加载的音频
			if (!item.Value.mIsResource)
			{
				destroyGameObject(item.Value.mClip);
			}
			UN_CLASS(item.Value);
		}
		mAudioClipList.Clear();
		base.destroy();
	}
	public override void resourceAvailable()
	{
		if (!mAutoLoad)
		{
			return;
		}
		loadAll(false);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		var usedList = mHelperList.startForeach();
		foreach (var item in usedList)
		{
			// 本身就小于0表示不自动销毁
			if (item.mRemainTime < 0.0f)
			{
				continue;
			}
			item.mRemainTime -= elapsedTime;
			if (item.mRemainTime <= 0.0f)
			{
				unusedHelper(item);
			}
		}
	}
	public AudioHelper getAudioHelper(float audioTime) { return getOneUnusedHelper(audioTime); }
	public AudioHelper getMusicHelper() { return mMusicHelper; }
	public void setAutoLoad(bool autoLoad) { mAutoLoad = autoLoad; }
	public void createStreamingAudio(string url, bool load = true)
	{
		if (!mAudioClipList.TryGetValue(url, out AudioInfo info))
		{
			CLASS(out info);
			info.mAudioName = url;
			info.mClip = null;
			info.mState = LOAD_STATE.UNLOAD;
			info.mIsResource = false;
			mAudioClipList.Add(url, info);
		}
		if (load && info.mClip == null)
		{
			loadAudio(info, true);
		}
	}
	// name是Sound下相对路径,不带后缀
	public bool unload(string name)
	{
		if (!mAudioClipList.TryGetValue(name, out AudioInfo info))
		{
			return false;
		}
		bool ret = mResourceManager.unload(ref info.mClip);
		if (ret)
		{
			info.mState = LOAD_STATE.UNLOAD;
		}
		return ret;
	}
	// 加载所有已经注册的音效
	public void loadAll(bool async)
	{
		foreach (var item in mAudioClipList)
		{
			if (item.Value.mClip == null && item.Value.mState == LOAD_STATE.UNLOAD)
			{
				loadAudio(item.Value, async);
			}
		}
	}
	// fileName是Sound下相对路径,不带后缀
	public void loadAudio(string fileName, bool async = true)
	{
		if (mAudioClipList.TryGetValue(fileName, out AudioInfo info))
		{
			loadAudio(info, async);
		}
	}
	public void loadAudio(int sound, bool async = true)
	{
		if (!mSoundDefineMap.TryGetValue(sound, out string fileName))
		{
			return;
		}
		if (mAudioClipList.TryGetValue(fileName, out AudioInfo info))
		{
			loadAudio(info, async);
		}
	}
	// name是Sound下相对路径,不带后缀
	public float getAudioLength(string name)
	{
		if (isEmpty(name) ||
			!mAudioClipList.TryGetValue(name, out AudioInfo info) ||
			info == null ||
			info.mClip == null)
		{
			return 0.0f;
		}
		return info.mClip.length;
	}
	public float getAudioLength(int sound) { return getAudioLength(sound != 0 ? mAudioManager.getAudioName(sound) : null); }
	// volume范围0-1,load表示如果音效未加载,则加载音效,name是Sound下相对路径,不带后缀
	public void playClip(AudioSource source, string name, bool loop, float volume, bool load = true)
	{
		if (!mAudioClipList.TryGetValue(name, out AudioInfo info))
		{
			return;
		}
		AudioClip clip = info.mClip;
		// 如果音效为空,则尝试加载
		if (clip == null)
		{
			if (!load || info.mState != LOAD_STATE.UNLOAD)
			{
				return;
			}
			loadAudio(info, false);
			clip = info.mClip;
		}
		if (source == null)
		{
			return;
		}
		source.enabled = true;
		source.clip = clip;
		source.loop = loop;
		source.volume = volume;
		source.Play();
	}
	public void stopClip(AudioSource source)
	{
		if (source != null)
		{
			source.volume = 0.0f;
		}
	}
	public void pauseClip(AudioSource source)
	{
		source?.Pause();
	}
	public bool isLoadDone() { return mLoadedCount == mAudioClipList.Count; }
	public float getLoadedPercent() { return (float)mLoadedCount / mAudioClipList.Count; }
	public float getVolumeScale(int sound)
	{
		if (mVolumeScale.TryGetValue(sound, out float volume))
		{
			return volume;
		}
		return 1.0f;
	}
	public string getAudioName(int soundDefine)
	{
		if (mSoundDefineMap.TryGetValue(soundDefine, out string name))
		{
			return name;
		}
		return null;
	}
	// 参数为Sound下的相对路径,并且不带后缀,只根据文件名查找音效
	public void registeAudio(string fileName)
	{
		fileName = removeSuffix(fileName);
		if (mAudioClipList.ContainsKey(fileName))
		{
			return;
		}
		CLASS(out AudioInfo newInfo);
		newInfo.mAudioName = fileName;
		newInfo.mClip = null;
		newInfo.mState = LOAD_STATE.UNLOAD;
		newInfo.mIsResource = true;
		mAudioClipList.Add(fileName, newInfo);
	}
	// 注册可以使用枚举访问的音效,fileName是Sound下的相对路径,不带后缀名
	public void registeSoundDefine(int soundID, string fileName, float volumeScale = 1.0f)
	{
		if (mSoundDefineMap.ContainsKey(soundID))
		{
			return;
		}
		fileName = removeSuffix(fileName);
		mSoundDefineMap.Add(soundID, fileName);
		if (!mVolumeScale.ContainsKey(soundID))
		{
			mVolumeScale.Add(soundID, volumeScale);
		}
		registeAudio(fileName);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void loadAudio(AudioInfo info, bool async)
	{
		if (info.mClip != null || info.mState != LOAD_STATE.UNLOAD)
		{
			return;
		}
		info.mState = LOAD_STATE.LOADING;
		// 如果不是Sound中的文件,则只能异步加载,因为无法区分是远端的资源还是本地的资源
		if (!info.mIsResource)
		{
			mResourceManager.loadAssetsFromUrl<AudioClip>(info.mAudioName, mAudioLoadCallback);
			return;
		}
		string fullName = FrameDefine.R_SOUND_PATH + info.mAudioName;
		if (async)
		{
			mResourceManager.loadResourceAsync<AudioClip>(fullName, mAudioLoadCallback);
		}
		else
		{
			++mLoadedCount;
			AudioClip audio = mResourceManager.loadResource<AudioClip>(fullName);
			if (audio != null)
			{
				info.mClip = audio;
				info.mState = LOAD_STATE.LOADED;
			}
		}
	}
	protected void onAudioLoaded(Object assets, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		if (assets != null)
		{
			// 移除路径前缀
			AudioInfo info = mAudioClipList[removeStartString(loadPath, FrameDefine.R_SOUND_PATH)];
			info.mClip = assets as AudioClip;
			info.mState = LOAD_STATE.LOADED;
		}
		++mLoadedCount;
	}
	protected AudioHelper getOneUnusedHelper(float audioTime)
	{
		CLASS(out AudioHelper helper);
		helper.setName("AudioHelper");
		helper.init();
		helper.mRemainTime = audioTime;
		mHelperList.add(helper);
		return helper;
	}
	protected void unusedHelper(AudioHelper helper)
	{
		if (!mHelperList.remove(helper))
		{
			return;
		}
		helper.destroy();
		UN_CLASS(helper);
	}
}