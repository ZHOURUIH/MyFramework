using System.Collections.Generic;
using UnityEngine;

// NGUI形状接口,自定义形状的UI元素需实现此接口
public interface INGUIShape
{
	void onPointsChanged();
	List<Vector3> getVertices();
	List<Color> getColors();
	List<Vector2> getUVs();
	void setColor(Color color);
	Color getColor();
	bool isDirty();
	void setDirty(bool dirty);
	List<Vector2> getPolygonPoints();
}