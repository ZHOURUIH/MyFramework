
public interface IScrollItem
{
	void lerpItem(IScrollContainer curItem, IScrollContainer nextItem, float percent);
	myUGUIObject getItemRoot();
}