using UnityEngine;
using System.Collections.Generic;
using static FrameBase;
using static MathUtility;

// 多指操作组件
public class ComponentMultiTouch : GameComponent
{
	protected Dictionary<int, Vector3> mTouchList = new();		// 当前触点列表,value是触点的初始位置
	protected Vector3Callback mTwoFingerMoveCallback;			// 双指进行平移操作的回调
	protected Float3Callback mTwoFingerScaleCallback;			// 双指进行缩放操作的回调
	protected FloatCallback mTwoFingerRotateCallback;			// 双指进行旋转操作的回调,只能进行二维平面的旋转
	protected MULTI_TOUCH_GESTURE mGesture;						// 当前手势类型
	protected float mRotateThreshold = 10.0f;					// 开始旋转的阈值,角度制
	protected float mScaleThreshold = 50.0f;					// 开始缩放的移动阈值
	protected float mMoveFingerStartDistanceThreshold = 400.0f;	// 开始平移的双指间距阈值,双指在开始平移时初始间距不能超过一定值
	protected float mMoveFingerDistanceThreshold = 30.0f;		// 开始平移的双指间距变化阈值,双指在开始平移时间距不能超过一定值
	protected float mMoveThreshold = 10.0f;                     // 开始平移的双指移动阈值,双指在开始平移时需要同时移动一定距离
	public override void resetProperty()
	{
		base.resetProperty();
		mTouchList.Clear();
		mTwoFingerMoveCallback = null;
		mTwoFingerScaleCallback = null;
		mTwoFingerRotateCallback = null;
		mGesture = MULTI_TOUCH_GESTURE.NONE;
		mRotateThreshold = 10.0f;
		mScaleThreshold = 50.0f;
		mMoveFingerStartDistanceThreshold = 400.0f;
		mMoveFingerDistanceThreshold = 30.0f;
		mMoveThreshold = 10.0f;
	}
	public override void setActive(bool active)
	{
		if (active == isActive())
		{
			return;
		}
		base.setActive(active);
		// 无论激活还是禁用,都需要将当前状态还原
		mGesture = MULTI_TOUCH_GESTURE.NONE;
		mTouchList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTouchList.Count == 2)
		{
			int touchID0 = 0;
			int touchID1 = 0;
			Vector3 startPos0 = Vector3.zero;
			Vector3 startPos1 = Vector3.zero;
			int index = 0;
			foreach (var item in mTouchList)
			{
				if (index == 0)
				{
					touchID0 = item.Key;
					startPos0 = item.Value;
				}
				else if (index == 1)
				{
					touchID1 = item.Key;
					startPos1 = item.Value;
				}
				++index;
			}
			TouchPoint touchPoint0 = mInputSystem.getTouchPoint(touchID0);
			TouchPoint touchPoint1 = mInputSystem.getTouchPoint(touchID1);
			if (touchPoint0 == null || touchPoint1 == null)
			{
				return;
			}
			Vector3 lastPos0 = touchPoint0.getLastPosition();
			Vector3 lastPos1 = touchPoint1.getLastPosition();
			Vector3 curPos0 = touchPoint0.getCurPosition();
			Vector3 curPos1 = touchPoint1.getCurPosition();
			if (mGesture == MULTI_TOUCH_GESTURE.NONE)
			{
				float startDistance = getLength(startPos0 - startPos1);
				float distanceDelta = Mathf.Abs(startDistance - getLength(curPos0 - curPos1));
				float angleDelta = toDegree(getAngleBetweenVector(startPos1 - startPos0, curPos1 - curPos0));
				// 旋转角度超过一定角度时,开始旋转
				if (angleDelta > mRotateThreshold)
				{
					mGesture = MULTI_TOUCH_GESTURE.TWO_FINGER_ROTATE;
				}
				// 旋转角度很小,但是双指距离增加超过一定距离,开始缩放
				else if (distanceDelta > mScaleThreshold)
				{
					mGesture = MULTI_TOUCH_GESTURE.TWO_FINGER_SCALE;
				}
				// 旋转角度和双指距离变化都很小,一起平移超过一定距离,开始移动
				else if (distanceDelta < mMoveFingerDistanceThreshold && startDistance < mMoveFingerStartDistanceThreshold)
				{
					if (lengthGreater(startPos0 - curPos0, mMoveThreshold))
					{
						mGesture = MULTI_TOUCH_GESTURE.TWO_FINGER_MOVE;
					}
				}
			}
			else if (mGesture == MULTI_TOUCH_GESTURE.TWO_FINGER_MOVE)
			{
				// 取双指中点的移动量
				Vector3 lastMidPoint = (lastPos0 + lastPos1) * 0.5f;
				Vector3 curMidPoint = (curPos0 + curPos1) * 0.5f;
				// 参数是每帧的变化量,这样更符合正常思维逻辑
				mTwoFingerMoveCallback?.Invoke(curMidPoint - lastMidPoint);
			}
			else if (mGesture == MULTI_TOUCH_GESTURE.TWO_FINGER_SCALE)
			{
				float scaleDisLast = getLength(lastPos0 - lastPos1);
				float scaleDisNow = getLength(curPos0 - curPos1);
				float scaleDisStart = getLength(startPos0 - startPos1);
				// 参数是相对于开始缩放操作时的缩放量,不是每帧的变化量,这样使误差更小一些
				mTwoFingerScaleCallback?.Invoke(divide(scaleDisNow, scaleDisStart), scaleDisNow - scaleDisStart, scaleDisNow - scaleDisLast);
			}
			else if (mGesture == MULTI_TOUCH_GESTURE.TWO_FINGER_ROTATE)
			{
				// 参数是相对于开始旋转时的旋转角度,弧度制,不是每帧的变化量,这样使误差更小一些
				mTwoFingerRotateCallback?.Invoke(getAngleVectorToVector(curPos0 - curPos1, startPos0 - startPos1, true));
			}
		}
		else
		{
			mGesture = MULTI_TOUCH_GESTURE.NONE;
		}
	}
	public void setTwoFingerMoveCallback(Vector3Callback callback)	{ mTwoFingerMoveCallback = callback; }
	// 传的第一个scale值是相对于缩放开始时的缩放量,不是每帧的变化量,取值范围是[0,+无限大)
	// 传的第二个scale值是双指移动的距离,是相对于缩放开始时的双指距离
	// 传的第三个scale是每帧的移动距离变化量
	public void setTwoFingerScaleCallback(Float3Callback callback)	{ mTwoFingerScaleCallback = callback; }
	public void setTwoFingerRotateCallback(FloatCallback callback)	{ mTwoFingerRotateCallback = callback; }
	public void setRotateThreshold(float value)						{ mRotateThreshold = value; }
	public void setScaleThreshold(float value)						{ mScaleThreshold = value; }
	public void setMoveFingerStartDistanceThreshold(float value)	{ mMoveFingerStartDistanceThreshold = value; }
	public void setMoveFingerDistanceThreshold(float value)			{ mMoveFingerDistanceThreshold = value; }
	public void setMoveThreshold(float value)						{ mMoveThreshold = value; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onTouchStart(Vector3 vec3, int touchID)
	{
		mTouchList.TryAdd(touchID, vec3);
	}
	protected void onTouchEnd(Vector3 vec3, int touchID)
	{
		mTouchList.Remove(touchID);
	}
}