
public interface IFramework
{
	public void update(float elapsedTime);
	public void fixedUpdate(float elapsedTime);
	public void lateUpdate(float elapsedTime);
	public void drawGizmos();
	public void onApplicationFocus(bool focus);
	public void onApplicationQuit();
	public void destroy();
}