using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraLinkerSmoothFollow : CameraLinker
{
	protected Dictionary<CHECK_DIRECTION, List<CheckLayer>> mCheckDirectionList;   // 对于任意层可以进行多个方向的检测,但是同一层在同一方向不能多次检测
	protected List<CheckLayer> mCheckLayer;     // 当摄像机碰撞到某些层的时候,需要移动摄像机的目标位置,避免穿插
	protected float mSpeedRecover;
	protected float mNormalSpeed;
	protected float mFollowPositionSpeed;
	protected bool mIgnoreY;      // 是否忽略Y轴的变化,当Y轴变化时摄像机在Y轴上的位置不会根据时间改变
	public CameraLinkerSmoothFollow()
	{
		mCheckLayer = new List<CheckLayer>();
		mCheckDirectionList = new Dictionary<CHECK_DIRECTION, List<CheckLayer>>();
		mSpeedRecover = 0.5f;
		mNormalSpeed = 5.0f;
		mFollowPositionSpeed = mNormalSpeed;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCheckDirectionList.Clear();
		mCheckLayer.Clear();
		mSpeedRecover = 0.5f;
		mNormalSpeed = 5.0f;
		mFollowPositionSpeed = mNormalSpeed;
		mIgnoreY = false; ;
	}
	public void setFollowPositionSpeed(float speed) { mFollowPositionSpeed = speed; }
	public void setIgnoreY(bool ignore) { mIgnoreY = ignore; }
	public bool isIgnoreY() { return mIgnoreY; }
	public void setNormalSpeed(float speed) { mNormalSpeed = speed; }
	public float getNormalSpeed() { return mNormalSpeed; }
	public void addCheckLayer(int layer, CHECK_DIRECTION direction, float checkDistance, float minDistance)
	{
		CheckLayer checkLayer = new CheckLayer(layer, direction, checkDistance, minDistance);
		mCheckLayer.Add(checkLayer);
		if (!mCheckDirectionList.TryGetValue(direction, out List<CheckLayer> layerList))
		{
			layerList = new List<CheckLayer>();
			mCheckDirectionList.Add(direction, layerList);
		}
		layerList.Add(checkLayer);
	}
	public void removeCheckLayer(int layer, CHECK_DIRECTION direction)
	{
		if (!mCheckDirectionList.TryGetValue(direction, out List<CheckLayer> layerList))
		{
			return;
		}
		int count = layerList.Count;
		for (int i = 0; i < count; ++i)
		{
			if (layerList[i].mLayerIndex == layer)
			{
				mCheckLayer.Remove(layerList[i]);
				layerList.RemoveAt(i);
				break;
			}
		}
	}
	//---------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		if (!isFloatEqual(mNormalSpeed, mFollowPositionSpeed))
		{
			mFollowPositionSpeed = lerp(mFollowPositionSpeed, mNormalSpeed, mSpeedRecover * elapsedTime);
		}
		Vector3 targetPos = mLinkObject.getWorldPosition();
		Vector3 relative = rotateVector3(mRelativePosition, toRadian(mLinkObject.getRotation().y));
		Vector3 nextPos = targetPos + relative;
		Ray ray = new Ray();
		// 判断与地面的交点,使摄像机始终位于地面上方
		if (mCheckLayer != null && mCheckLayer.Count > 0)
		{
			// 从摄像机目标点检测
			foreach (var item in mCheckDirectionList)
			{
				List<CheckLayer> layerList = item.Value;
				int count = layerList.Count;
				for (int i = 0; i < count; ++i)
				{
					CheckLayer layer = layerList[i];
					ray.origin = nextPos - layer.mDirectionVector;
					ray.direction = layer.mDirectionVector;
					if (Physics.Raycast(ray, out RaycastHit hit, layer.mCheckDistance, 1 << layer.mLayerIndex))
					{
						// 如果有碰撞到物体,交点距离在一定范围内
						Vector3 hitPoint = ray.origin + ray.direction * hit.distance;
						if (lengthLess(nextPos - hitPoint, layer.mMinDistance))
						{
							nextPos = hitPoint - layer.mDirectionVector * layer.mMinDistance;
						}
					}
				}
			}
		}
		// 得到摄像机当前位置
		Vector3 cameraNewPos = lerp(mCamera.getPosition(), nextPos, mFollowPositionSpeed * elapsedTime, 0.01f);
		applyRelativePosition(cameraNewPos - targetPos);
	}
}