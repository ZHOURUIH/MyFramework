using UnityEngine;
using System.Collections.Generic;
using static FrameUtility;
using static UnityUtility;
using static StringUtility;
using static FrameBase;
using static FrameDefine;

// 音频管理器
public class AudioManager : FrameSystem
{
	protected Dictionary<string, AudioInfo> mAudioList; // 音效资源列表,Key是音频文件的相对路径,不带后缀,相对于Sound,或者是一个通过url加载的音效
	protected Dictionary<int, string> mSoundDefineMap;      // 音效定义与音效名的映射
	protected SafeHashSet<AudioHelper> mHelperList;         // 音频播放的辅助物体列表,value表示销毁的剩余时间,有时候音效播放找不到合适的对象进行播放,就需要提供一个辅助播放的物体
	protected AssetLoadDoneCallback mAudioLoadCallback;     // 单个音效文件加载完毕的回调,为了避免GC
	protected AudioHelper mMusicHelper;                     // 全局的背景音乐播放辅助物体,认为同一时间只有一个背景音乐在播放
	protected int mLoadedCount;                             // 音效已加载数量
	public AudioManager()
	{
		mAudioList = new Dictionary<string, AudioInfo>();
		mSoundDefineMap = new Dictionary<int, string>();
		mHelperList = new SafeHashSet<AudioHelper>();
		mAudioLoadCallback = onAudioLoaded;
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<AudioManagerDebug>();
#endif
	}
	public override void lateInit()
	{
		base.lateInit();
	}
	public override void destroy()
	{
		clear();
		base.destroy();
	}
	public void clear()
	{
		foreach (var item in mAudioList)
		{
			// 只销毁通过链接加载的音频
			var temp = item.Value;
			if (!temp.mIsResource)
			{
				destroyGameObject(temp.mClip);
			}
			UN_CLASS(ref temp);
		}
		mAudioList.Clear();
	}
	public override void resourceAvailable()
	{
		// 背景音乐都是2D的
		mMusicHelper = getOneUnusedHelper(-1.0f);
		mMusicHelper.setSpatialBlend(0.0f);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mHelperList.startForeach())
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
		mHelperList.endForeach();
	}
	public Dictionary<string, AudioInfo> getAudioList() { return mAudioList; }
	public AudioHelper getAudioHelper(float audioTime) { return getOneUnusedHelper(audioTime); }
	public AudioHelper getMusicHelper() { return mMusicHelper; }
	public void createStreamingAudio(string url, bool load = true)
	{
		if (!mAudioList.TryGetValue(url, out AudioInfo info))
		{
			CLASS(out info);
			info.mAudioName = url;
			info.mClip = null;
			info.mState = LOAD_STATE.UNLOAD;
			info.mIsResource = false;
			mAudioList.Add(url, info);
		}
		if (load && info.mClip == null)
		{
			loadAudio(info, true);
		}
	}
	// name是Sound下相对路径,不带后缀
	public bool unload(string name)
	{
		if (isEmpty(name) || !mAudioList.TryGetValue(name, out AudioInfo info))
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
	// name是Sound下相对路径,不带后缀
	public bool unload(int sound)
	{
		if (!mSoundDefineMap.TryGetValue(sound, out string fileName))
		{
			return false;
		}
		return unload(fileName);
	}
	// 加载所有已经注册的音效
	public void loadAll(bool async)
	{
		foreach (var item in mAudioList)
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
		if (mAudioList.TryGetValue(fileName, out AudioInfo info))
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
		if (mAudioList.TryGetValue(fileName, out AudioInfo info))
		{
			loadAudio(info, async);
		}
	}
	// name是Sound下相对路径,不带后缀
	public float getAudioLength(string name)
	{
		if (isEmpty(name) || !mAudioList.TryGetValue(name, out AudioInfo info) || info?.mClip == null)
		{
			return 0.0f;
		}
		return info.mClip.length;
	}
	public float getAudioLength(int sound) { return getAudioLength(sound != 0 ? mAudioManager.getAudioName(sound) : null); }
	// volume范围0-1,load表示如果音效未加载,则加载音效,name是Sound下相对路径,不带后缀
	public void playClip(AudioSource source, string name, bool loop, float volume, bool load = true)
	{
		if (!mAudioList.TryGetValue(name, out AudioInfo info))
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
	public bool isLoadDone() { return mLoadedCount == mAudioList.Count; }
	public float getLoadedPercent() { return (float)mLoadedCount / mAudioList.Count; }
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
		if (mAudioList.ContainsKey(fileName))
		{
			return;
		}
		CLASS(out AudioInfo newInfo);
		newInfo.mAudioName = fileName;
		newInfo.mClip = null;
		newInfo.mState = LOAD_STATE.UNLOAD;
		newInfo.mIsResource = true;
		mAudioList.Add(fileName, newInfo);
	}
	// 注册可以使用枚举访问的音效,fileName是Sound下的相对路径,带后缀名
	public void registeSoundDefine(int soundID, string fileName)
	{
		if (mSoundDefineMap.ContainsKey(soundID))
		{
			return;
		}
		mSoundDefineMap.Add(soundID, fileName);
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
		string fullName = R_SOUND_PATH + info.mAudioName;
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
			else
			{
				info.mState = LOAD_STATE.UNLOAD;
			}
		}
	}
	protected void onAudioLoaded(Object assets, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		// 移除路径前缀
		AudioInfo info = mAudioList[removeStartString(loadPath, R_SOUND_PATH)];
		if (assets != null)
		{
			info.mClip = assets as AudioClip;
			info.mState = LOAD_STATE.LOADED;
		}
		else
		{
			info.mState = LOAD_STATE.UNLOAD;
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
		UN_CLASS(ref helper);
	}
}