using UnityEngine;
using System;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static UnityUtility;
using static FrameUtility;
using static FrameDefine;

// 特效池
public class GameEffectPool
{
	protected Dictionary<string, SafeList<GameEffect>> mUnusedEffectList = new();       // key是特效路径,value是未使用的特效列表
	protected Dictionary<string, List<GameEffect>> mInusedEffectList = new();           // key是特效路径,value是已使用的特效列表
	protected float mEffectTimer;														// 检查特效回收时间的计时器
	protected int mUnuseMaxTime = 60;													// 超过60秒未使用的特效将会被回收
	public void update(float elapsedTime)
	{
		if (tickTimerLoop(ref mEffectTimer, elapsedTime, 1.0f))
		{
            DateTime time = DateTime.Now;
            foreach (var item in mUnusedEffectList)
            {
                using var a = new SafeListReader<GameEffect>(item.Value);
                foreach (GameEffect effect in a.mReadList)
                {
                    if ((time - effect.getUnuseTime()).TotalSeconds > mUnuseMaxTime)
                    {
                        mEffectManager.destroyEffect(effect, true);
                    }
                }
            }
        }
	}
	public void setUnuseMaxTime(int time) { mUnuseMaxTime = time; }
	public void useEffect(GameEffect effect)
	{
		mInusedEffectList.getOrAddNew(effect.getFilePath()).Add(effect);
	}
	public void unuseEffect(GameEffect effect)
	{
		mInusedEffectList.get(effect.getFilePath())?.Remove(effect);
		mUnusedEffectList.getOrAddNew(effect.getFilePath()).add(effect);
		effect.setUnuseTime(DateTime.Now);
	}
	public void removeEffect(GameEffect effect)
	{
		mInusedEffectList.get(effect.getFilePath())?.Remove(effect);
		mUnusedEffectList.get(effect.getFilePath())?.remove(effect);
	}
	public GameEffect getOneEffect(GameObject parent, string nameWithPath, Vector3 pos, bool moveToHide, bool active, float lifeTime)
	{
		// 先从未使用列表中获取一个特效
		if (!mUnusedEffectList.TryGetValue(nameWithPath, out var effectList) || effectList.count() == 0)
		{
			return null;
		}
		using var a = new ProfilerScope(0);
		GameEffect effect = effectList.removeAt(effectList.count() - 1);
		if (effect.getGameObject() == null)
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
		useEffect(effect);
		return effect;
	}
	// 检查特效是否还有效,特效物体已经被销毁的视为无效特效,将被清除
	public void clearInvalidEffect()
	{
		using var b = new ListScope<GameEffect>(out var tempList);
		foreach (var item in mUnusedEffectList)
		{
			item.Value.For(effect => tempList.addIf(effect, !effect.checkValid()));
		}
		foreach (var item in mInusedEffectList)
		{
			item.Value.For(effect => tempList.addIf(effect, !effect.checkValid()));
		}
		tempList.For(effect => mEffectManager.destroyEffect(effect, true));
	}
}