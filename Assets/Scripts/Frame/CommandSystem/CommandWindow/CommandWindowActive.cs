using UnityEngine;
using System.Collections;

public class CommandWindowActive : Command
{
	public bool mActive;
	public override void init()
	{
		base.init();
		mActive = true;
	}
	public override void execute()
	{
		txUIObject uiObjcet = mReceiver as txUIObject;
		uiObjcet.setActive(mActive);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mActive:" + mActive;
	}
}