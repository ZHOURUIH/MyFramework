using UnityEngine;

// 横向排列所有子节点,不换行
[RequireComponent(typeof(RectTransform))]
public class LayoutGridHorizontal : MonoBehaviour
{
	public float mInterval;
	public bool mChangeRootSize;
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
		(transform as RectTransform).autoGridHorizontal(mInterval, mChangeRootSize);
	}
}