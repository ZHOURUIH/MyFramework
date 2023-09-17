using System.Collections.Generic;
using UnityEngine;

// 角色管理器的调试信息
public class CharacterManagerDebug : MonoBehaviour
{
	public List<string> CharacterList = new List<string>();	// 角色名字列表
	public void Update()
	{
		if (FrameBase.mGameFramework == null || !FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		CharacterList.Clear();
		var characterList = FrameBase.mCharacterManager.getCharacterList();
		foreach(var item in characterList)
		{
			CharacterList.Add(item.Key.ToString() + ", " + item.Value.getName());
		}
	}
}