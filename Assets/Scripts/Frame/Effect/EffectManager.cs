using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : FrameComponent
{
	private List<GameEffect> mTempList;
	protected List<GameEffect> mEffectList;
	protected List<GameEffect> mDeadList;
	public EffectManager(string name)
		:base(name)
	{
		mEffectList = new List<GameEffect>();
		mDeadList = new List<GameEffect>();
		mTempList = new List<GameEffect>();
	}
	public override void init()
	{
		base.init();
	}
	public override void destroy()
	{
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mDeadList.Clear();
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
				mDeadList.Add(item);
			}
		}
		// 销毁所有已死亡的特效
		foreach(var item in mDeadList)
		{
			destroyEffect(item);
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
		if(nameWithPath == null || nameWithPath.Length == 0)
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
		mTempList.Clear();
		mTempList.AddRange(mEffectList);
		foreach (var item in mTempList)
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
		mTempList.Clear();
	}
	//----------------------------------------------------------------------------------------------------------
	protected void onEffectLoaded(GameObject go, object userData)
	{
		if (userData as OnEffectLoadedCallback != null)
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