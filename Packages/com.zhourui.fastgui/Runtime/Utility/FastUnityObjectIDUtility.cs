using UnityEngine;

// Unity 6000.5 deprecates Object.GetInstanceID() as an error and replaces it with EntityId.
// Keep all direct API branching in one place so FastGUI compiles on both older Unity 6 builds
// and 6000.5+, without falling back to EntityId.GetHashCode() for identity-sensitive paths.
public static class FastUnityObjectIDUtility
{
	// Full object identity for ordering/debugging where the caller is free to use 64 bits.
	public static ulong getID64(Object obj)
	{
		if (obj == null)
		{
			return 0UL;
		}
#if UNITY_6000_5_OR_NEWER
		return EntityId.ToULong(obj.GetEntityId());
#else
		return unchecked((uint)obj.GetInstanceID());
#endif
	}

	// Existing FastGUI caches/batch keys historically store the legacy int InstanceID.
	// On Unity 6000.5+ reproduce GetInstanceID's former low-32-bit mapping exactly rather
	// than using GetHashCode(), so upgrading Unity does not silently change batch/cache keys.
	public static int getLegacyIntID(Object obj)
	{
		if (obj == null)
		{
			return 0;
		}
#if UNITY_6000_5_OR_NEWER
		return unchecked((int)(EntityId.ToULong(obj.GetEntityId()) & 0xFFFFFFFFUL));
#else
		return obj.GetInstanceID();
#endif
	}

	// Stable tie-break comparison. Preserve the historical GetInstanceID ordering exactly
	// so a Unity upgrade cannot reorder equal-sibling/equal-sort-value renderers.
	public static int compare(Object first, Object second)
	{
		if (ReferenceEquals(first, second))
		{
			return 0;
		}
		if (first == null)
		{
			return second == null ? 0 : -1;
		}
		if (second == null)
		{
			return 1;
		}
		return getLegacyIntID(first).CompareTo(getLegacyIntID(second));
	}

	public static string getDebugID(Object obj)
	{
		if (obj == null)
		{
			return "0";
		}
#if UNITY_6000_5_OR_NEWER
		return EntityId.ToULong(obj.GetEntityId()).ToString();
#else
		return obj.GetInstanceID().ToString();
#endif
	}
}
