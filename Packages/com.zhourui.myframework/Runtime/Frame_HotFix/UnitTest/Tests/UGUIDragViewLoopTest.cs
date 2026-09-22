using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static FrameUtility;
using static TestAssert;

// 循环滚动列表的数据绑定回归测试。
// 使用真实窗口、WindowStructPool和ClassPool,不复制setDataList/updateDisplayItem的实现。
// 在框架初始化完成后运行,与FrameHotFixTest中其他UI测试的运行条件一致。
public static class UGUIDragViewLoopTest
{
	public static void Run()
	{
		assertNotNull(mClassPool, "循环列表测试需要已经初始化的ClassPool");
		assertNotNull(mGlobalTouchSystem, "循环列表测试需要已经初始化的GlobalTouchSystem");
		assertNotNull(mEventSystem, "循环列表测试需要已经初始化的EventSystem");
		testSameCountRebindsExistingItems();
		testStartEndDataListRebind();
		testSameCountKeepsScrolledPosition();
		testForceRefreshRebindsOnce();
		testStationaryTickDoesNotRebind();
		testScrollKeepsOverlappingBindings();
		testCountChangeClearsBeforeRecycle();
		testClearThenPopulate();
		testSameListKeepsDataAlive();
		testSharedDataAndReorderKeepsDataAlive();
	}

	// 新旧数量和可见范围均相同,必须复用节点,但不能继续引用即将回收的旧Data。
	private static void testSameCountRebindsExistingItems()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> oldData = f.createData(6, 10);
		f.mLoop.setDataList(oldData);
		assertEqual(4, f.mLoop.getVisibleCount(), "200x80视口应显示两列两行");
		TestDragViewLoopItem item = f.mLoop.getVisibleItem(0);
		long assignID = item.getAssignID();
		int bindCount = item.mSetDataCount;
		int clearCount = item.mClearDataCount;
		Vector3 contentPos = f.mLoop.getContent().getPosition();
		Vector2 contentSize = f.mLoop.getContent().getSize();
		List<TestDragViewLoopData> newData = f.createData(6, 100);

		f.mLoop.setDataList(newData);

