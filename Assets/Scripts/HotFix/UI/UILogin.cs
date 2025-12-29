using Obfuz;
using static FrameUtility;
using static GBH;

	// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UILogin : LayoutScript
{
	protected myUGUIObject mLogin;
	protected ScrollViewPanel mScrollViewPanel;
	// auto generate member end
	public UILogin()
	{
		// auto generate constructor start
		mScrollViewPanel = new(this);
		// auto generate constructor end
		mNeedUpdate = true;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out mLogin, "Login");
		mScrollViewPanel.assignWindow(mRoot, "ScrollViewPanel");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		mLogin.registeCollider(onLoginClick);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mScrollViewPanel.update(elapsedTime);
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