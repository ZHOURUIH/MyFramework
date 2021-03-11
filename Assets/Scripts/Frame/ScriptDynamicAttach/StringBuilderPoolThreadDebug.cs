using UnityEngine;

public class StringBuilderPoolThreadDebug : MonoBehaviour
{
	public string Inuse;
	public string Unuse;
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		var inuse = FrameBase.mStringBuilderPoolThread.getInusedList();
		Inuse = inuse.Count + "个";
		var unuse = FrameBase.mStringBuilderPoolThread.getUnusedList();
		Unuse = unuse.Count + "个";
	}
	//-------------------------------------------------------------------------------------------------------
}