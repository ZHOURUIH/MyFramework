using UnityEngine;
using static TestAssert;

// GameCamera 相机封装深度测试
// 相机数学: FOVY(角度/弧度) / FOVX(由 aspect 与 FOVY 推导) / 视锥截面(视距下的宽高)
//           正交尺寸 / 相机深度 / overlay 深度 / 渲染层 / 近裁剪面 / copyCamera
//
// 可测性分析(源码确认):
//   getFOVX = (aspect * tan(fovY/2)).atan() * 2 —— 纯数学
//   getViewSize(distance) = (tan(fovY/2)*|distance|*2*aspect, tan(fovY/2)*|distance|*2) —— 纯数学
//   setFOVY/getFOVY/setOrthoSize/setCameraDepth/setOverlayDepth 全走 Camera 组件字段
//   setVisibleLayer(layer): layer=0 直接 return; 否则记录 mLastVisibleLayer + 设置 cullingMask
//   unlinkTarget 空安全(mCurLinker?.)
//   screenToWorld 依赖屏幕尺寸不精确断言(跳过)
//   linkTarget 依赖组件系统注册(CameraLinker), 不测
//
// 环境: 裸 GameObject + Camera 组件 + GameCamera.setObject(EditMode 自动加 CameraDebug)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class GameCameraDeepTest
{
	public static void Run()
	{
		testSetObjectCamera();
		testFOVYAngle();
		testFOVYRadians();
		testFOVX();
		testGetViewSize();
		testGetViewSizeNegative();
		testOrthoSize();
		testCameraDepth();
		testOverlayDepth();
		testVisibleLayer();
		testVisibleLayerZero();
		testNearClip();
		testUnlinkTargetNullSafe();
		testCopyCamera();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建相机(预加 Camera 组件, aspect 固定 2 保证数学确定性)
	// ═════════════════════════════════════════════════════════════════
	private static GameCamera createCamera(out GameObject go)
	{
		go = new GameObject("TestCamera");
		Camera unityCam = go.AddComponent<Camera>();
		unityCam.aspect = 2.0f;   // EditMode 下 aspect 由 GameView 决定, 必须显式固定
		GameCamera camera = new GameCamera();
		camera.setObject(go);
		return camera;
	}

	// setObject: 绑定 Camera 组件
	private static void testSetObjectCamera()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			assertTrue(camera.getCamera() != null, "setObject 绑定 Camera 组件");
			assertTrue(ReferenceEquals(go.GetComponent<Camera>(), camera.getCamera()), "绑定的是同一组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setFOVY/getFOVY: 角度制
	private static void testFOVYAngle()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setFOVY(60.0f);
			assertEqual(60.0f, camera.getFOVY(), 0.001f, "setFOVY(60) 角度读回");
			camera.setFOVY(45.0f);
			assertEqual(45.0f, camera.getFOVY(), 0.001f, "setFOVY(45) 角度读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setFOVY/getFOVY: 弧度制
	private static void testFOVYRadians()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setFOVY(90.0f.toRadian(), true);
			assertEqual(90.0f, camera.getFOVY(), 0.001f, "弧度设 90° 角度读回");
			assertEqual(90.0f.toRadian(), camera.getFOVY(true), 0.001f, "弧度读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// FOVX: aspect=2 + fovY=90 → atan(2*tan(45°))*2 = atan(2)*2 ≈ 2.2142974 弧度
	private static void testFOVX()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setFOVY(90.0f);
			float expected = Mathf.Atan(2.0f) * 2.0f;
			assertEqual(expected, camera.getFOVX(true), 0.001f, "FOVX 弧度 = atan(aspect*tan(fovY/2))*2");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getViewSize: aspect=2 + fovY=90 + distance=10 → 高=tan(45°)*10*2=20, 宽=20*2=40
	private static void testGetViewSize()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setFOVY(90.0f);
			Vector2 viewSize = camera.getViewSize(10.0f);
			assertEqual(40.0f, viewSize.x, 0.001f, "视距 10 视锥宽 = 40");
			assertEqual(20.0f, viewSize.y, 0.001f, "视距 10 视锥高 = 20");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getViewSize: 负距离用绝对值
	private static void testGetViewSizeNegative()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setFOVY(90.0f);
			Vector2 pos = camera.getViewSize(10.0f);
			Vector2 neg = camera.getViewSize(-10.0f);
			assertEqual(pos.x, neg.x, 0.001f, "负距离视锥宽同正距离");
			assertEqual(pos.y, neg.y, 0.001f, "负距离视锥高同正距离");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 正交尺寸
	private static void testOrthoSize()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setOrthoSize(5.0f);
			assertEqual(5.0f, camera.getOrthoSize(), 0.001f, "setOrthoSize(5) 读回");
			camera.setOrthoSize(2.5f);
			assertEqual(2.5f, camera.getOrthoSize(), 0.001f, "setOrthoSize(2.5) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 相机深度
	private static void testCameraDepth()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.setCameraDepth(3.0f);
			assertEqual(3.0f, camera.getCameraDepth(), 0.001f, "setCameraDepth(3) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// overlay 深度(纯字段)
	private static void testOverlayDepth()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			assertEqual(0, camera.getOverlayDepth(), "默认 overlay 深度 0");
			camera.setOverlayDepth(2);
			assertEqual(2, camera.getOverlayDepth(), "setOverlayDepth(2) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setVisibleLayer: 记录原层 + 设置新层
	private static void testVisibleLayer()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			int orig = go.GetComponent<Camera>().cullingMask;
			camera.setVisibleLayer(1 << 5);
			assertEqual(1 << 5, camera.getVisibleLayer(), "cullingMask 设为 layer 5");
			assertEqual(orig, camera.getLastVisibleLayer(), "记录原 cullingMask");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setVisibleLayer(0): 直接 return, 不改变
	private static void testVisibleLayerZero()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			int orig = go.GetComponent<Camera>().cullingMask;
			camera.setVisibleLayer(0);
			assertEqual(orig, camera.getVisibleLayer(), "layer=0 不改变 cullingMask");
			assertEqual(0, camera.getLastVisibleLayer(), "layer=0 不记录原层");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 近裁剪面
	private static void testNearClip()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			assertEqual(go.GetComponent<Camera>().nearClipPlane, camera.getNearClip(), 0.001f, "getNearClip 读 Camera 近裁剪面");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 未 linkTarget 时 unlinkTarget 空安全
	private static void testUnlinkTargetNullSafe()
	{
		GameCamera camera = createCamera(out GameObject go);
		try
		{
			camera.unlinkTarget();   // mCurLinker null, 空安全不崩
			assertTrue(camera.getCurLinker() == null, "unlink 后连接器为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// copyCamera: 复制另一相机的 fov/cullingMask
	private static void testCopyCamera()
	{
		GameCamera camera = createCamera(out GameObject go);
		GameObject srcGo = new GameObject("SrcCamera");
		Camera srcCam = srcGo.AddComponent<Camera>();
		srcCam.fieldOfView = 45.0f;
		srcCam.cullingMask = 1 << 3;
		try
		{
			camera.copyCamera(srcGo);
			assertEqual(45.0f, camera.getFOVY(), 0.001f, "复制 fov 45");
			assertEqual(1 << 3, camera.getVisibleLayer(), "复制 cullingMask");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
			UnityEngine.Object.DestroyImmediate(srcGo);
		}
	}
}
