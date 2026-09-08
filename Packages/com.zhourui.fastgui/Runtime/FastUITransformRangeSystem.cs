using EasyECS;
using System.Collections.Generic;
using UnityEngine;

// Transform -> RenderRange mapping used by visibility, clipping and SOA.
// RenderElements are already stored in hierarchy DFS order, so the range table is built
// in one forward pass with an open-ancestor stack. InstanceID is used as the hot-path key
// to avoid UnityEngine.Object equality/hash overhead.
public sealed class FastUITransformRangeSystem
{
	private readonly FastUITransformNode_ECSList mNodes;
	private readonly Int_ECSDictionary<int> mNodeMap;
	private readonly Int_ECSList mElementNodeIndices;
	private readonly Int_ECSList mOpenNodeIndices;
	private readonly Int_ECSList mOpenTransformIDs;
	private readonly List<Transform> mResolvePath = new(16);
	private bool mDirty = true;

	public FastUITransformRangeSystem(int capacity)
	{
		int safeCapacity = Mathf.Max(capacity, 4);
		mNodes = new FastUITransformNode_ECSList(safeCapacity);
		mNodeMap = new Int_ECSDictionary<int>(safeCapacity);
		mElementNodeIndices = new Int_ECSList(safeCapacity);
		mOpenNodeIndices = new Int_ECSList(16);
		mOpenTransformIDs = new Int_ECSList(16);
	}

	public int getCount()
	{
		return mNodes.Count;
	}
	public bool isDirty()
	{
		return mDirty;
	}
	public void markDirty()
	{
		mDirty = true;
	}
	public FastUITransformNode_ECSList getNodes()
	{
		return mNodes;
	}
	public Int_ECSList getElementNodeIndices()
	{
		return mElementNodeIndices;
	}
	public int getNodeIndexForRenderIndex(int renderIndex)
	{
		return (uint)renderIndex < (uint)mElementNodeIndices.Count ? mElementNodeIndices.getValueColumn()[renderIndex] : -1;
	}
	public bool tryGetRange(Transform root, out int renderStart, out int renderCount)
	{
		if (tryGetNodeIndex(root, out int nodeIndex))
		{
			renderStart = mNodes.getRenderStartColumn()[nodeIndex];
			renderCount = mNodes.getRenderCountColumn()[nodeIndex];
			return true;
		}
		renderStart = 0;
		renderCount = 0;
		return false;
	}
	public bool tryGetNodeIndex(Transform root, out int nodeIndex)
	{
		nodeIndex = -1;
		if (root == null || !mNodeMap.TryGetValue(FastUnityObjectIDUtility.getLegacyIntID(root), out int value))
		{
			return false;
		}
		nodeIndex = value;
		return true;
	}
	public FastUITransformNodeRef getNode(int index)
	{
		return mNodes[index];
	}

	public void rebuild(List<FastUIRenderElement> renderElements, FastCanvas owner, Transform canvasRoot)
	{
		mNodes.Clear();
		mNodeMap.Clear();
		mElementNodeIndices.Clear();
		mOpenNodeIndices.Clear();
		mOpenTransformIDs.Clear();
		mResolvePath.Clear();

		int renderElementCount = renderElements != null ? renderElements.Count : 0;
		int nodeReserveTarget = estimateInitialNodeCapacity(renderElementCount, canvasRoot);
		mNodes.EnsureCapacity(nodeReserveTarget);
		mNodeMap.EnsureCapacity(nodeReserveTarget);
		mElementNodeIndices.EnsureCapacity(Mathf.Max(renderElementCount, 4));

		if (canvasRoot != null)
		{
			int rootNodeIndex = addNode(canvasRoot, -1, 0);
			mOpenTransformIDs.Add(FastUnityObjectIDUtility.getLegacyIntID(canvasRoot));
			mOpenNodeIndices.Add(rootNodeIndex);
		}

		for (int renderIndex = 0; renderIndex < renderElementCount; ++renderIndex)
		{
			FastUIRenderElement element = renderElements[renderIndex];
			if (element == null || element.getCanvas() != owner)
			{
				mElementNodeIndices.Add(-1);
				continue;
			}

			Transform target = element.getRectTransform();
			int commonDepth = resolveCommonOpenAncestor(target, canvasRoot);
			closeOpenNodes(commonDepth + 1, renderIndex);

			for (int pathIndex = mResolvePath.Count - 1; pathIndex >= 0; --pathIndex)
			{
				Transform nodeTransform = mResolvePath[pathIndex];
				int parentIndex = mOpenNodeIndices.Count > 0 ? mOpenNodeIndices.getValueColumn()[mOpenNodeIndices.Count - 1] : -1;
				int nodeIndex = addNode(nodeTransform, parentIndex, renderIndex);
				mOpenTransformIDs.Add(FastUnityObjectIDUtility.getLegacyIntID(nodeTransform));
				mOpenNodeIndices.Add(nodeIndex);
			}

			int targetNodeIndex = mOpenNodeIndices.Count > 0 ? mOpenNodeIndices.getValueColumn()[mOpenNodeIndices.Count - 1] : -1;
			mElementNodeIndices.Add(targetNodeIndex);
		}

		closeOpenNodes(0, renderElementCount);

		var renderStarts = mNodes.getRenderStartColumn();
		var renderCounts = mNodes.getRenderCountColumn();
		var elementNodeIndices = mElementNodeIndices.getValueColumn();
		for (int i = 0; i < renderElementCount; ++i)
		{
			int nodeIndex = elementNodeIndices[i];
			if (nodeIndex < 0)
			{
				continue;
			}
			FastUIRenderElement element = renderElements[i];
			if (element != null)
			{
				element.setCanvasRenderRange(renderStarts[nodeIndex], renderCounts[nodeIndex]);
			}
		}
		mDirty = false;
	}

