using UnityEngine;

// UGUI对象接口,所有UGUI控件需实现的接口
public interface IUGUIObject
{
	public T tryGetUnityComponent<T>() where T : Component;
	public string getName();
}

// UGUI文本接口,所有文本控件需实现的接口
public interface IUGUIText : IUGUIObject
{
	public void setText(string text);
	public void setText(int text);
	public void setText(long text);
}

// UGUI图片接口,所有图片控件需实现的接口
public interface IUGUIImage : IUGUIObject
{
	public void setSpriteName(string spriteName);
}