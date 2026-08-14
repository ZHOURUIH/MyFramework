using System.Reflection;
using static TestAssert;

// myUGUIInputFieldTMP: TMP_InputField 封装
// 直接 new 不 init 测纯字段存储(init 需 AddComponent<TMP_InputField> 并 AddListener, 环境风险高;
// setText/cleanUp 等依赖 mInputField 组件, 不测)
public static class MyUGUIInputFieldTMPTest
{
	public static void Run()
	{
		testSetCallbacksStored();
		testSetCallbacksNull();
		testIsVisibleDefault();
	}

	// 反射读取私有回调字段
	private static FieldInfo getCallbackField(string name)
	{
		return typeof(myUGUIInputFieldTMP).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
	}

	// setOnEndEdit/setOnSubmitEdit/setOnEditing: 回调字段存储(StringCallback = void(string))
	private static void testSetCallbacksStored()
	{
		myUGUIInputFieldTMP field = new myUGUIInputFieldTMP();
		StringCallback endEdit = (string value) => { };
		StringCallback submitEdit = (string value) => { };
		StringCallback editing = (string value) => { };
		field.setOnEndEdit(endEdit);
		field.setOnSubmitEdit(submitEdit);
		field.setOnEditing(editing);
		assertTrue(ReferenceEquals(endEdit, getCallbackField("mOnEndEdit").GetValue(field)), "mOnEndEdit 已存储");
		assertTrue(ReferenceEquals(submitEdit, getCallbackField("mOnSubmitEdit").GetValue(field)), "mOnSubmitEdit 已存储");
		assertTrue(ReferenceEquals(editing, getCallbackField("mOnEditing").GetValue(field)), "mOnEditing 已存储");
	}

	// setOnXxx(null): 清空回调字段
	private static void testSetCallbacksNull()
	{
		myUGUIInputFieldTMP field = new myUGUIInputFieldTMP();
		field.setOnEndEdit(null);
		field.setOnSubmitEdit(null);
		field.setOnEditing(null);
		assertTrue(getCallbackField("mOnEndEdit").GetValue(field) == null, "mOnEndEdit 置 null");
		assertTrue(getCallbackField("mOnSubmitEdit").GetValue(field) == null, "mOnSubmitEdit 置 null");
		assertTrue(getCallbackField("mOnEditing").GetValue(field) == null, "mOnEditing 置 null");
	}

	// isVisible: = isActive() = Transformable.mActive 纯字段,
	// Transformable 构造里 mActive = false(窗口默认不激活, setObject 时才同步 activeSelf, Transformable.cs:30,59)
	private static void testIsVisibleDefault()
	{
		myUGUIInputFieldTMP field = new myUGUIInputFieldTMP();
		assertFalse(field.isVisible(), "直接 new 后 isVisible 默认 false(未 setObject, Transformable.mActive 构造为 false)");
	}
}
