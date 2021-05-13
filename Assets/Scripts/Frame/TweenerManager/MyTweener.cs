using System;

public class MyTweener : ComponentOwner
{
	public void init()
	{
		initComponents();
	}
	public virtual bool isDoing() { return false; }
}