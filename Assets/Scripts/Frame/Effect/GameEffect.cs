using UnityEngine;

// 表示3D特效的对象
public class GameEffect : MovableObject
{
	protected ParticleSystem[] mParticleSystems;        // 特效中包含的粒子系统组件列表
	protected OnEffectDestroy mEffectDestroyCallback;   // 特效销毁时的回调
	protected Animator[] mEffectAnimators;              // 特效中包含的动画组件列表
	protected MyTimer mActiveTimer;                     // 特效显示的持续时间计时器
	protected MyTimer mLifeTimer;                       // 特效生存时间计时器
	protected object mDestroyUserData;                  // 销毁回调的自定义参数
	protected float mMaxActiveTime;                     // 显示的最大持续时间
	protected bool mDefaultIgnoreTimeScale;				// 创建时是否忽略时间缩放,用于在停止时恢复忽略时间缩放的设置
	protected bool mExistedObject;                      // 为true表示特效节点是一个已存在的节点,false表示特效是实时加载的一个节点
	protected bool mIsDead;                             // 粒子系统是否已经死亡
	protected PLAY_STATE mPlayState;                    // 特效整体的播放状态
	public GameEffect()
	{
		mActiveTimer = new MyTimer();
		mLifeTimer = new MyTimer();
		mPlayState = PLAY_STATE.STOP;
	}
	public override void init()
	{
		base.init();
	}
	public override void setObject(GameObject obj)
	{
		base.setObject(obj);
		if (mTransform != null)
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
		if (mIgnoreTimeScale && mPlayState == PLAY_STATE.PLAY)
		{
			int count = mParticleSystems.Length;
			for (int i = 0; i < count; ++i)
			{
				ParticleSystem item = mParticleSystems[i];
				// 只模拟没有在预设中设置为忽略时间缩放的粒子系统,已经设置忽略时间缩放的会由粒子系统自己更新
				if (!item.main.useUnscaledTime)
				{
					item.Simulate(elapsedTime, false, false);
				}
			}
		}
		if (mLifeTimer.tickTimer(elapsedTime))
		{
			mIsDead = true;
		}
		if (mActiveTimer.tickTimer(elapsedTime))
		{
			setActive(false);
		}
	}
	public void setExistObject(bool existObject) { mExistedObject = existObject; }
	public bool isExistObject() { return mExistedObject; }
	public bool isDead() { return mIsDead; }
	public void setLifeTime(float time)
	{
		mLifeTimer.init(0.0f, time, false);
		if (time <= 0.0f)
		{
			mLifeTimer.stop(false);
		}
	}
	public void setMaxActiveTime(float time) { mMaxActiveTime = time; }
	public void resetActiveTime()
	{
		mActiveTimer.init(0.0f, mMaxActiveTime, false);
		if (mMaxActiveTime <= 0.0f)
		{
			mActiveTimer.stop(false);
		}
	}
	public void setActiveTime(float time)
	{
		mActiveTimer.init(0.0f, time, false);
		if (time <= 0.0f)
		{
			mActiveTimer.stop(false);
		}
	}
	public bool isValidEffect() { return mObject != null; }
	public PLAY_STATE getPlayState() { return mPlayState; }
	public override void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
	{
		mDefaultIgnoreTimeScale = ignore;
		// 如果特效中有动态模型,也需要设置是否受时间影响
		if (mEffectAnimators != null && mEffectAnimators.Length > 0)
		{
			int count = mEffectAnimators.Length;
			for (int i = 0; i < count; ++i)
			{
				Animator item = mEffectAnimators[i];
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
		if (mPlayState == PLAY_STATE.PLAY && !ignore)
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
		mPlayState = PLAY_STATE.PLAY;
		if (mParticleSystems == null)
		{
			return;
		}
		int particleCount = mParticleSystems.Length;
		for (int i = 0; i < particleCount; ++i)
		{
			mParticleSystems[i].Play();
		}
		// 如果时间已经被缩放了,而且有状态机已经设置了忽略时间缩放,则需要重新再设置一次,否则仍然会受到时间缩放影响
		// 因为Animator在时间为0时设置updateMode为UnscaledTime是不会立即生效的
		if (!isFloatEqual(Time.timeScale, 1.0f) && mEffectAnimators != null && mEffectAnimators.Length > 0)
		{
			int animatorCount = mEffectAnimators.Length;
			for (int i = 0; i < animatorCount; ++i)
			{
				Animator item = mEffectAnimators[i];
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
		mPlayState = PLAY_STATE.STOP;
		int count = mParticleSystems.Length;
		for (int i = 0; i < count; ++i)
		{
			mParticleSystems[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		setIgnoreTimeScale(mDefaultIgnoreTimeScale);
	}
	public void restart()
	{
		stop();
		play();
	}
	public void pause()
	{
		mPlayState = PLAY_STATE.PAUSE;
		int count = mParticleSystems.Length;
		for (int i = 0; i < count; ++i)
		{
			mParticleSystems[i].Pause();
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mParticleSystems = null;
		mEffectAnimators = null;
		mIsDead = false;
		mMaxActiveTime = -1.0f;
		mLifeTimer.stop();
		mActiveTimer.stop();
		mExistedObject = false;
		mEffectDestroyCallback = null;
		mDestroyUserData = null;
		mPlayState = PLAY_STATE.STOP;
		mDefaultIgnoreTimeScale = false;
	}
}