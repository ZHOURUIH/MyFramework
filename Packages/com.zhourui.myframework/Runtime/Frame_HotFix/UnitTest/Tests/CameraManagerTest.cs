using UnityEngine;
using static TestAssert;

// CameraManager 相机管理器深度测试
//   createCamera(GameObject): CLASS 池取 GameCamera + setName + init + 注册 mCameraList + 默认激活
//   getCamera(name): 按名字查回
//   setMainCamera/activeCamera/destroyCamera: 主相机切换链 / 激活切换 / 销毁清理
// 环境: new CameraManager()(FrameSystem 子类直接 new) + 裸 GameObject + Camera 组件
// 清理: destroyCamera + 手动 DestroyImmediate
public static class CameraManagerTest
{
	public static void Run()
	{
		testCreateAndGetCamera();
		testCreateDefaultActive();
		testGetCameraNotFound();
		testSetMainCamera();
		testSetMainCameraSwitch();
		testActiveCameraToggle();
		testDestroyCamera();
		testDestroyMainCameraFallback();
		testDestroyCameraDeactiveFalse();
		testDefaultAndUICameraNull();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createCameraGO(string name, out GameObject parent)
	{
		// 相机需要父节点(getCamera 里读 camera.getTransform().parent.gameObject)
		parent = new GameObject(name + "_Parent");
		GameObject go = new GameObject(name);
		go.transform.SetParent(parent.transform);
		go.AddComponent<Camera>();
		return go;
	}

	// createCamera → getCamera 查回
	private static void testCreateAndGetCamera()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("TestCam0", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			assertNotNull(camera, "createCamera 返回非 null");
			GameCamera found = manager.getCamera("TestCam0", parent);
			assertTrue(ReferenceEquals(camera, found), "getCamera 按名字查回同一实例");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// createCamera 默认激活
	private static void testCreateDefaultActive()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("TestCamActive", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			assertTrue(camera.isActiveInHierarchy(), "createCamera 默认 active");
			assertTrue(camera.getCamera().enabled, "Camera 组件默认启用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// getCamera 未注册名 → null
	private static void testGetCameraNotFound()
	{
		CameraManager manager = new CameraManager();
		try
		{
			assertTrue(manager.getCamera("NotExistCam") == null, "未注册相机返回 null");
		}
		finally
		{
			manager.destroy();
		}
	}

	// setMainCamera / getMainCamera
	private static void testSetMainCamera()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("MainCam", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			manager.setMainCamera(camera);
			assertTrue(ReferenceEquals(camera, manager.getMainCamera()), "setMainCamera 后 getMainCamera 一致");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// setMainCamera 切换链: 旧主相机 deactive, 新主相机 active
	private static void testSetMainCameraSwitch()
	{
		CameraManager manager = new CameraManager();
		GameObject goA = createCameraGO("CamA", out GameObject parentA);
		GameObject goB = createCameraGO("CamB", out GameObject parentB);
		try
		{
			GameCamera camA = manager.createCamera(goA);
			GameCamera camB = manager.createCamera(goB);
			manager.setMainCamera(camA);
			manager.setMainCamera(camB);
			assertTrue(ReferenceEquals(camB, manager.getMainCamera()), "主相机切换为 B");
			assertTrue(!camA.isActiveInHierarchy(), "旧主相机 A 被禁用");
			assertTrue(camB.isActiveInHierarchy(), "新主相机 B 激活");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentA);
			UnityEngine.Object.DestroyImmediate(parentB);
			manager.destroy();
		}
	}

		// activeCamera 激活切换
		//   activeCamera(camera, false): 只 setActive(false)(禁用节点), if(active) 分支才改 Camera.enabled
		private static void testActiveCameraToggle()
		{
			CameraManager manager = new CameraManager();
			GameObject go = createCameraGO("ToggleCam", out GameObject parent);
			try
			{
				GameCamera camera = manager.createCamera(go);
				manager.activeCamera(camera, false);
				assertTrue(!camera.isActiveInHierarchy(), "activeCamera(false) 禁用节点");
				assertTrue(camera.getCamera().enabled, "Camera 组件 enabled 不受影响(false 分支不改)");
				manager.activeCamera(camera, true);
				assertTrue(camera.isActiveInHierarchy(), "activeCamera(true) 重新激活节点");
				assertTrue(camera.getCamera().enabled, "activeCamera(true) 启用 Camera 组件");
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(parent);
				manager.destroy();
			}
	}

	// destroyCamera: 从列表移除 + getCamera 查不到
	private static void testDestroyCamera()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("DestroyCam", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			manager.destroyCamera(camera);
			assertTrue(manager.getCamera("DestroyCam", parent) == null, "销毁后查不到");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// 销毁主相机: 回落到默认相机(未设置时 null)且不崩
	private static void testDestroyMainCameraFallback()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("FallbackCam", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			manager.setMainCamera(camera);
			manager.destroyCamera(camera);
			assertTrue(manager.getMainCamera() == null, "销毁主相机后回落默认(null)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// destroyCamera(deactive=false): 不执行 deactive 操作
	// 注意: destroyCamera 里 UN_CLASS(ref camera) 池回收会 resetProperty 清空包装对象,
	//       所以销毁后断言要用真实 GO(go.activeInHierarchy), 不能读 camera.isActiveInHierarchy()
	private static void testDestroyCameraDeactiveFalse()
	{
		CameraManager manager = new CameraManager();
		GameObject go = createCameraGO("NoDeactiveCam", out GameObject parent);
		try
		{
			GameCamera camera = manager.createCamera(go);
			assertTrue(go.activeInHierarchy, "前置: 相机节点激活");
			manager.destroyCamera(camera, false);
			assertTrue(go.activeInHierarchy, "deactive=false 不改变激活状态");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parent);
			manager.destroy();
		}
	}

	// getDefaultCamera/getUICamera 初始 null
	private static void testDefaultAndUICameraNull()
	{
		CameraManager manager = new CameraManager();
		try
		{
			assertTrue(manager.getDefaultCamera() == null, "默认相机初始 null");
			assertTrue(manager.getUICamera() == null, "UI 相机初始 null");
		}
		finally
		{
			manager.destroy();
		}
	}
}
