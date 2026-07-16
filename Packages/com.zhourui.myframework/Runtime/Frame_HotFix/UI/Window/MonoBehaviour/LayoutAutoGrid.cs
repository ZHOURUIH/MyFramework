using UnityEngine;

// 排列子节点的组件,横向排列,满了就换行排下一排,确保所有子节点不会超过父节点的横向范围
[RequireComponent(typeof(RectTransform))]
public class LayoutAutoGrid : MonoBehaviour
{
	public float mIntervalX;
	public float mIntervalY;
	public bool mKeepTopSide;
	public HORIZONTAL_DIRECTION mHorizontal = HORIZONTAL_DIRECTION.CENTER;
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
		(transform as RectTransform).autoGrid(child.rect.size, new(mIntervalX, mIntervalY), mKeepTopSide, mHorizontal);
	}
}