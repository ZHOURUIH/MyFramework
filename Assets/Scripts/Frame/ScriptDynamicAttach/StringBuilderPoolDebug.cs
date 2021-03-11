using UnityEngine;

public class StringBuilderPoolDebug : MonoBehaviour
{
	public string Inuse;
	public string Unuse;
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		var inuse = FrameBase.mStringBuilderPool.getInusedList();
		Inuse = inuse.Count + "个";
		var unuse = FrameBase.mStringBuilderPool.getUnusedList();
		Unuse = unuse.Count + "个";
	}
	//-------------------------------------------------------------------------------------------------------
}