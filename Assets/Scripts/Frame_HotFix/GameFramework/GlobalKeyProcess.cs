using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static WidgetUtility;
using static FrameBase;
using static FrameEditorUtility;

// 只是汇集一些快捷键操作,不对外提供接口
public partial class GlobalKeyProcess : FrameSystem
{
	public override void update(float elapsedTime)
	{
		if (isEditor() || isWindows())
		{
			// F1切换日志等级
			if (isKeyCurrentDown(KeyCode.F1))
			{
				LOG_LEVEL newLevel = (LOG_LEVEL)(((int)getLogLevel() + 1) % (int)(LOG_LEVEL.FORCE + 1));
				setLogLevel(newLevel);
				log("当前日志等级:" + newLevel);
			}
			// F2检测当前鼠标坐标下有哪些窗口
			if (isKeyCurrentDown(KeyCode.F2))
			{
				Vector3 mousePos = getMousePosition();
				using var a = new ListScope<IMouseEventCollect>(out var hoverList);
				mGlobalTouchSystem.getAllHoverObject(hoverList, mousePos, null, true);
				foreach (IMouseEventCollect item in hoverList)
				{
					UIDepth depth = item.getDepth();
					if (item is MovableObject)
					{
						string info = "物体:" + item.getName();
						if (depth != null)
						{
							info += ", 深度:" + depth.toDepthString() + ", priority:" + depth.getPriority();
						}
						else
						{
							info += ", 深度:null";
						}
						info += ", passRay:" + item.isPassRay();
						log(info);
					}
					else if (item is myUIObject uiObj)
					{
						string info = "窗口:" + uiObj.getName() + ", 布局:" + uiObj.getLayout().getName();
						if (depth != null)
						{
							info += ", 深度:" + depth.toDepthString() + ", priority:" + depth.getPriority();
						}
						else
						{
							info += ", 深度:null";
						}
						info += ", passRay:" + uiObj.isPassRay();
						log(info);
					}
				}
			}
			// F3启用或禁用用作调试的脚本的更新
			if (isKeyCurrentDown(KeyCode.F3))
			{
				mGameFramework.mParam.mEnableScriptDebug = !mGameFramework.mParam.mEnableScriptDebug;
				if (mGameFramework.mParam.mEnableScriptDebug)
				{
					log("已开启调试脚本", Color.green);
				}
				else
				{
					log("已关闭调试脚本", Color.red);
				}
			}
			// F4启用或禁用
			if (isKeyCurrentDown(KeyCode.F4))
			{
				mGameFramework.mParam.mEnablePoolStackTrace = !mGameFramework.mParam.mEnablePoolStackTrace;
				if (mGameFramework.mParam.mEnablePoolStackTrace)
				{
					log("已开启对象池分配堆栈追踪", Color.green);
				}
				else
				{
					log("已关闭对象池分配堆栈追踪", Color.red);
				}
			}
			// F8检测UGUI事件系统中当前鼠标坐标下有哪些窗口
			if (isKeyCurrentDown(KeyCode.F8))
			{
				using var a = new ListScope<GameObject>(out var hoverList);
				checkUGUIInteractable(getMousePosition(), hoverList);
				foreach (GameObject item in hoverList)
				{
					log("窗口:" + item.name, item);
				}
			}
		}
	}
}