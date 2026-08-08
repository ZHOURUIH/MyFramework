using System.Collections.Generic;
using static TestAssert;

// AudioManager 纯逻辑测试
// 覆盖不依赖 Unity AudioClip 运行时 / 资源加载的入口方法:
//   音量/数量 getter-setter、注册查询、isLoadDone/getLoadedPercent、unload 守卫
// 不测: playClip/stopClip/loadAudio 等依赖 AudioSource/AudioClip 加载的运行时方法
public static class AudioManagerTest
{
	public static void Run()
	{
		testVolumeGettersSetters();
		testMaxAudioCount();
		testGetAudioListEmpty();
		testGetAudioUnregistered();
		testRegisteAudioThenGet();
		testRegisteAudioDuplicateIgnored();
		testRegisteSoundDefineThenLookup();
		testGetAudioNameUnregistered();
		testIsLoadDoneEmpty();
		testLoadedPercentEmpty();
		testUnloadEmptyName();
	}

	// ═══════════════════════════════════════════════════════════════════
	// 音量 / 数量 getter-setter
	// ═══════════════════════════════════════════════════════════════════

	private static void testVolumeGettersSetters()
	{
		var mgr = new AudioManager();
		assertEqual(1.0f, mgr.getSoundVolume(), 0.0001f, "初始音效音量=1.0");
		assertEqual(1.0f, mgr.getMusicVolume(), 0.0001f, "初始音乐音量=1.0");

		mgr.setSoundVolume(0.5f);
		mgr.setMusicVolume(0.25f);
		assertEqual(0.5f,  mgr.getSoundVolume(), 0.0001f, "setSoundVolume(0.5) 生效");
		assertEqual(0.25f, mgr.getMusicVolume(), 0.0001f, "setMusicVolume(0.25) 生效");

		mgr.setSoundVolume(0.0f);
		mgr.setMusicVolume(1.0f);
		assertEqual(0.0f, mgr.getSoundVolume(), 0.0001f, "setSoundVolume(0) 生效");
		assertEqual(1.0f, mgr.getMusicVolume(), 0.0001f, "setMusicVolume(1) 生效");
	}

	private static void testMaxAudioCount()
	{
		var mgr = new AudioManager();
		assertEqual(0, mgr.getMaxAudioCount(), "初始 maxAudioCount=0(不限制)");
		mgr.setMaxAudioCount(10);
		assertEqual(10, mgr.getMaxAudioCount(), "setMaxAudioCount(10) 生效");
		mgr.setMaxAudioCount(0);
		assertEqual(0, mgr.getMaxAudioCount(), "setMaxAudioCount(0) 恢复不限制");
	}

	// ═══════════════════════════════════════════════════════════════════
	// 音频列表查询
	// ═══════════════════════════════════════════════════════════════════

	private static void testGetAudioListEmpty()
	{
		var mgr = new AudioManager();
		assertEqual(0, mgr.getAudioList().Count, "新管理器音频列表为空");
	}

	private static void testGetAudioUnregistered()
	{
		var mgr = new AudioManager();
		assertNull(mgr.getAudio("not_registered.wav"), "未注册音频返回 null");
	}

	private static void testRegisteAudioThenGet()
	{
		var mgr = new AudioManager();
		mgr.registeAudio("effect_hit.wav", true);
		assertEqual(1, mgr.getAudioList().Count, "注册后列表有1项");
		var info = mgr.getAudio("effect_hit.wav");
		assertNotNull(info, "registeAudio 后可查询到 AudioInfo");
		assertEqual("effect_hit.wav", info.mAudioName, "AudioInfo.mAudioName 正确");
		assertTrue(info.mIsLocal, "isLocal=true 正确设置");
	}

	private static void testRegisteAudioDuplicateIgnored()
	{
		var mgr = new AudioManager();
		mgr.registeAudio("dup.wav", true);
		mgr.registeAudio("dup.wav", false);
		assertEqual(1, mgr.getAudioList().Count, "重复注册同一名称不新增条目");
		var info = mgr.getAudio("dup.wav");
		assertTrue(info.mIsLocal, "重复注册被忽略, 保持首次 isLocal=true");
	}

	private static void testRegisteSoundDefineThenLookup()
	{
		var mgr = new AudioManager();
		mgr.registeSoundDefine(1001, "bgm_main.wav", true);
		assertEqual("bgm_main.wav", mgr.getAudioName(1001), "soundDefine=1001 → 音频名");
		assertNotNull(mgr.getAudio("bgm_main.wav"), "registeSoundDefine 内部同步注册了音频");
		assertEqual(1, mgr.getAudioList().Count, "registeSoundDefine 后列表有1项");
	}

	private static void testGetAudioNameUnregistered()
	{
		var mgr = new AudioManager();
		assertNull(mgr.getAudioName(999), "未注册 soundDefine 返回 null");
	}

	// ═══════════════════════════════════════════════════════════════════
	// 加载进度
	// ═══════════════════════════════════════════════════════════════════

	private static void testIsLoadDoneEmpty()
	{
		var mgr = new AudioManager();
		assertTrue(mgr.isLoadDone(), "空列表(loadedCount=0==list.Count=0) isLoadDone=true");
	}

	private static void testLoadedPercentEmpty()
	{
		var mgr = new AudioManager();
		assertEqual(0.0f, mgr.getLoadedPercent(), 0.0001f, "0/0 divide 返回 defaultValue=0");
	}

	// ═══════════════════════════════════════════════════════════════════
	// unload 守卫
	// ═══════════════════════════════════════════════════════════════════

	private static void testUnloadEmptyName()
	{
		var mgr = new AudioManager();
		assertFalse(mgr.unload(""), "unload(空名) 返回 false");
	}
}
