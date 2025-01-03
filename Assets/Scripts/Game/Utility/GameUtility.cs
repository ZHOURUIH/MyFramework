
public class GameUtility
{
	public static bool isHotFixEnable()
	{
#if ENABLE_HOTFIX
		return true;
#else
		return false;
#endif
	}
}