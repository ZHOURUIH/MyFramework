using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UGUILine : GameBase
{
	protected List<Vector3> mPointList;
	protected MeshRenderer mMeshRenderer;
	protected Transform mTransform;
	protected GameObject mObject;
	protected Mesh mMesh;
	protected float mWidth;     // 宽度的一半
	public UGUILine()
	{
		mWidth = 10.0f;
		mPointList = new List<Vector3>();
	}
	public void init(GameObject obj)
	{
		mObject = obj;
		mObject.SetActive(true);
		mTransform = mObject.GetComponent<Transform>();
		mMeshRenderer = mObject.GetComponent<MeshRenderer>();
		mMesh = mObject.GetComponent<MeshFilter>().mesh;
	}
	public virtual void destroy()
	{
		mMesh.Clear();
		mObject?.SetActive(false);
	}
	public Material getMaterial() { return mMeshRenderer.material; }
	public void setActive(bool active) { mObject.SetActive(active); }
	public void setWidth(float width) { mWidth = width; }
	public void setPointList(List<Vector3> list)
	{
		mPointList.Clear();
		int count = list.Count;
		for (int i = 0; i < count; ++i)
		{
			if (mPointList.Count > 0 && isVectorEqual(list[i], mPointList[mPointList.Count - 1]))
			{
				continue;
			}
			mPointList.Add(list[i]);
		}
		onPointsChanged();
	}
	public void setPointList(Vector3[] list)
	{
		mPointList.Clear();
		int count = list.Length;
		for (int i = 0; i < count; ++i)
		{
			if (mPointList.Count > 0 && isVectorEqual(list[i], mPointList[mPointList.Count - 1]))
			{
				continue;
			}
			mPointList.Add(list[i]);
		}
		onPointsChanged();
	}
	public void onPointsChanged()
	{
		// 先将模型数据清空
		mMesh.Clear();
		int pointCount = mPointList.Count;
		if (pointCount < 2)
		{
			return;
		}
		// 计算顶点,纹理坐标,颜色
		Vector3[] vertices = new Vector3[pointCount * 2];
		Color[] colors = new Color[pointCount * 2];
		Vector2[] uv = new Vector2[pointCount * 2];
		for (int i = 0; i < pointCount; ++i)
		{
			// 如果当前点跟上一个点相同,则取上一点计算出的结果
			if (i > 0 && i < pointCount - 1 && isVectorEqual(mPointList[i - 1], mPointList[i]))
			{
				vertices[2 * i + 0] = vertices[2 * (i - 1) + 0];
				vertices[2 * i + 1] = vertices[2 * (i - 1) + 1];
				colors[2 * i + 0] = colors[2 * (i - 1) + 0];
				colors[2 * i + 1] = colors[2 * (i - 1) + 1];
				uv[2 * i + 0] = uv[2 * (i - 1) + 0];
				uv[2 * i + 1] = uv[2 * (i - 1) + 1];
			}
			else
			{
				if (i == 0)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					vertices[2 * i + 0] = rotateVector3(dir, q0) * mWidth / sin(halfAngle);
					vertices[2 * i + 1] = rotateVector3(dir, q1) * mWidth / sin(halfAngle);
				}
				else if (i > 0 && i < pointCount - 1)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					Vector3 dir1 = (mPointList[i - 1] - mPointList[i]).normalized;
					float halfAngle = getAngleFromVector3ToVector3(dir, dir1, false) * 0.5f;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					if (halfAngle >= 0.0f)
					{
						vertices[2 * i + 0] = rotateVector3(dir, q0) * mWidth;
						vertices[2 * i + 1] = rotateVector3(dir, q1) * mWidth;
					}
					else
					{
						vertices[2 * i + 0] = rotateVector3(dir, q1) * mWidth;
						vertices[2 * i + 1] = rotateVector3(dir, q0) * mWidth;
					}
				}
				else if (i == pointCount - 1)
				{
					Vector3 dir = (mPointList[i] - mPointList[i - 1]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					vertices[2 * i + 0] = rotateVector3(dir, q0) * mWidth / sin(halfAngle);
					vertices[2 * i + 1] = rotateVector3(dir, q1) * mWidth / sin(halfAngle);
				}
				vertices[2 * i + 0] += mPointList[i];
				vertices[2 * i + 1] += mPointList[i];
				colors[2 * i + 0] = Color.green;
				colors[2 * i + 1] = Color.green;
				uv[2 * i + 0] = Vector2.zero;
				uv[2 * i + 1] = Vector2.zero;
			}
		}
		// 计算顶点索引,每两个点之间两个三角面，每个三角面三个顶点
		int[] triangles = new int[(pointCount - 1) * 6];
		for (int i = 0; i < pointCount - 1; ++i)
		{
			int startIndex = i * 6;
			int indexValue = i * 2;
			triangles[startIndex + 0] = indexValue + 0;
			triangles[startIndex + 1] = indexValue + 1;
			triangles[startIndex + 2] = indexValue + 2;
			triangles[startIndex + 3] = indexValue + 2;
			triangles[startIndex + 4] = indexValue + 1;
			triangles[startIndex + 5] = indexValue + 3;
		}

		// 将顶点数据设置到模型中
		mMesh.vertices = vertices;
		mMesh.colors = colors;
		mMesh.uv = uv;
		mMesh.triangles = triangles;
	}
}