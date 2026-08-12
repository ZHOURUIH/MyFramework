using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// LocalizationManager 本地化系统深度测试
//   setCurrentLanguage: 清空语言表 + 调 ReloadLanguageCallback 重新填充 + 刷新已注册文本
//   getLocalize: 查表(命中/回退原文或 "Localization:id") + 参数 {0} 替换(参数本身也翻译)
//   registeAction/registeLocalization: 语言切换回调 + 文本对象自动刷新链
// 环境: new LocalizationManager()(FrameSystem 子类直接 new)
// 测试辅助: FakeLocalizeText 实现 IUGUIText(接口很轻: tryGetUnityComponent/getName/setText x3)
public static class LocalizationManagerDeepTest
{
	public static void Run()
	{
		testReloadCallbackChain();
		testGetLocalizeString();
		testGetLocalizeInt();
		testGetLocalizeWithParam();
		testLanguageSwitchChain();
		testRegisteAction();
		testRegisteLocalizationImmediate();
		testRegisteLocalizationRefreshOnSwitch();
		testRegisteLocalizationWithCallback();
		testEmptyTableFallback();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建带语言表注入的管理器
	// ═════════════════════════════════════════════════════════════════
	private static LocalizationManager createManager()
	{
		LocalizationManager manager = new LocalizationManager();
		manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
		{
			// 模拟从表格加载当前语言的数据
			zhKeyList.Add("hello", "你好");
			zhKeyList.Add("world", "世界");
			zhKeyList.Add("greet", "你好,{0}");
			idKeyList.Add(1001, "ID文本");
			idKeyList.Add(2002, "带参ID:{0}");
		});
		return manager;
	}

	// setCurrentLanguage → 重载回调链: 表被填充, getLocalize 可命中
	private static void testReloadCallbackChain()
	{
		LocalizationManager manager = createManager();
		try
		{
			string reloadedLanguage = null;
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				reloadedLanguage = language;
				zhKeyList.Add("hello", "你好");
			});
			manager.setCurrentLanguage("Chinese");
			assertEqual("Chinese", reloadedLanguage, "重载回调收到语言名");
			assertEqual("你好", manager.getLocalize("hello"), "表填充后命中翻译");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(str): 命中 / 未命中回退原文
	private static void testGetLocalizeString()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "中文命中");
			assertEqual("世界", manager.getLocalize("world"), "中文命中 2");
			assertEqual("not_exist", manager.getLocalize("not_exist"), "未命中回退原文");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(id): 命中 / 未命中回退 "Localization:id"
	private static void testGetLocalizeInt()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			assertEqual("ID文本", manager.getLocalize(1001), "ID 命中");
			assertEqual("Localization:9999", manager.getLocalize(9999), "ID 未命中回退 Localization:id");
		}
		finally
		{
			manager.destroy();
		}
	}

	// getLocalize(str, param): 参数先翻译再 {0} 替换
	private static void testGetLocalizeWithParam()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			// greet = "你好,{0}", param "world" → 翻译为 "世界" → "你好,世界"
			assertEqual("你好,世界", manager.getLocalize("greet", "world"), "参数 {0} 替换 + 参数翻译");
			assertEqual("你好,世界", manager.getLocalize("greet", new List<string> { "world" }), "List 参数重载");
			// 2002 = "带参ID:{0}", param "world" → "带参ID:世界"
			assertEqual("带参ID:世界", manager.getLocalize(2002, "world"), "ID 带参 {0} 替换");
		}
		finally
		{
			manager.destroy();
		}
	}

	// 语言切换链: zh → en 重新加载, 旧值失效新值生效
	private static void testLanguageSwitchChain()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			string current = null;
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				current = language;
				if (language == "Chinese")
				{
					zhKeyList.Add("hello", "你好");
				}
				else if (language == "English")
				{
					zhKeyList.Add("hello", "Hello");
				}
			});
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "中文阶段");
			manager.setCurrentLanguage("English");
			assertEqual("English", current, "切换到 English");
			assertEqual("Hello", manager.getLocalize("hello"), "英文阶段新值");
			manager.setCurrentLanguage("Chinese");
			assertEqual("你好", manager.getLocalize("hello"), "切回中文恢复");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeAction / unregisteAction: 语言切换回调
	private static void testRegisteAction()
	{
		LocalizationManager manager = createManager();
		try
		{
			int callbackCount = 0;
			Action action = () => callbackCount++;
			manager.registeAction(action);
			manager.setCurrentLanguage("Chinese");
			assertEqual(1, callbackCount, "语言切换触发回调");
			manager.unregisteAction(action);
			manager.setCurrentLanguage("English");
			assertEqual(1, callbackCount, "unregiste 后不再触发");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeLocalization(fake, "hello"): 注册时立即翻译并 setText
	private static void testRegisteLocalizationImmediate()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			manager.registeLocalization(fake, "hello");
			assertEqual("你好", fake.getText(), "注册时立即翻译 setText");
		}
		finally
		{
			manager.destroy();
		}
	}

	// setCurrentLanguage 切换后已注册文本自动刷新
	private static void testRegisteLocalizationRefreshOnSwitch()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			manager.setReloadLanguageCallback((language, zhKeyList, idKeyList) =>
			{
				if (language == "Chinese")
				{
					zhKeyList.Add("hello", "你好");
				}
				else
				{
					zhKeyList.Add("hello", "Hello");
				}
			});
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			manager.registeLocalization(fake, "hello");
			assertEqual("你好", fake.getText(), "中文注册");
			manager.setCurrentLanguage("English");
			assertEqual("Hello", fake.getText(), "切换英文自动刷新");
		}
		finally
		{
			manager.destroy();
		}
	}

	// registeLocalization 带 LocalizationCallback: 走回调分支而非 setText
	private static void testRegisteLocalizationWithCallback()
	{
		LocalizationManager manager = createManager();
		try
		{
			manager.setCurrentLanguage("Chinese");
			FakeLocalizeText fake = new FakeLocalizeText();
			string callbackText = null;
			// LocalizationCallback = (IUGUIText, string, List<string>) 三参
			manager.registeLocalization(fake, "hello", (textObj, text, param) => callbackText = text);
			assertEqual("你好", callbackText, "回调分支收到翻译文本");
		}
		finally
		{
			manager.destroy();
		}
	}

	// 未注入重载回调时: setCurrentLanguage 清表, getLocalize 全部回退
	private static void testEmptyTableFallback()
	{
		LocalizationManager manager = new LocalizationManager();
		try
		{
			manager.setCurrentLanguage("Chinese");   // 无回调, 表为空
			assertEqual("abc", manager.getLocalize("abc"), "空表回退原文");
			assertEqual("Localization:5", manager.getLocalize(5), "空表回退 ID 占位");
		}
		finally
		{
			manager.destroy();
		}
	}
}

// 测试辅助: 实现 IUGUIText 的假文本对象(记录 setText 调用)
public class FakeLocalizeText : IUGUIText
{
	private string mText;
	private string mName;

	public FakeLocalizeText(string name = "FakeText")
	{
		mName = name;
	}

	public string getText() { return mText; }

	public T tryGetUnityComponent<T>() where T : Component { return null; }

	public string getName() { return mName; }

	public void setText(string text) { mText = text; }

	public void setText(int text) { mText = text.ToString(); }

	public void setText(long text) { mText = text.LToS(); }
}
