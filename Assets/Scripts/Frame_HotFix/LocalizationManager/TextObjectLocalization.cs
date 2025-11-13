using System.Collections.Generic;

// 用于存储一个文本对象本地化需要的数据
public class TextObjectLocalization : ClassObject
{
    public IUGUIText mObject;           // 文本对象
    public string mText;                // 主文本内容,中文的
    public int mID;                     // 文本在表格中的ID,ID和文本内容只需要一个即可
    public List<string> mParam = new(); // 文本参数,用于填充文本中的占位符
    public LocalizationCallback mCallback;    // 某些特殊规则的文本对象,不能使用通用的方式进行翻译,则通过回调函数的方式自定义显示翻译文本
	public override void resetProperty()
    {
        base.resetProperty();
        mObject = null;
        mText = null;
        mID = 0;
        mParam.Clear();
        mCallback = null;
	}
}