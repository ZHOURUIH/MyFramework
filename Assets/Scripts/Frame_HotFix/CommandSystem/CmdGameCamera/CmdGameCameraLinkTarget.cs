using System;
using UnityEngine;

// 使用指定的连接器将摄像机连接到一个对象上
public class CmdGameCameraLinkTarget
{
	// 相对位置
	// 转换器的速度
	// 是否使用连接器原来的相对位置
	// 是否使用当前连接器的速度
	// 摄像机要连接的对象,仅自由连接器允许连接对象为空,其他的连接器都需要一个连接对象
	// 看向目标的位置偏移
	// 连接器的类型
	// 转换器的类型
	// 是否始终看向目标
	// 是否直接将摄像机设置到当前连接器的正常位置
	public static void execute(GameCamera camera, MovableObject target, Vector3 lookatOffset, Type linkerType, Type switchType, bool lookAtTarget, bool immediately)
	{
		execute(camera, target, lookatOffset, linkerType, switchType, lookAtTarget, immediately, Vector3.zero, 0.0f, false, false);
	}
	public static void execute(GameCamera camera, MovableObject target, Vector3 lookatOffset, Type linkerType, Type switchType, bool lookAtTarget, bool immediately, Vector3 relativePosition, float switchSpeed)
	{
		execute(camera, target, lookatOffset, linkerType, switchType, lookAtTarget, immediately, relativePosition, switchSpeed, true, true);
	}
	public static void execute(GameCamera camera, MovableObject target, Vector3 lookatOffset, Type linkerType, Type switchType, bool lookAtTarget, bool immediately, Vector3 relativePosition, float switchSpeed, bool useOriginRelative, bool useLastSwitchSpeed)
	{
		CameraLinker linker = camera.linkTarget(linkerType, target);
		if (linker == null)
		{
			return;
		}
		// 停止正在进行的摄像机运动
		camera.MOVE(camera.getPosition());
		camera.ROTATE(camera.getRotation());
		linker.setLookAtTarget(lookAtTarget);
		linker.setLookAtOffset(lookatOffset);
		// 不使用原来的相对位置,则设置新的相对位置
		if (!useOriginRelative)
		{
			// 只有在连接对象的命令中设置的相对位置才认为是原始位置
			linker.setOriginRelativePosition(relativePosition);
			if (switchType == null)
			{
				linker.setRelativePosition(relativePosition);
			}
			else
			{
				linker.setRelativePositionWithSwitch(relativePosition, switchType, useLastSwitchSpeed, switchSpeed);
			}
		}
		if (immediately)
		{
			linker.applyRelativePosition(linker.getRelativePosition());
		}
	}
}