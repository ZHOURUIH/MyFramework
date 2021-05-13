using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptLogin : LayoutScript
{
	protected myUGUIObject mLogin;
	public override void assignWindow()
	{
		newObject(out mLogin, "Login");
	}
	public override void init()
	{
		registeCollider(mLogin, onLoginClick);
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void onLoginClick(IMouseEventCollect go)
	{
		changeProcedure(typeof(MainSceneGaming));
	}
}