	public void dispose()
	{
		mNodes.Dispose();
		mNodeMap.Dispose();
		mElementNodeIndices.Dispose();
		mOpenNodeIndices.Dispose();
		mOpenTransformIDs.Dispose();
		mResolvePath.Clear();
	}

	private static int estimateInitialNodeCapacity(int renderElementCount, Transform canvasRoot)
	{
		int renderCapacity = Mathf.Max(renderElementCount, 4);
		if (canvasRoot != null && canvasRoot.root == canvasRoot)
		{
			int hierarchyCount = canvasRoot.hierarchyCount;
			if (hierarchyCount > 0)
			{
				return Mathf.Max(renderCapacity, hierarchyCount);
			}
		}
		long estimated = renderElementCount + (renderElementCount >> 1) + 16L;
		if (estimated > int.MaxValue)
		{
			estimated = int.MaxValue;
		}
		return Mathf.Max(renderCapacity, (int)estimated);
	}

	private int resolveCommonOpenAncestor(Transform target, Transform canvasRoot)
	{
		mResolvePath.Clear();
		Transform current = target;
		while (current != null)
		{
			int openDepth = findOpenDepth(FastUnityObjectIDUtility.getLegacyIntID(current));
			if (openDepth >= 0)
			{
				return openDepth;
			}
			mResolvePath.Add(current);
			if (current == canvasRoot)
			{
				break;
			}
			current = current.parent;
		}
		return -1;
	}

	private int findOpenDepth(int targetInstanceID)
	{
		var openTransformIDs = mOpenTransformIDs.getValueColumn();
		for (int i = mOpenTransformIDs.Count - 1; i >= 0; --i)
		{
			if (openTransformIDs[i] == targetInstanceID)
			{
				return i;
			}
		}
		return -1;
	}

	private int addNode(Transform target, int parentIndex, int renderIndex)
	{
		int nodeIndex = mNodes.Count;
		mNodeMap.Add(FastUnityObjectIDUtility.getLegacyIntID(target), nodeIndex);
		mNodes.Add(new FastUITransformNode(target, parentIndex, renderIndex, 0));
		return nodeIndex;
	}

	private void closeOpenNodes(int firstDepth, int renderEndExclusive)
	{
		int openCount = mOpenNodeIndices.Count;
		firstDepth = Mathf.Max(firstDepth, 0);
		if (firstDepth >= openCount)
		{
			return;
		}
		var openNodeIndices = mOpenNodeIndices.getValueColumn();
		var renderStarts = mNodes.getRenderStartColumn();
		var renderCounts = mNodes.getRenderCountColumn();
		for (int depth = openCount - 1; depth >= firstDepth; --depth)
		{
			int nodeIndex = openNodeIndices[depth];
			renderCounts[nodeIndex] = Mathf.Max(renderCounts[nodeIndex], renderEndExclusive - renderStarts[nodeIndex]);
		}
		int removeCount = openCount - firstDepth;
		mOpenNodeIndices.RemoveRange(firstDepth, removeCount);
		mOpenTransformIDs.RemoveRange(firstDepth, removeCount);
	}
}
