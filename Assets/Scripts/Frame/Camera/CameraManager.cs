using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : FrameSystem
{
	protected List<GameCamera> mCameraList;
	protected GameCamera mMainCamera;
	protected GameCamera mUGUICamera;
	protected GameCamera mUGUIBlurCamera;
	public CameraManager()
	{
		mCameraList = new List<GameCamera>();
	}
	public override void init()
	{
		base.init();
		mUGUICamera = createCamera(FrameDefine.UI_CAMERA, mLayoutManager.getRootObject());
		mUGUIBlurCamera = createCamera(FrameDefine.BLUR_CAMERA, mLayoutManager.getRootObject(), false, false);
		mMainCamera = createCamera(FrameDefine.MAIN_CAMERA);
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
	public GameCamera createCamera(string name, GameObject parent = null, bool newCamera = false, bool active = true)
	{
		GameCamera camera = null;
		// 摄像机节点是否是自己创建的
		bool isNewNode = false;
		GameObject obj = getGameObject(name, parent, false, false);
		if (obj == null && newCamera)
		{
			obj = createGameObject(name, parent);
			isNewNode = true;
		}
		if (obj != null)
		{
			CLASS_MAIN(out camera);
			camera.setName(name);
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
		if (mUGUICamera != null)
		{
			OT.ACTIVE(camera, active);
			// 如果有非UI摄像机的音频监听组件启用,则禁用UI摄像机的音频监听组件
			bool otherCameraListenerEnabled = false;
			int count = mCameraList.Count;
			for (int i = 0; i < count; ++i)
			{
				GameCamera item = mCameraList[i];
				if (item != mUGUICamera)
				{
					if (item.isActive() && item.isUnityComponentEnabled<AudioListener>())
					{
						otherCameraListenerEnabled = true;
						break;
					}
				}
			}
			// 设置UI摄像机的音频监听组件
			if (mUGUICamera != null && mUGUICamera.isActive())
			{
				mUGUICamera.enableUnityComponent<AudioListener>(!otherCameraListenerEnabled);
			}
		}
	}
	public GameCamera getMainCamera() { return mMainCamera; }
	public void setMainCamera(GameCamera mainCamera) { mMainCamera = mainCamera; }
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
	}
}