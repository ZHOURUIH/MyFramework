
// 游戏插件接口,实现此接口的插件可被框架统一管理生命周期
public interface IGamePlugin
{
	string getPluginName();
	void init();
	void update(float elapsedTime);
	void destroy();
}