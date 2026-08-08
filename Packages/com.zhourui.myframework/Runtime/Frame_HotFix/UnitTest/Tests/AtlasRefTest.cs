using static TestAssert;

// AtlasRef 单元测试：generateToken/引用计数/resetProperty/setAtlas/destroy/isValid
public static class AtlasRefTest
{
	public static void Run()
	{
		testGenerateTokenIncrements();
		testGenerateTokenUniqueness();
		testResetProperty();
		testSetAtlasValid();
		testIsValidNullAtlas();
		testIsValidValidAtlas();
		testDestroyValidAtlas();
		testGetToken();
		testSetAtlasThenReset();
		testMultipleAtlasRefs();
		testNullAtlasGettersReturnNull();
		testNullAtlasHasSpriteFalse();
		testNullAtlasGetAtlasSingleNameNull();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// mock AtlasBase 用于测试
	private class MockAtlas : AtlasBase
	{
		public bool mIsValid = true;

		public MockAtlas() : base(null) { }

		public override bool isValid() { return mIsValid; }
		public override string getName() { return "MockAtlas"; }
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testGenerateTokenIncrements()
	{
		var atlas = new MockAtlas();
		var ref1 = new AtlasRef();
		ref1.setAtlas(atlas);
		long token1 = ref1.getToken();
		assertTrue(token1 > 0, "token 应大于0");

		var ref2 = new AtlasRef();
		ref2.setAtlas(atlas);
		long token2 = ref2.getToken();
		assertTrue(token2 > token1, "第二个 token 应大于第一个");
	}

	private static void testGenerateTokenUniqueness()
	{
		var atlas = new MockAtlas();
		var tokens = new System.Collections.Generic.HashSet<long>();
		for (int i = 0; i < 100; i++)
		{
			var refObj = new AtlasRef();
			refObj.setAtlas(atlas);
			long token = refObj.getToken();
			assertFalse(tokens.Contains(token), "token 应唯一");
			tokens.Add(token);
		}
	}

	private static void testResetProperty()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		refObj.setAtlas(atlas);
		refObj.resetProperty();
		assertNull(refObj.getAtlas(), "resetProperty 后 mAtlas=null");
		assertEqual(0L, refObj.getToken(), "resetProperty 后 mToken=0");
	}

	private static void testSetAtlasValid()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		refObj.setAtlas(atlas);
		assertNotNull(refObj.getAtlas(), "setAtlas 后 atlas 不为 null");
		assertTrue(refObj.getToken() > 0, "setAtlas 后 token>0");
		assertTrue(refObj.isValid(), "isValid 返回 true");
	}

	private static void testIsValidNullAtlas()
	{
		var refObj = new AtlasRef();
		assertFalse(refObj.isValid(), "未设置 atlas 时 isValid=false");
	}

	private static void testIsValidValidAtlas()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		refObj.setAtlas(atlas);
		assertTrue(refObj.isValid(), "设置有效 atlas 后 isValid=true");
		atlas.mIsValid = false;
		assertFalse(refObj.isValid(), "atlas invalid 后 isValid=false");
	}

	private static void testDestroyValidAtlas()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		refObj.setAtlas(atlas);
		long tokenBefore = refObj.getToken();
		assertTrue(tokenBefore > 0, "销毁前 token>0");
		refObj.destroy();
		// destroy 调用 removeReference(ref mToken)，token 被清零
		assertEqual(0L, refObj.getToken(), "destroy 后 token=0");
	}

	private static void testGetToken()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		assertEqual(0L, refObj.getToken(), "未设置 atlas 时 token=0");
		refObj.setAtlas(atlas);
		long token = refObj.getToken();
		assertTrue(token > 0, "设置 atlas 后 token>0");
	}

	private static void testSetAtlasThenReset()
	{
		var atlas = new MockAtlas();
		var refObj = new AtlasRef();
		refObj.setAtlas(atlas);
		long token = refObj.getToken();
		assertTrue(token > 0, "setAtlas 后 token>0");
		refObj.resetProperty();
		// resetProperty 清空本地引用和 token
		assertNull(refObj.getAtlas(), "resetProperty 后 atlas=null");
		assertEqual(0L, refObj.getToken(), "resetProperty 后 token=0");
	}

	private static void testMultipleAtlasRefs()
	{
		var atlas1 = new MockAtlas();
		var atlas2 = new MockAtlas();
		var ref1 = new AtlasRef();
		var ref2 = new AtlasRef();

		ref1.setAtlas(atlas1);
		ref2.setAtlas(atlas2);

		assertTrue(ref1.getToken() > 0, "ref1 token>0");
		assertTrue(ref2.getToken() > 0, "ref2 token>0");
		assertFalse(ref1.getToken() == ref2.getToken(), "不同 AtlasRef token 不同");

		// 销毁 ref1 不影响 ref2
		ref1.destroy();
		assertEqual(0L, ref1.getToken(), "ref1 destroy 后 token=0");
		assertTrue(ref2.getToken() > 0, "ref2 token 不受影响");
	}

	// ─── mAtlas==null 守卫: 所有 `?.` getter 返回 null(正常使用入口, 未加载/已释放时安全) ──
	//     getSprite/getFirstSpriteName/getSpriteList/getFilePath/getAtlasSingleName 均有 `?.` 空安全
	private static void testNullAtlasGettersReturnNull()
	{
		var refObj = new AtlasRef();
		assertNull(refObj.getSprite("any"), "mAtlas==null 时 getSprite 应为 null");
		assertNull(refObj.getFirstSpriteName(), "mAtlas==null 时 getFirstSpriteName 应为 null");
		assertNull(refObj.getSpriteList(), "mAtlas==null 时 getSpriteList 应为 null");
		assertNull(refObj.getFilePath(), "mAtlas==null 时 getFilePath 应为 null");
	}

	// ─── mAtlas==null 守卫: hasSprite 返回 false ──────────────────
	private static void testNullAtlasHasSpriteFalse()
	{
		var refObj = new AtlasRef();
		assertFalse(refObj.hasSprite(null), "mAtlas==null 时 hasSprite 应为 false");
	}

	// ─── mAtlas==null 守卫: getAtlasSingleName 返回 null ──────────
	private static void testNullAtlasGetAtlasSingleNameNull()
	{
		var refObj = new AtlasRef();
		assertNull(refObj.getAtlasSingleName(), "mAtlas==null 时 getAtlasSingleName 应为 null");
	}
}
