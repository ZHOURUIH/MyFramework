using UnityEngine;

public interface ITransformable
{
	public Vector3 getPosition();
	public Vector3 getWorldPosition();
	public Vector3 getRotation();
	public Vector3 getWorldRotation();
	public Vector3 getScale();
	public void setRotation(Vector3 rot);
	public void setWorldRotation(Quaternion rot);
	public void setWorldRotation(Vector3 rot);
	public void setPosition(Vector3 position);
	public void setWorldPosition(Vector3 position);
	public void setScale(Vector3 position);
	public void setNeedUpdate(bool needUpdate);
	public long getAssignID();
	public bool isDestroy();
	public Vector3 localToWorldDirection(Vector3 direction);
	public T getOrAddComponent<T>(out T com) where T : GameComponent;
	public T getComponent<T>(out T com) where T : GameComponent;
	public T getComponent<T>() where T : GameComponent;
}