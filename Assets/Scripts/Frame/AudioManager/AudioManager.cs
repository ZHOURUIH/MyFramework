using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : FrameSystem
{
	protected Dictionary<string, AudioInfo> mAudioClipList;     // 音效资源列表
	protected Dictionary<SOUND_DEFINE, string> mSoundDefineMap; // 音效定义与音效名的映射
	protected Dictionary<SOUND_DEFINE, float> mVolumeScale;
	protected int mLoadedCount;
	public AudioManager()
	{
		mAudioClipList = new Dictionary<string, AudioInfo>();
		mSoundDefineMap = new Dictionary<SOUND_DEFINE, string>();
		mVolumeScale = new Dictionary<SOUND_DEFINE, float>();
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
		}
		mAudioClipList.Clear();
		base.destroy();
	}
	public void createStreamingAudio(string url, bool load = true)
	{
		string audioName = getFileNameNoSuffix(url, true);
		if(!mAudioClipList.ContainsKey(audioName))
		{
			AudioInfo newInfo = new AudioInfo();
			newInfo.mAudioName = audioName;
			newInfo.mAudioPath = getFilePath(url);
			newInfo.mClip = null;
			newInfo.mState = LOAD_STATE.UNLOAD;
			newInfo.mIsResource = false;
			newInfo.mSuffix = getFileSuffix(url);
			mAudioClipList.Add(audioName, newInfo);
		}
		AudioInfo info = mAudioClipList[audioName];
		if(load && info.mClip == null)
		{
			loadAudio(info, true);
		}
	}
	public bool unload(string name)
	{
		if(!mAudioClipList.ContainsKey(name))
		{
			return false;
		}
		bool ret = mResourceManager.unload(ref mAudioClipList[name].mClip);
		if(ret)
		{
			mAudioClipList[name].mState = LOAD_STATE.UNLOAD;
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
		if(mAudioClipList.ContainsKey(audioName))
		{
			loadAudio(mAudioClipList[audioName], async);
		}
	}
	public void loadAudio(SOUND_DEFINE sound, bool async = true)
	{
		if(!mSoundDefineMap.ContainsKey(sound))
		{
			return;
		}
		string audioName = mSoundDefineMap[sound];
		if(mAudioClipList.ContainsKey(audioName))
		{
			loadAudio(mAudioClipList[audioName], async);
		}
	}
	public float getAudioLength(string name)
	{
		if (isEmpty(name) || 
			!mAudioClipList.ContainsKey(name) || 
			mAudioClipList[name] == null || 
			mAudioClipList[name].mClip == null)
		{
			return 0.0f;
		}
		return mAudioClipList[name].mClip.length;
	}
	public float getAudioLength(SOUND_DEFINE sound)
	{
		string soundName = sound != SOUND_DEFINE.MIN ? mAudioManager.getAudioName(sound) : null;
		return getAudioLength(soundName);
	}
	// volume范围0-1,load表示如果音效未加载,则加载音效
	public void playClip(AudioSource source, string name, bool loop, float volume, bool load = true)
	{
		if(!mAudioClipList.ContainsKey(name))
		{
			return;
		}
		AudioClip clip = mAudioClipList[name].mClip;
		// 如果音效为空,则尝试加载
		if(clip == null)
		{
			if(load && mAudioClipList[name].mState == LOAD_STATE.UNLOAD)
			{
				loadAudio(mAudioClipList[name], false);
				clip = mAudioClipList[name].mClip;
			}
			else
			{
				return;
			}
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
		if(source != null)
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
		if(source != null)
		{
			source.volume = volume;
		}
	}
	public void setLoop(AudioSource source, bool loop)
	{
		if(source == null)
		{
			source.loop = loop;
		}
	}
	public bool isLoadDone() { return mLoadedCount == mAudioClipList.Count; }
	public float getLoadedPercent() { return (float)mLoadedCount / mAudioClipList.Count; }
	public float getVolumeScale(SOUND_DEFINE sound)
	{
		if(mVolumeScale.ContainsKey(sound))
		{
			return mVolumeScale[sound];
		}
		return 1.0f;
	}
	public string getAudioName(SOUND_DEFINE soundDefine)
	{
		if(mSoundDefineMap.ContainsKey(soundDefine))
		{
			return mSoundDefineMap[soundDefine];
		}
		return null;
	}
	// 参数为Sound下的相对路径,并且不带后缀,只根据文件名查找音效
	public void registeAudio(string fileName)
	{
		string audioName = getFileNameNoSuffix(fileName, true);
		if(!mAudioClipList.ContainsKey(audioName))
		{
			AudioInfo newInfo = new AudioInfo();
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
	public void registeSoundDefine(SOUND_DEFINE soundID, string audioName, string fileName, float volumeScale)
	{
		if(mSoundDefineMap.ContainsKey(soundID))
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
		if(info.mClip != null || info.mState != LOAD_STATE.UNLOAD)
		{
			return;
		}
		info.mState = LOAD_STATE.LOADING;
		string path = info.mAudioPath;
		addEndSlash(ref path);
		if(info.mIsResource)
		{
			string fullName = FrameDefine.R_SOUND_PATH + path + info.mAudioName;
			if(async)
			{
				mResourceManager.loadResourceAsync<AudioClip>(fullName, onAudioLoaded, null, false);
			}
			else
			{
				++mLoadedCount;
				AudioClip audio = mResourceManager.loadResource<AudioClip>(fullName, false);
				if(audio != null)
				{
					info.mClip = audio;
					info.mState = LOAD_STATE.LOADED;
				}
			}
		}
		else
		{
			string url = path + info.mAudioName + info.mSuffix;
			mResourceManager.loadAssetsFromUrl<AudioClip>(url, onAudioLoaded);
		}
	}
	protected void onAudioLoaded(Object assets, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		if (assets != null)
		{
			string name = getFileNameNoSuffix(assets.name, true);
			mAudioClipList[name].mClip = assets as AudioClip;
			mAudioClipList[name].mState = LOAD_STATE.LOADED;
		}
		++mLoadedCount;
	}
}