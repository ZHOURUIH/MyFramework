using UnityEngine;

public interface IUGUIObject
{
	public T tryGetUnityComponent<T>() where T : Component;
	public string getName();
}

public interface IUGUIText : IUGUIObject
{
	public void setText(string text);
	public void setText(int text);
	public void setText(long text);
}

public interface IUGUIImage : IUGUIObject
{
	public void setSpriteName(string spriteName);
}