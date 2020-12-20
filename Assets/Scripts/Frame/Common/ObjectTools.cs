using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// ObjectTools
public class OT : FrameBase
{
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 旋转
	#region 在普通更新中用关键帧旋转物体
	public static void ROTATE_FIXED(Transformable obj, bool lockRotation = true)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableRotateFixed cmd = newMainCmd(out cmd, false, false);
		cmd.mActive = lockRotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_FIXED(Transformable obj, Vector3 rot, bool lockRotation = true)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableRotateFixed cmd = newMainCmd(out cmd, false, false);
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
		if(obj == null)
		{
			return;
		}
		CommandTransformableRotate cmd = newMainCmd(out cmd, false, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE(Transformable obj, Vector3 start, Vector3 target, float time)
	{
		ROTATE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		ROTATE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		ROTATE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		ROTATE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE(Transformable obj, Vector3 rotation)");
			return;
		}
		CommandTransformableRotate cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, Vector3 rotation)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableRotate cmd = newMainCmd(out cmd, false, true);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float time)
	{
		return ROTATE_DELAY(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f);
	}
	public static CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		return ROTATE_DELAY(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f);
	}
	public static CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		return ROTATE_DELAY(obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f);
	}
	public static CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		if (obj == null)
		{
			return null;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用CommandTransformableRotate ROTATE_DELAY(Transformable obj, float delayTime, Vector3 rotation)");
			return null;
		}
		CommandTransformableRotate cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
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
		CommandTransformableRotateSpeed cmd = newMainCmd(out cmd, false, false);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableRotateSpeed ROTATE_SPEED_DELAY(Transformable obj, float delayTime, Vector3 speed)
	{
		return ROTATE_SPEED_DELAY(obj, delayTime, speed, Vector3.zero, Vector3.zero);
	}
	public static CommandTransformableRotateSpeed ROTATE_SPEED_DELAY(Transformable obj, float delayTime, Vector3 speed, Vector3 startAngle)
	{
		return ROTATE_SPEED_DELAY(obj, delayTime, speed, startAngle, Vector3.zero);
	}
	public static CommandTransformableRotateSpeed ROTATE_SPEED_DELAY(Transformable obj, float delayTime, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableRotateSpeed cmd = newMainCmd(out cmd, false, true);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧旋转物体
	public static void ROTATE_FIXED_PHY(Transformable obj, bool lockRotation = true)
	{
		CommandTransformableRotateFixedPhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mActive = lockRotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_FIXED_PHY(Transformable obj, Vector3 rot, bool lockRotation = true)
	{
		CommandTransformableRotateFixedPhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mActive = lockRotation;
		cmd.mFixedEuler = rot;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_PHY(Transformable obj, Vector3 rotation)
	{
		CommandTransformableRotatePhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		pushCommand(cmd, obj);
	}
	public static void ROTATE_PHY(Transformable obj, Vector3 start, Vector3 target, float time)
	{
		ROTATE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		ROTATE_PHY_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		ROTATE_PHY_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		ROTATE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PHY_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if(keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE_PHY(Transformable obj, Vector3 rotation)");
			return;
		}
		CommandTransformableRotatePhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, Vector3 rotation)
	{
		CommandTransformableRotatePhysics cmd = newMainCmd(out cmd, false, true);
		cmd.mOnceLength = 0.0f;
		cmd.mStartRotation = rotation;
		cmd.mTargetRotation = rotation;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float time)
	{
		return ROTATE_PHY_DELAY(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, time, false, 0.0f);
	}
	public static CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		return ROTATE_PHY_DELAY(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f);
	}
	public static CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		return ROTATE_PHY_DELAY(obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f);
	}
	public static CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用CommandTransformableRotatePhysics ROTATE_PHY_DELAY(Transformable obj, float delayTime, Vector3 rotation)");
			return null;
		}
		CommandTransformableRotatePhysics cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartRotation = start;
		cmd.mTargetRotation = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
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
		CommandMovableObjectRotateSpeedPhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectRotateSpeedPhysics ROTATE_SPEED_PHY_DELAY(Transformable obj, float delayTime, Vector3 speed)
	{
		return ROTATE_SPEED_PHY_DELAY(obj, delayTime, speed, Vector3.zero, Vector3.zero);
	}
	public static CommandMovableObjectRotateSpeedPhysics ROTATE_SPEED_PHY_DELAY(Transformable obj, float delayTime, Vector3 speed, Vector3 startAngle)
	{
		return ROTATE_SPEED_PHY_DELAY(obj, delayTime, speed, startAngle, Vector3.zero);
	}
	public static CommandMovableObjectRotateSpeedPhysics ROTATE_SPEED_PHY_DELAY(Transformable obj, float delayTime, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		CommandMovableObjectRotateSpeedPhysics cmd = newMainCmd(out cmd, false, true);
		cmd.mRotateSpeed = speed;
		cmd.mStartAngle = startAngle;
		cmd.mRotateAcceleration = rotateAccelerationValue;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 以指定角度列表旋转物体
	#region 以指定角度列表旋转物体
	public static void ROTATE_CURVE(Transformable obj)
	{
		pushMainCommand<CommandTransformableRotateCurve>(obj, false);
	}
	public static void ROTATE_CURVE(Transformable obj, List<Vector3> rotList, float onceLength)
	{
		ROTATE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ROTATE_CURVE_EX(obj, keyframe, rotList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CommandTransformableRotateCurve cmd = newMainCmd(out cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mRotList = rotList;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY(Transformable obj, float delayTime)
	{
		return pushDelayMainCommand<CommandTransformableRotateCurve>(obj, delayTime, false);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY(Transformable obj, float delayTime, List<Vector3> rotList, float onceLength)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, keyframe, rotList, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, keyframe, rotList, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, keyframe, rotList, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY_EX(Transformable obj, float delayTime, List<Vector3> rotList, float onceLength, KeyFrameCallback doneCallback)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY_EX(Transformable obj, float delayTime, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ROTATE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableRotateCurve ROTATE_CURVE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CommandTransformableRotateCurve cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mRotList = rotList;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 移动
	#region 在普通更新中用关键帧移动物体
	public static void MOVE(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableMove cmd = newMainCmd(out cmd, false, false);
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
		CommandTransformableMove cmd = newMainCmd(out cmd, false, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		pushCommand(cmd, obj);
	}
	public static void MOVE(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		MOVE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, null, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		MOVE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback tremblingCallBack, KeyFrameCallback trembleDoneCallBack)
	{
		MOVE_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, tremblingCallBack, trembleDoneCallBack);
	}
	public static void MOVE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback tremblingCallBack, KeyFrameCallback trembleDoneCallBack)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableMove cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = tremblingCallBack;
		cmd.mTrembleDoneCallBack = trembleDoneCallBack;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableMove MOVE_DELAY(Transformable obj, float delayTime, Vector3 pos)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableMove cmd = newMainCmd(out cmd, false, true);
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		cmd.mOnceLength = 0.0f;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandTransformableMove MOVE_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength)
	{
		return MOVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMove MOVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		return MOVE_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMove MOVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		return MOVE_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableMove MOVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		return MOVE_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableMove MOVE_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		return MOVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static CommandTransformableMove MOVE_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		return MOVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static CommandTransformableMove MOVE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback moveDoneCallback)
	{
		return MOVE_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, loop, offset, null, moveDoneCallback);
	}
	public static CommandTransformableMove MOVE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableMove cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = movingCallback;
		cmd.mTrembleDoneCallBack = moveDoneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧移动物体
	public static void MOVE_PHY(Transformable obj, Vector3 pos)
	{
		CommandTransformableMovePhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		pushCommand(cmd, obj);
	}
	public static void MOVE_PHY(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		MOVE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PHY(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PHY_EX(obj, keyframe, startPos, targetPos, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CommandTransformableMovePhysics cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY(Transformable obj, float delayTime, Vector3 pos)
	{
		CommandTransformableMovePhysics cmd = newMainCmd(out cmd, false, true);
		cmd.mStartPos = pos;
		cmd.mTargetPos = pos;
		cmd.mOnceLength = 0.0f;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return MOVE_PHY_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableMovePhysics MOVE_PHY_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CommandTransformableMovePhysics cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 以抛物线移动物体
	#region 以抛物线移动物体
	public static void MOVE_PARABOLA(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableMoveParabola>(obj, false);
	}
	public static void MOVE_PARABOLA(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength)
	{
		MOVE_PARABOLA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, offset, null, null);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_PARABOLA_EX(obj, keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(Transformable obj, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableMoveParabola cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTopHeight = topHeight;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandTransformableMoveParabola>(obj, delayTime, false);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float topHeight, float onceLength)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, keyframe, startPos, targetPos, topHeight, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doneCallback)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback doneCallback)
	{
		return MOVE_PARABOLA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, movingCallback, doneCallback);
	}
	public static CommandTransformableMoveParabola MOVE_PARABOLA_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset, KeyFrameCallback movingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableMoveParabola cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mStartPos = startPos;
		cmd.mTargetPos = targetPos;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTopHeight = topHeight;
		cmd.mTremblingCallBack = movingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表的路线移动物体
	#region 以指定点列表的路线移动物体
	public static void MOVE_CURVE(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableMoveCurve>(obj, false);
	}
	public static void MOVE_CURVE(Transformable obj, List<Vector3> posList, float onceLength)
	{
		MOVE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_CURVE(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop, float offset)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, offset, null, null);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, posList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, KEY_FRAME.ZERO_ONE, posList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		MOVE_CURVE_EX(obj, keyframe, posList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(Transformable obj, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableMoveCurve cmd = newMainCmd(out cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mPosList = posList;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandTransformableMoveCurve>(obj, delayTime, false);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY(Transformable obj, float delayTime, List<Vector3> posList, float onceLength)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> posList, float onceLength)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, keyframe, posList, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, keyframe, posList, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop, float offset)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, keyframe, posList, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY_EX(Transformable obj, float delayTime, List<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY_EX(Transformable obj, float delayTime, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return MOVE_CURVE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableMoveCurve MOVE_CURVE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, List<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableMoveCurve cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mPosList = posList;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线移动物体
	#region 以指定点列表以及时间点的路线移动物体
	public static void MOVE_PATH(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableMovePath>(obj, false);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		MOVE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableMovePath cmd = newMainCmd(out cmd, false);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandTransformableMovePath>(obj, delayTime, false);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return MOVE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableMovePath MOVE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableMovePath cmd = newMainCmd(out cmd, false, true);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------
	// 插值位置
	#region 插值位置
	public static void LERP_POSITION(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableLerpPosition>(obj, false);
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
		CommandTransformableLerpPosition cmd = newMainCmd(out cmd, false);
		cmd.mTargetPosition = targetPosition;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mLerpingCallBack = doingCallback;
		cmd.mLerpDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static void LERP_POSITION_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return;
		}
		pushDelayMainCommand<CommandTransformableLerpPosition>(obj, delayTime, false);
	}
	public static void LERP_POSITION_DELAY(Transformable obj, float delayTime, Vector3 targetPosition, float lerpSpeed)
	{
		LERP_POSITION_DELAY_EX(obj, delayTime, targetPosition, lerpSpeed, null, null);
	}
	public static void LERP_POSITION_DELAY_EX(Transformable obj, float delayTime, Vector3 targetPosition, float lerpSpeed, LerpCallback doneCallback)
	{
		LERP_POSITION_DELAY_EX(obj, delayTime, targetPosition, lerpSpeed, null, doneCallback);
	}
	public static void LERP_POSITION_DELAY_EX(Transformable obj, float delayTime, Vector3 targetPosition, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_POSITION_DELAY(Transformable obj, float delayTime)");
			return;
		}
		CommandTransformableLerpPosition cmd = newMainCmd(out cmd, false, true);
		cmd.mTargetPosition = targetPosition;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mLerpingCallBack = doingCallback;
		cmd.mLerpDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------
	// 插值旋转
	#region 插值旋转
	public static void LERP_ROTATION(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableLerpRotation>(obj, false);
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
		CommandTransformableLerpRotation cmd = newMainCmd(out cmd, false);
		cmd.mTargetRotation = targetRotation;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mLerpingCallBack = doingCallback;
		cmd.mLerpDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static void LERP_ROTATION_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return;
		}
		pushDelayMainCommand<CommandTransformableLerpRotation>(obj, delayTime, false);
	}
	public static void LERP_ROTATION_DELAY(Transformable obj, float delayTime, Vector3 targetRotation, float lerpSpeed)
	{
		LERP_ROTATION_DELAY_EX(obj, delayTime, targetRotation, lerpSpeed, null, null);
	}
	public static void LERP_ROTATION_DELAY_EX(Transformable obj, float delayTime, Vector3 targetRotation, float lerpSpeed, LerpCallback doneCallback)
	{
		LERP_ROTATION_DELAY_EX(obj, delayTime, targetRotation, lerpSpeed, null, doneCallback);
	}
	public static void LERP_ROTATION_DELAY_EX(Transformable obj, float delayTime, Vector3 targetRotation, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_ROTATION_DELAY(Transformable obj, float delayTime)");
			return;
		}
		CommandTransformableLerpRotation cmd = newMainCmd(out cmd, false, true);
		cmd.mTargetRotation = targetRotation;
		cmd.mLerpSpeed = lerpSpeed;
		cmd.mLerpingCallBack = doingCallback;
		cmd.mLerpDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
	}
	#endregion
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线旋转物体
	#region 以指定点列表以及时间点的路线旋转物体
	public static void ROTATE_PATH(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableRotatePath>(obj, false);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ROTATE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		ROTATE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableRotatePath cmd = newMainCmd(out cmd, false);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandTransformableRotatePath>(obj, delayTime, false);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ROTATE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableRotatePath ROTATE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableRotatePath cmd = newMainCmd(out cmd, false, true);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 追踪物体
	#region 追踪物体
	public static void TRACK_TARGET(Transformable obj)
	{
		TRACK_TARGET(obj, null, 0.0f, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed)
	{
		TRACK_TARGET(obj, target, speed, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, Vector3 offset)
	{
		TRACK_TARGET(obj, target, speed, offset, null, null);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, TrackCallback doingCallback, TrackCallback doneCallback)
	{
		TRACK_TARGET(obj, target, speed, Vector3.zero, doingCallback, doneCallback);
	}
	public static void TRACK_TARGET(Transformable obj, Transformable target, float speed, Vector3 offset, TrackCallback trackingCallback, TrackCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableTrackTarget cmd = newMainCmd(out cmd, false);
		cmd.mTarget = target;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mDoingCallback = trackingCallback;
		cmd.mDoneCallback = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
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
		CommandTransformableRotateFocus cmd = newMainCmd(out cmd, false);
		cmd.mTarget = target;
		cmd.mOffset = offset;
		pushCommand(cmd, obj);
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 锁定世界坐标分量
	#region 锁定世界坐标的指定分量
	public static void LOCK_POSITION_X(Transformable obj, float x)
	{
		LOCK_POSITION(obj, new Vector3(x, 0.0f, 0.0f), true, false, false);
	}
	public static void LOCK_POSITION_Y(Transformable obj, float y)
	{
		LOCK_POSITION(obj, new Vector3(0.0f, y, 0.0f), false, true, false);
	}
	public static void LOCK_POSITION_Z(Transformable obj, float z)
	{
		LOCK_POSITION(obj, new Vector3(0.0f, 0.0f, z), false, false, true);
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
		CommandTransformableLockPosition cmd = newMainCmd(out cmd, false);
		cmd.mLockPosition = pos;
		cmd.mLockX = lockX;
		cmd.mLockY = lockY;
		cmd.mLockZ = lockZ;
		pushCommand(cmd, obj);
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
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
		CommandTransformableScale cmd = newMainCmd(out cmd, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushCommand(cmd, obj);
	}
	public static void SCALE(Transformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		SCALE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void SCALE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, offset, null, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		SCALE_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(Transformable obj, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableScale cmd = newMainCmd(out cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableScale SCALE_DELAY(Transformable obj, float delayTime, Vector3 scale)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableScale cmd = newMainCmd(out cmd, false, true);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandTransformableScale SCALE_DELAY(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength)
	{
		return SCALE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableScale SCALE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		return SCALE_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTransformableScale SCALE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		return SCALE_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTransformableScale SCALE_DELAY(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		return SCALE_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CommandTransformableScale SCALE_DELAY_EX(Transformable obj, float delayTime, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return SCALE_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableScale SCALE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return SCALE_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableScale SCALE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return SCALE_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableScale SCALE_DELAY_EX(Transformable obj, float delayTime, KEY_FRAME keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableScale cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线缩放物体
	#region 以指定点列表以及时间点的路线缩放物体
	public static void SCALE_PATH(Transformable obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandTransformableScalePath>(obj, false);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, Vector3.one, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void SCALE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		SCALE_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(Transformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandTransformableScalePath cmd = newMainCmd(out cmd, false);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandTransformableScalePath>(obj, delayTime, false);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, Vector3.one, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return SCALE_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandTransformableScalePath SCALE_PATH_DELAY_EX(Transformable obj, float delayTime, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandTransformableScalePath cmd = newMainCmd(out cmd, false, true);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 摄像机视角
	#region 摄像机视角
	public static void FOV(GameCamera obj, float fov)
	{
		if (obj == null)
		{
			return;
		}
		CommandCameraFOV cmd = newMainCmd(out cmd, false);
		cmd.mStartFOV = fov;
		cmd.mTargetFOV = fov;
		cmd.mOnceLength = 0.0f;
		pushCommand(cmd, obj);
	}
	public static void FOV(GameCamera obj, float start, float target, float onceLength)
	{
		FOV_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV(GameCamera obj, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		FOV_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void FOV_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		FOV_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void FOV_EX(GameCamera obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandCameraFOV cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartFOV = start;
		cmd.mTargetFOV = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallBack;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static void ORTHO_SIZE(GameCamera obj, float start, float target, float onceLength)
	{
		ORTHO_SIZE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ORTHO_SIZE_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ORTHO_SIZE_EX(GameCamera obj, KEY_FRAME keyframe, float startFOV, float targetFOV, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandCameraOrthoSize cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartOrthoSize = startFOV;
		cmd.mTargetOrthoSize = targetFOV;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mTremblingCallBack = doingCallBack;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 显示
	#region 物体的显示和隐藏
	public static void ACTIVE(MovableObject obj, bool active = true)
	{
		obj?.setActive(active);
	}
	public static CommandMovableObjectActive ACTIVE_DELAY(MovableObject obj, bool active, float delayTime)
	{
		return ACTIVE_DELAY_EX(obj, active, delayTime, null);
	}
	public static CommandMovableObjectActive ACTIVE_DELAY_EX(MovableObject obj, bool active, float dealyTime, CommandCallback startCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandMovableObjectActive cmd = newMainCmd(out cmd, false, true);
		cmd.mActive = active;
		cmd.addStartCommandCallback(startCallback);
		pushDelayCommand(cmd, obj, dealyTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 时间缩放
	#region 时间缩放
	public static void TIME(float scale)
	{
		CommandTimeManagerScaleTime cmd = newMainCmd(out cmd, true, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		pushCommand(cmd, mTimeManager);
	}
	public static void TIME(float start, float target, float onceLength)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void TIME(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void TIME_EX(float start, float target, float onceLength, float offsetTime, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(KEY_FRAME.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		TIME_EX(keyframe, start, target, onceLength, loop, 0.0f, doingCallBack, doneCallback);
	}
	public static void TIME_EX(KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallBack, KeyFrameCallback doneCallback)
	{
		CommandTimeManagerScaleTime cmd = newMainCmd(out cmd, false, false);
		cmd.mKeyframe = keyframe;
		cmd.mOnceLength = onceLength;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallBack;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, mTimeManager);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY(float delayTime, float scale)
	{
		CommandTimeManagerScaleTime cmd = newMainCmd(out cmd, false, true);
		cmd.mStartScale = scale;
		cmd.mTargetScale = scale;
		cmd.mOnceLength = 0.0f;
		pushDelayCommand(cmd, mTimeManager, delayTime);
		cmd.setIgnoreTimeScale(true);
		return cmd;
	}
	public static CommandTimeManagerScaleTime TIME_DELAY(float delayTime, float start, float target, float onceLength)
	{
		return TIME_DELAY_EX(delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, float start, float target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		return TIME_DELAY_EX(delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, float start, float target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		return TIME_DELAY_EX(delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		return TIME_DELAY_EX(delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		return TIME_DELAY_EX(delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		return TIME_DELAY_EX(delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CommandTimeManagerScaleTime TIME_DELAY_EX(float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		CommandTimeManagerScaleTime cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mStartScale = start;
		cmd.mTargetScale = target;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = movingCallback;
		cmd.mDoneCallBack = moveDoneCallback;
		pushDelayCommand(cmd, mTimeManager, delayTime);
		cmd.setIgnoreTimeScale(true);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 物体音效
	#region 播放物体音效
	public static void AUDIO(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandMovableObjectPlayAudio>(obj, false);
	}
	public static void AUDIO(MovableObject obj, string sound, bool loop, float volume)
	{
		if (obj == null)
		{
			return;
		}
		if (isEmpty(sound))
		{
			logError("sound name must be valid, use void AUDIO(MovableObject obj) to stop sound");
			return;
		}
		CommandMovableObjectPlayAudio cmd = newMainCmd(out cmd, false);
		cmd.mSoundFileName = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		pushCommand(cmd, obj);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop)
	{
		AUDIO(obj, sound, loop, 1.0f, true);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop, float volume)
	{
		AUDIO(obj, sound, loop, volume, true);
	}
	public static void AUDIO(MovableObject obj, SOUND_DEFINE sound, bool loop, float volume, bool useVolumeCoe)
	{
		if (obj == null)
		{
			return;
		}
		string name = (sound != SOUND_DEFINE.MIN) ? mAudioManager.getAudioName(sound) : null;
		if (isEmpty(name))
		{
			logError("sound name must be valid, use void AUDIO(MovableObject obj) to stop sound");
			return;
		}
		CommandMovableObjectPlayAudio cmd = newMainCmd(out cmd, false);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		cmd.mUseVolumeCoe = useVolumeCoe;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandMovableObjectPlayAudio>(obj, delayTime, false);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime, SOUND_DEFINE sound)
	{
		return AUDIO_DELAY(obj, delayTime, sound, false, 1.0f, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime, SOUND_DEFINE sound, float volume)
	{
		return AUDIO_DELAY(obj, delayTime, sound, false, volume, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime, SOUND_DEFINE sound, bool loop)
	{
		return AUDIO_DELAY(obj, delayTime, sound, loop, 1.0f, true);
	}
	public static CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime, SOUND_DEFINE sound, bool loop, float volume, bool useVolumeCoe)
	{
		if (obj == null)
		{
			return null;
		}
		string name = (sound != SOUND_DEFINE.MIN) ? mAudioManager.getAudioName(sound) : null;
		if (isEmpty(name))
		{
			logError("sound name must be valid, use CommandMovableObjectPlayAudio AUDIO_DELAY(MovableObject obj, float delayTime) to stop sound");
			return null;
		}
		CommandMovableObjectPlayAudio cmd = newMainCmd(out cmd, false, true);
		cmd.mSound = sound;
		cmd.mLoop = loop;
		cmd.mVolume = volume;
		cmd.mUseVolumeCoe = useVolumeCoe;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//--------------------------------------------------------------------------------------------------------------------------------------------
	// 透明度
	#region 透明度
	public static void ALPHA(MovableObject obj, float alpha = 1.0f)
	{
		if (obj == null)
		{
			return;
		}
		CommandMovableObjectAlpha cmd = newMainCmd(out cmd, false);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushCommand(cmd, obj);
	}
	public static void ALPHA(MovableObject obj, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void ALPHA(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		ALPHA_EX(obj, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ALPHA_EX(MovableObject obj, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ALPHA(MovableObject obj, float alpha)");
			return;
		}
		CommandMovableObjectAlpha cmd = newMainCmd(out cmd, false);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartAlpha = start;
		cmd.mTargetAlpha = target;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, float alpha)
	{
		if (obj == null)
		{
			return null;
		}
		CommandMovableObjectAlpha cmd = newMainCmd(out cmd, false, true);
		cmd.mOnceLength = 0.0f;
		cmd.mStartAlpha = alpha;
		cmd.mTargetAlpha = alpha;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength)
	{
		return ALPHA_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop)
	{
		return ALPHA_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		return ALPHA_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(MovableObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(MovableObject obj, float delayTime, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(obj, delayTime, KEY_FRAME.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPHA_DELAY_EX(obj, delayTime, keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlpha ALPHA_DELAY_EX(MovableObject obj, float delayTime, KEY_FRAME keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		if (keyframe == KEY_FRAME.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,CommandMovableObjectAlpha ALPHA_DELAY(MovableObject obj, float delayTime, float alpha)");
			return null;
		}
		CommandMovableObjectAlpha cmd = newMainCmd(out cmd, false, true);
		cmd.mKeyframe = keyframe;
		cmd.mLoop = loop;
		cmd.mOnceLength = onceLength;
		cmd.mOffset = offset;
		cmd.mStartAlpha = start;
		cmd.mTargetAlpha = target;
		cmd.mTremblingCallBack = doingCallback;
		cmd.mTrembleDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线设置物体透明度
	#region 以指定点列表以及时间点的路线设置物体透明度
	public static void ALPHA_PATH(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		pushMainCommand<CommandMovableObjectAlphaPath>(obj, false);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ALPHA_PATH(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		ALPHA_PATH_EX(obj, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ALPHA_PATH_EX(MovableObject obj, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return;
		}
		CommandMovableObjectAlphaPath cmd = newMainCmd(out cmd, false);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushCommand(cmd, obj);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime)
	{
		if (obj == null)
		{
			return null;
		}
		return pushDelayMainCommand<CommandMovableObjectAlphaPath>(obj, delayTime, false);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, 1.0f, 1.0f, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, loop, offset, null, null);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		return ALPH_PATH_DELAY_EX(obj, delayTime, valueKeyFrame, valueOffset, speed, false, 0.0f, doingCallback, doneCallback);
	}
	public static CommandMovableObjectAlphaPath ALPH_PATH_DELAY_EX(MovableObject obj, float delayTime, Dictionary<float, float> valueKeyFrame, float valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (obj == null)
		{
			return null;
		}
		CommandMovableObjectAlphaPath cmd = newMainCmd(out cmd, false, true);
		cmd.mValueKeyFrame = valueKeyFrame;
		cmd.mValueOffset = valueOffset;
		cmd.mSpeed = speed;
		cmd.mOffset = offset;
		cmd.mLoop = loop;
		cmd.mDoingCallBack = doingCallback;
		cmd.mDoneCallBack = doneCallback;
		pushDelayCommand(cmd, obj, delayTime);
		return cmd;
	}
	#endregion
}