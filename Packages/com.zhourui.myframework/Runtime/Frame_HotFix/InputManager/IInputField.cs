
// 输入框接口,用于判断当前是否正在输入文本,影响InputSystem的焦点掩码
// InputSystem通过此接口判断当前焦点是否在输入框中,从而决定是否屏蔽快捷键响应
public interface IInputField
{
	bool isFocused();
	bool isVisible();
}