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
		mUGUIBlurCamera = createCamera(FrameDefine.BLUR_CAMERA, mLayoutManager.getRootObject(), false, false, false);
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
	public GameCamera createCamera(string name, GameObject parent = null, bool newCamera = false, bool active = true, bool errorIfFailed = true)
	{
		// 摄像机节点是否是自己创建的
		bool isNewNode = false;
		GameObject obj = getGameObject(name, parent, false, false);
		if (obj == null && newCamera)
		{
			obj = createGameObject(name, parent);
			isNewNode = true;
		}
		if (obj == null)
		{
			if (errorIfFailed)
			{
				logError("创建摄像机失败, name:" + name);
			}
			return null;
		}
		CLASS_MAIN(out GameCamera camera);
		camera.setName(name);
		camera.init();
		camera.setObject(obj);
		// 只有自己创建的摄像机节点才可以销毁
		camera.setDestroyObject(isNewNode);
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
	//-------------------------------------------------------------------------------------------------------------------------------------
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