using System.Collections.Generic;
using UnityEngine;

// 特效管理器调试信息
public class EffectManagerDebug : MonoBehaviour
{
	public List<GameObject> EffectList = new List<GameObject>();	// 特效列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		EffectList.Clear();
		var effectList = FrameBase.mEffectManager.getEffectList();
		foreach(var item in effectList)
		{
			EffectList.Add(item.getObject());
		}
	}
}