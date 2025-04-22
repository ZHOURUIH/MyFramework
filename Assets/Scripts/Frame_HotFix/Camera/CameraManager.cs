using System.Collections.Generic;
using UnityEngine;
#if USE_URP
using UnityEngine.Rendering.Universal;
#endif
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;
using static FrameDefine;
using static MathUtility;

// 游戏摄像机管理器
public class CameraManager : FrameSystem
{
#if USE_URP
	protected List<GameCamera> mOverlayCameraList = new();  // 所有Overlay类型的摄像机
#endif
	protected List<GameCamera> mCameraList = new();			// 所有摄像机的列表
	protected GameCamera mUGUIBlurCamera;					// UGUI的背景模糊摄像机
	protected GameCamera mDefaultCamera;					// 默认的主摄像机
	protected GameCamera mMainCamera;						// 主摄像机
	protected GameCamera mUGUICamera;						// UGUI的摄像机
	public override void init()
	{
		base.init();
		mUGUICamera = createCamera(UI_CAMERA, mLayoutManager.getRootObject(), 0);
		mUGUIBlurCamera = createCamera(BLUR_CAMERA, mLayoutManager.getRootObject(), 0, false, false);
		mDefaultCamera = createCamera(MAIN_CAMERA);
		mMainCamera = mDefaultCamera;
		// 主动调用主摄像机的激活操作,这样可以确认主摄像机的音频监听组件是生效的
		activeCamera(mMainCamera, true);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (GameCamera camera in mCameraList)
		{
			camera.update(camera.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime);
		}
		// 编辑器中检查是否主摄像机被隐藏,任何情况下,主摄像机都不能被隐藏,这样会无法刷新屏幕
#if USE_URP && UNITY_EDITOR
		if (!mMainCamera.isActiveInHierarchy())
		{
			logError("不能隐藏主摄像机");
		}
#endif
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		foreach (GameCamera camera in mCameraList)
		{
			camera.lateUpdate(elapsedTime);
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		foreach (GameCamera camera in mCameraList)
		{
			camera.fixedUpdate(elapsedTime);
		}
	}
	// 获得摄像机,名字是场景中摄像机的名字
	public GameCamera getCamera(string name, GameObject parent = null)
	{
		foreach (GameCamera camera in mCameraList)
		{
			if (camera.getName() == name && camera.getTransform().parent.gameObject == parent)
			{
				return camera;
			}
		}
		return null;
	}
	// 使用一个摄像机节点,创建一个GameCamera对象,如果是用于渲染到纹理的摄像机,则不能添加UI摄像机到显示栈中
	// overlayCameraDepth是Overlay类型的摄像机,也就是UI摄像机的排序深度,越大渲染在越上面
	public GameCamera createCamera(GameObject obj, int overlayCameraDepth = 0, bool active = true, bool addUICameraStack = true)
	{
		CLASS(out GameCamera camera);
		camera.setName(obj.name);
		camera.setObject(obj);
		camera.init();
		// 只有自己创建的摄像机节点才可以销毁
		mCameraList.Add(camera);
		activeCamera(camera, active);
#if USE_URP
		var cameraData = camera.getOrAddUnityComponent<UniversalAdditionalCameraData>();
		if (cameraData.renderType == CameraRenderType.Base)
		{
			if (addUICameraStack)
			{
				foreach (GameCamera overlayCamera in mOverlayCameraList)
				{
					cameraData.cameraStack.addUnique(overlayCamera.getCamera());
				}
			}
		}
		else if (cameraData.renderType == CameraRenderType.Overlay)
		{
			camera.setOverlayDepth(overlayCameraDepth);
			mOverlayCameraList.Add(camera);
			mOverlayCameraList.Sort((a, b) => { return sign(a.getOverlayDepth() - b.getOverlayDepth()); });
		}
#endif
		return camera;
	}
	public GameCamera createCamera(string name, int overlayCameraDepth = 0, bool active = true, bool errorIfFailed = true, bool addUICameraStack = true)
	{
		// 摄像机节点是否是自己创建的
		GameObject obj = getRootGameObject(name, errorIfFailed);
		if (obj == null)
		{
			return null;
		}
		return createCamera(obj, overlayCameraDepth, active, addUICameraStack);
	}
	// 查找一个已经存在的摄像机节点,并且创建一个GameCamera对象,如果是用于渲染到纹理的摄像机,则不能添加UI摄像机到显示栈中
	public GameCamera createCamera(string name, GameObject parent, int overlayCameraDepth = 0, bool active = true, bool errorIfFailed = true, bool addUICameraStack = true)
	{
		// 摄像机节点是否是自己创建的
		GameObject obj = getGameObject(name, parent, errorIfFailed, false);
		if (obj == null)
		{
			return null;
		}
		return createCamera(obj, overlayCameraDepth, active, addUICameraStack);
	}
	public void activeCamera(GameCamera camera, bool active, bool checkAudioListener = true)
	{
		if (camera == null)
		{
			return;
		}
		camera.setActive(active);
		// 也要确认Camera组件启用
		if (active)
		{
			camera.getCamera().enabled = active;
		}
		if (checkAudioListener)
		{
			checkCameraAudioListener();
		}
	}
	public void setMainCamera(GameCamera mainCamera, bool active = true)
	{
		// 如果当前已经有主摄像机了,则禁用主摄像机
		if (mMainCamera != null)
		{
			activeCamera(mMainCamera, false, false);
		}
		mMainCamera = mainCamera;
		activeCamera(mMainCamera, active, true);
	}
	public void resetToDefaultMainCamera()
	{
		setMainCamera(mDefaultCamera);
	}
	public GameCamera getMainCamera() { return mMainCamera; }
	public GameCamera getDefaultCamera() { return mDefaultCamera; }
	public GameCamera getUICamera() { return mUGUICamera; }
	public GameCamera getUIBlurCamera() { return mUGUIBlurCamera; }
	public void activeBlurCamera(bool active) { mUGUIBlurCamera?.setActive(active); }
	public void destroyCamera(GameCamera camera, bool deactive = true)
	{
		if (camera == null)
		{
			return;
		}
		if (deactive)
		{
			activeCamera(camera, false);
		}
		mCameraList.Remove(camera);
		if (camera == mMainCamera)
		{
			mMainCamera = null;
		}
		else if (camera == mUGUICamera)
		{
			mUGUICamera = null;
		}
		else if (camera == mUGUIBlurCamera)
		{
			mUGUIBlurCamera = null;
		}
#if USE_URP
		mOverlayCameraList.Remove(camera);
#endif
		UN_CLASS(ref camera);
		// 当销毁的是主摄像机时,将默认的摄像机作为当前主摄像机,确保任何时候都有一个主摄像机
		if (mMainCamera == null)
		{
			setMainCamera(mDefaultCamera);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 检查所有摄像机的音频监听,确保主摄像机优先作为音频监听,没有主摄像机,则使用UI相机,没有UI相机,才使用第一个被启用的摄像机作为音频监听
	protected void checkCameraAudioListener()
	{
		if (mMainCamera != null && mMainCamera.isActiveInHierarchy())
		{
			setCameraAudioListener(mMainCamera);
		}
		else if (mUGUICamera != null && mUGUICamera.isActiveInHierarchy())
		{
			setCameraAudioListener(mUGUICamera);
		}
		else
		{
			foreach (GameCamera camera in mCameraList)
			{
				if (camera.isActiveInHierarchy())
				{
					setCameraAudioListener(camera);
				}
			}
		}
	}
	protected void setCameraAudioListener(GameCamera camera)
	{
		if (!camera.isActiveInHierarchy())
		{
			return;
		}
		foreach (GameCamera item in mCameraList)
		{
			item.enableUnityComponent<AudioListener>(camera == item);
		}
	}
}