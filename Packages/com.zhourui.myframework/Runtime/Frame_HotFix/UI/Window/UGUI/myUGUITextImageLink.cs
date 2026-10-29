
// myUGUITextImage使用的超链接碰撞子节点,链接跨行时每行对应一个节点。
public class myUGUITextImageLink : myUGUIObject
{
	protected string mLinkID;
	protected TextImageLinkCallback mCallback;
	public override void init()
	{
		base.init();
		registeCollider(onClick);
	}
	public void setLink(string id, TextImageLinkCallback callback)
	{
		mLinkID = id;
		mCallback = callback;
	}
	protected void onClick() { mCallback?.Invoke(mLinkID, this); }
}