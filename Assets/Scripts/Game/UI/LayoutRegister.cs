using static LayoutManager;
using static GameBase;

public class LayoutRegister
{
	public static void registeAllLayout()
	{
		registeLayoutResPart<UIDemoStart>((script) => { mUIDemoStart = script; });
		registeLayoutResPart<UIDemo>((script) => { mUIDemo = script; });
	}
}