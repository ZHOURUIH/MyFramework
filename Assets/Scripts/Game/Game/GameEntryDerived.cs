
public class GameEntryDerived : GameEntry
{
	public override void Awake()
	{
		base.Awake();
		Game framework = new();
		framework.init();
		setFrameworkAOT(framework);
	}
}