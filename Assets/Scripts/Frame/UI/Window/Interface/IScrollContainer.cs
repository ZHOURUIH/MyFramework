using UnityEngine;

public interface IScrollContainer
{
	void initOrigin();
	float getIndex();
	void setIndex(int index);
	float getOriginAlpha();
	Vector3 getOriginPosition();
	Vector3 getOriginScale();
	Vector3 getOriginRotation();
	int getOriginDepth();
	float getPosFromLast(IScrollContainer lastItem);
	float getControlValue();
	void setControlValue(float value);
}