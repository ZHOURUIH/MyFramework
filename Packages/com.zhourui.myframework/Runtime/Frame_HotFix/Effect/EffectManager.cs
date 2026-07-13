using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseUtility;

// 特效管理器,用于管理所有的3D特效
// 特效有三种
// 1:普通从PrefabPool获得GameObject来创建的GameEffect,函数名中会带NoPool
// 2:从GameEffectPool中获得已经构建好的GameEffect,最常用的方法
// 3:使用同一个特效同时重复在不同位置播放非循环粒子,适用于非循环不带拖尾的特效
public class EffectManager : FrameSystem
{
	protected Dictionary<Transformable, HashSet<GameEffect>> mEffectAttachList = new(); // 记录每个物体上都挂了哪些特效,防止特效由于父物体的销毁而意外被销毁
	protected SafeDictionary<string, QuickEffect> mQuickEffectList = new();				// 用于快速播放的特效列表
	protected SafeList<GameEffect> mEffectList = new();                                 // 正在使用中的特效列表,不含快速播放的特效
	protected GameEffectPool mGameEffectPool = new();                                   // 特效池,用于存储临时特效
	protected ClassObjectCallback mObjectDestroyCallback;                               // 物体销毁时的回调,用于在物体销毁时确认销毁所有挂接到此物体上的特效
	protected float mQuickEffectTimer;													// 检查QuickEffect的计时器
	protected int mQuickEffectTime = 60;												// 超过60秒未播放的快速特效将会被回收
	public EffectManager()
	{
		mCreateObject = true;
		mObjectDestroyCallback = onObjectDestroy;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<EffectManagerDebug>();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (effect.isActiveInHierarchy())
			{
				effect.update(!effect.isIgnoreTimeScale() ? elapsedTime : mGameFrameworkHotFix.getUnscaledTime());
			}
			// 销毁已死亡的特效
			if (effect.isDead())
			{
				if (effect.isInEffectPool())
				{
					destroyEffectInPool(effect);
				}
				else
				{
					destroyEffectNoPool(effect);
				}
			}
		}

		// 检查有没有长时间没有播放的QuickEffect
		if (tickTimerLoop(ref mQuickEffectTimer, elapsedTime, 1.0f))
		{
			using var b = new SafeDictionaryReader<string, QuickEffect>(mQuickEffectList);
			DateTime curTime = DateTime.Now;
			foreach (var item in b.mReadList)
			{
				if ((curTime - item.Value.getLastPlayTime()).TotalSeconds > mQuickEffectTime)
				{
					destroyEffectNoPool(item.Value);
				}
			}
		}

