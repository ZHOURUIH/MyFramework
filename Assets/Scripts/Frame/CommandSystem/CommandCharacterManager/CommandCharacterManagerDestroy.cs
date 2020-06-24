using UnityEngine;
using System.Collections;

public class CommandCharacterManagerDestroy : Command
{
	public uint mGUID;
	public string mName;
	public override void init()
	{
		base.init();
		mGUID = 0;
		mName = EMPTY_STRING;
	}
	public override void execute()
	{
		if(mGUID != 0)
		{
			mCharacterManager.destroyCharacter(mGUID);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mName:" + mName + ", mGUID:" + mGUID;
	}
}