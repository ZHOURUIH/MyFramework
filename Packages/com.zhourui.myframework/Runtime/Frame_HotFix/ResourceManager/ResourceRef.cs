using static UnityUtility;
using static FrameBaseHotFix;
using static FrameUtility;
using UObject = UnityEngine.Object;

// 资源引用,用于标记资源的引用计数
public class ResourceRef<T> : ClassObject where T : UObject
{
	protected T mResource;                  // 引用的资源
	protected long mToken;					// 引用凭证,一般不允许外部直接访问
	public override void resetProperty()
	{
		base.resetProperty();
		mResource = null;
		mToken = 0;
	}
	public void set(T res)
	{
		mResource = res;
		if (mResource == null)
		{
			logError("resource is null");
			return;
		}
		mToken = mResourceManager.addReference(mResource);
	}
	public bool isValid() { return mResource != null; }
	public T get() { return mResource; }
	public long getToken() { return mToken; }
	// 在UN_CLASS时自动被调用
	public override void destroy()
	{
		base.destroy();
		if (mResource == null)
		{
			logError("resource is null");
			return;
		}
		mResourceManager.removeReference(mResource, ref mToken);
	}
	// 对当前资源新创建一个引用对象出来,用于使多个地方对同一个资源拥有生命周期所有权
	public ResourceRef<T> copyRef()
	{
		CLASS(out ResourceRef<T> newObjRef).set(mResource);
		return newObjRef;
	}
}