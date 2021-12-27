using System.Collections.Generic;
using UnityEngine;

// 摄像机调试信息
public class CameraDebug : MonoBehaviour
{
	protected GameCamera mGameCamera;			// 摄像机对象
	public string CurLinkerName;				// 当前连接器名字
	public string LinkedObjectName;				// 当前连接的物体名字
	public GameObject LinkedObject;				// 当前连接的物体
	public Vector3 Relative;					// 与物体的正常的相对距离
	public Vector3 CurRelative;					// 与物体的当前的相对距离,因为可能会插值计算
	public List<string> ActiveComponent = new List<string>();	// 激活的组件列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
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
			CurLinkerName = CSharpUtility.Typeof(linker).ToString();
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
			CurLinkerName = StringUtility.EMPTY;
			LinkedObjectName = StringUtility.EMPTY;
			LinkedObject = null;
			Relative = Vector3.zero;
			CurRelative = Vector3.zero;
		}
		ActiveComponent.Clear();
		var allComponents = mGameCamera.getAllComponent().startForeach();
		foreach (var item in allComponents)
		{
			if (item.Value.isActive())
			{
				ActiveComponent.Add(item.Key.ToString());
			}
		}
	}
	public void setCamera(GameCamera camera)
	{
		mGameCamera = camera;
	}
}