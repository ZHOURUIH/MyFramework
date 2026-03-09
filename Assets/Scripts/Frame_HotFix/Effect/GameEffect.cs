using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static UnityUtility;
using static MathUtility;
using static FrameUtility;

// 表示3D特效的对象
public class GameEffect : MovableObject
{
	protected List<ParticleSystem> mParticleSystems = new();    // 特效中包含的粒子系统组件列表
	protected List<TrailRenderer> mTrailRenderers = new();      // 特效中包含的拖尾组件列表
	protected List<Animator> mEffectAnimators = new();          // 特效中包含的动画组件列表
	protected GameEffectCallback mEffectDestroyCallback;        // 特效销毁时的回调
	protected string mFilePath;                                 // 特效文件的路径,用于在某些时候获取路径
	protected float mLifeTimer = -1.0f;                         // 特效生存时间计时器
	protected int mTag;											// 在PrefabPool中的tag
	protected bool mDefaultIgnoreTimeScale;                     // 创建时是否忽略时间缩放,用于在停止时恢复忽略时间缩放的设置
	protected bool mExistedObject;                              // 为true表示特效节点是一个已存在的节点,false表示特效是实时加载的一个节点
	protected bool mMoveToHide;                                 // 是否通过移动到远处来隐藏
	protected bool mIsDead;                                     // 特效是否已经死亡
	protected bool mIsTemp;                                     // 是否为临时特效,临时特效会将特效对象存放到池中,不会重置对象的属性,提高效率
	protected bool mIsQuick;									// 是否为快速播放的特效,不保证效果与prefab中的效果完全一致,因为会忽略很多参数
	protected PLAY_STATE mPlayState = PLAY_STATE.STOP;          // 特效整体的播放状态
	public override void setObject(GameObject obj)
	{
		base.setObject(obj);
		mParticleSystems.Clear();
		mTrailRenderers.Clear();
		mEffectAnimators.Clear();
		if (mTransform != null)
		{
			mTransform.GetComponentsInChildren(mParticleSystems);
			mTransform.GetComponentsInChildren(mTrailRenderers);
			mTransform.GetComponentsInChildren(mEffectAnimators);
			foreach (ParticleSystem item in mParticleSystems)
			{
				var main = item.main;
				// 移到屏幕外时,一律暂停更新
				main.cullingMode = ParticleSystemCullingMode.Pause;
			}
			// 关掉自动销毁
			mTrailRenderers.For(trail => trail.autodestruct = false);
		}
	}
	public override void destroy()
	{
		stop();
		mEffectDestroyCallback?.Invoke(this);
		mEffectDestroyCallback = null;
		mParticleSystems.Clear();
		mTrailRenderers.Clear();
		mEffectAnimators.Clear();
		if (!mExistedObject)
		{
			mPrefabPoolManager?.destroyObject(ref mObject, false);
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
			float remainTime = Time.unscaledDeltaTime - Time.deltaTime;
			foreach (ParticleSystem item in mParticleSystems)
			{
				// 只模拟没有在预设中设置为忽略时间缩放的粒子系统,已经设置忽略时间缩放的会由粒子系统自己更新
				if (!item.main.useUnscaledTime)
				{
					item.Simulate(remainTime, false, false);
				}
			}
		}
		if (tickTimerOnce(ref mLifeTimer, elapsedTime))
		{
			mIsDead = true;
		}
	}
	public bool isExistObject()					{ return mExistedObject; }
	public bool isDead()						{ return mIsDead; }
	public bool isTemp()						{ return mIsTemp; }
	public bool isMoveToHide()					{ return mMoveToHide; }
	public bool isValidEffect()					{ return mObject != null; }
	public bool isQuick()						{ return mIsQuick; }
	public string getFilePath()					{ return mFilePath; }
	public int getTag()							{ return mTag; }
	public PLAY_STATE getPlayState()			{ return mPlayState; }
	public void setExistObject(bool exist)		{ mExistedObject = exist; }
	public void setLifeTime(float time)			{ mLifeTimer = time; }
	public void setFilePath(string path)		{ mFilePath = path; }
	public void setTag(int tag)					{ mTag = tag; }
	public void setTemp(bool temp)				{ mIsTemp = temp; }
	public void setDead(bool dead)				{ mIsDead = dead; }
	public void setMoveToHide(bool moveToHide)	{ mMoveToHide = moveToHide; }
	public void setEffectDestroyCallback(GameEffectCallback effect) { mEffectDestroyCallback = effect; }
	// forceIgnoreLoop为true表示确认当前特效是循环特效,但是仍然需要设置为快速播放的特效,也就是明确知道不会有单独停止某个指定粒子的操作,要么全部同时播放,要么全部停止
	public void setQuick(bool forceIgnoreLoop = false)
	{
		if (mIsQuick)
		{
			return;
		}
		mIsQuick = true;
		if (!mTrailRenderers.isEmpty())
		{
			logError("拖尾效果不适用于设置为快速播放的特效,因为特效节点本身是不会改变位置的" + mFilePath);
			return;
		}
		foreach (ParticleSystem item in mParticleSystems)
		{
			ParticleSystem.MainModule main = item.main;
			if (main.loop && !forceIgnoreLoop)
			{
				logWarning("循环粒子不适用于设置为快速播放的特效,因为没办法停止指定的粒子:" + mFilePath);
			}
			main.maxParticles = 10000;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
			ParticleSystem.EmissionModule emission = item.emission;
			emission.enabled = false;
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = item.velocityOverLifetime;
			if (velocityOverLifetime.enabled)
			{
				logWarning("部分带特殊参数的粒子不适用于设置为快速播放的特效,效果会与prefab中的不一致:" + mFilePath);
			}
		}
	}
	public bool checkValid()
	{
		// 虽然此处mObject != null已经足够判断特效是否还存在
		// 但是这样逻辑不严谨,因为依赖于引擎在销毁物体时将所有的引用都置为空
		// 这是引擎自己的特性,并不是通用操作,所以使用更严谨一些的判断
		if (isExistObject())
		{
			return mObject != null;
		}
		bool effectValid = mPrefabPoolManager.isExistInPool(getObject());
		// 如果特效物体不是空的,可能是销毁物体时引擎不是立即销毁的,需要手动设置为空
		if (!effectValid && mObject != null)
		{
			setObject(null);
		}
		return effectValid;
	}
	public override bool setActive(bool active)
	{
		if (active == mObject.activeSelf)
		{
			return active;
		}
		if (!active)
		{
			stop();
		}
		return base.setActive(active);
	}
	public override void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
	{
		mDefaultIgnoreTimeScale = ignore;
		// 如果特效中有动态模型,也需要设置是否受时间影响
		foreach (Animator item in mEffectAnimators.safe())
		{
			if (item != null)
			{
				item.updateMode = ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
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
	// 快速播放,使用一个粒子系统调用Emit来达到同时播放多个相同粒子系统的效果,且可以合批,效率非常高
	public void playQuick(Vector3 pos)
	{
		if (!mIsQuick)
		{
			logError("当前特效没有设置为快速播放的特效,不能进行快速播放");
			return;
		}
		setPosition(pos);
		foreach (ParticleSystem item in mParticleSystems)
		{
			ParticleSystem.EmissionModule emission = item.emission;
			for (int j = 0; j < emission.burstCount; ++j)
			{
				item.Emit(emission.GetBurst(j).maxCount);
			}
		}
	}
	public void play()
	{
		using var a = new ProfilerScope(0);
		if (!isActive())
		{
			setActive(true);
		}
		mPlayState = PLAY_STATE.PLAY;
		foreach (ParticleSystem particle in mParticleSystems)
		{
			// 如果是允许移动到远处来隐藏,且是循环的粒子,则可以不执行播放,因为会一直都保持播放状态
			if (!particle.isPlaying)
			{
				using var b = new ProfilerScope("particle.Play");
				particle.Play(false);
			}
		}

		// 如果时间已经被缩放了,而且有状态机已经设置了忽略时间缩放,则需要重新再设置一次,否则仍然会受到时间缩放影响
		// 因为Animator在时间为0时设置updateMode为UnscaledTime是不会立即生效的
		if (!isFloatEqual(Time.timeScale, 1.0f))
		{
			foreach (Animator item in mEffectAnimators.safe())
			{
				if (item != null && item.updateMode == AnimatorUpdateMode.UnscaledTime)
				{
					item.updateMode = AnimatorUpdateMode.Normal;
					item.updateMode = AnimatorUpdateMode.UnscaledTime;
				}
			}
		}
	}
	public void stopAndMove(Vector3 pos)
	{
		using var a = new ProfilerScope(0);
		mPlayState = PLAY_STATE.STOP;
		foreach (ParticleSystem particle in mParticleSystems)
		{
			if (particle.isPlaying)
			{
				using var bb = new ProfilerScope("particle.Stop");
				particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		setIgnoreTimeScale(mDefaultIgnoreTimeScale);
		// 先修改位置,再清除拖尾
		setPosition(pos);
		clearTrail();
	}
	public void stop()
	{
		using var a = new ProfilerScope(0);
		mPlayState = PLAY_STATE.STOP;
		foreach (ParticleSystem particle in mParticleSystems)
		{
			if (particle.isPlaying)
			{
				using var bb = new ProfilerScope("particle.Stop");
				particle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
		clearTrail();
		setIgnoreTimeScale(mDefaultIgnoreTimeScale);
	}
	public void clearTrail()
	{
		mTrailRenderers.For(trail => trail.Clear());
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mParticleSystems.Clear();
		mTrailRenderers.Clear();
		mEffectAnimators.Clear();
		mEffectDestroyCallback = null;
		mFilePath = null;
		mLifeTimer = -1.0f;
		mTag = 0;
		mDefaultIgnoreTimeScale = false;
		mExistedObject = false;
		mMoveToHide = false;
		mIsDead = false;
		mIsTemp = false;
		mIsQuick = false;
		mPlayState = PLAY_STATE.STOP;
	}
}