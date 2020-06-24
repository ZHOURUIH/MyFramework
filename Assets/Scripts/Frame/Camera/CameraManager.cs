using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : FrameComponent
{
	protected HashSet<GameCamera> mCameraList;
	protected GameCamera mMainCamera;
	protected GameCamera mNGUICamera;
	protected GameCamera mUGUICamera;
	protected GameCamera mUGUIBlurCamera;
	protected GameCamera mNGUIBlurCamera;
	public CameraManager(string name)
		: base(name)
	{
		mCameraList = new HashSet<GameCamera>();
	}
	public override void init()
	{
		base.init();
		mNGUICamera = createCamera(CommonDefine.UI_CAMERA, mLayoutManager.getRootObject(true));
		mUGUICamera = createCamera(CommonDefine.UI_CAMERA, mLayoutManager.getRootObject(false));
		mNGUIBlurCamera = createCamera(CommonDefine.BLUR_CAMERA, mLayoutManager.getRootObject(true), false, false);
		mUGUIBlurCamera = createCamera(CommonDefine.BLUR_CAMERA, mLayoutManager.getRootObject(false), false, false);
		mMainCamera = createCamera("MainCamera");
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var camera in mCameraList)
		{
			if(!camera.isIgnoreTimeScale())
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
		foreach (var camera in mCameraList)
		{
			camera.lateUpdate(elapsedTime);
		}
	}
	// 获得摄像机,名字是场景中摄像机的名字
	public GameCamera getCamera(string name, GameObject parent = null, bool createIfNull = true)
	{
		foreach (var item in mCameraList)
		{
			if (item.getName() == name && item.getTransform().parent.gameObject == parent)
			{
				return item;
			}
		}
		if (createIfNull)
		{
			return createCamera(name, parent);
		}
		return null;
	}
	public GameCamera createCamera(string name, GameObject parent = null, bool newCamera = false, bool active = true)
	{
		GameCamera camera = null;
		// 摄像机节点是否是自己创建的
		bool isNewNode = false;
		GameObject obj = getGameObject(parent, name, false, false);
		if (obj == null && newCamera)
		{
			obj = createGameObject(name, parent);
			isNewNode = true;
		}
		if (obj != null)
		{
			camera = new GameCamera(name);
			camera.init();
			camera.setObject(obj);
			// 只有自己创建的摄像机节点才可以销毁
			camera.setDestroyObject(isNewNode);
			mCameraList.Add(camera);
		}
		activeCamera(camera, active);
		return camera;
	}
	public void activeCamera(GameCamera camera, bool active)
	{
		if (camera == null)
		{
			return;
		}
		if (mNGUICamera != null || mUGUICamera != null)
		{
			OT.ACTIVE(camera, active);
			// 如果有非UI摄像机的音频监听组件启用,则禁用UI摄像机的音频监听组件
			bool otherCameraListenerEnabled = false;
			foreach (var item in mCameraList)
			{
				if (item != mUGUICamera && item != mNGUICamera)
				{
					if (item.isActive() && item.isUnityComponentEnabled<AudioListener>())
					{
						otherCameraListenerEnabled = true;
						break;
					}
				}
			}
			// 设置UI摄像机的音频监听组件
			if (mNGUICamera != null && mNGUICamera.isActive())
			{
				mNGUICamera.enableUnityComponent<AudioListener>(!otherCameraListenerEnabled);
			}
			if (mUGUICamera != null && mUGUICamera.isActive())
			{
				mUGUICamera.enableUnityComponent<AudioListener>(!otherCameraListenerEnabled);
			}
		}
	}
	public GameCamera getMainCamera() { return mMainCamera; }
	public void setMainCamera(GameCamera mainCamera) { mMainCamera = mainCamera; }
	public new GameCamera getUICamera(bool ngui) { return ngui ? mNGUICamera : mUGUICamera; }
	public GameCamera getUIBlurCamera(bool ngui) { return ngui ? mNGUIBlurCamera : mUGUIBlurCamera; }
	public void activeBlurCamera(bool ngui, bool active)
	{
		if(ngui)
		{
			OT.ACTIVE(mNGUIBlurCamera, active);
		}
		else
		{
			OT.ACTIVE(mUGUIBlurCamera, active);
		}
	}
	public void destroyCamera(GameCamera camera)
	{
		if (camera == null)
		{
			return;
		}
		activeCamera(camera, false);
		camera.destroy();
		mCameraList.Remove(camera);
		if (camera == mMainCamera)
		{
			mMainCamera = null;
		}
		else if (camera == mNGUICamera)
		{
			mNGUICamera = null;
		}
		else if (camera == mUGUICamera)
		{
			mUGUICamera = null;
		}
		else if (camera == mNGUIBlurCamera)
		{
			mNGUIBlurCamera = null;
		}
		else if (camera == mUGUIBlurCamera)
		{
			mUGUIBlurCamera = null;
		}
	}
}