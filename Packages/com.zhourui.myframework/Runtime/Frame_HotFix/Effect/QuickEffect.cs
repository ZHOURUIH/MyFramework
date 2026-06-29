using System;
using UnityEngine;
using static UnityUtility;

// 快速播放的特效, 不保证效果与prefab中的效果完全一致, 因为会忽略很多参数
public class QuickEffect : GameEffect
{
	protected DateTime mLastPlayTime;			// 记录上一次播放的时间,如果长时间未播放,就可以销毁特效,回收到PrefabPool中
	public override void setObject(GameObject obj)
	{
		base.setObject(obj);
		if (!mTrailRenderers.isEmpty())
		{
			logError("拖尾效果不适用于设置为快速播放的特效,因为特效节点本身是不会改变位置的" + mFilePath);
			return;
		}
		foreach (ParticleSystem item in mParticleSystems)
		{
			ParticleSystem.MainModule main = item.main;
			if (main.loop)
			{
				logWarning("循环粒子不适用于设置为快速播放的特效,因为没办法停止指定的粒子,如果是特意这样做的,由于无法停止指定粒子,所以要么全部同时播放,要么全部停止:" + mFilePath);
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
	// 快速播放,使用一个粒子系统调用Emit来达到同时播放多个相同粒子系统的效果,且可以合批,效率非常高
	public void playQuick(Vector3 pos)
	{
		mLastPlayTime = DateTime.Now;
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
	public DateTime getLastPlayTime() { return mLastPlayTime; }
	public override void resetProperty()
	{
		base.resetProperty();
		mLastPlayTime = DateTime.MinValue;
	}
}