using UnityEngine;
using System.Collections;

#if USE_NGUI

public class txNGUIText : txNGUIObject
{	
	protected UILabel mLabel;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		mLabel = getUnityComponent<UILabel>();
	}
	public string getText(){return mLabel.text;}
	public void setText(string label){mLabel.text = label;}
	public override void setAlpha(float alpha, bool fadeChild) {mLabel.alpha = alpha;}
	public override void setDepth(int depth)
	{
		mLabel.depth = depth;
		base.setDepth(depth);
	}
	public override int getDepth(){return mLabel.depth;}
	public void setColor(Color color) { mLabel.color = color; }
	public Color getColor() { return mLabel.color; }
}

#endif