using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static CSharpUtility;
using static FrameBase;
using System;

// 摄像机调试信息
public class CameraDebug : MonoBehaviour
{
	protected GameCamera mGameCamera;				// 摄像机对象
	public string CurLinkerName;					// 当前连接器名字
	public string LinkedObjectName;					// 当前连接的物体名字
	public GameObject LinkedObject;					// 当前连接的物体
	public Vector3 Relative;						// 与物体的正常的相对距离
	public Vector3 CurRelative;						// 与物体的当前的相对距离,因为可能会插值计算
	public List<string> ActiveComponent = new();	// 激活的组件列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		if (mGameCamera == null)
		{
			return;
		}
		CameraLinker linker = mGameCamera.getCurLinker();
		if (linker != null)
		{
			CurLinkerName = linker.GetType().ToString();
			if(linker.getLinkObject() != null)
			{
				LinkedObject = linker.getLinkObject().getObject();
				LinkedObjectName = linker.getLinkObject().getName();
			}
			Relative = linker.getRelativePosition();
			if (LinkedObject != null)
			{
				CurRelative = mGameCamera.getWorldPosition() - LinkedObject.transform.position;
			}
			else
			{
				CurRelative = Vector3.zero;
			}
		}
		else
		{
			CurLinkerName = EMPTY;
			LinkedObjectName = EMPTY;
			LinkedObject = null;
			Relative = Vector3.zero;
			CurRelative = Vector3.zero;
		}
		ActiveComponent.Clear();
		var allComponent = mGameCamera.getAllComponent();
		if (allComponent != null)
		{
			using var a = new SafeDictionaryReader<Type, GameComponent>(allComponent);
			foreach (var item in a.mReadList)
			{
				if (item.Value.isActive())
				{
					ActiveComponent.Add(item.Key.ToString());
				}
			}
		}
	}
	public void setCamera(GameCamera camera)
	{
		mGameCamera = camera;
	}
}