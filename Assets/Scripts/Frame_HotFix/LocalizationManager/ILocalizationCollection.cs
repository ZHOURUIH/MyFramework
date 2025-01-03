
// 包含myUGUIText和myUGUIImage的收集类的接口,派生类需要收集所有相关的需要本地化的myUGUIText和myUGUIImage对象
public interface ILocalizationCollection
{
	public void addLocalizationObject(myUIObject obj);
}