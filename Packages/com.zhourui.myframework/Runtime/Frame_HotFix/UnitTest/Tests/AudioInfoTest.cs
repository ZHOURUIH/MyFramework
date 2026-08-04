using static TestAssert;

// AudioInfo 单元测试：getClip优先级逻辑/resetProperty/字段重置
public static class AudioInfoTest
{
	public static void Run()
	{
		testGetClipFromResourceRef();
		testGetClipFallbackToRawClip();
		testGetClipNullWhenBothNull();
		testGetClipResourceRefNullButRawClipValid();
		testResetProperty();
		testResetPropertyClearsAll();
		testDefaultState();
		testGetClipAfterReset();
		testSetRawClipThenReset();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static UnityEngine.AudioClip createClip()
	{
		return UnityEngine.AudioClip.Create("TestClip", 44100, 1, 44100, false);
	}

	private static AudioInfo createInfo()
	{
		return new AudioInfo();
	}

	private static void testGetClipFromResourceRef()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = new ResourceRef<UnityEngine.AudioClip>();
		var field = typeof(ResourceRef<UnityEngine.AudioClip>).GetField("mResource",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(info.mClip, clip);
		var result = info.getClip();
		assertNotNull(result, "有 ResourceRef 时 getClip 返回 clip");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testGetClipFallbackToRawClip()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = null;
		info.mRawClip = clip;
		var result = info.getClip();
		assertNotNull(result, "ResourceRef 为 null 时 getClip 返回 mRawClip");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testGetClipNullWhenBothNull()
	{
		var info = createInfo();
		info.mClip = null;
		info.mRawClip = null;
		var result = info.getClip();
		assertNull(result, "两者都为 null 时 getClip 返回 null");
	}

	private static void testGetClipResourceRefNullButRawClipValid()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = null;
		info.mRawClip = clip;
		var result = info.getClip();
		assertEqual(clip, result, "mClip 为 null 时返回 mRawClip");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testResetProperty()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = new ResourceRef<UnityEngine.AudioClip>();
		var field = typeof(ResourceRef<UnityEngine.AudioClip>).GetField("mResource",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(info.mClip, clip);
		info.mRawClip = clip;
		info.mState = LOAD_STATE.LOADED;
		info.mAudioName = "test.mp3";
		info.mIsLocal = true;

		info.resetProperty();

		assertNull(info.mClip, "resetProperty 后 mClip=null");
		assertNull(info.mRawClip, "resetProperty 后 mRawClip=null");
		assertEqual(LOAD_STATE.NONE, info.mState, "resetProperty 后 mState=NONE");
		assertNull(info.mAudioName, "resetProperty 后 mAudioName=null");
		assertFalse(info.mIsLocal, "resetProperty 后 mIsLocal=false");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testResetPropertyClearsAll()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = new ResourceRef<UnityEngine.AudioClip>();
		var field = typeof(ResourceRef<UnityEngine.AudioClip>).GetField("mResource",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(info.mClip, clip);
		info.mRawClip = clip;
		info.mAudioName = "sound/test.wav";
		info.mIsLocal = true;
		info.mState = LOAD_STATE.LOADING;

		info.resetProperty();

		assertNull(info.mClip, "mClip 被清空");
		assertNull(info.mRawClip, "mRawClip 被清空");
		assertNull(info.mAudioName, "mAudioName 被清空");
		assertFalse(info.mIsLocal, "mIsLocal=false");
		assertEqual(LOAD_STATE.NONE, info.mState, "mState=NONE");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testDefaultState()
	{
		var info = createInfo();
		assertNull(info.mClip, "默认 mClip=null");
		assertNull(info.mRawClip, "默认 mRawClip=null");
		assertEqual(LOAD_STATE.NONE, info.mState, "默认 mState=NONE");
		assertNull(info.mAudioName, "默认 mAudioName=null");
		assertFalse(info.mIsLocal, "默认 mIsLocal=false");
	}

	private static void testGetClipAfterReset()
	{
		var info = createInfo();
		var clip = createClip();
		info.mClip = new ResourceRef<UnityEngine.AudioClip>();
		var field = typeof(ResourceRef<UnityEngine.AudioClip>).GetField("mResource",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(info.mClip, clip);
		info.mRawClip = clip;

		info.resetProperty();
		var result = info.getClip();
		assertNull(result, "resetProperty 后 getClip 返回 null");
		UnityEngine.Object.DestroyImmediate(clip);
	}

	private static void testSetRawClipThenReset()
	{
		var info = createInfo();
		var clip = createClip();
		info.mRawClip = clip;
		info.mIsLocal = true;
		assertEqual(clip, info.getClip(), "设置 mRawClip 后 getClip 返回 clip");

		info.resetProperty();
		assertNull(info.mRawClip, "resetProperty 后 mRawClip=null");
		assertNull(info.getClip(), "resetProperty 后 getClip 返回 null");
		UnityEngine.Object.DestroyImmediate(clip);
	}
}
