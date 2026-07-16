using UnityEngine;

// 纵向排列所有子节点
[RequireComponent(typeof(RectTransform))]
public class LayoutGridVertical : MonoBehaviour
{
	public float mInterval;
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
		(transform as RectTransform).autoGridVertical(mInterval);
	}
}