		assertTrue(ReferenceEquals(item, f.mLoop.getVisibleItem(0)), "同数量刷新不能重新创建显示节点");
		assertEqual(assignID, item.getAssignID(), "同数量刷新不能把保留节点回收后重新分配");
		assertEqual(bindCount + 1, item.mSetDataCount, "保留节点必须重新绑定一次");
		assertEqual(clearCount + 1, item.mClearDataCount, "重新绑定前必须清理旧绑定");
		assertEqual(contentPos, f.mLoop.getContent().getPosition(), "同数量刷新保持原Content位置");
		assertEqual(contentSize, f.mLoop.getContent().getSize(), "同数量刷新保持原Content尺寸");
		checkVisibleBindings(f.mLoop);
		assertEqual(100, item.readBoundValue(), "刷新后业务读取必须得到新数据,不是已回收Data的默认值");
		checkRecycled(oldData);
	}

	private static void testStartEndDataListRebind()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> list = f.mLoop.startSetDataList();
		for (int i = 0; i < 6; ++i)
		{
			list.Add(f.createDataItem(i));
		}
		f.mLoop.endSetDataList();
		TestDragViewLoopItem first = f.mLoop.getVisibleItem(0);
		list = f.mLoop.startSetDataList();
		for (int i = 0; i < 6; ++i)
		{
			list.Add(f.createDataItem(200 + i));
		}
		f.mLoop.endSetDataList(true);
		assertTrue(ReferenceEquals(first, f.mLoop.getVisibleItem(0)), "start/end入口也应复用显示节点");
		assertEqual(200, first.readBoundValue(), "start/end入口应绑定新数据");
		checkVisibleBindings(f.mLoop);
	}

	private static void testSameCountKeepsScrolledPosition()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> oldData = f.createData(12, 10);
		f.mLoop.setDataList(oldData);
		f.scrollDown(40.0f);
		TestDragViewLoopItem item = f.mLoop.getVisibleItem(2);
		assertNotNull(item, "下移一行后下标2应可见");
		Vector3 pos = f.mLoop.getContent().getPosition();
		List<TestDragViewLoopData> newData = f.createData(12, 300);
		f.mLoop.setDataList(newData, true);
		assertEqual(pos, f.mLoop.getContent().getPosition(), "同数量刷新不能改变滚动位置");
		assertTrue(ReferenceEquals(item, f.mLoop.getVisibleItem(2)), "滚动后的同数量刷新仍复用节点");
		assertEqual(302, item.readBoundValue(), "非零起始下标必须绑定对应的新数据");
		checkVisibleBindings(f.mLoop);
		checkRecycled(oldData);
	}

	private static void testForceRefreshRebindsOnce()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> data = f.createData(6, 10);
		f.mLoop.setDataList(data);
		TestDragViewLoopItem item = f.mLoop.getVisibleItem(0);
		int setCount = item.mSetDataCount;
		int clearCount = item.mClearDataCount;
		data[0].mValue = 777;
		f.mLoop.updateDisplayItem(true);
		assertEqual(setCount + 1, item.mSetDataCount, "强制刷新应重新绑定一次,不能绑定两次");
		assertEqual(clearCount + 1, item.mClearDataCount, "强制刷新应先清理一次");
		assertEqual(777, item.mDisplayedValue, "强制刷新应更新节点记录的显示值");
		assertFalse(data[0].isDestroy(), "仅刷新显示不能回收数据");
		checkVisibleBindings(f.mLoop);
	}

	private static void testStationaryTickDoesNotRebind()
	{
		using var f = new Fixture();
		f.mLoop.setDataList(f.createData(6, 10));
		TestDragViewLoopItem item = f.mLoop.getVisibleItem(0);
		int setCount = item.mSetDataCount;
		int clearCount = item.mClearDataCount;
		for (int i = 0; i < 16; ++i)
		{
			f.mLoop.updateDragView();
			f.mLoop.updateDisplayItem(false);
		}
		assertEqual(setCount, item.mSetDataCount, "位置和范围不变时不能每帧重新绑定");
		assertEqual(clearCount, item.mClearDataCount, "位置和范围不变时不能每帧清理绑定");
		checkVisibleBindings(f.mLoop);
	}

	private static void testScrollKeepsOverlappingBindings()
	{
		using var f = new Fixture();
		f.mLoop.setDataList(f.createData(12, 10));
		TestDragViewLoopItem overlap = f.mLoop.getVisibleItem(2);
		int setCount = overlap.mSetDataCount;
		int clearCount = overlap.mClearDataCount;
		f.scrollDown(40.0f);
		assertTrue(ReferenceEquals(overlap, f.mLoop.getVisibleItem(2)), "普通滚动应保留重叠范围的节点");
		assertEqual(setCount, overlap.mSetDataCount, "普通滚动不能重复绑定重叠节点");
		assertEqual(clearCount, overlap.mClearDataCount, "普通滚动不能清理重叠节点");
		assertTrue(f.mLoop.getVisibleItem(0) == null, "离开可见范围的节点应移出索引");
		assertNotNull(f.mLoop.getVisibleItem(4), "新进入范围的下标4应创建或复用节点");
		checkVisibleBindings(f.mLoop);
		foreach (TestDragViewLoopData data in f.mLoop.getDataList())
		{
			assertFalse(data.isDestroy(), "普通滚动不能回收列表拥有的数据");
		}
	}

	private static void testCountChangeClearsBeforeRecycle()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> oldData = f.createData(6, 10);
		f.mLoop.setDataList(oldData);
		List<TestDragViewLoopData> newData = f.createData(2, 500);
		// TestDragViewLoopItem.recycle会检查:进入节点池以前旧绑定已经清理。
		f.mLoop.setDataList(newData);
		assertEqual(2, f.mLoop.getVisibleCount(), "缩短列表后仅保留新数量的可见节点");
		assertEqual(40.0f, f.mLoop.getContent().getSize().y, 0.001f, "缩短列表后重算Content高度");
		checkVisibleBindings(f.mLoop);
		checkRecycled(oldData);
	}

	private static void testClearThenPopulate()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> oldData = f.createData(6, 10);
		f.mLoop.setDataList(oldData);
		f.mLoop.setDataList(null);
		assertEqual(0, f.mLoop.getDataList().Count, "null列表应清空数据");
		assertEqual(0, f.mLoop.getVisibleCount(), "清空以后不能残留可见节点");
		assertEqual(0.0f, f.mLoop.getContent().getSize().y, 0.001f, "清空后Content高度为0");
		checkRecycled(oldData);
		f.mLoop.setDataList(new List<TestDragViewLoopData>());
		f.mLoop.setDataList(f.createData(6, 600));
		assertEqual(4, f.mLoop.getVisibleCount(), "清空后重新填充应恢复正确可见范围");
		checkVisibleBindings(f.mLoop);
	}

	private static void testSameListKeepsDataAlive()
	{
		using var f = new Fixture();
		f.mLoop.setDataList(f.createData(6, 10));
		List<TestDragViewLoopData> sameList = f.mLoop.getDataList();
		TestDragViewLoopData first = sameList[0];
		first.mValue = 888;
		f.mLoop.setDataList(sameList, true);
		assertTrue(ReferenceEquals(first, f.mLoop.getDataList()[0]), "同一个列表刷新应保留原Data引用");
		assertFalse(first.isDestroy(), "不能回收新列表仍引用的Data");
		assertEqual(888, f.mLoop.getVisibleItem(0).mDisplayedValue, "同一个列表刷新也应更新显示");
		checkVisibleBindings(f.mLoop);
	}

	private static void testSharedDataAndReorderKeepsDataAlive()
	{
		using var f = new Fixture();
		List<TestDragViewLoopData> oldData = f.createData(6, 10);
		f.mLoop.setDataList(oldData);
		TestDragViewLoopData replacement = f.createDataItem(999);
		List<TestDragViewLoopData> newData = new()
		{
			oldData[3], oldData[1], replacement, oldData[0], oldData[4], oldData[5]
		};
		f.mLoop.setDataList(newData, true);
		assertTrue(oldData[2].isDestroy(), "不再被新列表引用的旧数据必须回收");
		foreach (TestDragViewLoopData data in newData)
		{
			assertFalse(data.isDestroy(), "重新排序保留的数据不能被回收");
		}
		assertEqual(13, f.mLoop.getVisibleItem(0).readBoundValue(), "重新排序以后节点必须绑定新下标的数据");
		assertEqual(999, f.mLoop.getVisibleItem(2).readBoundValue(), "局部替换的数据应正常显示");
		checkVisibleBindings(f.mLoop);
	}

	private static void checkVisibleBindings(TestDragViewLoopWindow loop)
	{
		foreach (TestDragViewLoopItem item in loop.getVisibleItems())
		{
			assertTrue(ReferenceEquals(loop.getDataList()[item.getIndex()], item.mData), "可见节点必须绑定当前列表对应下标的数据");
			assertFalse(item.mData.isDestroy(), "可见节点不能绑定已回收的数据");
			assertEqual(item.mData.mValue, item.mDisplayedValue, "显示值必须同步到当前数据");
		}
	}

	private static void checkRecycled(List<TestDragViewLoopData> list)
	{
		foreach (TestDragViewLoopData data in list)
		{
			assertTrue(data.isDestroy(), "被完全替换的旧数据必须回收");
			assertEqual(0, data.mValue, "旧数据回收后必须执行resetProperty");
		}
	}

	private sealed class Fixture : IDisposable
	{
		public TestDragViewLoopWindow mLoop;                     // 被测循环列表
		private GameObject mRootObject;                          // 本次测试的独立Unity根节点
		private myUGUICanvas mRoot;                              // 根节点包装对象,统一清理其子树
		private readonly List<TestDragViewLoopData> mCreated = new(); // 测试创建的数据,失败时清理未交给列表的数据

		public Fixture()
		{
			try
			{
				TestDragViewLoopLayout script = new();
				GameLayout layout = new();
				layout.setName("UGUIDragViewLoopTest");
				layout.setScript(script);
				script.setLayout(layout);
				mRootObject = new GameObject("UGUIDragViewLoopTest", typeof(RectTransform), typeof(Canvas));
				mRoot = new myUGUICanvas();
				mRoot.setObject(mRootObject);
				mRoot.setLayout(layout);
				mRoot.init();
				script.setRoot(mRoot);
				myUGUIObject viewport = script.createUGUIObject<myUGUIObject>(mRoot, "Viewport", true);
				viewport.setSize(new Vector2(200.0f, 80.0f));

				// Content/Item只创建GameObject,由assignWindow/assignTemplate创建一次包装对象。
				// 避免预先创建包装对象后,框架newObject再次包装同一个GameObject。
				GameObject content = createBareRect(viewport.getGameObject(), "Content", new Vector2(200.0f, 80.0f));
				content.AddComponent<ScaleAnchor>();
				createBareRect(content, "Item", new Vector2(100.0f, 40.0f));
				mLoop = new TestDragViewLoopWindow(script);
				mLoop.assignWindow(viewport);
				mLoop.assignTemplate("Item");
				mLoop.init();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public TestDragViewLoopData createDataItem(int value)
		{
			TestDragViewLoopData data = CLASS<TestDragViewLoopData>();
			assertNotNull(data, "测试数据必须从ClassPool成功创建");
			assertFalse(data.isDestroy(), "ClassPool分配出的测试数据必须处于有效状态");
			data.mValue = value;
			mCreated.Add(data);
			return data;
		}
		public List<TestDragViewLoopData> createData(int count, int firstValue)
		{
			List<TestDragViewLoopData> data = new(count);
			for (int i = 0; i < count; ++i)
			{
				data.Add(createDataItem(firstValue + i));
			}
			return data;
		}
		public void scrollDown(float distance)
		{
			myUGUIDragView content = mLoop.getContent();
			content.setPosition(content.getPosition() + new Vector3(0.0f, distance, 0.0f));
			mLoop.updateDragView();
		}
		public void Dispose()
		{
			try
			{
				if (mLoop != null && mLoop.getContent() != null)
				{
					mLoop.setDataList(null);
				}
			}
			finally
			{
				try
				{
					mLoop?.destroy();
					mLoop = null;
				}
				finally
				{
					foreach (TestDragViewLoopData data in mCreated)
					{
						if (!data.isDestroy())
						{
							UN_CLASS(data);
						}
					}
					mCreated.Clear();
					if (mRoot != null)
					{
						LayoutScript.destroyObject(mRoot, true);
						mRoot = null;
					}
					if (mRootObject != null)
					{
						UnityEngine.Object.DestroyImmediate(mRootObject);
						mRootObject = null;
					}
				}
			}
		}
		private static GameObject createBareRect(GameObject parent, string name, Vector2 size)
		{
			GameObject go = new(name, typeof(RectTransform));
			RectTransform rect = go.GetComponent<RectTransform>();
			rect.SetParent(parent.transform, false);
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.sizeDelta = size;
			return go;
		}
	}
}

public class TestDragViewLoopData : ClassObject
{
	public int mValue; // 模拟TitleItem.Data中的业务字段,回收以后归零
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = 0;
	}
}

