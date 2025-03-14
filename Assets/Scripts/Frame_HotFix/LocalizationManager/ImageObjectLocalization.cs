
// 用于存储一个图片对象本地化需要的数据
public class ImageObjectLocalization : ClassObject
{
    public IUGUIImage mObject;              // 图片对象
    public string mImageNameWithoutSuffix;  // 不带语言后缀的图片名
	public override void resetProperty()
    {
        base.resetProperty();
        mObject = null;
        mImageNameWithoutSuffix = null;
	}
}