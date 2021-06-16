using UnityEngine;
using System.Collections.Generic;

public class AudioManager : FrameSystem
{
	protected Dictionary<string, AudioInfo> mAudioClipList;     // 音效资源列表
	protected Dictionary<int, string> mSoundDefineMap;			// 音效定义与音效名的映射
	protected Dictionary<int, float> mVolumeScale;				// 音量缩放表,记录每个音效的音量缩放值
	protected AssetLoadDoneCallback mAudioLoadCallback;			// 单个音效文件加载完毕的回调,为了避免GC
	protected int mLoadedCount;									// 音效已加载数量
	public AudioManager()
	{
		mAudioClipList = new Dictionary<string, AudioInfo>();
		mSoundDefineMap = new Dictionary<int, string>();
		mVolumeScale = new Dictionary<int, float>();
		mAudioLoadCallback = onAudioLoaded;
	}
	public override void destroy()
	{
		foreach(var item in mAudioClipList)
		{
			// 只销毁通过链接加载的音频
			if(!item.Value.mIsResource)
			{
				destroyGameObject(item.Value.mClip);
			}
			UN_CLASS(item.Value);
		}
		mAudioClipList.Clear();
		base.destroy();
	}
	public void createStreamingAudio(string url, bool load = true)
	{
		string audioName = getFileNameNoSuffix(url, true);
		if(!mAudioClipList.TryGetValue(audioName, out AudioInfo info))
		{
			CLASS(out info);
			info.mAudioName = audioName;
			info.mAudioPath = getFilePath(url);
			info.mClip = null;
			info.mState = LOAD_STATE.UNLOAD;
			info.mIsResource = false;
			info.mSuffix = getFileSuffix(url);
			mAudioClipList.Add(audioName, info);
		}
		if(load && info.mClip == null)
		{
			loadAudio(info, true);
		}
	}
	public bool unload(string name)
	{
		if(!mAudioClipList.TryGetValue(name, out AudioInfo info))
		{
			return false;
		}
		bool ret = mResourceManager.unload(ref info.mClip);
		if(ret)
		{
			info.mState = LOAD_STATE.UNLOAD;
		}
		return ret;
	}
	// 加载所有已经注册的音效
	public void loadAll(bool async)
	{
		foreach(var item in mAudioClipList)
		{
			if(item.Value.mClip == null && item.Value.mState == LOAD_STATE.UNLOAD)
			{
				loadAudio(item.Value, async);
			}
		}
	}
	public void loadAudio(string fileName, bool async = true)
	{
		string audioName = getFileNameNoSuffix(fileName, true);
		if (mAudioClipList.TryGetValue(audioName, out AudioInfo info))
		{
			loadAudio(info, async);
		}
	}
	public void loadAudio(int sound, bool async = true)
	{
		if(!mSoundDefineMap.TryGetValue(sound, out string audioName))
		{
			return;
		}
		if (mAudioClipList.TryGetValue(audioName, out AudioInfo info))
		{
			loadAudio(info, async);
		}
	}
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
	public float getAudioLength(int sound)
	{
		string soundName = sound != 0 ? mAudioManager.getAudioName(sound) : null;
		return getAudioLength(soundName);
	}
	// volume范围0-1,load表示如果音效未加载,则加载音效
	public void playClip(AudioSource source, string name, bool loop, float volume, bool load = true)
	{
		if(!mAudioClipList.TryGetValue(name, out AudioInfo info))
		{
			return;
		}
		AudioClip clip = info.mClip;
		// 如果音效为空,则尝试加载
		if(clip == null)
		{
			if (!load || info.mState != LOAD_STATE.UNLOAD)
			{
				return;
			}
			loadAudio(info, false);
			clip = info.mClip;
		}
		if(source == null)
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
	public void setVolume(AudioSource source, float volume)
	{
		if (source != null)
		{
			source.volume = volume;
		}
	}
	public void setLoop(AudioSource source, bool loop)
	{
		if (source == null)
		{
			source.loop = loop;
		}
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
		string audioName = getFileNameNoSuffix(fileName, true);
		if(!mAudioClipList.ContainsKey(audioName))
		{
			CLASS(out AudioInfo newInfo);
			newInfo.mAudioName = audioName;
			newInfo.mAudioPath = getFilePath(fileName);
			newInfo.mClip = null;
			newInfo.mState = LOAD_STATE.UNLOAD;
			newInfo.mIsResource = true;
			newInfo.mSuffix = null;
			mAudioClipList.Add(audioName, newInfo);
		}
	}
	// 注册可以使用枚举访问的音效
	public void registeSoundDefine(int soundID, string audioName, string fileName, float volumeScale)
	{
		if (mSoundDefineMap.ContainsKey(soundID))
		{
			return;
		}
		mSoundDefineMap.Add(soundID, audioName);
		if (!mVolumeScale.ContainsKey(soundID))
		{
			mVolumeScale.Add(soundID, volumeScale);
		}
		registeAudio(fileName);
	}
	//--------------------------------------------------------------------------------------------------------------------------------------
	// name为Resource下相对路径,不带后缀
	protected void loadAudio(AudioInfo info, bool async)
	{
		if (info.mClip != null || info.mState != LOAD_STATE.UNLOAD)
		{
			return;
		}
		info.mState = LOAD_STATE.LOADING;
		string path = info.mAudioPath;
		addEndSlash(ref path);
		if (!info.mIsResource)
		{
			mResourceManager.loadAssetsFromUrl<AudioClip>(path + info.mAudioName + info.mSuffix, mAudioLoadCallback);
			return;
		}
		string fullName = FrameDefine.R_SOUND_PATH + path + info.mAudioName;
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
			AudioInfo info = mAudioClipList[getFileNameNoSuffix(assets.name, true)];
			info.mClip = assets as AudioClip;
			info.mState = LOAD_STATE.LOADED;
		}
		++mLoadedCount;
	}
}