using UnityEngine;
using static WidgetUtility;

[RequireComponent(typeof(RectTransform))]
public class LayoutGridHorizontal : MonoBehaviour
{
	public float mInterval;
	public bool mChangeRootSize;
	public bool mRefresh;
	public void Awake()
	{
		enabled = false;
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
		autoGridHorizontal(transform as RectTransform, mInterval, mChangeRootSize);
	}
}