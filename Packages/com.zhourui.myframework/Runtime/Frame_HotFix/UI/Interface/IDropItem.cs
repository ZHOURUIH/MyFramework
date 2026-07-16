
// 自定义的下拉列表的项
public interface IDropItem
{
	public string getText();
	public int getCustomValue();
	public void setText(string text);
	public void setCustomValue(int value);
	public void setParent(UGUIDropListBase parent);
}