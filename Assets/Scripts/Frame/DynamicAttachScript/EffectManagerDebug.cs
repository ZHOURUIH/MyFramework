using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManagerDebug : MonoBehaviour
{
	public List<GameObject> EffectList = new List<GameObject>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
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