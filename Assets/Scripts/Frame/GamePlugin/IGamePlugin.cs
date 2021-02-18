using System;

public interface IGamePlugin
{
	string getPluginName();
	void init();
	void update(float elapsedTime);
	void destroy();
}