using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void OnEffectDestroy(GameEffect effect, object userData);

public class GameEffect : MovableObject
{
	protected ParticleSystem[] mParticleSystems;        // 特效中包含的粒子系统组件列表
	protected Animator[] mEffectAnimators;
	protected OnEffectDestroy mEffectDestroyCallback;   // 特效销毁时的回调
	protected PLAY_STATE mPlayState;	// 特效整体的播放状态
	protected object mDestroyUserData;	// 销毁回调的自定义参数
	protected float mLifeTime;          // 特效生存时间,超过生存时间后就会被销毁
	protected float mMaxActiveTime;		// 显示的最大持续时间
	protected float mActiveTime;		// 特效显示的持续时间,超过持续时间就会被隐藏
	protected bool mExistedObject;      // 为true表示特效节点是一个已存在的节点,false表示特效是实时加载的一个节点
	protected bool mIsDead;             // 粒子系统是否已经死亡
	protected bool mNextIgnoreTimeScale;
	public GameEffect()
		:base(EMPTY_STRING)
	{
		mPlayState = PLAY_STATE.PS_STOP;
	}
	public override void init()
	{
		base.init();
		setDestroyObject(false);
	}
	public override void setObject(GameObject obj, bool destroyOld = true)
	{
		base.setObject(obj, destroyOld);
		if(mTransform != null)
		{
			mParticleSystems = mTransform.GetComponentsInChildren<ParticleSystem>();
			mEffectAnimators = mTransform.GetComponentsInChildren<Animator>();
		}
	}
	public override void destroy()
	{
		mEffectDestroyCallback?.Invoke(this, mDestroyUserData);
		mEffectDestroyCallback = null;
		mParticleSystems = null;
		mEffectAnimators = null;
		if (!mExistedObject)
		{
			mObjectPool.destroyObject(ref mObject, false);
		}
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 在忽略时间缩放时需要手动进行模拟
		// 播放后忽略了时间缩放,则只有在停止时才能修改忽略时间缩放,中间修改是无效的
		if (mIgnoreTimeScale && mPlayState == PLAY_STATE.PS_PLAY)
		{
			foreach (var item in mParticleSystems)
			{
				// 只模拟没有在预设中设置为忽略时间缩放的粒子系统,已经设置忽略时间缩放的会由粒子系统自己更新
				if (!item.main.useUnscaledTime)
				{
					item.Simulate(elapsedTime, false, false);
				}
			}
		}
		if (mLifeTime > 0.0f)
		{
			mLifeTime -= elapsedTime;
			// 生存时间小于等于0,则销毁该特效
			if (mLifeTime <= 0.0f)
			{
				mLifeTime = -1.0f;
				mIsDead = true;
			}
		}
		if(mActiveTime > 0.0f)
		{
			mActiveTime -= elapsedTime;
			if(mActiveTime <= 0.0f)
			{
				mActiveTime = -1.0f;
				setActive(false);
			}
		}
	}
	public void setExistObject(bool existObject) { mExistedObject = existObject; }
	public bool isExistObject() { return mExistedObject; }
	public bool isDead() { return mIsDead; }
	public void setLifeTime(float time) { mLifeTime = time; }
	public void setMaxActiveTime(float time) { mMaxActiveTime = time; }
	public void resetActiveTime() { mActiveTime = mMaxActiveTime; }
	public void setActiveTime(float time) { mActiveTime = time; }
	public bool isValidEffect() { return mObject != null; }
	public PLAY_STATE getPlayState() { return mPlayState; }
	public override void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
	{
		mNextIgnoreTimeScale = ignore;
		// 如果特效中有动态模型,也需要设置是否受时间影响
		if (mEffectAnimators != null && mEffectAnimators.Length > 0)
		{
			foreach (var item in mEffectAnimators)
			{
				if (item != null)
				{
					item.updateMode = ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
				}
			}
		}
		if (ignore == mIgnoreTimeScale)
		{
			return;
		}
		// 正在播放时无法取消忽略时间缩放
		if (mPlayState == PLAY_STATE.PS_PLAY && !ignore)
		{
			return;
		}
		base.setIgnoreTimeScale(ignore, componentOnly);
	}
	public void setEffectDestroyCallback(OnEffectDestroy effect, object userData)
	{
		mEffectDestroyCallback = effect;
		mDestroyUserData = userData;
	}
	public void play()
	{
		mPlayState = PLAY_STATE.PS_PLAY;
		if(mParticleSystems == null)
		{
			return;
		}
		foreach (var item in mParticleSystems)
		{
			item.Play();
		}
		// 如果时间已经被缩放了,而且有状态机已经设置了忽略时间缩放,则需要重新再设置一次,否则仍然会受到时间缩放影响
		// 因为Animator在时间为0时设置updateMode为UnscaledTime是不会立即生效的
		if (!isFloatEqual(Time.timeScale, 1.0f) && mEffectAnimators != null && mEffectAnimators.Length > 0)
		{
			foreach (var item in mEffectAnimators)
			{
				if (item != null && item.updateMode == AnimatorUpdateMode.UnscaledTime)
				{
					item.updateMode = AnimatorUpdateMode.Normal;
					item.updateMode = AnimatorUpdateMode.UnscaledTime;
				}
			}
		}
	}
	public void stop()
	{
		mPlayState = PLAY_STATE.PS_STOP;
		foreach (var item in mParticleSystems)
		{
			item.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		setIgnoreTimeScale(mNextIgnoreTimeScale);
	}
	public void restart()
	{
		stop();
		play();
	}
	public void pause()
	{
		mPlayState = PLAY_STATE.PS_PAUSE;
		foreach (var item in mParticleSystems)
		{
			item.Pause();
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mParticleSystems = null;
		mIsDead = false;
		mLifeTime = -1.0f;
		mMaxActiveTime = -1.0f;
		mActiveTime = -1.0f;
		mExistedObject = false;
		mEffectDestroyCallback = null;
		mDestroyUserData = null;
		mPlayState = PLAY_STATE.PS_STOP;
	}
}