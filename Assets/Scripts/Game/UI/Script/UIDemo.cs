using UnityEngine;
using UnityEngine.UI;

public class UIDemo : GameLayout
{
	protected Transform mBackground;
	protected Text mLabel;
	public override void assignWindow()
	{
		getUIComponent(out mBackground, "Background");
		getUIComponent(out mLabel, "Label");
	}
	public override void init(){}
	public void setText(string text)
	{
		mLabel.text = text;
	}
}