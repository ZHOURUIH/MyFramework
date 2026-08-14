using static TestAssert;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

// myUGUISlider: UGUI Slider 组件封装——值范围约束/回调触发/组件引用
public static class MyUGUISliderTest
{
	public static void Run()
	{
		testInitAutoAddsSlider();
		testSetRange();
		testSetValueGetValue();
		testValueClampedByRange();
		testSliderCallback();
		testSetFillRectHandleRect();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// init: 无 Slider 组件时 setIsNewObject(true) 自动 AddComponent, getSlider 非 null
	private static void testInitAutoAddsSlider()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		try
		{
			assertNotNull(slider.getSlider(), "init 后 getSlider 非 null(自动补 Slider 组件)");
			assertNotNull(go.GetComponent<Slider>(), "GameObject 上已添加 Slider 组件");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setRange: minValue/maxValue 写入 Slider 组件
	private static void testSetRange()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		try
		{
			slider.setRange(0, 100);
			assertEqual(0, (int)slider.getSlider().minValue, "minValue=0");
			assertEqual(100, (int)slider.getSlider().maxValue, "maxValue=100");
			slider.setRange(-50, 50);
			assertEqual(-50, (int)slider.getSlider().minValue, "负范围 minValue=-50");
			assertEqual(50, (int)slider.getSlider().maxValue, "负范围 maxValue=50");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setValue/getValue: 值读写
	private static void testSetValueGetValue()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		try
		{
			slider.setRange(0, 100);
			slider.setValue(50.0f);
			assertEqual(50.0f, slider.getValue(), 0.001f, "setValue(50) 后 getValue=50");
			slider.setValue(0.0f);
			assertEqual(0.0f, slider.getValue(), 0.001f, "setValue(0) 后 getValue=0");
			slider.setValue(100.0f);
			assertEqual(100.0f, slider.getValue(), 0.001f, "setValue(100) 后 getValue=100");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// 值超出范围时被 Slider 组件 clamp 到 [minValue, maxValue]
	private static void testValueClampedByRange()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		try
		{
			slider.setRange(10, 20);
			slider.setValue(99.0f);
			assertEqual(20.0f, slider.getValue(), 0.001f, "超出上限被 clamp 到 maxValue=20");
			slider.setValue(-99.0f);
			assertEqual(10.0f, slider.getValue(), 0.001f, "低于下限被 clamp 到 minValue=10");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setSliderCallback: value 变化触发回调
	private static void testSliderCallback()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		try
		{
			// 必须先设置范围: Slider 组件默认范围 [0,1], setValue(42) 会被 clamp 到 1
			slider.setRange(0, 100);
			float received = -1.0f;
			int callCount = 0;
			slider.setSliderCallback((value) => { received = value; ++callCount; });
			slider.setValue(42.0f);
			assertTrue(callCount > 0, "setValue 应触发 onValueChanged 回调");
			assertEqual(42.0f, received, 0.001f, "回调收到新值 42");
			slider.setValue(43.0f);
			assertTrue(callCount >= 2, "再次 setValue 再次触发回调");
			assertEqual(43.0f, received, 0.001f, "回调收到新值 43");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setFillRect/setHandleRect: 组件引用设置
	private static void testSetFillRectHandleRect()
	{
		GameObject go = new GameObject("Slider");
		myUGUISlider slider = createSlider(go);
		GameObject fillGo = new GameObject("Fill", typeof(RectTransform));
		GameObject handleGo = new GameObject("Handle", typeof(RectTransform));
		myUGUIObject fill = new myUGUIObject();
		fill.setObject(fillGo);
		fill.init();
		myUGUIObject handle = new myUGUIObject();
		handle.setObject(handleGo);
		handle.init();
		try
		{
			slider.setFillRect(fill);
			slider.setHandleRect(handle);
			assertTrue(ReferenceEquals(fill.getRectTransform(), slider.getSlider().fillRect), "fillRect 已设置");
			assertTrue(ReferenceEquals(handle.getRectTransform(), slider.getSlider().handleRect), "handleRect 已设置");
		}
		finally
		{
			UObject.DestroyImmediate(fillGo);
			UObject.DestroyImmediate(handleGo);
			UObject.DestroyImmediate(go);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static myUGUISlider createSlider(GameObject go)
	{
		myUGUISlider slider = new myUGUISlider();
		// 无 Slider 组件时自动补组件, 避免 init 的 logError 分支
		slider.setIsNewObject(true);
		slider.setObject(go);
		slider.init();
		return slider;
	}
}
