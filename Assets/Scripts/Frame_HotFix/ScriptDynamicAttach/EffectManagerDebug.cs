using System.Collections.Generic;
using UnityEngine;
using static FrameBase;

// 特效管理器调试信息
public class EffectManagerDebug : MonoBehaviour
{
	public List<GameObject> EffectList = new(); // 特效列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		EffectList.Clear();
		foreach (GameEffect item in mEffectManager.getEffectList().getMainList())
		{
			EffectList.Add(item.getObject());
		}
	}
}