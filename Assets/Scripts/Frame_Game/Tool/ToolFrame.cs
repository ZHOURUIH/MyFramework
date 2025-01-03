using UnityEngine;
using static FrameUtility;

// 用于操作Transformable,大部分的操作是用于代替Dotween的缓动操作,以及扩展的其他操作
public class FT
{
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
	public static void SCALE(Transformable obj, Vector3 scale)
	{
		if (obj == null)
		{
			return;
		}
		CmdTransformableScale.execute(obj, scale, scale, 0.0f, 0.0f, 0, false, null, null);
	}
}