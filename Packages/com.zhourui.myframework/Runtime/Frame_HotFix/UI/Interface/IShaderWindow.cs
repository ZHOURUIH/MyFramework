
// 着色器窗口接口,支持自定义着色器效果的窗口需实现此接口
public interface IShaderWindow
{
	void setWindowShader(WindowShader shader);
	WindowShader getWindowShader();
}