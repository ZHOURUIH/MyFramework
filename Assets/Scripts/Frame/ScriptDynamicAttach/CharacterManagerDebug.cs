using System.Collections.Generic;
using UnityEngine;

public class CharacterManagerDebug : MonoBehaviour
{
	public List<string> CharacterList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
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