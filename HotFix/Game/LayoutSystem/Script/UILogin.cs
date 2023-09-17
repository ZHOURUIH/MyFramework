using System;
using UnityEngine;
using static FrameUtility;

public class UILogin : LayoutScript
{
	protected myUGUIObject mLogin;
	public UILogin()
	{
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		newObject(out mLogin, "Login");
	}
	public override void init()
	{
		registeCollider(mLogin, onLoginClick);
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void onLoginClick(IMouseEventCollect go, Vector3 pos)
	{
		changeProcedure(typeof(MainSceneGaming));
	}
}