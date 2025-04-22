using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 特效管理器调试信息
public class EffectManagerDebug : MonoBehaviour
{
	public List<GameObject> EffectList = new(); // 特效列表
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
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