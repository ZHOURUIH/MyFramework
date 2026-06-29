using static TestAssert;

public static class MiscDataResetTest
{
	public static void Run()
	{
		testAtlasAndSpriteRefResetOnly();
		testAudioInfoResetAndGetClip();
		testMouseCastObjectSetBasic();
		testPacketAndSceneRegisterInfoFields();
		testSceneInstanceStateCallbacksAndReset();
		testTypeIDIsStableAndUniquePerType();
	}
	private static void testAtlasAndSpriteRefResetOnly()
	{
		AtlasLoadParam param = new();
		bool called = false;
		param.mName = "ui";
		param.mErrorIfNull = true;
		param.mCallback = atlas => called = true;
		param.resetProperty();
		assertNull(param.mName);
		assertNull(param.mCallback);
		assertFalse(param.mErrorIfNull);
		assertFalse(called);
		AtlasRef atlasRef = new();
		assertFalse(atlasRef.isValid());
		assertNull(atlasRef.getAtlas());
		assertEqual(0L, atlasRef.getToken());
		atlasRef.resetProperty();
		assertFalse(atlasRef.isValid());
		SpriteRef spriteRef = new();
		assertFalse(spriteRef.isValid());
		assertNull(spriteRef.getSprite());
		assertNull(spriteRef.getSpriteName());
		spriteRef.resetProperty();
		assertFalse(spriteRef.isValid());
	}
	private static void testAudioInfoResetAndGetClip()
	{
		AudioInfo info = new();
		info.mAudioName = "sfx/click.wav";
		info.mIsLocal = true;
		info.mState = LOAD_STATE.LOADED;
		assertNull(info.getClip(), "没有 mClip/mRawClip 时 getClip 返回 null");
		info.resetProperty();
		assertNull(info.mClip);
		assertNull(info.mRawClip);
		assertNull(info.mAudioName);
		assertFalse(info.mIsLocal);
		assertEqual(LOAD_STATE.NONE, info.mState);
	}
	private static void testMouseCastObjectSetBasic()
	{
		MouseCastObjectSet set = new();
		assertTrue(set.isEmpty());
		set.addObject(null);
		assertFalse(set.isEmpty());
		assertTrue(set.removeObject(null));
		assertTrue(set.isEmpty());
		set.addObject(null);
		set.resetProperty();
		assertTrue(set.isEmpty());
		assertNull(set.mCamera);
	}
	private static void testPacketAndSceneRegisterInfoFields()
	{
		PacketRegisterInfo packet = new(){ mTypeID = 12, mClassType = typeof(PacketAndPurchaseInfoTest) };
		assertEqual((ushort)12, packet.mTypeID);
		assertEqual(typeof(PacketAndPurchaseInfoTest), packet.mClassType);
		bool sceneCallbackCalled = false;
		SceneRegisteInfo scene = new()
		{
			mName = "Main",
			mScenePath = "Assets/Scenes/Main.unity",
			mSceneType = typeof(SceneInstance),
			mCallback = instance => sceneCallbackCalled = true,
		};
		assertEqual("Main", scene.mName);
		assertEqual("Assets/Scenes/Main.unity", scene.mScenePath);
		assertEqual(typeof(SceneInstance), scene.mSceneType);
		scene.mCallback(null);
		assertTrue(sceneCallbackCalled);
	}
	private static void testSceneInstanceStateCallbacksAndReset()
	{
		SceneInstance scene = new();
		int loaded = 0;
		float loading = 0.0f;
		scene.setType(typeof(SceneInstance));
		scene.setName("Battle");
		scene.setState(LOAD_STATE.LOADED);
		scene.setActiveLoaded(true);
		scene.setMainScene(true);
		scene.setLoadedCallback(() => ++loaded);
		scene.setLoadingCallback(v => loading = v);
		scene.callLoading(0.75f);
		scene.callLoaded();
		assertEqual(0.75f, loading);
		assertEqual(1, loaded);
		assertEqual(typeof(SceneInstance), scene.getType());
		assertEqual("Battle", scene.getName());
		assertEqual(LOAD_STATE.LOADED, scene.getState());
		assertTrue(scene.isActiveLoaded());
		assertTrue(scene.isMainScene());
		scene.resetProperty();
		assertNull(scene.getType());
		assertNull(scene.getName());
		assertEqual(LOAD_STATE.NONE, scene.getState());
		assertFalse(scene.isActiveLoaded());
		assertFalse(scene.isMainScene());
		assertFalse(scene.isInited());
		assertNull(scene.getRoot());
	}
	private static void testTypeIDIsStableAndUniquePerType()
	{
		int intID0 = TypeID<int>.ID;
		int intID1 = TypeID<int>.ID;
		int stringID = TypeID<string>.ID;
		assertEqual(intID0, intID1, "同一泛型类型的 ID 应稳定");
		assertTrue(intID0 != stringID, "不同泛型类型的 ID 应不同");
	}
}