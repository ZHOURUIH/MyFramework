using System;
using System.Collections.Generic;
using UnityEngine;

// 游戏摄像机管理器
public class CameraManager : FrameSystem
{
	protected List<GameCamera> mCameraList;	// 所有摄像机的列表
	protected GameCamera mUGUIBlurCamera;	// UGUI的背景模糊摄像机
	protected GameCamera mDefaultCamera;	// 默认的主摄像机
	protected GameCamera mMainCamera;		// 主摄像机
	protected GameCamera mUGUICamera;		// UGUI的摄像机
	public CameraManager()
	{
		mCameraList = new List<GameCamera>();
	}
	public override void init()
	{
		base.init();
		mUGUICamera = createCamera(FrameDefine.UI_CAMERA, mLayoutManager.getRootObject());
		mUGUIBlurCamera = createCamera(FrameDefine.BLUR_CAMERA, mLayoutManager.getRootObject(), false, false);
		mDefaultCamera = createCamera(FrameDefine.MAIN_CAMERA, null);
		mMainCamera = mDefaultCamera;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		int count = mCameraList.Count;
		for (int i = 0; i < count; ++i)
		{
			GameCamera camera = mCameraList[i];
			if (!camera.isIgnoreTimeScale())
			{
				camera.update(elapsedTime);
			}
			else
			{
				camera.update(Time.unscaledDeltaTime);
			}
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		int count = mCameraList.Count;
		for (int i = 0; i < count; ++i)
		{
			mCameraList[i].lateUpdate(elapsedTime);
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		int count = mCameraList.Count;
		for (int i = 0; i < count; ++i)
		{
			mCameraList[i].fixedUpdate(elapsedTime);
		}
	}
	// 获得摄像机,名字是场景中摄像机的名字
	public GameCamera getCamera(string name, GameObject parent = null, bool createIfNull = true)
	{
		int count = mCameraList.Count;
		for (int i = 0; i < count; ++i)
		{
			GameCamera camera = mCameraList[i];
			if (camera.getName() == name && camera.getTransform().parent.gameObject == parent)
			{
				return camera;
			}
		}
		if (createIfNull)
		{
			return createCamera(name, parent);
		}
		return null;
	}
	// 查找一个已经存在的摄像机节点,并且创建一个GameCamera对象
	public GameCamera createCamera(string name, GameObject parent, bool active = true, bool errorIfFailed = true)
	{
		// 摄像机节点是否是自己创建的
		GameObject obj = getGameObject(name, parent, false, false);
		if (obj == null)
		{
			if (errorIfFailed)
			{
				logError("创建摄像机失败, name:" + name);
			}
			return null;
		}
		CLASS(out GameCamera camera);
		camera.setName(name);
		camera.init();
		camera.setObject(obj);
		// 只有自己创建的摄像机节点才可以销毁
		mCameraList.Add(camera);
		activeCamera(camera, active);
		return camera;
	}
	public void activeCamera(GameCamera camera, bool active)
	{
		if (camera == null)
		{
			return;
		}
		OT.ACTIVE(camera, active);
		checkCameraAudioListener();
	}
	public new GameCamera getMainCamera() { return mMainCamera; }
	public void setMainCamera(GameCamera mainCamera)
	{
		// 如果当前已经有主摄像机了,则禁用主摄像机
		if (mMainCamera != null)
		{
			activeCamera(mMainCamera, false);
		}
		mMainCamera = mainCamera;
		checkCameraAudioListener();
	}
	public new GameCamera getUICamera() { return mUGUICamera; }
	public GameCamera getUIBlurCamera() { return mUGUIBlurCamera; }
	public void activeBlurCamera(bool active) { OT.ACTIVE(mUGUIBlurCamera, active); }
	public void destroyCamera(GameCamera camera)
	{
		if (camera == null)
		{
			return;
		}
		activeCamera(camera, false);
		camera.destroy();
		UN_CLASS(camera);
		mCameraList.Remove(camera);
		// 当销毁的是主摄像机时,将默认的摄像机作为当前主摄像机,确保任何时候都有一个主摄像机
		if (camera == mMainCamera)
		{
			mMainCamera = mDefaultCamera;
		}
		else if (camera == mUGUICamera)
		{
			mUGUICamera = null;
		}
		else if (camera == mUGUIBlurCamera)
		{
			mUGUIBlurCamera = null;
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 检查所有摄像机的音频监听,确保主摄像机优先作为音频监听,没有主摄像机,则使用UI相机,没有UI相机,才使用第一个被启用的摄像机作为音频监听
	protected void checkCameraAudioListener()
	{
		if (mMainCamera != null && mMainCamera.isActive())
		{
			setCameraAudioListener(mMainCamera);
		}
		else if (mUGUICamera != null && mUGUICamera.isActive())
		{
			setCameraAudioListener(mUGUICamera);
		}
		else
		{
			int count = mCameraList.Count;
			for (int i = 0; i < count; ++i)
			{
				GameCamera item = mCameraList[i];
				if (item.isActive())
				{
					setCameraAudioListener(item);
				}
			}
		}
	}
	protected void setCameraAudioListener(GameCamera camera)
	{
		if (!camera.isActive())
		{
			return;
		}
		int count = mCameraList.Count;
		for (int i = 0; i < count; ++i)
		{
			GameCamera item = mCameraList[i];
			item.enableUnityComponent<AudioListener>(camera == item);
		}
	}
}