public class TestDragViewLoopItem : DragViewItem<TestDragViewLoopData>
{
	public TestDragViewLoopData mData; // 节点当前绑定的数据
	public int mDisplayedValue;       // setData时同步到显示侧的值
	public int mSetDataCount;         // 用于检测重复绑定和每帧无效刷新
	public int mClearDataCount;       // 用于检测清理旧数据的次数
	public TestDragViewLoopItem(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal() { }
	public override void clearData()
	{
		base.clearData();
		if (mData != null)
		{
			assertFalse(mData.isDestroy(), "clearData执行时旧数据必须仍然有效");
		}
		mData = null;
		mDisplayedValue = 0;
		++mClearDataCount;
	}
	public override void setData(TestDragViewLoopData data)
	{
		assertTrue(mData == null, "重新绑定之前必须先clearData");
		assertNotNull(data, "本组测试只提交有效的Data");
		assertFalse(data.isDestroy(), "不能把已回收的数据绑定到可见节点");
		mData = data;
		mDisplayedValue = data.mValue;
		++mSetDataCount;
	}
	public override void recycle()
	{
		base.recycle();
		assertTrue(mData == null, "回收显示节点前必须清理旧绑定");
	}
	public int readBoundValue()
	{
		assertNotNull(mData, "业务读取时节点必须已经绑定数据");
		assertFalse(mData.isDestroy(), "业务读取不能访问已回收数据");
		return mData.mValue;
	}
}

public class TestDragViewLoopWindow : UGUIDragViewLoop<TestDragViewLoopItem, TestDragViewLoopData>
{
	public TestDragViewLoopWindow(IWindowObjectOwner parent) : base(parent) { }
	public TestDragViewLoopItem getVisibleItem(int index)
	{
		mDisplayItemMap.TryGetValue(index, out TestDragViewLoopItem item);
		return item;
	}
	public int getVisibleCount() { return mDisplayItemPool.getUsedList().Count; }
	public List<TestDragViewLoopItem> getVisibleItems() { return mDisplayItemPool.getUsedList(); }
}

public class TestDragViewLoopLayout : LayoutScript
{
	public override void assignWindow() { }
}
