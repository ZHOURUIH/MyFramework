using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIDropdown 深度测试(UGUI Dropdown 封装):
//   init: 无 Dropdown 组件时自动添加(isNewObject 不 logError), 有则直接绑定
//   clearOptions/addOptions: 操作 UGUI Dropdown 的选项列表
//   setSelect/getSelect: 读写当前选中项(getSelect 走 mDropdown.value)
//   getText: 读取当前选中项的文本
// 环境: 裸 GameObject + RectTransform + myUGUIDropdown(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUIDropdownTest
{
	public static void Run()
	{
		testInitWithComponent();
		testInitAutoAddDropdown();
		testAddOptions();
		testSetSelectGetSelect();
		testGetText();
		testClearOptions();
		testOptionsSelectFullChain();
		testClearThenReaddChain();
		testAppendOptionsSequence();
		testDefaultSelectZero();
		testSetSelectLastIndex();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUIDropdown
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIDropdown createDropdown(out GameObject go)
	{
		go = new GameObject("DropdownGO");
		go.AddComponent<RectTransform>();
		myUGUIDropdown dd = new myUGUIDropdown();
		dd.setIsNewObject(true);   // 自动补 Dropdown 组件, 避免 init 里 getLayout() NRE(logError 分支)
		dd.setObject(go);
		dd.init();
		return dd;
	}

	// init: 预加 Dropdown 组件 → 直接绑定
	private static void testInitWithComponent()
	{
		GameObject go = new GameObject("DropdownWithComp");
		go.AddComponent<RectTransform>();
		go.AddComponent<Dropdown>();
		try
		{
			myUGUIDropdown dd = new myUGUIDropdown();
			dd.setObject(go);
			dd.init();
			assertTrue(dd.getDropdown() != null, "预加 Dropdown 组件后 init 绑定成功");
			assertTrue(ReferenceEquals(go.GetComponent<Dropdown>(), dd.getDropdown()), "绑定的是同一个组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// init: 无 Dropdown 组件 + isNewObject=true → 自动 AddComponent
	private static void testInitAutoAddDropdown()
	{
		GameObject go = new GameObject("DropdownAuto");
		go.AddComponent<RectTransform>();
		try
		{
			myUGUIDropdown dd = new myUGUIDropdown();
			dd.setIsNewObject(true);   // 自动补组件, 不触发 logError
			dd.setObject(go);
			dd.init();
			assertTrue(dd.getDropdown() != null, "init 自动添加 Dropdown 组件");
			assertTrue(go.GetComponent<Dropdown>() != null, "GameObject 上存在 Dropdown 组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// addOptions: 添加选项列表
	private static void testAddOptions()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> options = new List<string>();
			options.Add("选项A");
			options.Add("选项B");
			options.Add("选项C");
			dd.addOptions(options);
			assertEqual(3, dd.getDropdown().options.Count, "addOptions 后选项数 = 3");
			assertEqual("选项A", dd.getDropdown().options[0].text, "第 0 项文本正确");
			assertEqual("选项C", dd.getDropdown().options[2].text, "第 2 项文本正确");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSelect/getSelect: 读写选中项
	private static void testSetSelectGetSelect()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> options = new List<string>();
			options.Add("A");
			options.Add("B");
			options.Add("C");
			dd.addOptions(options);
			dd.setSelect(1);
			assertEqual(1, dd.getSelect(), "setSelect(1) 读回 1");
			dd.setSelect(2);
			assertEqual(2, dd.getSelect(), "setSelect(2) 读回 2");
			dd.setSelect(0);
			assertEqual(0, dd.getSelect(), "setSelect(0) 读回 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getText: 读取当前选中项文本
	private static void testGetText()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> options = new List<string>();
			options.Add("第一项");
			options.Add("第二项");
			dd.addOptions(options);
			dd.setSelect(0);
			assertEqual("第一项", dd.getText(), "选中第 0 项时 getText 返回对应文本");
			dd.setSelect(1);
			assertEqual("第二项", dd.getText(), "选中第 1 项时 getText 返回对应文本");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clearOptions: 清空选项列表
	private static void testClearOptions()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> options = new List<string>();
			options.Add("A");
			options.Add("B");
			dd.addOptions(options);
			assertEqual(2, dd.getDropdown().options.Count, "清空前选项数 = 2");
			dd.clearOptions();
			assertEqual(0, dd.getDropdown().options.Count, "clearOptions 后选项数 = 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合序列: 5 个选项全量遍历选中 → 每个 getText 对应
	// ═════════════════════════════════════════════════════════════════
	private static void testOptionsSelectFullChain()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> options = new List<string> { "item0", "item1", "item2", "item3", "item4" };
			dd.addOptions(options);
			for (int i = 0; i < options.Count; ++i)
			{
				dd.setSelect(i);
				assertEqual(i, dd.getSelect(), "选中第 " + i + " 项");
				assertEqual("item" + i, dd.getText(), "getText 对应第 " + i + " 项");
			}
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合序列: 清空 → 重新 addOptions → 选中 → 读取(选项生命周期)
	// ═════════════════════════════════════════════════════════════════
	private static void testClearThenReaddChain()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			List<string> first = new List<string> { "A", "B" };
			dd.addOptions(first);
			dd.setSelect(1);
			assertEqual("B", dd.getText(), "第一轮选中 B");
			// 清空后重新填充
			dd.clearOptions();
			List<string> second = new List<string> { "X", "Y", "Z" };
			dd.addOptions(second);
			dd.setSelect(2);
			assertEqual("Z", dd.getText(), "重填后选中 Z");
			assertEqual(2, dd.getSelect(), "重填后 getSelect 2");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合序列: 多次 addOptions 累加选项
	// ═════════════════════════════════════════════════════════════════
	private static void testAppendOptionsSequence()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			dd.addOptions(new List<string> { "a", "b" });
			dd.addOptions(new List<string> { "c", "d" });
			dd.setSelect(3);
			assertEqual("d", dd.getText(), "两次追加后第 4 项为 d");
			assertEqual(3, dd.getSelect(), "getSelect 3");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认选中: addOptions 后未 setSelect → 默认选中第 0 项
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultSelectZero()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			dd.addOptions(new List<string> { "first", "second" });
			assertEqual(0, dd.getSelect(), "未 setSelect 默认 getSelect 0");
			assertEqual("first", dd.getText(), "默认选中首项文本");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 边界: 选中末项(最后一个下标)
	// ═════════════════════════════════════════════════════════════════
	private static void testSetSelectLastIndex()
	{
		myUGUIDropdown dd = createDropdown(out GameObject go);
		try
		{
			dd.addOptions(new List<string> { "a", "b", "c" });
			dd.setSelect(2);
			assertEqual("c", dd.getText(), "末项文本 c");
			dd.setSelect(0);
			assertEqual("a", dd.getText(), "回到首项文本 a");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
