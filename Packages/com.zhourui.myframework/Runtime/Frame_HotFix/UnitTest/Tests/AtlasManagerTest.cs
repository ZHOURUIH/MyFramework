using static TestAssert;

// AtlasManager 纯逻辑单测(图集管理) + AtlasLoadParam
//
// 设计要点:
//   - AtlasRef/SpriteRef 已分别在 AtlasRefTest/SpriteRefTest 全覆盖本次新增聚焦 AtlasManager 本体与 AtlasLoadParam。
//   - AtlasManager 继承 FrameSystem; 构造时(编辑器下) mCreateObject=true, 但因未调用 init(), mObject=null,
//     destroy() 走 destroyUnityObject(null) 空安全; destroy() 还执行 SpriteAtlasManager.atlasRequested -= 订阅(空安全)。
//   - getAtlas/getAtlasAsyncSafe/getSprite/atlasLoaded/initAsync/update 依赖资源异步加载/编辑器真图集, 副作用大, 不测。
//   - 只测空状态与空引用分支; unloadAtlas 用 MockAtlas 注册进 mAtlasList 走成功路径(不触发 logError)。
public static class AtlasManagerTest
{
	public static void Run()
	{
		testNewAtlasManagerEmptyList();
		testGetAtlasListEmpty();
		testUnloadAtlasNull();
		testUnloadAtlasNullRef();
		testUnloadAtlasRegisteredSuccess();
		testDestroyAllOnEmpty();
		testAddDontUnloadAtlas();
		testDestroySafe();
		testAtlasLoadParamDefault();
		testAtlasLoadParamResetProperty();
		testUnloadSpriteNull();
		testUnloadAtlasAfterUnload();
	}

	// ─── new + destroy 空安全 ────────────────────────────────────────
	private static void testNewAtlasManagerEmptyList()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			// 构造不触发任何资源加载, mAtlasList 为空
			assertNotNull(mgr.getAtlasList(), "getAtlasList 不应为 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── getAtlasList 初始为空 ───────────────────────────────────────
	private static void testGetAtlasListEmpty()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			assertEqual(0, mgr.getAtlasList().count(), "初始图集列表为空");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── unloadAtlas(null) → false, 无 logError ─────────────────────
	private static void testUnloadAtlasNull()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			assertFalse(mgr.unloadAtlas((AtlasRef)null), "unloadAtlas(null) 应返回 false");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── unloadAtlas(ref null) → false 且置空 ───────────────────────
	private static void testUnloadAtlasNullRef()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			AtlasRef ptr = null;
			mgr.unloadAtlas(ref ptr);
			assertNull(ptr, "unloadAtlas(ref null) 后引用保持 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── unloadAtlas 成功路径(注册 MockAtlas, 引用一致 → 无 logError) ──
	private static void testUnloadAtlasRegisteredSuccess()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			MockAtlas atlas = new();
			atlas.setFilePath("test/atlas.ab");
			// 直接把 mock 图集放入 mAtlasList(避免走 getAtlas 的资源加载路径)
			mgr.getAtlasList().add("test/atlas.ab", atlas);

			// 注意: 必须用 CLASS(out ...) 从类池创建 AtlasRef(而非裸 new)!
			// unloadAtlas 内部对 atlasPtr 调 UN_CLASS -> destroyClass -> removeInuse,
			// 要求对象在类池 inuse 列表里; 裸 new 绕过类池不在列表, 会触发
			// "Inused List not contains class object" logError(实测踩坑)。
			// 链式写法与源码 AtlasManager 一致(out 与返回值是同一池对象)。
			FrameUtility.CLASS(out AtlasRef at).setAtlas(atlas);
			// atlasPtr.getAtlas() 与 mAtlasList 中存的是同一实例 → 不触发"图集不一致" logError
			assertTrue(mgr.unloadAtlas(at), "注册过的图集 unloadAtlas 应返回 true");
			// unloadAtlas 已 UN_CLASS 回收 at(AtlasRef.destroy 会 removeReference 配平 addReference)
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── destroyAll 空列表安全 ───────────────────────────────────────
	private static void testDestroyAllOnEmpty()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			// 空 mAtlasList 时 forValue 不执行, clear 空列表, 无副作用
			mgr.destroyAll();
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── addDontUnloadAtlas 无副作用(加入不允许卸载集合) ────────────
	private static void testAddDontUnloadAtlas()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			// 加入不允许卸载的图集名, 仅改内部分哈希集合, 可重复调用安全
			mgr.addDontUnloadAtlas("keep.atlas");
			mgr.addDontUnloadAtlas("keep.atlas");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── destroy 空安全 ──────────────────────────────────────────────
	private static void testDestroySafe()
	{
		AtlasManager mgr = new AtlasManager();
		// mObject=null(mCreateObject=true 但未 init) → destroyUnityObject(null) 提前返回; 订阅反注册安全
		mgr.destroy();
	}

	// ─── unloadSprite ────────────────────────────────────────────────

	// unloadSprite(null 引用) 空安全
	private static void testUnloadSpriteNull()
	{
		AtlasManager mgr = new AtlasManager();
		SpriteRef ptr = null;
		mgr.unloadSprite(ref ptr);
		// 无异常即通过
	}

	// 注: unloadSprite 有效引用不可测——SpriteRef 必须 CLASS() 从池创建(裸 new 不在池 inuse 列表
	//     → removeInuse logError)且 destroy 需要真 Sprite(未 setSprite → "sprite is null" logError)——合法跳过

	// unloadAtlas(ref) 重载版: 卸载后外部引用置 null
	private static void testUnloadAtlasAfterUnload()
	{
		AtlasManager mgr = new AtlasManager();
		try
		{
			MockAtlas atlas = new();
			atlas.setFilePath("test/atlas2.ab");
			mgr.getAtlasList().add("test/atlas2.ab", atlas);
			FrameUtility.CLASS(out AtlasRef at).setAtlas(atlas);
			mgr.unloadAtlas(ref at);
			assertNull(at, "unloadAtlas(ref) 后外部引用置 null");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── AtlasLoadParam 默认字段 ─────────────────────────────────────
	private static void testAtlasLoadParamDefault()
	{
		AtlasLoadParam param = new AtlasLoadParam();
		assertNull(param.mName, "默认 mName 为 null");
		assertNull(param.mCallback, "默认 mCallback 为 null");
		assertFalse(param.mErrorIfNull, "默认 mErrorIfNull 为 false");
	}

	// ─── AtlasLoadParam.resetProperty 复原 ───────────────────────────
	private static void testAtlasLoadParamResetProperty()
	{
		AtlasLoadParam param = new AtlasLoadParam();
		param.mName = "xx.ab";
		param.mCallback = _ => { };
		param.mErrorIfNull = true;
		param.resetProperty();
		assertNull(param.mName, "resetProperty 后 mName 为 null");
		assertNull(param.mCallback, "resetProperty 后 mCallback 为 null");
		assertFalse(param.mErrorIfNull, "resetProperty 后 mErrorIfNull 为 false");
	}

	// ─── mock AtlasBase, 用于测试(等价 AtlasRefTest.MockAtlas) ─────
	private class MockAtlas : AtlasBase
	{
		public bool mIsValid = true;
		public MockAtlas() : base(null) { }
		public override bool isValid() { return mIsValid; }
		public override string getName() { return "MockAtlas"; }
	}
}
