using UnityEngine;
using System.Collections;

#if USE_NGUI

public class txNGUIPanel : txNGUIObject
{
	protected UIPanel mPanel;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		mPanel = getUnityComponent<UIPanel>();
	}
	public override void setDepth(int depth)
	{
		// 不调用基类函数
		mPanel.depth = depth;
		if(mLayout.getRenderOrder() != depth)
		{
			mLayout.setRenderOrder(depth);
		}
		// 如果是布局的panel,则应该刷新该布局中所有按钮的深度
		if(this == mLayout.getLayoutPanel())
		{
			mLayout.refreshObjectDepth();
		}
	}
	public override int getDepth() { return mPanel.depth; }
	public bool isStatic() { return mPanel.widgetsAreStatic; }
	public void setStatic(bool s) { mPanel.widgetsAreStatic = s; }
}

#endif