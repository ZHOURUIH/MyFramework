using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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