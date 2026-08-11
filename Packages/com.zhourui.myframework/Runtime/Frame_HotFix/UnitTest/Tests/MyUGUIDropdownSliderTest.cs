using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TestAssert;

// myUGUIDropdown + myUGUISlider 深度测试(UGUI 组件封装)
//   Dropdown: init(预加组件) / getDropdown / clearOptions / addOptions
//             setSelect / getSelect / getText
//   Slider:   init(预加组件) / getSlider / setRange / setValue / getValue
//             setSliderCallback(onValueChanged 触发) / setFillRect / setHandleRect
public static class MyUGUIDropdownSliderTest
{
	public static void Run()
	{
		testDropdownInit();
		testDropdownOptions();
		testDropdownSelect();
		testSliderInit();
		testSliderRangeValue();
		testSliderCallback();
		testSliderFillHandleRect();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createDropdown(out myUGUIDropdown dd)
	{
		GameObject go = new GameObject("Dropdown");
		go.AddComponent<RectTransform>();
		go.AddComponent<Dropdown>();
		dd = new myUGUIDropdown();
		dd.setObject(go);
		dd.init();
		return go;
	}

	private static GameObject createSlider(out myUGUISlider slider)
	{
		GameObject go = new GameObject("Slider");
		go.AddComponent<RectTransform>();
		go.AddComponent<Slider>();
		slider = new myUGUISlider();
		slider.setObject(go);
		slider.init();
		return go;
	}

	private static myUGUIObject createChildUI(string name, Transform parent)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.transform.SetParent(parent);
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// ═════════════════════════════════════════════════════════════════
	// Dropdown
	// ═════════════════════════════════════════════════════════════════
	// init: 预加 Dropdown → getDropdown 返回同一组件
	private static void testDropdownInit()
	{
		GameObject go = createDropdown(out myUGUIDropdown dd);
		try
		{
			Dropdown comp = go.GetComponent<Dropdown>();
			assertNotNull(dd.getDropdown(), "init 后 getDropdown 非 null");
			assertTrue(ReferenceEquals(comp, dd.getDropdown()), "getDropdown 返回同一组件");
			assertEqual(0, dd.getDropdown().options.Count, "初始无选项");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clearOptions/addOptions/getText 联动
	private static void testDropdownOptions()
	{
		GameObject go = createDropdown(out myUGUIDropdown dd);
		try
		{
			dd.addOptions(new List<string> { "A", "B", "C" });
			assertEqual(3, dd.getDropdown().options.Count, "addOptions 3 个选项");
			// value 默认 0, getText 读当前选中项文案
			assertEqual("A", dd.getText(), "默认选中第 0 项");
			dd.clearOptions();
			assertEqual(0, dd.getDropdown().options.Count, "clearOptions 清空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSelect/getSelect: value 读写联动 getText
	private static void testDropdownSelect()
	{
		GameObject go = createDropdown(out myUGUIDropdown dd);
		try
		{
			dd.addOptions(new List<string> { "A", "B", "C", "D" });
			dd.setSelect(2);
			assertEqual(2, dd.getSelect(), "setSelect(2) 读回");
			assertEqual("C", dd.getText(), "选中第 2 项文案 C");
			dd.setSelect(3);
			assertEqual("D", dd.getText(), "选中第 3 项文案 D");
			dd.setSelect(0);
			assertEqual("A", dd.getText(), "回到第 0 项文案 A");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Slider
	// ═════════════════════════════════════════════════════════════════
	// init: 预加 Slider → getSlider 返回同一组件
	private static void testSliderInit()
	{
		GameObject go = createSlider(out myUGUISlider slider);
		try
		{
			Slider comp = go.GetComponent<Slider>();
			assertNotNull(slider.getSlider(), "init 后 getSlider 非 null");
			assertTrue(ReferenceEquals(comp, slider.getSlider()), "getSlider 返回同一组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setRange/setValue/getValue
	private static void testSliderRangeValue()
	{
		GameObject go = createSlider(out myUGUISlider slider);
		try
		{
			slider.setRange(0, 100);
			assertEqual(0.0f, slider.getSlider().minValue, 0.001f, "setRange 下限 0");
			assertEqual(100.0f, slider.getSlider().maxValue, 0.001f, "setRange 上限 100");
			slider.setValue(30.0f);
			assertEqual(30.0f, slider.getValue(), 0.001f, "setValue(30) 读回");
			slider.setValue(0.0f);
			assertEqual(0.0f, slider.getValue(), 0.001f, "setValue(0) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSliderCallback: onValueChanged 监听, 值变化才触发
	private static void testSliderCallback()
	{
		GameObject go = createSlider(out myUGUISlider slider);
		try
		{
			int count = 0;
			float lastValue = -1.0f;
			slider.setSliderCallback((v) =>
			{
				++count;
				lastValue = v;
			});
			slider.setRange(0, 100);
			slider.setValue(40.0f);   // 触发一次
			assertEqual(1, count, "值变化触发回调");
			assertEqual(40.0f, lastValue, 0.001f, "回调收到新值");
			slider.setValue(40.0f);   // 值未变, 不触发
			assertEqual(1, count, "值未变不重复触发");
			slider.setValue(10.0f);
			assertEqual(2, count, "再次变化触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setFillRect/setHandleRect: 绑定子节点 RectTransform
	private static void testSliderFillHandleRect()
	{
		GameObject go = createSlider(out myUGUISlider slider);
		try
		{
			myUGUIObject fill = createChildUI("Fill", go.transform);
			myUGUIObject handle = createChildUI("Handle", go.transform);
			slider.setFillRect(fill);
			slider.setHandleRect(handle);
			assertTrue(ReferenceEquals(fill.getRectTransform(), slider.getSlider().fillRect), "fillRect 绑定");
			assertTrue(ReferenceEquals(handle.getRectTransform(), slider.getSlider().handleRect), "handleRect 绑定");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
