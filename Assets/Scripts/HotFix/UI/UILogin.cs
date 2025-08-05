using static FrameUtility;
using static GBH;

public class UILogin : LayoutScript
{
	// auto generate member start
	protected myUGUIObject mLogin;
	// auto generate member end
	public UILogin()
	{
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out mLogin, "Login");
		// auto generate assignWindow end
	}
	public override void init()
	{
		mLogin.registeCollider(onLoginClick);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLoginClick()
	{
		if (mNetManager.isConnected())
		{
			CSLogin.send();
		}
		else
		{
			changeProcedure<MainSceneGaming>();
		}
	}
}