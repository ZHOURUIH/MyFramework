
public interface IScrollItem
{
	void lerpItem(IScrollContainer curItem, IScrollContainer nextItem, float percent);
	myUIObject getItemRoot();
}