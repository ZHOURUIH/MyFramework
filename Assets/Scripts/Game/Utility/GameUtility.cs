
public class GameUtility
{
	public static byte[] getAESKeyBytes() { return null; }
	public static byte[] getAESIVBytes() { return null; }
	public static bool isHotFixEnable()
	{
#if ENABLE_HOTFIX
		return true;
#else
		return false;
#endif
	}
}