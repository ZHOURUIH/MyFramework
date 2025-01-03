using UnityEngine;
using System.Collections.Generic;
using static FrameBase;
using static UnityUtility;

// 用于辅助生成渲染到纹理,比如在UI上显示角色,或者其他要将模型渲染到纹理的情况
// 默认要渲染的模型位于原点
public class AvatarRendererHelper
{
	protected Queue<GameCamera> mPostCameraUnuseList = new();
	protected Queue<GameCamera> mNoPostCameraUnuseList = new();
	protected List<GameCamera> mPostCameraUsedList = new();
	protected List<GameCamera> mNoPostCameraUsedList = new();
	protected GameCamera mCameraPostTemplate;
	protected GameCamera mCameraNoPostTemplate;
	protected Vector3 mCameraOriginPos;
	protected Vector3 mAvatarOriginPos;
	protected int mCurIndex;
	public void resetProperty()
	{
		mPostCameraUnuseList.Clear();
		mNoPostCameraUnuseList.Clear();
		mPostCameraUsedList.Clear();
		mNoPostCameraUsedList.Clear();
		mCameraPostTemplate = null;
		mCameraNoPostTemplate = null;
		mCameraOriginPos = Vector3.zero;
		mAvatarOriginPos = Vector3.zero;
		mCurIndex = 0;
	}
	public void destroy()
	{
		foreach (GameCamera camera in mPostCameraUnuseList)
		{
			mCameraManager?.destroyCamera(camera);
		}
		foreach (GameCamera camera in mNoPostCameraUnuseList)
		{
			mCameraManager?.destroyCamera(camera);
		}
		foreach (GameCamera camera in mPostCameraUsedList)
		{
			mCameraManager?.destroyCamera(camera);
		}
		foreach (GameCamera camera in mNoPostCameraUsedList)
		{
			mCameraManager?.destroyCamera(camera);
		}
		mPostCameraUnuseList.Clear();
		mNoPostCameraUnuseList.Clear();
		mPostCameraUsedList.Clear();
		mNoPostCameraUsedList.Clear();
		mCameraManager?.destroyCamera(mCameraPostTemplate);
		mCameraManager?.destroyCamera(mCameraNoPostTemplate);
		mCameraPostTemplate = null;
		mCameraNoPostTemplate = null;
	}
	// 需要在init之前调用
	public void setPostCamera(GameObject go)
	{
		mCameraPostTemplate = mCameraManager.createCamera(go, 0, true, false);
		mCameraPostTemplate.setPostProcessing(true);
		mCameraPostTemplate.setActive(false);
		mCameraOriginPos = mCameraPostTemplate.getPosition();
	}
	public void setNoPostCamera(GameObject go)
	{
		mCameraNoPostTemplate = mCameraManager.createCamera(go, 0, true, false);
		mCameraNoPostTemplate.setPostProcessing(false);
		mCameraNoPostTemplate.setActive(false);
	}
	public void setAvatarPosition(Vector3 pos) { mAvatarOriginPos = pos; }
	public void createRenderTarget(myUGUIRawImage ui, out Vector3 avatarPos)
	{
		GameCamera postCamera;
		GameCamera noPostCamera;
		// 从未使用列表中获得一个摄像机
		if (mPostCameraUnuseList.Count > 0)
		{
			postCamera = mPostCameraUnuseList.Dequeue();
			postCamera.setActive(true);

			noPostCamera = mNoPostCameraUnuseList.Dequeue();
			noPostCamera.setActive(true);
		}
		// 创建一个新的摄像机
		else
		{
			GameObject goPost = cloneObject(mCameraPostTemplate.getObject(), "CameraPost");
			goPost.transform.SetParent(mCameraPostTemplate.getTransform().parent);
			postCamera = mCameraManager.createCamera(goPost, 0, true, false);

			GameObject goNoPost = cloneObject(mCameraNoPostTemplate.getObject(), "CameraNoPost");
			goNoPost.transform.SetParent(mCameraNoPostTemplate.getTransform().parent);
			noPostCamera = mCameraManager.createCamera(goNoPost, 0, true, false);
		}
		mPostCameraUsedList.Add(postCamera);
		mNoPostCameraUsedList.Add(noPostCamera);
		// 计算一个新的位置
		Vector3 posOffset = new(mCurIndex++ * 1000, 0.0f, 0.0f);
		avatarPos = mAvatarOriginPos + posOffset;
		postCamera.setPosition(mAvatarOriginPos + posOffset + mCameraOriginPos);
		noPostCamera.setPosition(mAvatarOriginPos + posOffset + mCameraOriginPos);
		ui.setActive(true);
		// 这里需要材质的搭配才能生效
		Material mat = ui.getMaterial();
		if (!mat.HasTexture("_map2"))
		{
			logError("材质不是MergeTextureMat,无法创建渲染纹理");
			return;
		}
		mat.SetTexture("_map2", postCamera.createRenderTarget(ui.getWindowSize()));
		mat.SetTexture("_map1", noPostCamera.createRenderTarget(ui.getWindowSize()));
	}
	public void destroyRenderTarget(myUGUIRawImage ui)
	{
		ui.setActive(false);
		int count0 = mNoPostCameraUsedList.Count;
		for (int i = 0; i < count0; ++i)
		{
			GameCamera camera = mNoPostCameraUsedList[i];
			if (camera.getRenderTarget() == ui.getMaterial().GetTexture("_map1"))
			{
				camera.destroyRenderTexture();
				camera.setActive(false);
				mNoPostCameraUnuseList.Enqueue(camera);
				mNoPostCameraUsedList.RemoveAt(i);
				break;
			}
		}
		int count1 = mPostCameraUsedList.Count;
		for (int i = 0; i < count1; ++i)
		{
			GameCamera camera = mPostCameraUsedList[i];
			if (camera.getRenderTarget() == ui.getMaterial().GetTexture("_map2"))
			{
				camera.destroyRenderTexture();
				camera.setActive(false);
				mPostCameraUnuseList.Enqueue(camera);
				mPostCameraUsedList.RemoveAt(i);
				break;
			}
		}
	}
}