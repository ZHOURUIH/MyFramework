
// 加载图集的参数,用于回调的传参
public class AtlasLoadParam : ClassObject
{
	public string mName;						// 图集名
	public UGUIAtlasPtrCallback mCallback;		// 回调
	public bool mErrorIfNull;					// 加载失败时是否报错
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
		mCallback = null;
		mErrorIfNull = false;
	}
}