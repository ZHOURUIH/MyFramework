using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;

// 用于操作Transformable,大部分的操作是用于代替Dotween的缓动操作,以及扩展的其他操作
public class FT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 旋转
	#region 在普通更新中用关键帧旋转物体
	public static void ROTATE_FIXED(Transformable obj, bool lockRotation = true)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotateFixed cmd, LOG_LEVEL.LOW);
		cmd.mActive = lockRotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_FIXED(Transformable obj, Vector3 rot, bool lockRotation = true)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotateFixed cmd, LOG_LEVEL.LOW);
		cmd.mActive = lockRotation;
		cmd.mFixedEuler = rot;
		pushCommand(cmd, obj);
	}
	public static void ROTATE(Transformable obj)
	{
		ROTATE(obj, Vector3.zero);
	}
	public static void ROTATE(Transformable obj, Vector3 rotation)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotate cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE(Transformable obj, Vector3 start, Vector3 target, float time)
	{
		ROTATE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		ROTATE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		ROTATE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		ROTATE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE(Transformable obj, Vector3 rotation)");
			return;
		}
		CMD(out CmdTransformableRotate cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_SPEED(Transformable obj)
	{
		ROTATE_SPEED(obj, Vector3.zero, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED(Transformable obj, Vector3 speed)
	{
		ROTATE_SPEED(obj, speed, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED(Transformable obj, Vector3 speed, Vector3 startAngle)
	{
		ROTATE_SPEED(obj, speed, startAngle, Vector3.zero);
	}
	public static void ROTATE_SPEED(Transformable obj, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotateSpeed cmd, LOG_LEVEL.LOW);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧旋转物体
	public static void ROTATE_FIXED_PHY(Transformable obj, bool lockRotation = true)
	{
		CMD(out CmdTransformableRotateFixed cmd, LOG_LEVEL.LOW);
		cmd.mActive = lockRotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_FIXED_PHY(Transformable obj, Vector3 rot, bool lockRotation = true)
	{
		CMD(out CmdTransformableRotateFixed cmd, LOG_LEVEL.LOW);
		cmd.mActive = lockRotation;
		cmd.mFixedEuler = rot;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_PHY(Transformable obj, Vector3 rotation)
	{
		CMD(out CmdTransformableRotate cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		cmd.mUpdateInFixedTick = true;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_PHY(Transformable obj, Vector3 start, Vector3 target, float time)
	{
		ROTATE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		ROTATE_PHY_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		ROTATE_PHY_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		ROTATE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PHY_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE_PHY(Transformable obj, Vector3 rotation)");
			return;
		}
		CMD(out CmdTransformableRotate cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		cmd.mUpdateInFixedTick = true;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_SPEED_PHY(Transformable obj)
	{
		ROTATE_SPEED(obj, Vector3.zero, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(Transformable obj, Vector3 speed)
	{
		ROTATE_SPEED(obj, speed, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(Transformable obj, Vector3 speed, Vector3 startAngle)
	{
		ROTATE_SPEED(obj, speed, startAngle, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(Transformable obj, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		CMD(out CmdTransformableRotateSpeed cmd, LOG_LEVEL.LOW);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		cmd.mUpdateInFixedTick = true;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定角度列表旋转物体
	#region 以指定角度列表旋转物体
	public static void ROTATE_CURVE(Transformable obj)
	{
		pushCommand<CmdTransformableRotateCurve>(obj, LOG_LEVEL.LOW);
	}
	public static void ROTATE_CURVE(Transformable obj, List<Vector3> rotList, float onceLength)
	{
		ROTATE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, rotList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, rotList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CMD(out CmdTransformableRotateCurve cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mRotateList = rotList;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 移动
	#region 在普通更新中用关键帧移动物体
	public static void MOVE(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableMove cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartPos = Vector3.zero;
		cmd.mTargetPos = Vector3.zero;
		pushCommand(cmd, obj);
	}
	public static void MOVE(Transformable obj, Vector3 pos)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableMove cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		pushCommand(cmd, obj);
	}
	public static void MOVE(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, null, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback tremblingCallBack, KeyFrameCallback trembleDoneCallBack)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, tremblingCallBack, trembleDoneCallBack);
	}
	public static void MOVE_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback tremblingCallBack, KeyFrameCallback trembleDoneCallBack)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableMove cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = tremblingCallBack;
		cmd.mDoneCallback = trembleDoneCallBack;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧移动物体
	public static void MOVE_PHY(Transformable obj, Vector3 pos)
	{
		CMD(out CmdTransformableMove cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		cmd.mUpdateInFixedTick = true;
		pushCommand(cmd, obj);
	}
	public static void MOVE_PHY(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		MOVE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PHY(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOV_PHY(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CMD(out CmdTransformableMove cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		cmd.mUpdateInFixedTick = true;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以抛物线移动物体
	#region 以抛物线移动物体
	public static void MOVE_PARABOLA(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdTransformableMoveParabola>(obj, LOG_LEVEL.LOW);
	}
	public static void MOVE_PARABOLA(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength)
	{
		MOVE_PARABOLA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, offset, null, null);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableMoveParabola cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTopHeight = topHeight;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表的路线移动物体
	#region 以指定点列表的路线移动物体
	public static void MOVE_CURVE(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdTransformableMoveCurve>(obj, LOG_LEVEL.LOW);
	}
	public static void MOVE_CURVE(Transformable obj, List<Vector3> posList, float onceLength)
	{
		MOVE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, int keyframe, List<Vector3> posList, float onceLength)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, float offset)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, offset, null, null);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, posList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, posList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, int keyframe, Span<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveCurveSpan.execute(obj, posList, onceLength, 0.0f, keyframe, false, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, int keyframe, Span<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveCurveSpan.execute(obj, posList, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableMoveCurve cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mPosList = posList;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线移动物体
	#region 以指定点列表以及时间点的路线移动物体
	public static void MOVE_PATH(Transformable obj)
	{
		CmdTransformableMovePath.execute(obj);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, keyframe, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CmdTransformableMovePath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, keyframe, loop);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 插值位置
	#region 插值位置
	public static void LERP_POSITION(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdTransformableLerpPosition>(obj, LOG_LEVEL.LOW);
	}
	public static void LERP_POSITION(Transformable obj, Vector3 targetPosition, float lerpSpeed)
	{
		LERP_POSITION_EX(obj, targetPosition, lerpSpeed, null, null);
	}
	public static void LERP_POSITION_EX(Transformable obj, Vector3 targetPosition, float lerpSpeed, LerpCallback doneCallback)
	{
		LERP_POSITION_EX(obj, targetPosition, lerpSpeed, null, doneCallback);
	}
	public static void LERP_POSITION_EX(Transformable obj, Vector3 targetPosition, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_POSITION(Transformable obj)");
			return;
		}
		CMD(out CmdTransformableLerpPosition cmd, LOG_LEVEL.LOW);
		cmd.mTargetPosition = targetPosition;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 插值旋转
	#region 插值旋转
	public static void LERP_ROTATION(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdTransformableLerpRotation>(obj, LOG_LEVEL.LOW);
	}
	public static void LERP_ROTATION(Transformable obj, Vector3 targetRotation, float lerpSpeed)
	{
		LERP_ROTATION_EX(obj, targetRotation, lerpSpeed, null, null);
	}
	public static void LERP_ROTATION_EX(Transformable obj, Vector3 targetRotation, float lerpSpeed, LerpCallback doneCallback)
	{
		LERP_ROTATION_EX(obj, targetRotation, lerpSpeed, null, doneCallback);
	}
	public static void LERP_ROTATION_EX(Transformable obj, Vector3 targetRotation, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_ROTATION(Transformable obj)");
			return;
		}
		CMD(out CmdTransformableLerpRotation cmd, LOG_LEVEL.LOW);
		cmd.mTargetRotation = targetRotation;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线旋转物体
	#region 以指定点列表以及时间点的路线旋转物体
	public static void ROTATE_PATH(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushCommand<CmdTransformableRotatePath>(obj, LOG_LEVEL.LOW);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ROTATE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		ROTATE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(Transformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotatePath cmd, LOG_LEVEL.LOW);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		cmd.mKeyframe = keyframe;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 追踪物体
	#region 追踪物体
	public static void TRACK_TARGET(Transformable obj)
	{
		TRACK_TARGET(obj, null, 0.0f, 0.0f, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed)
	{
		TRACK_TARGET(obj, target, speed, 0.0f, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, 0.0f, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, Vector3 offset)
	{
		TRACK_TARGET(obj, target, speed, 0.0f, offset, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, TrackCallback doingCallback, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, 0.0f, Vector3.zero, doingCallback, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, Vector3 offset, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, 0.0f, offset, null, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, float nearRange, Vector3 offset, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, nearRange, offset, null, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, float nearRange, Vector3 offset, TrackCallback trackingCallback, TrackCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableTrackTarget cmd, LOG_LEVEL.LOW);
		cmd.mTarget = target;
		cmd.mDoingCallback = trackingCallback;
		cmd.mDoneCallback = doneCallback;
		cmd.mOffset = offset;
		cmd.mNearRange = nearRange;
		cmd.mSpeed = speed;
		pushCommand(cmd, obj);
	}
	#endregion
	#region 抛物线追踪物体
	public static void TRACK_TARGET_PARABOLA(Transformable obj)
	{
		TRACK_TARGET_PARABOLA(obj, null, 0.0f, 0.0f, 0.0f, Vector3.zero, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET_PARABOLA(Transformable obj, Transformable target, float speed, float maxHeight, Vector3 start, TrackCallback doneCallback)
	{
		TRACK_TARGET_PARABOLA(obj, target, speed, 0.0f, maxHeight, start, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(Transformable obj, Transformable target, float speed, float maxHeight, Vector3 start, Vector3 offset, TrackCallback doneCallback)
	{
		TRACK_TARGET_PARABOLA(obj, target, speed, 0.0f, maxHeight, start, offset, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(Transformable obj, Transformable target, float speed, float nearRange, float maxHeight, Vector3 start, TrackCallback doneCallback)
	{
		TRACK_TARGET_PARABOLA(obj, target, speed, nearRange, maxHeight, start, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(Transformable obj, Transformable target, float speed, float nearRange, float maxHeight, Vector3 start, Vector3 offset, TrackCallback trackingCallback, TrackCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableTrackTargetParabola cmd, LOG_LEVEL.LOW);
		cmd.mTarget = target;
		cmd.mDoingCallback = trackingCallback;
		cmd.mDoneCallback = doneCallback;
		cmd.mStartPosition = start;
		cmd.mOffset = offset;
		cmd.mNearRange = nearRange;
		cmd.mSpeed = speed;
		cmd.mMaxHeight = maxHeight;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 始终朝向物体
	#region 始终朝向物体
	public static void ROTATE_FOCUS(Transformable obj)
	{
		ROTATE_FOCUS(obj, null, Vector3.zero);
	}
	public static void ROTATE_FOCUS(Transformable obj, Transformable target)
	{
		ROTATE_FOCUS(obj, target, Vector3.zero);
	}
	public static void ROTATE_FOCUS(Transformable obj, Transformable target, Vector3 offset)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableRotateFocus cmd, LOG_LEVEL.LOW);
		cmd.mTarget = target;
		cmd.mOffset = offset;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 锁定世界坐标分量
	#region 锁定世界坐标的指定分量
	public static void LOCK_POSITION_X(Transformable obj, float x)
	{
		LOCK_POSITION(obj, new(x, 0.0f, 0.0f), true, false, false);
	}
	public static void LOCK_POSITION_Y(Transformable obj, float y)
	{
		LOCK_POSITION(obj, new(0.0f, y, 0.0f), false, true, false);
	}
	public static void LOCK_POSITION_Z(Transformable obj, float z)
	{
		LOCK_POSITION(obj, new(0.0f, 0.0f, z), false, false, true);
	}
	public static void LOCK_POSITION(Transformable obj)
	{
		LOCK_POSITION(obj, Vector3.zero, false, false, false);
	}
	public static void LOCK_POSITION(Transformable obj, Vector3 pos, bool lockX, bool lockY, bool lockZ)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableLockPosition cmd, LOG_LEVEL.LOW);
		cmd.mLockPosition = pos;
		cmd.mLockX = lockX;
		cmd.mLockY = lockY;
		cmd.mLockZ = lockZ;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 缩放
	#region 用关键帧缩放物体
	public static void SCALE(Transformable obj)
	{
		SCALE(obj, Vector3.one);
	}
	public static void SCALE(Transformable obj, Vector3 scale)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableScale cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushCommand(cmd, obj);
	}
	public static void SCALE(Transformable obj, float scale)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableScale cmd, LOG_LEVEL.LOW);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = new(scale, scale, scale);
		cmd.mTargetScale = new(scale, scale, scale);
		pushCommand(cmd, obj);
	}
	public static void SCALE(Transformable obj, float start, float target, float onceLength)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, float start, float target, float onceLength)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, loop, offset, null, null);
	}
	public static void SCALE(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void SCALE_EX(Transformable obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, loop, offset, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CMD(out CmdTransformableScale cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线缩放物体
	#region 以指定点列表以及时间点的路线缩放物体
	public static void SCALE_PATH(Transformable obj)
	{
		CmdTransformableScalePath.execute(obj);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.one, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void SCALE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		SCALE_PATH_EX(obj, KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(Transformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableScalePath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, keyframe, loop);
	}
	#endregion
	// 以指定角度列表旋转物体
	#region 以指定角度列表旋转物体
	public static void SCALE_CURVE(Transformable obj)
	{
		pushCommand<CmdTransformableScaleCurve>(obj, LOG_LEVEL.LOW);
	}
	public static void SCALE_CURVE(Transformable obj, List<Vector3> scaleList, float onceLength)
	{
		SCALE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE_CURVE(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength)
	{
		SCALE_CURVE_EX(obj, keyframe, scaleList, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE_CURVE(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop)
	{
		SCALE_CURVE_EX(obj, keyframe, scaleList, onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE_CURVE(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, float offset)
	{
		SCALE_CURVE_EX(obj, keyframe, scaleList, onceLength, loop, offset, null, null);
	}
	public static void SCALE_CURVE_EX(Transformable obj, List<Vector3> scaleList, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, List<Vector3> scaleList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, List<Vector3> scaleList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, List<Vector3> scaleList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, keyframe, scaleList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_CURVE_EX(obj, keyframe, scaleList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(Transformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CMD(out CmdTransformableScaleCurve cmd, LOG_LEVEL.LOW);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mScaleList = scaleList;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallback = doingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
}