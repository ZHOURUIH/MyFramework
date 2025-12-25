using UnityEngine;
using static WidgetUtility;

[RequireComponent(typeof(RectTransform))]
public class LayoutAutoGrid : MonoBehaviour
{
	public float mIntervalX;
	public float mIntervalY;
	public HORIZONTAL_DIRECTION mHorizontal = HORIZONTAL_DIRECTION.CENTER;
	public VERTICAL_DIRECTION mVertical = VERTICAL_DIRECTION.TOP;
	public bool mRefresh;
	public void Awake()
	{
		if (Application.isPlaying)
		{
			enabled = false;
		}
	}
	public void OnValidate()
	{
		if (mRefresh)
		{
			mRefresh = false;
			doAutoGrid();
		}
	}
	public void doAutoGrid()
	{
		var child = transform.GetChild(0) as RectTransform;
		if (child == null)
		{
			Debug.LogError("第一个子节点需要是RectTransform");
			return;
		}
		autoGrid(transform as RectTransform, child.rect.size, new(mIntervalX, mIntervalY), mHorizontal, mVertical);
	}
}