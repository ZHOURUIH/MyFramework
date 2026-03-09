using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseUtility;

// 特效管理器,用于管理所有的3D特效
public class EffectManager : FrameSystem
{
	protected Dictionary<Transformable, HashSet<GameEffect>> mEffectAttachList = new(); // 记录每个物体上都挂了哪些特效,防止特效由于父物体的销毁而意外被销毁
	protected Dictionary<string, List<GameEffect>> mUnusedEffectList = new();           // key是特效路径,value是未使用的特效列表
	protected Dictionary<string, List<GameEffect>> mInusedEffectList = new();           // key是特效路径,value是已使用的特效列表
	protected Dictionary<string, List<KeyValuePair<GameEffectCallback, CustomAsyncOperation>>> mLoadingEffectList = new();  // 正在异步加载的特效列表
	protected SafeList<GameEffect> mEffectList = new();                                 // 特效列表
	protected ClassObjectCallback mObjectDestroyCallback;
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
				effect.update(!effect.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime);
			}
			// 销毁已死亡的特效
			if (effect.isDead())
			{
				if (effect.isTemp())
				{
					destroyEffectTemp(effect);
				}
				else
				{
					destroyEffect(effect);
				}
			}
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (effect.isActiveInHierarchy())
			{
				effect.lateUpdate(!effect.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime);
			}
		}
	}
	public SafeList<GameEffect> getEffectList() { return mEffectList; }
	// 根据场景中的一个节点创建一个特效对象
	public GameEffect createEffectWithExistObject(GameObject effectObject, bool active, float lifeTime = -1.0f)
	{
		return mEffectList.add(create(effectObject, effectObject.name, null, 0, active, lifeTime, true, true));
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffect(string nameWithPath, Transformable attachedParent, GameObject parent, int tag, bool active, bool moveToHide, float lifeTime = -1.0f)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		return createEffect(nameWithPath, attachedParent, parent, tag, active, moveToHide, Vector3.zero, lifeTime, false);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffect(string nameWithPath, Transformable attachedParent, GameObject parent, int tag, bool active, bool moveToHide, Vector3 pos, float lifeTime = -1.0f, bool isTemp = false)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		// 在parent下创建一个资源路径为nameWithPath的GameObject
		GameObject go = mPrefabPoolManager.createObject(nameWithPath, tag, moveToHide, true, parent);
		return postCreateEffectFromPool(go, nameWithPath, attachedParent, parent, tag, pos, moveToHide, active, lifeTime, isTemp);
	}
	// 带Temp后缀的函数是会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 同步从prefab中加载一个特效
	public GameEffect createEffectTemp(string nameWithPath, Transformable attachedParent, GameObject parent, int tag, bool active, bool moveToHide, Vector3 pos, float lifeTime = -1.0f)
	{
		if (nameWithPath.isEmpty())
		{
			return null;
		}
		// 先从未使用列表中获取一个特效
		GameEffect effect = getEffectFromUnused(parent, nameWithPath, pos, moveToHide, active, lifeTime);
		if (effect != null)
		{
			return effect;
		}
		return createEffect(nameWithPath, attachedParent, parent, tag, active, moveToHide, pos, lifeTime, true);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, string nameWithPath, int tag, float lifeTime, bool moveToHide)
	{
		return createEffectAsync(attachedParent, attachedParent?.getObject(), nameWithPath, tag, moveToHide, null, true, lifeTime);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, string nameWithPath, int tag, bool active, float lifeTime, bool moveToHide)
	{
		return createEffectAsync(attachedParent, attachedParent?.getObject(), nameWithPath, tag, moveToHide, null, active, lifeTime);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, string nameWithPath, int tag, bool moveToHide, GameEffectCallback callback)
	{
		return createEffectAsync(attachedParent, attachedParent?.getObject(), nameWithPath, tag, moveToHide, callback, false, -1.0f);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, string nameWithPath, int tag, bool moveToHide, GameEffectCallback callback, bool active)
	{
		return createEffectAsync(attachedParent, attachedParent?.getObject(), nameWithPath, tag, moveToHide, callback, active, -1.0f);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, float lifeTime, bool moveToHide)
	{
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, moveToHide, null, true, lifeTime);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, bool active, float lifeTime, bool moveToHide)
	{
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, moveToHide, null, active, lifeTime);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, bool moveToHide, GameEffectCallback callback)
	{
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, moveToHide, callback, false, -1.0f);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, bool moveToHide, GameEffectCallback callback, bool active)
	{
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, moveToHide, callback, active, -1.0f);
	}
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsync(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, bool moveToHide, GameEffectCallback callback, bool active, float lifeTime)
	{
		return createEffectAsyncSafe(nameWithPath, null, attachedParent, parent, moveToHide, callback, Vector3.zero, tag, active, lifeTime);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, bool moveToHide, GameEffectCallback callback, int tag, bool active)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, Vector3.zero, tag, active, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, bool moveToHide, GameEffectCallback callback, Vector3 pos, int tag, bool active)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, pos, tag, active, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, bool moveToHide, int tag, GameEffectCallback callback)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, null, null, moveToHide, callback, Vector3.zero, tag, true, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, int tag, bool active = true)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, attachedParent, attachedParent?.getObject(), moveToHide, callback, Vector3.zero, tag, active, -1.0f, null);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, Vector3 pos, int tag, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		return createEffectAsyncSafe(nameWithPath, relatedObject, attachedParent, attachedParent?.getObject(), moveToHide, callback, pos, tag, active, lifeTime, failCallback);
	}
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在parent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	// 当前管理器中所有的异步加载最终都是调的这个函数
	public CustomAsyncOperation createEffectAsyncSafe(string nameWithPath, ClassObject relatedObject, Transformable attachedParent, GameObject parent, bool moveToHide, GameEffectCallback callback, Vector3 pos, int tag, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null, bool isTemp = false)
	{
		var op = mPrefabPoolManager.createObjectAsyncSafe(relatedObject, nameWithPath, tag, moveToHide, active, (GameObject go) =>
		{
			// 这里不能合并成一行,否则callback为null时无法执行创建特效的函数
			GameEffect effect = postCreateEffectFromPool(go, nameWithPath, attachedParent, parent, tag, pos, moveToHide, active, lifeTime, isTemp);
			mLoadingEffectList.get(nameWithPath).For(item => item.Key?.Invoke(effect));
		}, failCallback);
		mLoadingEffectList.getOrAddNew(nameWithPath).add(new(callback, op));
		return op;
	}
	// 带Temp后缀的函数是会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafeTemp(string nameWithPath, ClassObject relatedObject, Transformable attachedParent, bool moveToHide, GameEffectCallback callback, Vector3 pos, int tag, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		return createEffectAsyncSafeTemp(nameWithPath, relatedObject, attachedParent, attachedParent?.getObject(), moveToHide, callback, pos, tag, active, lifeTime, failCallback);
	}
	// 带Temp后缀的函数是会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在attachedParent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafeTemp(string nameWithPath, ClassObject relatedObject, bool moveToHide, int tag, GameEffectCallback callback)
	{
		return createEffectAsyncSafeTemp(nameWithPath, relatedObject, null, null, moveToHide, callback, Vector3.zero, tag, true, -1.0f, null);
	}
	// 带Temp后缀的函数是会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 如果特效加载完成之前relatedObject被销毁了,则不会播放特效,并且会将特效挂在parent节点下,并且会在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncSafeTemp(string nameWithPath, ClassObject relatedObject, Transformable attachedParent, GameObject parent, bool moveToHide, GameEffectCallback callback, Vector3 pos, int tag, bool active = true, float lifeTime = -1.0f, BoolCallback failCallback = null)
	{
		// 先从未使用列表中获取一个特效
		GameEffect effect = getEffectFromUnused(parent, nameWithPath, pos, moveToHide, active, lifeTime);
		if (effect != null)
		{
			callback?.Invoke(effect);
			return new CustomAsyncOperation().setFinish();
		}
		return createEffectAsyncSafe(nameWithPath, relatedObject, attachedParent, parent, moveToHide, callback, pos, tag, active, lifeTime, failCallback, true);
	}
	// 带Temp后缀的函数是会从当前类中缓存的特效对象来获取,而不是从通用对象池中获取一个已经被重置过的对象
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncTemp(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, Vector3 pos, bool moveToHide, GameEffectCallback callback, bool active, float lifeTime)
	{
		// 先从未使用列表中获取一个特效
		GameEffect effect = getEffectFromUnused(parent, nameWithPath, pos, moveToHide, active, lifeTime);
		if (effect != null)
		{
			callback?.Invoke(effect);
			return new CustomAsyncOperation().setFinish();
		}
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, moveToHide, callback, true, lifeTime);
	}
	// 带Quick后缀的函数是创建一个可以快速播放的特效,调用GameEffect对象的playQuick可快速播放,效率很高
	// 在attachedParent销毁时确认销毁所有挂接到此物体上的特效
	// 异步从prefab中加载一个特效
	public CustomAsyncOperation createEffectAsyncQuick(Transformable attachedParent, GameObject parent, string nameWithPath, int tag, GameEffectCallback callback)
	{
		foreach (var item in mEffectList.getMainList())
		{
			if (item.getFilePath() == nameWithPath)
			{
				if (!item.isQuick())
				{
					logError("特效最初加载时未被标记为快速播放的特效,当前又作为快速播放的特效来播放,可能会出现错误:" + nameWithPath);
				}
				callback?.Invoke(item);
				return new CustomAsyncOperation().setFinish();
			}
		}
		// 快速播放的特效与其他特效不一样,同一个文件不会加载多个实例,因为都是通过同一个实例来播放的
		// 其他特效则是每次加载都会加载出一个单独的实例来进行播放
		// 所以如果已经有正在异步加载中的快速播放特效,则需要等待其加载完毕后直接进行播放,而不是再次加载一个新的实例
		var curCallbackList = mLoadingEffectList.get(nameWithPath);
		if (!curCallbackList.isEmpty())
		{
			curCallbackList.add(new(callback, null));
			return curCallbackList[0].Value;
		}
		return createEffectAsync(attachedParent, parent, nameWithPath, tag, false, (effect) =>
		{
			effect.setQuick();
			effect.stop();
			callback?.Invoke(effect);
		}, true, -1.0f);
	}
	// 在relatedObject的位置上播放一个特效,如果特效加载完成之前relatedObject被销毁了,则不会播放特效
	public CustomAsyncOperation playEffectAsync(string nameWithPath, Transformable relatedObject, float lifeTime, bool moveToHide, int tag)
	{
		return createEffectAsyncSafeTemp(nameWithPath, relatedObject, null, moveToHide, null, relatedObject.getPosition(), tag, true, lifeTime);
	}
	public CustomAsyncOperation playEffectAsyncAtPosition(string nameWithPath, Vector3 pos, float lifeTime, bool moveToHide, int tag)
	{
		return createEffectAsyncTemp(null, null, nameWithPath, tag, pos, moveToHide, null, true, lifeTime);
	}
	public CustomAsyncOperation playEffectAsyncAtPositionQuick(string nameWithPath, Vector3 pos, int tag)
	{
		return createEffectAsyncQuick(null, null, nameWithPath, tag, effect => effect.playQuick(pos));
	}
	public CustomAsyncOperation playEffectAsyncAtPosition(string nameWithPath, Vector3 pos, Vector3 rotation, float lifeTime, bool moveToHide, int tag)
	{
		return createEffectAsyncTemp(null, null, nameWithPath, tag, pos, moveToHide, (GameEffect effect) =>
		{
			effect.setRotation(rotation);
		}, true, lifeTime);
	}
	// 只回收到未使用列表中,不是真的销毁,但是有时候也需要真的去销毁一个临时特效,所以还是加了一个destroyReally参数,用于在必要时销毁临时特效
	public void destroyEffectTemp(ref GameEffect effect, bool destroyReally = false, bool removeFromAttachList = true)
	{
		if (effect == null)
		{
			return;
		}
		if (effect.isQuick())
		{
			logError("快速播放的特效不能进行回收,其本身就是一个会被不断复用的对象:" + effect.getFilePath());
			return;
		}
		if (!effect.isTemp())
		{
			logError("需要使用destroyEffect销毁非临时特效:" + effect.getFilePath());
			return;
		}
		string effectName = effect.getFilePath();
		mInusedEffectList.getOrAddNew(effectName).Remove(effect);
		// 当GameObject为空时,可能是在销毁无效的特效,所以只能彻底清除
		if (destroyReally || effect.getObject() == null)
		{
			effect.setTemp(false);
			// 也要从mUnusedEffectList中移除
			mUnusedEffectList.getOrAddNew(effectName).Remove(effect);
			destroyEffect(ref effect);
			return;
		}
		// 从mEffectAttachList中移除
		if (removeFromAttachList)
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

		mEffectList.remove(effect);
		// 回收到未使用列表中
		mUnusedEffectList.getOrAddNew(effectName).Add(effect);
		// 该特效支持移动到远处来隐藏,并且父节点是Prefab对象池的节点时,才能进行移动隐藏
		if (effect.isMoveToHide() && effect.getObject().transform.parent == mPrefabPoolManager.getObject().transform)
		{
			effect.stopAndMove(FAR_POSITION);
		}
		else
		{
			effect.setActive(false);
			effect.setParent(mPrefabPoolManager.getObject());
		}
	}
	public void destroyEffectAuto(GameEffect effect, bool removeFromAttachList = true)
	{
		if (effect.isTemp())
		{
			destroyEffectTemp(ref effect, false, removeFromAttachList);
		}
		else
		{
			destroyEffect(ref effect, removeFromAttachList);
		}
	}
	public void destroyEffectTemp(GameEffect effect, bool destroyReally = false)
	{
		destroyEffectTemp(ref effect, destroyReally);
	}
	// 销毁特效对象,但是GameObject还是回收到Prefab的池中
	public void destroyEffect(ref GameEffect effect, bool removeFromAttachList = true)
	{
		if (effect == null)
		{
			return;
		}
		if (effect.isTemp())
		{
			logError("需要使用destroyEffectTemp销毁临时特效:" + effect.getFilePath());
			return;
		}
		// 从mEffectAttachList中移除
		if (removeFromAttachList)
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

		mEffectList.remove(effect);
		// 销毁特效
		effect.setIgnoreTimeScale(false);
		UN_CLASS(ref effect);
	}
	public void destroyEffect(GameEffect effect)
	{
		destroyEffect(ref effect);
	}
	public void destroyAllEffectWithTag(int tag)
	{
		using var a = new SafeListReader<GameEffect>(mEffectList);
		foreach (GameEffect effect in a.mReadList)
		{
			if (effect.getTag() == tag)
			{
				destroyEffectAuto(effect);
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
				destroyEffectAuto(effect);
			}
		}
		using var b = new ListScope<GameEffect>(out var tempList);
		foreach (var item in mUnusedEffectList)
		{
			item.Value.For(effect => tempList.addIf(effect, !effect.checkValid()));
		}
		tempList.For(effect => destroyEffectAuto(effect));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected GameEffect create(GameObject go, string name, string nameWithPath, int tag, bool active, float lifeTime, bool existObject, bool moveToHide)
	{
		// 查找该物体是否来自于特效池
		if (mPrefabPoolManager.isExistInPool(go) && existObject)
		{
			logError("物体来自于特效池,但是标记为外部物体:" + name);
			return null;
		}
		CLASS(out GameEffect gameEffect);
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
	protected GameEffect getEffectFromUnused(GameObject parent, string nameWithPath, Vector3 pos, bool moveToHide, bool active, float lifeTime)
	{
		// 先从未使用列表中获取一个特效
		if (!mUnusedEffectList.TryGetValue(nameWithPath, out var effectList) || effectList.Count == 0)
		{
			return null;
		}
		using var a = new ProfilerScope(0);
		GameEffect effect = effectList.popBack();
		if (effect.getObject() == null)
		{
			logError("GameObject is null:" + effect.getFilePath());
		}
		effect.setLifeTime(lifeTime);
		effect.setDead(false);
		// 由于此处会跳过PrefabPool,所以隐藏和显示需要自己处理
		if (!moveToHide)
		{
			effect.setPosition(pos);
			effect.setActive(active);
		}
		else
		{
			if (!active)
			{
				effect.stopAndMove(FAR_POSITION);
			}
			else
			{
				effect.setPosition(pos);
				effect.clearTrail();
				effect.play();
			}
		}
		if (parent != null)
		{
			effect.setParent(parent);
		}
		mInusedEffectList.getOrAddNew(nameWithPath).Add(effect);
		return mEffectList.add(effect);
	}
	protected GameEffect postCreateEffectFromPool(GameObject go, string nameWithPath, Transformable attachedParent, GameObject parent, int tag, Vector3 pos, bool moveToHide, bool active, float lifeTime, bool isTemp)
	{
		if (go == null)
		{
			return null;
		}
		using var a = new ProfilerScope(0);
		GameEffect effect = create(go, go.name, nameWithPath, tag, active, lifeTime, false, moveToHide);
		if (attachedParent != null)
		{
			attachedParent.addDestroyCallback(mObjectDestroyCallback);
			mEffectAttachList.getOrAddListPersist(attachedParent).Add(effect);
		}
		if (parent != null)
		{
			effect.setParent(parent);
		}
		effect.setPosition(pos);
		effect.clearTrail();
		if (active)
		{
			effect.play();
		}
		if (isTemp)
		{
			mInusedEffectList.getOrAddNew(nameWithPath).Add(effect);
		}
		effect.setTemp(isTemp);
		return mEffectList.add(effect);
	}
	protected void onObjectDestroy(ClassObject obj)
	{
		if (obj is Transformable trans && mEffectAttachList.Remove(trans, out var list))
		{
			list.For(effect => destroyEffectAuto(effect, false));
			UN_SET(ref list);
		}
	}
}