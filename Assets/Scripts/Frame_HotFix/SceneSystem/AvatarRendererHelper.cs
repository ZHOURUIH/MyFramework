using UnityEngine;
using System.Collections.Generic;
using static FrameBaseHotFix;
using static FrameBaseUtility;
using static UnityUtility;

// 用于辅助生成渲染到纹理,比如在UI上显示角色,或者其他要将模型渲染到纹理的情况
// 默认要渲染的模型位于原点
public class AvatarRenderer : FrameSystem
{
	protected Queue<GameCamera> mPostCameraUnuseList = new();
	protected Queue<GameCamera> mNoPostCameraUnuseList = new();
	protected List<GameCamera> mPostCameraUsedList = new();
	protected List<GameCamera> mNoPostCameraUsedList = new();
	protected GameCamera mCameraPostTemplate;
	protected GameCamera mCameraNoPostTemplate;
	protected Vector3 mCameraPosOffset;
	protected Vector3 mAvatarOriginPos;
	protected int mCurIndex;
	public override void resetProperty()
	{
		base.resetProperty();
		mPostCameraUnuseList.Clear();
		mNoPostCameraUnuseList.Clear();
		mPostCameraUsedList.Clear();
		mNoPostCameraUsedList.Clear();
		mCameraPostTemplate = null;
		mCameraNoPostTemplate = null;
		mCameraPosOffset = Vector3.zero;
		mAvatarOriginPos = Vector3.zero;
		mCurIndex = 0;
	}
	public override void destroy()
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
		base.destroy();
	}
	public void setOriginPosition(Vector3 cameraPosOffset, Vector3 avatarPos)
	{
		mCameraPosOffset = cameraPosOffset;
		mAvatarOriginPos = avatarPos; 
	}
	public Vector3 createRenderTarget(myUGUIRawImage ui)
	{
		if (mCameraNoPostTemplate == null && mCameraPostTemplate == null)
		{
			initCamera();
		}

		// 先尝试将当前的RT回收
		destroyRenderTarget(ui);

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
		Vector3 avatarPos = mAvatarOriginPos + posOffset;
		postCamera.setPosition(avatarPos + mCameraPosOffset);
		noPostCamera.setPosition(avatarPos + mCameraPosOffset);
		ui.setActive(true);
		// 这里需要材质的搭配才能生效
		Material mat = ui.getMaterial();
		if (!mat.HasTexture("_map2"))
		{
			logError("材质不是MergeTextureMat,无法创建渲染纹理");
			return avatarPos;
		}
		mat.SetTexture("_map2", postCamera.createRenderTarget(ui.getWindowSize()));
		mat.SetTexture("_map1", noPostCamera.createRenderTarget(ui.getWindowSize()));
		return avatarPos;
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void initCamera()
	{
		GameObject go0 = getRootGameObject("RTCamera");
		mCameraNoPostTemplate = mCameraManager.createCamera(go0, 0, true, false);
		mCameraNoPostTemplate.setPostProcessing(false);
		mCameraNoPostTemplate.setActive(false);

		GameObject go1 = getRootGameObject("RTCameraPost");
		mCameraPostTemplate = mCameraManager.createCamera(go1, 0, true, false);
		mCameraPostTemplate.setPostProcessing(true);
		mCameraPostTemplate.setActive(false);
		mCameraPosOffset = mCameraPostTemplate.getPosition();
	}
}