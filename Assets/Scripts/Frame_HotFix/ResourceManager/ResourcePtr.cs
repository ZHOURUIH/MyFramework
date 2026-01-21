using static UnityUtility;
using UObject = UnityEngine.Object;

public class ResourcePtr<T> : ClassObject where T : UObject
{
	protected T mResource;                  // 引用的资源
	protected long mToken;					// 引用凭证,一般不允许外部直接访问
	protected static long mTokenSeed;       // 用于生成一个引用凭证
	public override void resetProperty()
	{
		base.resetProperty();
		mResource = null;
		mToken = 0;
	}
	public void setResource(T atlas)
	{
		mResource = atlas;
		mToken = 0;
		if (isValid())
		{
			use();
		}
	}
	public bool isValid() { return mResource != null; }
	public T getResource() { return mResource; }
	public long getToken() { return mToken; }
	// 只能由ResourceManager调用
	public void unuse()
	{
		if (mResource == null)
		{
			logError("atlas is null");
			return;
		}
		mToken = 0;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private void use()
	{
		if (mResource == null)
		{
			logError("resource is null");
			return;
		}
		mToken = ++mTokenSeed;
	}
}