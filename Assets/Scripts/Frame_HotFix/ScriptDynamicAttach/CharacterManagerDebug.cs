using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 角色管理器的调试信息
public class CharacterManagerDebug : MonoBehaviour
{
	public List<string> CharacterList = new();	// 角色名字列表
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}
		CharacterList.Clear();
		foreach(var item in mCharacterManager.getCharacterList())
		{
			CharacterList.Add(item.Key.ToString() + ", " + item.Value.getName());
		}
	}
}