using System;
using System.Collections.Generic;
using UnityEngine;

public struct Point
{
	public int x;
	public int y;
	public Point(int xx, int yy)
	{
		x = xx;
		y = yy;
	}
	public int toIndex(int width)
	{
		return y * width + x;
	}
}