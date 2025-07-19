using UnityEngine;
using System;
using System.Collections.Generic;
using static FrameUtility;
using static UnityUtility;
using static FrameBaseHotFix;
using static CSharpUtility;
using static MathUtility;
using static FrameDefine;
using static FrameBaseUtility;

// 音频管理器
public class AudioManager : FrameSystem
{
	protected Dictionary<string, AudioInfo> mAudioList = new();		// 音效资源列表,Key是音频文件的相对路径,带后缀,相对于GameResources,或者是一个通过url加载的音效
	protected Dictionary<int, string> mSoundDefineMap = new();      // 音效定义与音效名的映射
	protected SafeHashSet<AudioHelper> mHelperList = new();         // 音频播放的辅助物体列表,value表示销毁的剩余时间,有时候音效播放找不到合适的对象进行播放,就需要提供一个辅助播放的物体,包含mMusicHelper
	protected Queue<AudioHelper> mUnusedList = new();				// 未使用列表,用于更轻量缓存AudioHelper对象
	protected AudioHelper mMusicHelper;								// 全局的背景音乐播放辅助物体,认为同一时间只有一个背景音乐在播放
	protected float mSoundVolume = 1.0f;							// 音效音量大小
	protected float mMusicVolume = 1.0f;							// 背景音乐音量大小
	protected int mLoadedCount;                                     // 音效已加载数量
	protected int mMaxAudioCount;									// 音效的同时存在数量上限,为0表示不限制
	public AudioManager()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<AudioManagerDebug>();
		}
	}
	public override void initAsync(Action callback)
	{
		mPrefabPoolManager.initObjectToPoolAsync(AUDIO_HELPER_FILE, 0, 1, false, callback);
	}
	public override void destroy()
	{
		// 销毁所有音频播放辅助对象
		unusedAllHelper();
		destroyAllHelper();
		mMusicHelper = null;

		// 销毁所有音效信息
		foreach (AudioInfo item in mAudioList.Values)
		{
			// 只销毁通过链接加载的音频
			if (!item.mIsLocal)
			{
				destroyUnityObject(item.mClip);
			}
		}
		UN_CLASS_LIST(mAudioList);
		base.destroy();
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
		using var a = new SafeHashSetReader<AudioHelper>(mHelperList);
		foreach (AudioHelper item in a.mReadList)
		{
			// 本身就小于0表示不自动销毁
			if (tickTimerOnce(ref item.mRemainTime, elapsedTime))
			{
				unusedHelper(item);
			}
		}
	}
	public Dictionary<string, AudioInfo> getAudioList() { return mAudioList; }
	public AudioHelper getMusicHelper() { return mMusicHelper; }
	public int getMaxAudioCount() { return mMaxAudioCount; }
	public float getSoundVolume() { return mSoundVolume; }
	public float getMusicVolume() { return mMusicVolume; }
	public AudioHelper getAudioHelper(float audioTime) { return getOneUnusedHelper(audioTime); }
	public void setSoundVolume(float volume) { mSoundVolume = volume; }
	public void setMusicVolume(float volume) { mMusicVolume = volume; }
	public void destroyAudioHelper(AudioHelper helper) { unusedHelper(helper); }
	public void setMaxAudioCount(int maxCount) { mMaxAudioCount = maxCount; }
	public void createStreamingAudio(string url, AudioInfoCallback callback, bool load = true)
	{
		if (!mAudioList.getOrAddClass(url, out AudioInfo info))
		{
			info.mAudioName = url;
			info.mClip = null;
			info.mState = LOAD_STATE.NONE;
			info.mIsLocal = false;
		}
		if (load && info.mClip == null)
		{
			loadAudioAsync(info, callback);
		}
	}
	// name是GameResources下相对路径,带后缀
	public bool unload(string name)
	{
		if (name.isEmpty() || !mAudioList.TryGetValue(name, out AudioInfo info))
		{
			return false;
		}
		bool ret = mResourceManager.unload(ref info.mClip);
		if (ret)
		{
			info.mState = LOAD_STATE.NONE;
		}
		return ret;
	}
	public bool unload(int sound)
	{
		return mSoundDefineMap.TryGetValue(sound, out string fileName) && unload(fileName);
	}
	// 同步加载所有已经注册的音效
	public void loadAll()
	{
		foreach (AudioInfo item in mAudioList.Values)
		{
			if (item.mClip == null && item.mState == LOAD_STATE.NONE)
			{
				loadAudio(item);
			}
		}
	}
	// 异步加载所有已经注册的音效
	public void loadAllAsync()
	{
		foreach (AudioInfo item in mAudioList.Values)
		{
			if (item.mClip == null && item.mState == LOAD_STATE.NONE)
			{
				loadAudioAsync(item, null);
			}
		}
	}
	// fileName是GameResources下相对路径,带后缀
	public void loadAudio(string fileName)
	{
		loadAudio(mAudioList.get(fileName));
	}
	public void loadAudioAsync(string fileName, AudioInfoCallback callback)
	{
		loadAudioAsync(mAudioList.get(fileName), callback);
	}
	public void loadAudio(int sound)
	{
		if (!mSoundDefineMap.TryGetValue(sound, out string fileName))
		{
			return;
		}
		loadAudio(mAudioList.get(fileName));
	}
	public void loadAudioAsync(int sound, AudioInfoCallback callback)
	{
		if (!mSoundDefineMap.TryGetValue(sound, out string fileName))
		{
			callback?.Invoke(null);
			return;
		}
		loadAudioAsync(mAudioList.get(fileName), callback);
	}
	// name是GameResources下相对路径,带后缀
	public float getAudioLength(string name)
	{
		if (name.isEmpty() || !mAudioList.TryGetValue(name, out AudioInfo info) || info?.mClip == null)
		{
			return 0.0f;
		}
		return info.mClip.length;
	}
	public float getAudioLength(int sound) { return getAudioLength(mAudioManager.getAudioName(sound)); }
	// volume范围0-1,load表示如果音效未加载,则加载音效,name是GameResources下相对路径,带后缀
	public void playClip(AudioSource source, string name, bool loop, float volume, bool load = true)
	{
		if (!mAudioList.TryGetValue(name, out AudioInfo info))
		{
			return;
		}
		// 如果音效为空,则尝试加载
		if (info.mClip == null)
		{
			if (load && info.mState == LOAD_STATE.NONE)
			{
				loadAudioAsync(info, (AudioInfo outInfo) =>
				{
					if (source == null)
					{
						return;
					}
					source.enabled = true;
					source.clip = outInfo.mClip;
					source.loop = loop;
					source.volume = volume;
					source.Play();
				});
			}
		}
		else
		{
			if (source == null)
			{
				return;
			}
			source.enabled = true;
			source.clip = info.mClip;
			source.loop = loop;
			source.volume = volume;
			source.Play();
		}
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
		if (source != null)
		{
			source.Pause();
		}
	}
	public bool isLoadDone() { return mLoadedCount == mAudioList.Count; }
	public float getLoadedPercent() { return divide(mLoadedCount, mAudioList.Count); }
	public string getAudioName(int soundDefine) { return mSoundDefineMap.get(soundDefine); }
	// 参数为GameResources下的相对路径,带后缀,只根据文件名查找音效
	public void registeAudio(string fileNameNoSuffix, bool isLocal)
	{
		if (mAudioList.ContainsKey(fileNameNoSuffix))
		{
			return;
		}
		AudioInfo newInfo = mAudioList.addClass(fileNameNoSuffix);
		newInfo.mAudioName = fileNameNoSuffix;
		newInfo.mClip = null;
		newInfo.mState = LOAD_STATE.NONE;
		newInfo.mIsLocal = isLocal;
	}
	// 注册可以使用枚举访问的音效,fileName是GameResources下的相对路径,带后缀名
	public void registeSoundDefine(int soundID, string fileNameWithSuffix, bool isLocal = true)
	{
		if (!mSoundDefineMap.TryAdd(soundID, fileNameWithSuffix))
		{
			return;
		}
		registeAudio(fileNameWithSuffix, isLocal);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void loadAudio(AudioInfo info)
	{
		if (info == null || info.mClip != null || info.mState != LOAD_STATE.NONE)
		{
			return;
		}
		info.mState = LOAD_STATE.LOADING;
		// 如果不是GameResources中的文件,则只能异步加载,因为无法区分是远端的资源还是本地的资源
		if (!info.mIsLocal)
		{
			logError("非本地音效无法同步加载");
			return;
		}
		string fullName = info.mAudioName;
		audioLoaded(mResourceManager.loadGameResource<AudioClip>(fullName), fullName);
	}
	protected void loadAudioAsync(AudioInfo info, AudioInfoCallback callback)
	{
		if (info == null || info.mClip != null || info.mState != LOAD_STATE.NONE)
		{
			callback?.Invoke(null);
			return;
		}
		info.mState = LOAD_STATE.LOADING;
		if (info.mIsLocal)
		{
			mResourceManager.loadGameResourceAsync(info.mAudioName, (AudioClip assets, string fileName) =>
			{
				audioLoaded(assets, fileName);
				callback?.Invoke(info);
			});
		}
		else
		{
			ResourceManager.loadAssetsFromUrl(info.mAudioName, (AudioClip assets, string fileName) =>
			{
				audioLoaded(assets, fileName);
				callback?.Invoke(info);
			});
		}
	}
	protected void audioLoaded(AudioClip assets, string fileName)
	{
		AudioInfo info = mAudioList.get(fileName);
		if (assets != null)
		{
			info.mClip = assets;
			info.mState = LOAD_STATE.LOADED;
		}
		else
		{
			info.mState = LOAD_STATE.NONE;
		}
		++mLoadedCount;
	}
	protected AudioHelper getOneUnusedHelper(float audioTime)
	{
		AudioHelper helper = null;
		if (mUnusedList.Count > 0)
		{
			helper = mUnusedList.Dequeue();
		}
		else
		{
			CLASS(out helper);
			helper.setName("AudioHelper");
			helper.init();
		}
		helper.setAudioEnable(true);
		helper.mRemainTime = audioTime;
		if (mMaxAudioCount > 0 && mHelperList.count() >= mMaxAudioCount)
		{
			// 移除剩余时间最短的
			AudioHelper minItem = null;
			float minTime = 9999.0f;
			foreach (AudioHelper item in mHelperList.getMainList())
			{
				if (item.mRemainTime < minTime)
				{
					minTime = item.mRemainTime;
					minItem = item;
				}
			}
			if (minItem != null)
			{
				unusedHelper(minItem);
			}
		}
		mHelperList.add(helper);
		return helper;
	}
	protected void unusedHelper(AudioHelper helper)
	{
		if (!mHelperList.remove(helper))
		{
			return;
		}
		helper.setAudioEnable(false);
		mUnusedList.Enqueue(helper);
	}
	protected void unusedAllHelper()
	{
		foreach (AudioHelper item in mHelperList.getMainList())
		{
			item.setAudioEnable(false);
			mUnusedList.Enqueue(item);
		}
		mHelperList.clear();
	}
	protected void destroyAllHelper()
	{
		UN_CLASS_LIST(mHelperList.getMainList());
		mHelperList.clear();
	}
}