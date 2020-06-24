using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPooledWindow
{
	void setScript(LayoutScript script);
	void assignWindow(txUIObject parent, txUIObject template, string name);
	void init();
	void destroy();
	void reset();
	void setVisible(bool visible);
	void setParent(txUIObject obj);
}