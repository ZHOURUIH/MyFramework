using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : FrameComponent
{
	protected List<GameEffect> mEffectList;
	public EffectManager(string name)
		:base(name)
	{
		mEffectList = new List<GameEffect>();
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
		foreach (var item in mEffectList)
		{
			if(item.isActive())
			{
				if(!item.isIgnoreTimeScale())
				{
					item.update(elapsedTime);
				}
				else
				{
					item.update(Time.unscaledDeltaTime);
				}
			}
			if(item.isDead())
			{
				if(deadList == null)
				{
					deadList = mListPool.newList(out deadList);
				}
				deadList.Add(item);
			}
		}
		// 销毁所有已死亡的特效
		if(deadList != null)
		{
			foreach (var item in deadList)
			{
				destroyEffect(item);
			}
			mListPool.destroyList(deadList);
		}
	}
	public List<GameEffect> getEffectList() { return mEffectList; }
	public void createEffectToPool(string nameWithPath, bool async, string tag)
	{
		if(async)
		{
			mObjectPool.createObjectAsync(nameWithPath, onPreLoadEffectLoaded, tag, null);
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
	public GameEffect createEffect(string nameWithPath, GameObject parent, string tag, bool active, float lifeTime = -1.0f)
	{
		if (isEmpty(nameWithPath))
		{
			return null;
		}
		// 在parent下创建一个资源路径为nameWithPath的GameObject
		GameObject go = mObjectPool.createObject(nameWithPath, tag);
		if (go == null)
		{
			return null;
		}
		setNormalProperty(go, parent);
		return create(go, getFileName(nameWithPath), active, lifeTime, false);
	}
	public void createEffectAsync(string nameWithPath, string tag, OnEffectLoadedCallback callback)
	{
		mObjectPool.createObjectAsync(nameWithPath, onEffectLoaded, tag, callback);
	}
	public void destroyEffect(ref GameEffect effect)
	{
		if(effect != null)
		{
			effect.setIgnoreTimeScale(false);
			effect.destroy();
			mEffectList.Remove(effect);
			mClassPool.destroyClass(effect);
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
		List<GameEffect> tempList = mListPool.newList(out tempList);
		tempList.AddRange(mEffectList);
		foreach (var item in tempList)
		{
			GameEffect effect = item;
			// 虽然此处isValidEffect已经足够判断特效是否还存在
			// 但是这样逻辑不严谨,因为依赖于引擎在销毁物体时将所有的引用都置为空
			// 这是引擎自己的特性,并不是通用操作,所以使用更严谨一些的判断
			bool effectValid;
			if(effect.isExistObject())
			{
				effectValid = effect.isValidEffect();
			}
			else
			{
				effectValid = mObjectPool.isExistInPool(effect.getObject());
				// 如果特效物体不是空的,可能是销毁物体时引擎不是立即销毁的,需要手动设置为空
				if(!effectValid && effect.getObject() != null)
				{
					effect.setObject(null);
				}
			}
			if(!effectValid)
			{
				destroyEffect(ref effect);
			}
		}
		mListPool.destroyList(tempList);
	}
	//----------------------------------------------------------------------------------------------------------
	protected void onEffectLoaded(GameObject go, object userData)
	{
		if (userData is OnEffectLoadedCallback)
		{
			if (go == null)
			{
				return;
			}
			(userData as OnEffectLoadedCallback)(create(go, go.name, false, -1.0f, false), null);
		}
	}
	protected void onPreLoadEffectLoaded(GameObject go, object userData)
	{
		mObjectPool.destroyObject(ref go, false);
	}
	protected GameEffect create(GameObject go, string name, bool active, float lifeTime, bool existObject)
	{
		// 查找该物体是否来自于特效池
		if (mObjectPool.isExistInPool(go) && existObject)
		{
			logError("物体来自于特效池,但是标记为外部物体:" + name);
			return null;
		}
		GameEffect gameEffect;
		mClassPool.newClass(out gameEffect);
		gameEffect.setName(name);
		gameEffect.setObject(go);
		gameEffect.setExistObject(existObject);
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