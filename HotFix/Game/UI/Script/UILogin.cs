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
<<<<<<< HEAD:HotFix/Game/UI/Script/UILogin.cs
	protected void onLoginClick(IMouseEventCollect go, Vector3 pos)
=======
	protected void onLoginClick(IMouseEventCollect go, Vector3 mousePos)
>>>>>>> 6ec2010caa4bea5077ba1ae3b503cd20701df589:HotFix/Game/UI/Script/ScriptLogin.cs
	{
		changeProcedure(typeof(MainSceneGaming));
	}
}