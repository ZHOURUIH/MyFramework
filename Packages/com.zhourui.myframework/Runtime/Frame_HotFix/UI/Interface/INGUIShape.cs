using System.Collections.Generic;
using UnityEngine;

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