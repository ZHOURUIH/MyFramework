using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBase;

// 窗口的碰撞检测相关逻辑
public class COMWindowCollider : GameComponent
{
	protected BoxCollider mBoxCollider;                         // 碰撞组件
	public override void resetProperty()
	{
		base.resetProperty();
		mBoxCollider = null;
	}
	public void setBoxCollider(BoxCollider collider)
	{
		mBoxCollider = collider;
		myUIObject window = mComponentOwner as myUIObject;
		GameLayout layout = window.getLayout();
		if (layout != null && layout.isCheckBoxAnchor() && mLayoutManager.isUseAnchor())
		{
			mBoxCollider.isTrigger = true;
			string layoutName = layout.getName();
			string windowName = window.getName();
			GameObject go = window.getObject();
			// BoxCollider的中心必须为0,因为UIWidget会自动调整BoxCollider的大小和位置,而且调整后位置为0,所以在制作时BoxCollider的位置必须为0
			if (!isFloatZero(mBoxCollider.center.sqrMagnitude))
			{
				logWarning("BoxCollider's center must be zero! Otherwise can not adapt to the screen sometimes! name : " + windowName + ", layout : " + layoutName);
			}
			if (!go.TryGetComponent<ResScaleAnchor>(out _) && !go.TryGetComponent<ResPaddingAnchor>(out _))
			{
				logWarning("Window with BoxCollider and Widget must has ScaleAnchor! Otherwise can not adapt to the screen sometimes! name : " + windowName + ", layout : " + layoutName);
			}
		}
	}
	public BoxCollider getBoxCollider() { return mBoxCollider; }
	public bool isHandleInput() { return mBoxCollider != null && mBoxCollider.enabled; }
	public bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		if (!mBoxCollider)
		{
			hit = new();
			return false;
		}
		return mBoxCollider.Raycast(ray, out hit, maxDistance);
	}
	public void enableCollider(bool enable)
	{
		if (mBoxCollider != null)
		{
			mBoxCollider.enabled = enable;
		}
	}
	public void setColliderSize(Vector2 size)
	{
		if (mBoxCollider == null)
		{
			return;
		}
		if (!isFloatEqual(size.x, mBoxCollider.size.x) || !isFloatEqual(size.y, mBoxCollider.size.y))
		{
			mBoxCollider.size = size;
			mBoxCollider.center = Vector2.zero;
		}
	}
	public void setColliderSize(RectTransform transform)
	{
		if (mBoxCollider == null || transform == null)
		{
			return;
		}
		Vector2 size = transform.rect.size;
		if (!isFloatEqual(size.x, mBoxCollider.size.x) || !isFloatEqual(size.y, mBoxCollider.size.y))
		{
			mBoxCollider.size = size;
			mBoxCollider.center = multiVector2(size, new Vector2(0.5f, 0.5f) - transform.pivot);
		}
	}
}