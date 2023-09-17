using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static StringUtility;

// 特效管理器,用于管理所有的3D特效
public class EffectManager : FrameSystem
{
	protected List<GameEffect> mEffectList;					// 特效列表
	protected CreateObjectCallback mPreloadEffectCallback;	// 预加载完成时的回调,避免GC用
	public EffectManager()
	{
		mEffectList = new List<GameEffect>();
		mPreloadEffectCallback = onPreLoadEffectLoaded;
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<EffectManagerDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		List<GameEffect> deadList = null;
		int effectCount = mEffectList.Count;
		for (int i = 0; i < effectCount; ++i)
		{
			GameEffect item = mEffectList[i];
			if (item.isActive())
			{
				if (!item.isIgnoreTimeScale())
				{
					item.update(elapsedTime);
				}
				else
				{
					item.update(Time.unscaledDeltaTime);
				}
			}
			if (item.isDead())
			{
				if (deadList == null)
				{
					LIST(out deadList);
				}
				deadList.Add(item);
			}
		}
		// 销毁所有已死亡的特效
		if (deadList != null)
		{
			int deadCount = deadList.Count;
			for (int i = 0; i < deadCount; ++i)
			{
				destroyEffect(deadList[i]);
			}
			UN_LIST(ref deadList);
		}
	}
	public List<GameEffect> getEffectList() { return mEffectList; }
	public void createEffectToPool(string nameWithPath, bool async, int tag)
	{
		if (async)
		{
			mPrefabPoolManager.createObjectAsync(nameWithPath, tag, mPreloadEffectCallback);
		}
		else
		{
			GameEffect effect = createEffect(nameWithPath, null, tag, false);
			destroyEffect(ref effect);
		}
	}
	// 根据场景中的一个节点创建一个特效对象
	public GameEffect createEffect(GameObject effectObject, bool active, float lifeTime = -1.0f)
	{
		return create(effectObject, effectObject.name, active, lifeTime, true);
	}
	public GameEffect createEffect(string nameWithPath, GameObject parent, int tag, bool active, float lifeTime = -1.0f)
	{
		if (isEmpty(nameWithPath))
		{
			return null;
		}
		// 在parent下创建一个资源路径为nameWithPath的GameObject
		GameObject go = mPrefabPoolManager.createObject(nameWithPath, tag, parent);
		if (go == null)
		{
			return null;
		}
		return create(go, getFileName(nameWithPath), active, lifeTime, false);
	}
	public void createEffectAsync(string nameWithPath, int tag, float lifeTime)
	{
		createEffectAsync(nameWithPath, tag, null, true, lifeTime);
	}
	public void createEffectAsync(string nameWithPath, int tag, bool active, float lifeTime)
	{
		createEffectAsync(nameWithPath, tag, null, active, lifeTime);
	}
	public void createEffectAsync(string nameWithPath, int tag, OnEffectLoadedCallback callback)
	{
		createEffectAsync(nameWithPath, tag, callback, false, -1.0f);
	}
	public void createEffectAsync(string nameWithPath, int tag, OnEffectLoadedCallback callback, bool active)
	{
		createEffectAsync(nameWithPath, tag, callback, active, -1.0f);
	}
	public void createEffectAsync(string nameWithPath, int tag, OnEffectLoadedCallback callback, bool active, float lifeTime)
	{
		mPrefabPoolManager.createObjectAsync(nameWithPath, tag, (GameObject go, object userData)=>
		{
			if (go == null)
			{
				return;
			}
			callback?.Invoke(create(go, go.name, active, lifeTime, false), null);
		});
	}
	public void destroyEffect(ref GameEffect effect)
	{
		if (effect != null)
		{
			effect.setIgnoreTimeScale(false);
			effect.destroy();
			mEffectList.Remove(effect);
			UN_CLASS(ref effect);
			effect = null;
		}
	}
	public void destroyEffect(GameEffect effect)
	{
		destroyEffect(ref effect);
	}
	// 检查特效是否还有效,特效物体已经被销毁的视为无效特效,将被清除
	public void clearInvalidEffect()
	{
		using (new ListScope<GameEffect>(out var tempList))
		{
			tempList.AddRange(mEffectList);
			int count = tempList.Count;
			for (int i = 0; i < count; ++i)
			{
				GameEffect effect = tempList[i];
				// 虽然此处isValidEffect已经足够判断特效是否还存在
				// 但是这样逻辑不严谨,因为依赖于引擎在销毁物体时将所有的引用都置为空
				// 这是引擎自己的特性,并不是通用操作,所以使用更严谨一些的判断
				bool effectValid;
				if (effect.isExistObject())
				{
					effectValid = effect.isValidEffect();
				}
				else
				{
					effectValid = mPrefabPoolManager.isExistInPool(effect.getObject());
					// 如果特效物体不是空的,可能是销毁物体时引擎不是立即销毁的,需要手动设置为空
					if (!effectValid && effect.getObject() != null)
					{
						effect.setObject(null);
					}
				}
				if (!effectValid)
				{
					destroyEffect(ref effect);
				}
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onEffectLoaded(GameObject go, object userData)
	{
		if (go == null)
		{
			return;
		}
		(userData as OnEffectLoadedCallback)?.Invoke(create(go, go.name, false, -1.0f, false), null);
	}
	protected void onPreLoadEffectLoaded(GameObject go, object userData)
	{
		mPrefabPoolManager.destroyObject(ref go, false);
	}
	protected GameEffect create(GameObject go, string name, bool active, float lifeTime, bool existObject)
	{
		// 查找该物体是否来自于特效池
		if (mPrefabPoolManager.isExistInPool(go) && existObject)
		{
			logError("物体来自于特效池,但是标记为外部物体:" + name);
			return null;
		}
		CLASS(out GameEffect gameEffect);
		gameEffect.setName(name);
		gameEffect.setExistObject(existObject);
		gameEffect.setObject(go);
		gameEffect.init();
		gameEffect.setLifeTime(lifeTime);
		mEffectList.Add(gameEffect);
		gameEffect.setActive(active);
		if (active)
		{
			gameEffect.stop();
			gameEffect.play();
		}
		return gameEffect;
	}
}