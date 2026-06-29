
public class GameEntry : GameEntryBase
{
	public override void Awake()
	{
		base.Awake();
		Game framework = new();
		framework.init();
		setFrameworkAOT(framework);
	}
}