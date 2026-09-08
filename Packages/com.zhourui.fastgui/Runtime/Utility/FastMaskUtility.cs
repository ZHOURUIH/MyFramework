using UnityEngine;

public static class FastMaskUtility
{
	public const int MAX_MASK_DEPTH = 8;
	public static FastUIMaskState calculateState(FastUIRenderElement element)
	{
		if (element == null)
		{
			return default;
		}
		FastCanvas canvas = element.getCanvas();
		FastMask attachedMask = element.getAttachedMask();
		if (attachedMask != null && attachedMask.isMaskActive() && attachedMask.getGraphic() == element)
		{
			int parentDepth = attachedMask.getDepth();
			if (parentDepth >= MAX_MASK_DEPTH)
			{
				attachedMask.reportDepthOverflow(parentDepth + 1);
				return FastUIMaskState.reader(MAX_MASK_DEPTH);
			}
			return FastUIMaskState.writer(parentDepth, attachedMask.getShowMaskGraphic());
		}
		int depth = countActiveMasks(element.transform.parent, canvas);
		if (element is not FastClipStencilGraphic &&
			element.transform.TryGetComponent(out FastRectMask2D selfClip) &&
			selfClip.isClipActiveForCanvas(canvas))
		{
			++depth;
		}
		if (depth <= 0)
		{
			return default;
		}
		return FastUIMaskState.reader(Mathf.Min(depth, MAX_MASK_DEPTH));
	}
	public static int countActiveMasks(Transform parent, FastCanvas canvas)
	{
		int depth = 0;
		Transform stop = canvas != null ? canvas.transform : null;
		Transform current = parent;
		while (current != null && current != stop)
		{
			if (current.TryGetComponent(out FastMask mask) && mask.isMaskActive())
			{
				++depth;
			}
			if (current.TryGetComponent(out FastRectMask2D clip) && clip.isClipActiveForCanvas(canvas))
			{
				++depth;
			}
			current = current.parent;
		}
		return depth;
	}
	public static void refreshSubtree(Transform root)
	{
		if (root == null)
		{
			return;
		}
		refreshRecursive(root);
	}
	private static void refreshRecursive(Transform node)
	{
		if (node.TryGetComponent(out FastUIRenderElement image))
		{
			image.refreshMaskStateFromHierarchy();
		}
		for (int i = 0; i < node.childCount; ++i)
		{
			Transform child = node.GetChild(i);
			if (child != null)
			{
				refreshRecursive(child);
			}
		}
	}
}
