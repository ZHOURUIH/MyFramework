using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 用于操作ITransformable,大部分的操作是用于代替Dotween的缓动操作,以及扩展的其他操作
public static class FT
{
	//------------------------------------------------------------------------------------------------------------------------------
	// 旋转
	#region 在普通更新中用关键帧旋转物体
	public static void ROTATE_FIXED(this ITransformable obj, bool lockRotation = true)
	{
		CmdTransformableRotateFixed.execute(obj, Vector3.zero, lockRotation);
	}
	public static void ROTATE_FIXED(this ITransformable obj, Vector3 rot, bool lockRotation = true)
	{
		CmdTransformableRotateFixed.execute(obj, rot, lockRotation);
	}
	public static void ROTATE(this ITransformable obj)
	{
		obj.ROTATE(Vector3.zero);
	}
	public static void ROTATE(this ITransformable obj, Vector3 rotation)
	{
		CmdTransformableRotate.execute(obj, rotation);
	}
	public static void ROTATE(this ITransformable obj, Vector3 start, Vector3 target, float time)
	{
		obj.ROTATE_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		obj.ROTATE_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		obj.ROTATE_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_EX(this ITransformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_EX(this ITransformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE(ITransformable obj, Vector3 rotation)");
			return;
		}
		CmdTransformableRotate.execute(obj, start, target, onceLength, offset, keyframe, loop, false, doingCallback, doneCallback);
	}
	public static void ROTATE_SPEED(this ITransformable obj)
	{
		obj.ROTATE_SPEED(Vector3.zero, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED(this ITransformable obj, Vector3 speed)
	{
		obj.ROTATE_SPEED(speed, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED(this ITransformable obj, Vector3 speed, Vector3 startAngle)
	{
		obj.ROTATE_SPEED(speed, startAngle, Vector3.zero);
	}
	public static void ROTATE_SPEED(this ITransformable obj, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		CmdTransformableRotateSpeed.execute(obj, rotateAccelerationValue, speed, startAngle, false);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧旋转物体
	public static void ROTATE_FIXED_PHY(this ITransformable obj, bool lockRotation = true)
	{
		CmdTransformableRotateFixed.execute(obj, Vector3.zero, lockRotation);
	}
	public static void ROTATE_FIXED_PHY(this ITransformable obj, Vector3 rot, bool lockRotation = true)
	{
		CmdTransformableRotateFixed.execute(obj, rot, lockRotation);
	}
	public static void ROTATE_PHY(this ITransformable obj, Vector3 rotation)
	{
		CmdTransformableRotate.execute(obj, rotation, rotation, 0.0f, 0.0f, 0, false, true, null, null);
	}
	public static void ROTATE_PHY(this ITransformable obj, Vector3 start, Vector3 target, float time)
	{
		obj.ROTATE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		obj.ROTATE_PHY_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_PHY(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		obj.ROTATE_PHY_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float time, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, time, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PHY_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		if (keyframe == KEY_CURVE.NONE || isFloatZero(onceLength))
		{
			logError("时间或关键帧不能为空,如果要停止组件,请使用void ROTATE_PHY(ITransformable obj, Vector3 rotation)");
			return;
		}
		CmdTransformableRotate.execute(obj, start, target, onceLength, offset, keyframe, loop, true, doingCallback, doneCallback);
	}
	public static void ROTATE_SPEED_PHY(this ITransformable obj)
	{
		obj.ROTATE_SPEED(Vector3.zero, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(this ITransformable obj, Vector3 speed)
	{
		obj.ROTATE_SPEED(speed, Vector3.zero, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(this ITransformable obj, Vector3 speed, Vector3 startAngle)
	{
		obj.ROTATE_SPEED(speed, startAngle, Vector3.zero);
	}
	public static void ROTATE_SPEED_PHY(this ITransformable obj, Vector3 speed, Vector3 startAngle, Vector3 rotateAccelerationValue)
	{
		CmdTransformableRotateSpeed.execute(obj, rotateAccelerationValue, speed, startAngle, true);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定角度列表旋转物体
	#region 以指定角度列表旋转物体
	public static void ROTATE_CURVE(this ITransformable obj)
	{
		CmdTransformableRotateCurve.execute(obj);
	}
	public static void ROTATE_CURVE(this ITransformable obj, List<Vector3> rotList, float onceLength)
	{
		obj.ROTATE_CURVE_EX(KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength)
	{
		obj.ROTATE_CURVE_EX(keyframe, rotList, onceLength, false, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop)
	{
		obj.ROTATE_CURVE_EX(keyframe, rotList, onceLength, loop, 0.0f, null, null);
	}
	public static void ROTATE_CURVE(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset)
	{
		obj.ROTATE_CURVE_EX(keyframe, rotList, onceLength, loop, offset, null, null);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(KEY_CURVE.ZERO_ONE, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(KEY_CURVE.ZERO_ONE, rotList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, List<Vector3> rotList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(KEY_CURVE.ZERO_ONE, rotList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(keyframe, rotList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_CURVE_EX(keyframe, rotList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void ROTATE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> rotList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableRotateCurve.execute(obj, rotList, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 移动
	#region 在普通更新中用关键帧移动物体
	public static void MOVE(this ITransformable obj)
	{
		CmdTransformableMove.execute(obj, Vector3.zero);
	}
	public static void MOVE(this ITransformable obj, Vector3 pos)
	{
		CmdTransformableMove.execute(obj, pos);
	}
	public static void MOVE(this ITransformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOVE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback moveDoneCallback)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, moveDoneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback moveDoneCallback)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, null, moveDoneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback movingCallback, KeyFrameCallback moveDoneCallback)
	{
		obj.MOVE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, movingCallback, moveDoneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback tremblingCallBack, KeyFrameCallback trembleDoneCallBack)
	{
		obj.MOVE_EX(keyframe, startPos, targetPos, onceLength, loop, 0.0f, tremblingCallBack, trembleDoneCallBack);
	}
	public static void MOVE_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMove.execute(obj, startPos, targetPos, onceLength, offset, keyframe, loop, false, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	#region 在物理更新中用关键帧移动物体
	public static void MOVE_PHY(this ITransformable obj, Vector3 pos)
	{
		CmdTransformableMove.execute(obj, pos);
	}
	public static void MOVE_PHY(this ITransformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		obj.MOVE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop)
	{
		obj.MOVE_PHY_EX(keyframe, startPos, targetPos, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PHY(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset)
	{
		obj.MOVE_PHY_EX(keyframe, startPos, targetPos, onceLength, loop, offset, null, null);
	}
	public static void MOV_PHY(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength)
	{
		obj.MOVE_PHY_EX(keyframe, startPos, targetPos, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(keyframe, startPos, targetPos, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PHY_EX(keyframe, startPos, targetPos, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PHY_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMove.execute(obj, startPos, targetPos, onceLength, offset, keyframe, loop, true, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以抛物线移动物体
	#region 以抛物线移动物体
	public static void MOVE_PARABOLA(this ITransformable obj)
	{
		CmdTransformableMoveParabola.execute(obj);
	}
	public static void MOVE_PARABOLA(this ITransformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength)
	{
		obj.MOVE_PARABOLA_EX(KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength)
	{
		obj.MOVE_PARABOLA_EX(keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop)
	{
		obj.MOVE_PARABOLA_EX(keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_PARABOLA(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset)
	{
		obj.MOVE_PARABOLA_EX(keyframe, startPos, targetPos, topHeight, onceLength, loop, offset, null, null);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, Vector3 start, Vector3 target, float topHeight, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(KEY_CURVE.ZERO_ONE, start, target, topHeight, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(keyframe, startPos, targetPos, topHeight, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PARABOLA_EX(keyframe, startPos, targetPos, topHeight, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_PARABOLA_EX(this ITransformable obj, int keyframe, Vector3 startPos, Vector3 targetPos, float topHeight, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveParabola.execute(obj, startPos, targetPos, onceLength, topHeight, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表的路线移动物体
	#region 以指定点列表的路线移动物体
	public static void MOVE_CURVE(this ITransformable obj)
	{
		CmdTransformableMoveCurve.execute(obj);
	}
	public static void MOVE_CURVE(this ITransformable obj, List<Vector3> posList, float onceLength)
	{
		obj.MOVE_CURVE_EX(KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength)
	{
		obj.MOVE_CURVE_EX(keyframe, posList, onceLength, false, 0.0f, null, null);
	}
	public static void MOVE_CURVE(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop)
	{
		obj.MOVE_CURVE_EX(keyframe, posList, onceLength, loop, 0.0f, null, null);
	}
	public static void MOVE_CURVE(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, float offset)
	{
		obj.MOVE_CURVE_EX(keyframe, posList, onceLength, loop, offset, null, null);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(KEY_CURVE.ZERO_ONE, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(KEY_CURVE.ZERO_ONE, posList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, List<Vector3> posList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(KEY_CURVE.ZERO_ONE, posList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(keyframe, posList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.MOVE_CURVE_EX(keyframe, posList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, int keyframe, Span<Vector3> posList, float onceLength, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveCurveSpan.execute(obj, posList, onceLength, 0.0f, keyframe, false, null, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, int keyframe, Span<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveCurveSpan.execute(obj, posList, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	public static void MOVE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> posList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMoveCurve.execute(obj, posList, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线移动物体
	#region 以指定点列表以及时间点的路线移动物体
	public static void MOVE_PATH(this ITransformable obj)
	{
		CmdTransformableMovePath.execute(obj);
	}
	public static void MOVE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void MOVE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void MOVE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(this ITransformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PATH_EX(keyframe, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		obj.MOVE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void MOVE_PATH_EX(this ITransformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableMovePath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, keyframe, loop);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 插值位置
	#region 插值位置
	public static void LERP_POSITION(this ITransformable obj)
	{
		CmdTransformableLerpPosition.execute(obj);
	}
	public static void LERP_POSITION(this ITransformable obj, Vector3 targetPosition, float lerpSpeed)
	{
		obj.LERP_POSITION_EX(targetPosition, lerpSpeed, null, null);
	}
	public static void LERP_POSITION_EX(this ITransformable obj, Vector3 targetPosition, float lerpSpeed, LerpCallback doneCallback)
	{
		obj.LERP_POSITION_EX(targetPosition, lerpSpeed, null, doneCallback);
	}
	public static void LERP_POSITION_EX(this ITransformable obj, Vector3 targetPosition, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_POSITION(ITransformable obj)");
			return;
		}
		CmdTransformableLerpPosition.execute(obj, targetPosition, lerpSpeed, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 插值旋转
	#region 插值旋转
	public static void LERP_ROTATION(this ITransformable obj)
	{
		CmdTransformableLerpRotation.execute(obj);
	}
	public static void LERP_ROTATION(this ITransformable obj, Vector3 targetRotation, float lerpSpeed)
	{
		obj.LERP_ROTATION_EX(targetRotation, lerpSpeed, null, null);
	}
	public static void LERP_ROTATION_EX(this ITransformable obj, Vector3 targetRotation, float lerpSpeed, LerpCallback doneCallback)
	{
		obj.LERP_ROTATION_EX(targetRotation, lerpSpeed, null, doneCallback);
	}
	public static void LERP_ROTATION_EX(this ITransformable obj, Vector3 targetRotation, float lerpSpeed, LerpCallback doingCallback, LerpCallback doneCallback)
	{
		if (isFloatZero(lerpSpeed))
		{
			logError("速度不能为0,如果要停止组件,请使用void LERP_ROTATION(ITransformable obj)");
			return;
		}
		CmdTransformableLerpRotation.execute(obj, targetRotation, lerpSpeed, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线旋转物体
	#region 以指定点列表以及时间点的路线旋转物体
	public static void ROTATE_PATH(this ITransformable obj)
	{
		CmdTransformableRotatePath.execute(obj);
	}
	public static void ROTATE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.zero, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void ROTATE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void ROTATE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		obj.ROTATE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void ROTATE_PATH_EX(this ITransformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableRotatePath.execute(obj, valueKeyFrame, valueOffset, offset, speed, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 追踪物体
	#region 追踪物体
	public static void TRACK_TARGET(this ITransformable obj)
	{
		obj.TRACK_TARGET(null, 0.0f, 0.0f, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed)
	{
		obj.TRACK_TARGET(target, speed, 0.0f, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET(target, speed, 0.0f, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, Vector3 offset)
	{
		obj.TRACK_TARGET(target, speed, 0.0f, offset, null, null);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, BoolCallback doingCallback, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET(target, speed, 0.0f, Vector3.zero, doingCallback, doneCallback);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, Vector3 offset, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET(target, speed, 0.0f, offset, null, doneCallback);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, float nearRange, Vector3 offset, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET(target, speed, nearRange, offset, null, doneCallback);
	}
	public static void TRACK_TARGET(this ITransformable obj, ITransformable target, float speed, float nearRange, Vector3 offset, BoolCallback trackingCallback, BoolCallback doneCallback)
	{
		CmdTransformableTrackTarget.execute(obj, target, offset, nearRange, speed, false, trackingCallback, doneCallback);
	}
	#endregion
	#region 抛物线追踪物体
	public static void TRACK_TARGET_PARABOLA(this ITransformable obj)
	{
		obj.TRACK_TARGET_PARABOLA(null, 0.0f, 0.0f, 0.0f, Vector3.zero, Vector3.zero, null, null);
	}
	public static void TRACK_TARGET_PARABOLA(this ITransformable obj, ITransformable target, float speed, float maxHeight, Vector3 start, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET_PARABOLA(target, speed, 0.0f, maxHeight, start, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(this ITransformable obj, ITransformable target, float speed, float maxHeight, Vector3 start, Vector3 offset, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET_PARABOLA(target, speed, 0.0f, maxHeight, start, offset, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(this ITransformable obj, ITransformable target, float speed, float nearRange, float maxHeight, Vector3 start, BoolCallback doneCallback)
	{
		obj.TRACK_TARGET_PARABOLA(target, speed, nearRange, maxHeight, start, Vector3.zero, null, doneCallback);
	}
	public static void TRACK_TARGET_PARABOLA(this ITransformable obj, ITransformable target, float speed, float nearRange, float maxHeight, Vector3 start, Vector3 offset, BoolCallback trackingCallback, BoolCallback doneCallback)
	{
		CmdTransformableTrackTargetParabola.execute(obj, target, start, offset, nearRange, speed, maxHeight, false, trackingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 始终朝向物体
	#region 始终朝向物体
	public static void ROTATE_FOCUS(this ITransformable obj)
	{
		obj.ROTATE_FOCUS(null, Vector3.zero);
	}
	public static void ROTATE_FOCUS(this ITransformable obj, ITransformable target)
	{
		obj.ROTATE_FOCUS(target, Vector3.zero);
	}
	public static void ROTATE_FOCUS(this ITransformable obj, ITransformable target, Vector3 offset)
	{
		CmdTransformableRotateFocus.execute(obj, target, offset);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 锁定世界坐标分量
	#region 锁定世界坐标的指定分量
	public static void LOCK_POSITION_X(this ITransformable obj, float x)
	{
		obj.LOCK_POSITION(new(x, 0.0f, 0.0f), true, false, false);
	}
	public static void LOCK_POSITION_Y(this ITransformable obj, float y)
	{
		obj.LOCK_POSITION(new(0.0f, y, 0.0f), false, true, false);
	}
	public static void LOCK_POSITION_Z(this ITransformable obj, float z)
	{
		obj.LOCK_POSITION(new(0.0f, 0.0f, z), false, false, true);
	}
	public static void LOCK_POSITION(this ITransformable obj)
	{
		obj.LOCK_POSITION(Vector3.zero, false, false, false);
	}
	public static void LOCK_POSITION(this ITransformable obj, Vector3 pos, bool lockX, bool lockY, bool lockZ)
	{
		CmdTransformableLockPosition.execute(obj, pos, lockX, lockY, lockZ);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 缩放
	#region 用关键帧缩放物体
	public static void SCALE(this ITransformable obj)
	{
		obj.SCALE(Vector3.one);
	}
	public static void SCALE(this ITransformable obj, Vector3 scale)
	{
		CmdTransformableScale.execute(obj, scale);
	}
	public static void SCALE(this ITransformable obj, float scale)
	{
		CmdTransformableScale.execute(obj, new(scale, scale, scale));
	}
	public static void SCALE(this ITransformable obj, float start, float target, float onceLength)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, Vector3 start, Vector3 target, float onceLength)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, float start, float target, float onceLength)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, float start, float target, float onceLength, bool loop)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, float start, float target, float onceLength, bool loop, float offset)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, loop, offset, null, null);
	}
	public static void SCALE(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, loop, offset, null, null);
	}
	public static void SCALE_EX(this ITransformable obj, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(KEY_CURVE.ZERO_ONE, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, loop, 0.0f, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, float start, float target, float onceLength, bool loop, float offset, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, loop, offset, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, loop, offset, null, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, float start, float target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, float start, float target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, new(start, start, start), new(target, target, target), onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_EX(keyframe, start, target, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_EX(this ITransformable obj, int keyframe, Vector3 start, Vector3 target, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableScale.execute(obj, start, target, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
	//------------------------------------------------------------------------------------------------------------------------------
	// 以指定点列表以及时间点的路线缩放物体
	#region 以指定点列表以及时间点的路线缩放物体
	public static void SCALE_PATH(this ITransformable obj)
	{
		CmdTransformableScalePath.execute(obj);
	}
	public static void SCALE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, Vector3.one, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, null);
	}
	public static void SCALE_PATH(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, loop, 0.0f, null, null);
	}
	public static void SCALE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, KeyFrameCallback doneCallback)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, 1.0f, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(this ITransformable obj, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, KeyFrameCallback doneCallback)
	{
		obj.SCALE_PATH_EX(KEY_CURVE.ZERO_ONE, valueKeyFrame, valueOffset, speed, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_PATH_EX(this ITransformable obj, int keyframe, Dictionary<float, Vector3> valueKeyFrame, Vector3 valueOffset, float speed, bool loop, float timeOffset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableScalePath.execute(obj, valueKeyFrame, doingCallback, doneCallback, valueOffset, timeOffset, speed, keyframe, loop);
	}
	#endregion
	// 以指定角度列表旋转物体
	#region 以指定角度列表旋转物体
	public static void SCALE_CURVE(this ITransformable obj)
	{
		CmdTransformableScaleCurve.execute(obj);
	}
	public static void SCALE_CURVE(this ITransformable obj, List<Vector3> scaleList, float onceLength)
	{
		obj.SCALE_CURVE_EX(KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE_CURVE(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength)
	{
		obj.SCALE_CURVE_EX(keyframe, scaleList, onceLength, false, 0.0f, null, null);
	}
	public static void SCALE_CURVE(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop)
	{
		obj.SCALE_CURVE_EX(keyframe, scaleList, onceLength, loop, 0.0f, null, null);
	}
	public static void SCALE_CURVE(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, float offset)
	{
		obj.SCALE_CURVE_EX(keyframe, scaleList, onceLength, loop, offset, null, null);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, List<Vector3> scaleList, float onceLength, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, null, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, List<Vector3> scaleList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, List<Vector3> scaleList, float onceLength, float offsetTime, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, offsetTime, null, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, List<Vector3> scaleList, float onceLength, float offsetTime, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(KEY_CURVE.ZERO_ONE, scaleList, onceLength, false, offsetTime, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(keyframe, scaleList, onceLength, false, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		obj.SCALE_CURVE_EX(keyframe, scaleList, onceLength, loop, 0.0f, doingCallback, doneCallback);
	}
	public static void SCALE_CURVE_EX(this ITransformable obj, int keyframe, List<Vector3> scaleList, float onceLength, bool loop, float offset, KeyFrameCallback doingCallback, KeyFrameCallback doneCallback)
	{
		CmdTransformableScaleCurve.execute(obj, scaleList, onceLength, offset, keyframe, loop, doingCallback, doneCallback);
	}
	#endregion
}