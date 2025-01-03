using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

// 相对位置会平滑插值的第三人称连接器
public class CameraLinkerSmoothFollow : CameraLinkerThirdPerson
{
	protected Dictionary<CHECK_DIRECTION, List<CheckLayer>> mCheckDirectionList = new();   // 对于任意层可以进行多个方向的检测,但是同一层在同一方向不能多次检测
	protected List<CheckLayer> mCheckLayer = new();     // 当摄像机碰撞到某些层的时候,需要移动摄像机的目标位置,避免穿插
	protected float mFollowPositionSpeed = 5.0f;		// 跟随的速度
	protected float mSpeedRecover = 0.5f;				// 速度恢复的速度
	protected float mNormalSpeed = 5.0f;				// 正常时的速度
	protected bool mIgnoreY;							// 是否忽略Y轴的变化,当Y轴变化时摄像机在Y轴上的位置不会根据时间改变
	public override void resetProperty()
	{
		base.resetProperty();
		mCheckDirectionList.Clear();
		mCheckLayer.Clear();
		mFollowPositionSpeed = 5.0f;
		mSpeedRecover = 0.5f;
		mNormalSpeed = 5.0f;
		mIgnoreY = false;
	}
	public void setFollowPositionSpeed(float speed) { mFollowPositionSpeed = speed; }
	public void setIgnoreY(bool ignore) { mIgnoreY = ignore; }
	public bool isIgnoreY() { return mIgnoreY; }
	public void setNormalSpeed(float speed) { mNormalSpeed = speed; }
	public float getNormalSpeed() { return mNormalSpeed; }
	public void addCheckLayer(int layer, CHECK_DIRECTION direction, float checkDistance, float minDistance)
	{
		CheckLayer checkLayer = new(layer, direction, checkDistance, minDistance);
		mCheckLayer.Add(checkLayer);
		mCheckDirectionList.tryGetOrAddNew(direction).Add(checkLayer);
	}
	public void removeCheckLayer(int layer, CHECK_DIRECTION direction)
	{
		var layerList = mCheckDirectionList.get(direction);
		int count = layerList.count();
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void updateLinker(float elapsedTime)
	{
		if (!isFloatEqual(mNormalSpeed, mFollowPositionSpeed))
		{
			mFollowPositionSpeed = lerp(mFollowPositionSpeed, mNormalSpeed, mSpeedRecover * elapsedTime);
		}
		Vector3 relative;
		if (mUseTargetYaw)
		{
			relative = rotateVector3(mRelativePosition, toRadian(mLinkObject.getRotation().y));
		}
		else
		{
			relative = mRelativePosition;
		}
		Vector3 targetPos = mLinkObject.getWorldPosition();
		Vector3 nextPos = targetPos + relative;
		// 判断与地面的交点,使摄像机始终位于地面上方
		if (mCheckLayer.Count > 0)
		{
			Ray ray = new();
			// 从摄像机目标点检测
			foreach (var layerList in mCheckDirectionList.Values)
			{
				foreach (CheckLayer layer in layerList)
				{
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

		// mNormalSpeed速度为0就表示不再插值,直接设置到目标位置即可
		if (isFloatZero(mNormalSpeed))
		{
			applyRelativePosition(nextPos - targetPos);
			return;
		}

		// 得到摄像机当前位置
		Vector3 cameraNewPos = lerp(mCamera.getPosition(), nextPos, mFollowPositionSpeed * elapsedTime, 0.01f);
		applyRelativePosition(cameraNewPos - targetPos);
	}
}