using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IGamePlugin
{
	string getPluginName();
	void init();
	void update(float elapsedTime);
	void destroy();
}