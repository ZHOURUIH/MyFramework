using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void onGenerateTriangle(List<Vector2> points, ref List<Vector3> triangleList);

public class NGUICustomShape : FrameBase, INGUIShape
{
	public List<Vector3> mVertices;
	public List<Color> mColors;
	public List<Vector2> mUVs;
	public Color mColor = Color.white;
	public List<Vector2> mPolygonPoints;
	public bool mDirty = true;
	public onGenerateTriangle mOnGenerateTriangle;
	public NGUICustomShape()
	{
		mVertices = new List<Vector3>();
		mColors = new List<Color>();
		mUVs = new List<Vector2>();
		mPolygonPoints = new List<Vector2>();
		mOnGenerateTriangle = generateTriangle;
	}
	public List<Vector3> getVertices() { return mVertices; }
	public List<Color> getColors() { return mColors; }
	public List<Vector2> getUVs() { return mUVs; }
	public Color getColor() { return mColor; }
	public bool isDirty() { return mDirty; }
	public void setDirty(bool dirty) { mDirty = dirty; }
	public void setColor(Color color)
	{
		mColor = color;
		mDirty = true;
	}
	public List<Vector2> getPolygonPoints() { return mPolygonPoints; }
	public void setPolygonPoints(List<Vector2> polygonPoints)
	{
		mPolygonPoints.Clear();
		mPolygonPoints.AddRange(polygonPoints);
		mDirty = true;
	}
	public void setPolygonPoints(List<Vector3> polygonPoints)
	{
		mPolygonPoints.Clear();
		int count = polygonPoints.Count;
		for(int i = 0; i < count; ++i)
		{
			mPolygonPoints.Add(polygonPoints[i]);
		}
		mDirty = true;
	}
	public void onPointsChanged()
	{
		// 计算顶点纹理坐标,颜色
		mVertices.Clear();
		mColors.Clear();
		mUVs.Clear();
		List<Vector2> polygonPoints = newList(out polygonPoints);
		polygonPoints.AddRange(mPolygonPoints);
		mOnGenerateTriangle?.Invoke(polygonPoints, ref mVertices);
		int verticeCount = mVertices.Count;
		for (int i = 0; i < verticeCount; ++i)
		{
			mUVs.Add(Vector2.zero);
			mColors.Add(mColor);
		}
		destroyList(polygonPoints);
	}
	protected static void generateTriangle(List<Vector2> vertice, ref List<Vector3> triangleList)
	{
		// 从第0个顶点开始,连接不相邻的顶点,如果有顶点不能相连的停止循环,移除已经生成三角形的顶点,再在剩下的点中，从当前点开始继续连接顶点
		int verticeCount = vertice.Count;
		int curIndex = 0;
		// 移除点时当前点的下标
		int removedIndex = -1;
		while (vertice.Count > 3)
		{
			if (canConnectPoint(vertice, curIndex, (curIndex + 2) % vertice.Count))
			{
				triangleList.Add(vertice[curIndex]);
				triangleList.Add(vertice[(curIndex + 1) % vertice.Count]);
				triangleList.Add(vertice[(curIndex + 2) % vertice.Count]);
				triangleList.Add(vertice[(curIndex + 2) % vertice.Count]);
				vertice.RemoveAt((curIndex + 1) % vertice.Count);
				curIndex %= vertice.Count;
				removedIndex = curIndex;
			}
			else
			{
				curIndex = (curIndex + 1) % vertice.Count;
				// 如果列表中所有的点遍历完以后都没发现可以连接的点,则退出循环,舍弃这部分无法连接的点
				if(removedIndex == curIndex)
				{
					break;
				}
			}
		}
		if (vertice.Count == 3)
		{
			triangleList.Add(vertice[0]);
			triangleList.Add(vertice[1]);
			triangleList.Add(vertice[2]);
			triangleList.Add(vertice[2]);
		}
	}
}