using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IScrollItem
{
	void lerp(IScrollContainer curItem, IScrollContainer nextItem, float percent);
	void setCurControlValue(float value);
	float getCurControlValue();
	void setVisible(bool visible);
	bool isVisible();
}