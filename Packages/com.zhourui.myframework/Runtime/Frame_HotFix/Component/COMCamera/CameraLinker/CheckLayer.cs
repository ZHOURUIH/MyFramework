using UnityEngine;

// CameraLinkerSmoothFollow中使用的摄像机碰撞相关设置
public struct CheckLayer
{
	public CHECK_DIRECTION mDirection;		// 检测方向
	public Vector3 mDirectionVector;		// 根据方向得出的检测向量
	public float mCheckDistance;			// 检测距离
	public float mMinDistance;				// 检测最近距离
	public int mLayerIndex;					// 检测的层
	public CheckLayer(int layerIndex, CHECK_DIRECTION direction, float checkDistance, float minDistance)
	{
		mLayerIndex = layerIndex;
		mDirection = direction;
		mCheckDistance = checkDistance;
		mMinDistance = minDistance;
		switch (direction)
		{
			case CHECK_DIRECTION.DOWN:		mDirectionVector = Vector3.down;	break;
			case CHECK_DIRECTION.UP:		mDirectionVector = Vector3.up;		break;
			case CHECK_DIRECTION.LEFT:		mDirectionVector = Vector3.left;	break;
			case CHECK_DIRECTION.RIGHT:		mDirectionVector = Vector3.right;	break;
			case CHECK_DIRECTION.FORWARD:	mDirectionVector = Vector3.forward; break;
			case CHECK_DIRECTION.BACK:		mDirectionVector = Vector3.back;	break;
			default:						mDirectionVector = Vector3.zero;	break;
		}
	}
}