		mGameEffectPool.update(elapsedTime);
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (effect.isActiveInHierarchy())
			{
				effect.lateUpdate(!effect.isIgnoreTimeScale() ? elapsedTime : mGameFrameworkHotFix.getUnscaledTime());
			}
		}
	}
	public void setQuickEffectTime(int time) { mQuickEffectTime = time; }
	public void setUnuseMaxTime(int time) { mGameEffectPool.setUnuseMaxTime(time); }
	public SafeList<GameEffect> getEffectList() { return mEffectList; }
	// 根据场景中的一个节点创建一个特效对象
	public GameEffect createEffectWithExistObject(GameObject effectObject, bool active, float lifeTime = -1.0f)
	{
		return mEffectList.add(create(effectObject, effectObject.name, null, 0, active, lifeTime, true, true, false));
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffectNoPool(string nameWithPath, Transformable attachedParent, GameObject parent, bool active, bool moveToHide, int tag = 0, float lifeTime = -1.0f)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		return createEffectNoPool(nameWithPath, attachedParent, parent, active, moveToHide, Vector3.zero, tag, lifeTime, false);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffectNoPool(string nameWithPath, Transformable attachedParent, GameObject parent, bool active, bool moveToHide, Vector3 pos, int tag = 0, float lifeTime = -1.0f, bool isTemp = false)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		// 在parent下创建一个资源路径为nameWithPath的GameObject
		GameObject go = mPrefabPoolManager.createObject(nameWithPath, moveToHide, true, tag, parent);
		GameEffect effect = postCreateFromPrefabPool(go, nameWithPath, attachedParent, parent, tag, moveToHide, active, lifeTime, isTemp, false);
        effect.setPosition(pos);
		return effect;
    }
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, string nameWithPath, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, attachedParent?.getGameObject(), nameWithPath, moveToHide, null, true, lifeTime, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, string nameWithPath, bool active, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, attachedParent?.getGameObject(), nameWithPath, moveToHide, null, active, lifeTime, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, string nameWithPath, bool moveToHide, GameEffectCallback callback, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, attachedParent?.getGameObject(), nameWithPath, moveToHide, callback, false, -1.0f, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, string nameWithPath, bool moveToHide, GameEffectCallback callback, bool active, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, attachedParent?.getGameObject(), nameWithPath, moveToHide, callback, active, -1.0f, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, GameObject parent, string nameWithPath, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, moveToHide, null, true, lifeTime, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, GameObject parent, string nameWithPath, bool active, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, moveToHide, null, active, lifeTime, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, GameObject parent, string nameWithPath, bool moveToHide, GameEffectCallback callback, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, moveToHide, callback, false, -1.0f, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, GameObject parent, string nameWithPath, bool moveToHide, GameEffectCallback callback, bool active, int tag = 0)
	{
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, moveToHide, callback, active, -1.0f, false, tag);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsync(Transformable attachedParent, GameObject parent, string nameWithPath, bool moveToHide, GameEffectCallback callback, bool active, float lifeTime, bool isQuick, int tag = 0)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, null, attachedParent, parent, moveToHide, callback, tag, active, lifeTime, null, false, isQuick);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, bool moveToHide, GameEffectCallback callback, bool active, bool isQuick, int tag = 0)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, tag, active, -1.0f, null, isQuick);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, bool moveToHide, GameEffectCallback callback, bool active, int tag = 0)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, tag, active, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, bool moveToHide, GameEffectCallback callback, int tag = 0)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, tag, true, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, int tag = 0, bool active = true)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, attachedParent, attachedParent?.getGameObject(), moveToHide, callback, tag, active, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, int tag = 0, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, attachedParent, attachedParent?.getGameObject(), moveToHide, callback, tag, active, lifeTime, failCallback);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在parent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	// 当前管理器中所有的异步加载最终都是调的这个函数
	public CustomAsyncOperation createEffectNoPoolAsyncSafe(string nameWithPath, IRecyclable relatedObject, Transformable attachedParent, GameObject parent, bool moveToHide, GameEffectCallback callback, int tag = 0, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null, bool isTemp = false, bool isQuick = false)
	{
		var op = mPrefabPoolManager.createObjectAsyncSafe(relatedObject, nameWithPath, moveToHide, active, (GameObject go) =>
		{
			GameEffect effect = postCreateFromPrefabPool(go, nameWithPath, attachedParent, parent, tag, moveToHide, active, lifeTime, isTemp, isQuick);
			callback?.Invoke(effect);
		}, tag, failCallback);
		return op;
	}
	// 会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffect(string nameWithPath, Transformable attachedParent, GameObject parent, bool active, bool moveToHide, Vector3 pos, int tag = 0, float lifeTime = -1.0f)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		// 先从未使用列表中获取一个特效
		GameEffect effect = mGameEffectPool.getOneEffect(parent, nameWithPath, pos, moveToHide, active, lifeTime);
		if (effect != null)
		{
			return mEffectList.add(effect);
		}
		return createEffectNoPool(nameWithPath, attachedParent, parent, active, moveToHide, pos, tag, lifeTime, true);
	}
    public GameEffect createEffect(string nameWithPath, Transformable attachedParent, GameObject parent, bool active, bool moveToHide, int tag = 0)
	{
		return createEffect(nameWithPath, attachedParent, parent, active, moveToHide, Vector3.zero, -1, tag);
	}
    // 会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
    // 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
    // 异步从prefab中加载一个特效
    public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, IRecyclable relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, int tag = 0, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, attachedParent, attachedParent?.getGameObject(), moveToHide, callback, tag, active, lifeTime, failCallback);
	}
    public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, int tag = 0)
    {
        return createEffectAsyncSafe(nameWithPath, attachedParent, attachedParent, attachedParent?.getGameObject(), moveToHide, callback, tag, true, -1, null);
    }
    // 会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
    // 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
    // 异步从prefab中加载一个特效
    public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, IRecyclable relatedObject, bool moveToHide, GameEffectCallback callback, int tag = 0)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, tag, true, -1.0f, null);
	}
	// 会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在parent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	// 异步创建时不再传一个位置了,因为加载完毕时真正需要的位置不一定跟传的一致,可能会导致隐式的bug,所以还是在回调中自己去设置位置
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, IRecyclable relatedObject, Transformable attachedParent, GameObject parent, bool moveToHide, GameEffectCallback callback, int tag = 0, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		// 先从未使用列表中获取一个特效
		GameEffect effect = mGameEffectPool.getOneEffect(parent, nameWithPath, Vector3.zero, moveToHide, active, lifeTime);
		if (effect != null)
		{
			callback?.Invoke(mEffectList.add(effect));
			return new CustomAsyncOperation().setFinish();
		}
		return createEffectNoPoolAsyncSafe(nameWithPath, relatedObject, attachedParent, parent, moveToHide, callback, tag, active, lifeTime, failCallback, true);
	}
	// 会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, bool moveToHide, GameEffectCallback callback, bool active, float lifeTime, int tag = 0)
	{
		// 先从未使用列表中获取一个特效
		GameEffect effect = mGameEffectPool.getOneEffect(parent, nameWithPath, Vector3.zero, moveToHide, active, lifeTime);
		if (effect != null)
		{
			callback?.Invoke(mEffectList.add(effect));
			return new CustomAsyncOperation().setFinish();
		}
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, moveToHide, callback, true, lifeTime, false, tag);
	}
	// 带Quick后缀的函数是创建一个可以快速播放的特效,调用GameEffect对象的playQuick可快速播放,效率很高
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncQuick(Transformable attachedParent, GameObject parent, string nameWithPath, GameEffectCallback callback, int tag = 0)
	{
		if (mQuickEffectList.tryGetValue(nameWithPath, out QuickEffect effect))
		{
			callback?.Invoke(effect);
			return new CustomAsyncOperation().setFinish();
		}
		return createEffectNoPoolAsync(attachedParent, parent, nameWithPath, false, (effect) =>
		{
			effect.stop();
			callback?.Invoke(effect);
		}, true, -1.0f, true, tag);
	}
	// 在relatedObject的位置上播放一个特效,如果特效加载完成之前relatedObject被销毁了,则不会播放特效
	public CustomAsyncOperation playEffectAsync(string nameWithPath, Transformable relatedObject, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, null, moveToHide, (effect)=>
		{
			effect.setPosition(relatedObject.getPosition());
		}, tag, true, lifeTime);
	}
	public CustomAsyncOperation playEffectAsyncAtPosition(string nameWithPath, Vector3 pos, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectAsync(null, null, nameWithPath, moveToHide, (effect) => { effect.setPosition(pos); }, true, lifeTime, tag);
	}
	public CustomAsyncOperation playEffectAsyncAtPosition(string nameWithPath, Vector3 pos, Vector3 rotation, float lifeTime, bool moveToHide, int tag = 0)
	{
		return createEffectAsync(null, null, nameWithPath, moveToHide, (GameEffect effect) =>
		{
			effect.setPosition(pos);
			effect.setRotation(rotation);
		}, true, lifeTime, tag);
	}
	public CustomAsyncOperation playEffectAsyncAtPositionQuick(string nameWithPath, Vector3 pos, int tag = 0)
	{
		return createEffectAsyncQuick(null, null, nameWithPath, effect => (effect as QuickEffect).playQuick(pos), tag);
	}
	public void destroyEffect(ref GameEffect effect, bool destroyReally = false, bool removeFromAttachList = true)
	{
		if (effect.isInEffectPool())
		{
			destroyEffectInPool(ref effect, destroyReally, removeFromAttachList);
		}
		else
		{
			destroyEffectNoPool(ref effect, removeFromAttachList);
		}
	}
	public void destroyEffect(GameEffect effect, bool destroyReally = false, bool removeFromAttachList = true)
	{
		destroyEffect(ref effect, destroyReally, removeFromAttachList);
	}
	public void destroyAllEffectWithTag(int tag)
	{
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (effect.getTag() == tag)
			{
				destroyEffect(effect);
			}
		}
	}
	// 检查特效是否还有效,特效物体已经被销毁的视为无效特效,将被清除
	public void clearInvalidEffect()
	{
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (!effect.checkValid())
			{
				destroyEffect(effect);
			}
		}
		mGameEffectPool.clearInvalidEffect();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected GameEffect create(GameObject go, string name, string nameWithPath, int tag, bool active, float lifeTime, bool existObject, bool moveToHide, bool isQuick)
	{
		// 查找该物体是否来自于特效池
		if (mPrefabPoolManager.isExistInPool(go) && existObject)
		{
			logError("物体来自于特效池,但是标记为外部物体:" + name);
			return null;
		}
		GameEffect gameEffect;
		if (isQuick)
		{
			gameEffect = CLASS<QuickEffect>();
		}
		else
		{
			CLASS(out gameEffect);
		}
		gameEffect.setName(name);
		gameEffect.setFilePath(nameWithPath);
		gameEffect.setExistObject(existObject);
		gameEffect.setObject(go);
		gameEffect.init();
		gameEffect.setTag(tag);
		gameEffect.setActive(active);
		gameEffect.setLifeTime(lifeTime);
		gameEffect.setMoveToHide(moveToHide);
		return gameEffect;
	}
	protected GameEffect postCreateFromPrefabPool(GameObject go, string nameWithPath, Transformable attachedParent, GameObject parent, int tag, bool moveToHide, bool active, float lifeTime, bool isTemp, bool isQuick)
	{
		if (go == null)
		{
			return null;
		}
		using var a = new ProfilerScope(0);
		GameEffect effect = create(go, go.name, nameWithPath, tag, active, lifeTime, false, moveToHide, isQuick);
		if (attachedParent != null)
		{
			attachedParent.addDestroyCallback(mObjectDestroyCallback);
			mEffectAttachList.getOrAddListPersist(attachedParent).Add(effect);
		}
		if (parent != null)
		{
			effect.setParent(parent);
		}
		effect.clearTrail();
		if (active)
		{
			effect.play();
		}
		if (isTemp)
		{
			mGameEffectPool.useEffect(effect);
		}
		effect.setInEffectPool(isTemp);
		if (effect is QuickEffect quickEffect)
		{
			mQuickEffectList.add(nameWithPath, quickEffect);
		}
		else
		{
			mEffectList.add(effect);
		}
		return effect;
	}
	protected void onObjectDestroy(ClassObject obj)
	{
		if (obj is Transformable trans && mEffectAttachList.Remove(trans, out var list))
		{
			list.For(effect => destroyEffect(effect, false));
			UN_SET(ref list);
		}
	}
	protected void removeFromAttachList(GameEffect effect)
	{
		foreach (var item in mEffectAttachList)
		{
			var list = item.Value;
			if (list.Remove(effect))
			{
				if (list.Count == 0)
				{
					UN_SET(ref list);
					mEffectAttachList.Remove(item.Key);
				}
				break;
			}
		}
	}
	// 只回收到未使用列表中,不是真的销毁,但是有时候也需要真的去销毁一个临时特效,所以还是加了一个destroyReally参数,用于在必要时销毁临时特效
	protected void destroyEffectInPool(ref GameEffect effect, bool destroyReally = false, bool needRemoveFromAttachList = true)
	{
		if (effect == null)
		{
			return;
		}
		if (!effect.isInEffectPool())
		{
			logError("需要使用destroyEffect销毁非临时特效:" + effect.getFilePath());
			return;
		}
		// 当GameObject为空时,可能是在销毁无效的特效,所以只能彻底清除
		if (destroyReally || effect.getGameObject() == null)
		{
			mGameEffectPool.removeEffect(effect);
			destroyEffectNoPool(ref effect);
			return;
		}
		// 回收到未使用列表中
		mGameEffectPool.unuseEffect(effect);
		// 从mEffectAttachList中移除
		if (needRemoveFromAttachList)
		{
			removeFromAttachList(effect);
		}

		mEffectList.remove(effect);
		// 该特效支持移动到远处来隐藏,并且父节点是Prefab对象池的节点时,才能进行移动隐藏
		if (effect.isMoveToHide() && effect.getGameObject().transform.parent == mPrefabPoolManager.getObject().transform)
		{
			effect.stopAndMove(FAR_POSITION);
		}
		else
		{
			effect.setActive(false);
			effect.setParent(mPrefabPoolManager.getObject());
		}
	}
	protected void destroyEffectInPool(GameEffect effect, bool destroyReally = false)
	{
		destroyEffectInPool(ref effect, destroyReally);
	}
	// 销毁特效对象,但是GameObject还是回收到Prefab的池中
	protected void destroyEffectNoPool(ref GameEffect effect, bool needRemoveFromAttachList = true)
	{
		if (effect == null)
		{
			return;
		}
		if (effect.isInEffectPool())
		{
			logError("需要使用destroyEffectTemp销毁临时特效:" + effect.getFilePath());
			return;
		}
		// 从mEffectAttachList中移除
		if (needRemoveFromAttachList)
		{
			removeFromAttachList(effect);
		}

		mEffectList.remove(effect);
		if (effect is QuickEffect quickEffect)
		{
			mQuickEffectList.remove(quickEffect.getFilePath());
		}
		// 销毁特效
		effect.setIgnoreTimeScale(false);
		UN_CLASS(ref effect);
	}
	protected void destroyEffectNoPool(GameEffect effect)
	{
		destroyEffectNoPool(ref effect);
	}
}