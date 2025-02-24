
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
		mLogin.registeCollider(onLoginClick);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLoginClick()
	{
		CSLogin.send();
	}
}