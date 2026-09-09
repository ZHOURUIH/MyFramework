using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 只在Editor事件触发时扫描，不进入Player，也不在Editor每帧扫描。
// FixedStructure与单层SparseStructure都可以给出建议；不会自动添加组件或修改StructureMode。
// Adjacent SOA Merge只分析相邻顶层Fixed Group的Batch收益，不做空间重叠检测；是否允许跨Group重排由开发者显式确认。
[InitializeOnLoad]
public static class FastSOAEditorAdvisor
{
	private struct BatchKey
	{
		public int mCompatibilityID;
		public Material mMaterial;
		public Texture mTexture;
		public BatchKey(FastUIRenderElement element)
		{
			mCompatibilityID = element != null ? element.getSOACompatibilityID() : 0;
			mMaterial = element != null ? element.getMaterial() : null;
			mTexture = element != null ? element.getRenderTexture() : null;
		}
		public bool equals(BatchKey other)
		{
			return mCompatibilityID == other.mCompatibilityID && mMaterial == other.mMaterial && mTexture == other.mTexture;
		}
	}
	private sealed class SparseRoleCandidate
	{
		public int mCompatibilityID;
		public readonly List<FastUIRenderElement> mMembers = new();
		public readonly List<SparseRoleCandidate> mOutgoing = new();
		public int mIndegree;
		public bool mEmitted;
		public SparseRoleCandidate(int compatibilityID)
		{
			mCompatibilityID = compatibilityID;
		}
	}
	private const int MIN_RECORD_COUNT = 3;
	private const int MIN_ELEMENT_COUNT_PER_RECORD = 2;
	private const int MIN_BATCH_SAVING = 2;
	private static readonly HashSet<int> mWarnedCandidates = new();
	private static readonly HashSet<int> mCurrentCandidates = new();
	private static readonly HashSet<long> mWarnedAdjacentPairs = new();
	private static readonly HashSet<long> mCurrentAdjacentPairs = new();
	private static bool mScanScheduled;
	static FastSOAEditorAdvisor()
	{
		EditorApplication.hierarchyChanged += scheduleScan;
		EditorApplication.projectChanged += scheduleScan;
		EditorApplication.playModeStateChanged += onPlayModeStateChanged;
		Undo.undoRedoPerformed += scheduleScan;
		scheduleScan();
	}
	[MenuItem("FastGUI/Scan SOA Suggestions")]
	public static void scanFromMenu()
	{
		mWarnedCandidates.Clear();
		mWarnedAdjacentPairs.Clear();
		int candidateCount = scanNow();
		Debug.Log("[FastGUI SOA建议] 扫描完成 | Mode=" + (Application.isPlaying ? "Play" : "Edit") + " | Candidates=" + candidateCount);
	}
	private static void onPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
		{
			scheduleScan();
		}
	}
	private static void scheduleScan()
	{
		if (mScanScheduled)
		{
			return;
		}
		mScanScheduled = true;
		EditorApplication.delayCall += runScheduledScan;
	}
	private static void runScheduledScan()
	{
		mScanScheduled = false;
		scanNow();
	}
	private static int scanNow()
	{
		if (EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
		{
			return 0;
		}
		mCurrentCandidates.Clear();
		mCurrentAdjacentPairs.Clear();
		FastCanvas[] canvases = Resources.FindObjectsOfTypeAll<FastCanvas>();
		for (int i = 0; i < canvases.Length; ++i)
		{
			FastCanvas canvas = canvases[i];
			if (!isSceneObject(canvas))
			{
				continue;
			}
			scanTransform(canvas.transform, canvas, true);
			scanAdjacentMergeSuggestions(canvas);
		}
		mWarnedCandidates.RemoveWhere(instanceID => !mCurrentCandidates.Contains(instanceID));
		mWarnedAdjacentPairs.RemoveWhere(pairKey => !mCurrentAdjacentPairs.Contains(pairKey));
		return mCurrentCandidates.Count + mCurrentAdjacentPairs.Count;
	}
	private static bool isSceneObject(FastCanvas canvas)
	{
		return canvas != null && canvas.gameObject.scene.IsValid() && canvas.gameObject.scene.isLoaded &&
			(canvas.hideFlags & HideFlags.HideAndDontSave) == 0;
	}
	private static void scanTransform(Transform root, FastCanvas canvas, bool isCanvasRoot)
	{
		if (root == null)
		{
			return;
		}
		if (!isCanvasRoot && root.TryGetComponent(out FastCanvas nestedCanvas) && nestedCanvas != canvas)
		{
			return;
		}
		if (tryEvaluateCandidate(root, canvas, out int recordCount, out int minElementCount, out int maxElementCount,
			out int currentRuns, out int soaRuns, out FastSOAStructureMode suggestedMode))
		{
			bool nestedCandidate = hasSOAAncestor(root, canvas);
			bool containsNested = hasSOADescendant(root);
			bool modeSupportedHere = suggestedMode != FastSOAStructureMode.SparseStructure || (!nestedCandidate && !containsNested);
			root.TryGetComponent(out FastSOARenderGroup existingGroup);
			bool needsChange = existingGroup == null || existingGroup.getStructureMode() != suggestedMode;
			if (modeSupportedHere && needsChange)
			{
				int instanceID = FastUnityObjectIDUtility.getLegacyIntID(root);
				mCurrentCandidates.Add(instanceID);
				if (mWarnedCandidates.Add(instanceID))
				{
					string elementText = minElementCount == maxElementCount ? minElementCount.ToString() : minElementCount + "-" + maxElementCount;
					string action = existingGroup == null ?
						"建议在该节点添加FastSOARenderGroup并设置StructureMode=" + suggestedMode :
						"当前已有FastSOARenderGroup，但建议将StructureMode从" + existingGroup.getStructureMode() + "切换为" + suggestedMode;
					Debug.LogWarning("[FastGUI SOA建议] 检测到重复Record结构存在明显Batch合并空间." +
						" Path=" + getHierarchyPath(root) +
						" | Mode=" + suggestedMode +
						" | Records=" + recordCount +
						" | Elements/Record=" + elementText +
						" | EstimatedBatch=" + currentRuns + "->" + soaRuns +
						" | Saving=" + (currentRuns - soaRuns) +
						(nestedCandidate ? " | NestedCandidate=True" : "") +
						"。" + action + "；这里只提示，不会自动修改.", root);
				}
			}
		}
		for (int i = 0; i < root.childCount; ++i)
		{
			scanTransform(root.GetChild(i), canvas, false);
		}
	}
	private sealed class AdjacentGroupCandidate
	{
		public FastSOARenderGroup mGroup;
		public int mFirstOrder;
		public int mLastOrder;
		public int mElementCount;
		public readonly List<List<FastUIRenderElement>> mRecords = new();
	}
	private static void scanAdjacentMergeSuggestions(FastCanvas canvas)
	{
		if (canvas == null)
		{
			return;
		}
		List<FastUIRenderElement> canvasOrder = new();
		collectCanvasRenderElements(canvas.transform, canvas, canvasOrder);
		if (canvasOrder.Count == 0)
		{
			return;
		}
		FastDictionary<FastUIRenderElement, int> orderMap = new(canvasOrder.Count);
		for (int i = 0; i < canvasOrder.Count; ++i)
		{
			if (canvasOrder[i] != null && !orderMap.ContainsKey(canvasOrder[i]))
			{
				orderMap.Add(canvasOrder[i], i);
			}
		}
		FastSOARenderGroup[] groups = canvas.GetComponentsInChildren<FastSOARenderGroup>(true);
		List<AdjacentGroupCandidate> candidates = new();
		for (int i = 0; i < groups.Length; ++i)
		{
			FastSOARenderGroup group = groups[i];
			if (group == null || !group.enabled || !group.gameObject.activeInHierarchy || group.getStructureMode() != FastSOAStructureMode.FixedStructure ||
				hasSOAAncestor(group.transform, canvas) || hasSOADescendant(group.transform))
			{
				continue;
			}
			if (tryBuildAdjacentCandidate(group, canvas, orderMap, out AdjacentGroupCandidate candidate))
			{
				candidates.Add(candidate);
			}
		}
		candidates.Sort((a, b) => a.mFirstOrder.CompareTo(b.mFirstOrder));
		for (int i = 0; i + 1 < candidates.Count; ++i)
		{
			AdjacentGroupCandidate first = candidates[i];
			AdjacentGroupCandidate second = candidates[i + 1];
			if (first.mLastOrder + 1 != second.mFirstOrder || first.mElementCount != second.mElementCount ||
				!haveSameSlotCompatibility(first.mRecords, second.mRecords, first.mElementCount))
			{
				continue;
			}
			List<BatchKey> currentKeys = new();
			appendFixedSOAKeys(first.mRecords, first.mElementCount, currentKeys);
			appendFixedSOAKeys(second.mRecords, second.mElementCount, currentKeys);
			int currentRuns = countKeyRuns(currentKeys);
			List<List<FastUIRenderElement>> mergedRecords = new(first.mRecords.Count + second.mRecords.Count);
			mergedRecords.AddRange(first.mRecords);
			mergedRecords.AddRange(second.mRecords);
			List<BatchKey> mergedKeys = new();
			appendFixedSOAKeys(mergedRecords, first.mElementCount, mergedKeys);
			int mergedRuns = countKeyRuns(mergedKeys);
			if (currentRuns - mergedRuns < 1 || (first.mGroup.getAllowAdjacentGroupMerge() && second.mGroup.getAllowAdjacentGroupMerge()))
			{
				continue;
			}
			long pairKey = makePairKey(FastUnityObjectIDUtility.getLegacyIntID(first.mGroup), FastUnityObjectIDUtility.getLegacyIntID(second.mGroup));
			mCurrentAdjacentPairs.Add(pairKey);
			if (!mWarnedAdjacentPairs.Add(pairKey))
			{
				continue;
			}
			Debug.LogWarning("[FastGUI SOA建议] 检测到相邻FastSOARenderGroup存在进一步Batch合并空间." +
				" First=" + getHierarchyPath(first.mGroup.transform) +
				" | Second=" + getHierarchyPath(second.mGroup.transform) +
				" | EstimatedBatch=" + currentRuns + "->" + mergedRuns +
				" | Saving=" + (currentRuns - mergedRuns) +
				"。如果确认两个Group之间不存在必须保持的绘制层级依赖，请在双方开启‘允许相邻SOA合并’；这里只分析Batch收益，不做空间重叠检测，也不会自动修改.", first.mGroup);
		}
	}
	private static bool tryBuildAdjacentCandidate(FastSOARenderGroup group, FastCanvas canvas, FastDictionary<FastUIRenderElement, int> orderMap,
		out AdjacentGroupCandidate candidate)
	{
		candidate = null;
		RectTransform root = group.getRectTransform();
		if (root == null || root.childCount < 2)
		{
			return false;
		}
		AdjacentGroupCandidate result = new() { mGroup = group, mFirstOrder = int.MaxValue, mLastOrder = -1 };
		for (int recordIndex = 0; recordIndex < root.childCount; ++recordIndex)
		{
			List<FastUIRenderElement> elements = new();
			bool hasBarrier = false;
			collectRenderElements(root.GetChild(recordIndex), canvas, elements, ref hasBarrier);
			if (hasBarrier || elements.Count == 0 || (result.mElementCount > 0 && result.mElementCount != elements.Count))
			{
				return false;
			}
			if (result.mElementCount == 0)
			{
				result.mElementCount = elements.Count;
			}
			result.mRecords.Add(elements);
			for (int elementIndex = 0; elementIndex < elements.Count; ++elementIndex)
			{
				if (!orderMap.TryGetValue(elements[elementIndex], out int order))
				{
					return false;
				}
				result.mFirstOrder = Mathf.Min(result.mFirstOrder, order);
				result.mLastOrder = Mathf.Max(result.mLastOrder, order);
			}
		}
		if (!tryGetFixedElementCount(result.mRecords, out int fixedCount) || fixedCount != result.mElementCount ||
			result.mLastOrder - result.mFirstOrder + 1 != root.childCount * result.mElementCount)
		{
			return false;
		}
		candidate = result;
		return true;
	}
	private static void collectCanvasRenderElements(Transform root, FastCanvas canvas, List<FastUIRenderElement> result)
	{
		if (root == null)
		{
			return;
		}
		if (root != canvas.transform && root.TryGetComponent(out FastCanvas nestedCanvas) && nestedCanvas != canvas)
		{
			return;
		}
		FastUIRenderElement[] elements = root.GetComponents<FastUIRenderElement>();
		for (int i = 0; i < elements.Length; ++i)
		{
			result.Add(elements[i]);
		}
		for (int i = 0; i < root.childCount; ++i)
		{
			collectCanvasRenderElements(root.GetChild(i), canvas, result);
		}
	}
	private static bool haveSameSlotCompatibility(List<List<FastUIRenderElement>> first, List<List<FastUIRenderElement>> second, int elementCount)
	{
		if (first.Count == 0 || second.Count == 0)
		{
			return false;
		}
		for (int slot = 0; slot < elementCount; ++slot)
		{
			if (first[0][slot].getSOACompatibilityID() != second[0][slot].getSOACompatibilityID())
			{
				return false;
			}
		}
		return true;
	}
	private static void appendFixedSOAKeys(List<List<FastUIRenderElement>> records, int elementCount, List<BatchKey> output)
	{
		for (int slot = 0; slot < elementCount; ++slot)
		{
			for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
			{
				output.Add(new BatchKey(records[recordIndex][slot]));
			}
		}
	}
	private static int countKeyRuns(List<BatchKey> keys)
	{
		int runs = 0;
		bool hasPrevious = false;
		BatchKey previous = default;
		for (int i = 0; i < keys.Count; ++i)
		{
			BatchKey current = keys[i];
			if (!hasPrevious || !current.equals(previous))
			{
				++runs;
			}
			previous = current;
			hasPrevious = true;
		}
		return runs;
	}
	private static long makePairKey(int first, int second)
	{
		return ((long)(uint)first << 32) | (uint)second;
	}
	private static bool tryEvaluateCandidate(Transform root, FastCanvas canvas, out int recordCount, out int minElementCount,
		out int maxElementCount, out int currentRuns, out int soaRuns, out FastSOAStructureMode suggestedMode)
	{
		recordCount = root != null ? root.childCount : 0;
		minElementCount = 0;
		maxElementCount = 0;
		currentRuns = 0;
		soaRuns = 0;
		suggestedMode = FastSOAStructureMode.FixedStructure;
		if (root == null || recordCount < MIN_RECORD_COUNT)
		{
			return false;
		}
		List<List<FastUIRenderElement>> records = new(recordCount);
		List<Transform> recordRoots = new(recordCount);
		minElementCount = int.MaxValue;
		for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
		{
			Transform recordRoot = root.GetChild(recordIndex);
			List<FastUIRenderElement> elements = new();
			bool hasBarrier = false;
			collectRenderElements(recordRoot, canvas, elements, ref hasBarrier);
			if (hasBarrier || elements.Count < MIN_ELEMENT_COUNT_PER_RECORD)
			{
				return false;
			}
			recordRoots.Add(recordRoot);
			records.Add(elements);
			minElementCount = Mathf.Min(minElementCount, elements.Count);
			maxElementCount = Mathf.Max(maxElementCount, elements.Count);
		}
		currentRuns = countRecordMajorRuns(records);
		if (tryGetFixedElementCount(records, out int fixedElementCount))
		{
			int fixedRuns = countFixedSOARuns(records, fixedElementCount);
			if (hasEnoughSaving(currentRuns, fixedRuns))
			{
				soaRuns = fixedRuns;
				suggestedMode = FastSOAStructureMode.FixedStructure;
				return true;
			}
			return false;
		}
		if (!tryBuildSparseLanes(recordRoots, records, out List<SparseRoleCandidate> orderedRoles))
		{
			return false;
		}
		int sparseRuns = countSparseSOARuns(orderedRoles);
		if (!hasEnoughSaving(currentRuns, sparseRuns))
		{
			return false;
		}
		soaRuns = sparseRuns;
		suggestedMode = FastSOAStructureMode.SparseStructure;
		return true;
	}
	private static bool tryGetFixedElementCount(List<List<FastUIRenderElement>> records, out int elementCount)
	{
		elementCount = records.Count > 0 ? records[0].Count : 0;
		for (int recordIndex = 1; recordIndex < records.Count; ++recordIndex)
		{
			if (records[recordIndex].Count != elementCount)
			{
				return false;
			}
		}
		for (int slot = 0; slot < elementCount; ++slot)
		{
			int referenceCompatibilityID = records[0][slot].getSOACompatibilityID();
			for (int recordIndex = 1; recordIndex < records.Count; ++recordIndex)
			{
				if (records[recordIndex][slot].getSOACompatibilityID() != referenceCompatibilityID)
				{
					return false;
				}
			}
		}
		return true;
	}
	private static bool tryBuildSparseLanes(List<Transform> recordRoots, List<List<FastUIRenderElement>> records,
		out List<SparseRoleCandidate> orderedRoles)
	{
		orderedRoles = new();
		FastDictionary<string, SparseRoleCandidate> roleMap = new();
		List<SparseRoleCandidate> roles = new();
		for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
		{
			HashSet<SparseRoleCandidate> recordRoleSet = new();
			SparseRoleCandidate previousRole = null;
			List<FastUIRenderElement> elements = records[recordIndex];
			for (int elementIndex = 0; elementIndex < elements.Count; ++elementIndex)
			{
				FastUIRenderElement element = elements[elementIndex];
				string roleKey = getSparseRoleKey(recordRoots[recordIndex], element.transform);
				if (string.IsNullOrEmpty(roleKey))
				{
					return false;
				}
				if (!roleMap.TryGetValue(roleKey, out SparseRoleCandidate role))
				{
					role = new SparseRoleCandidate(element.getSOACompatibilityID());
					roleMap.Add(roleKey, role);
					roles.Add(role);
				}
				else if (role.mCompatibilityID != element.getSOACompatibilityID())
				{
					return false;
				}
				if (!recordRoleSet.Add(role))
				{
					return false;
				}
				role.mMembers.Add(element);
				if (previousRole != null && previousRole != role && !previousRole.mOutgoing.Contains(role))
				{
					previousRole.mOutgoing.Add(role);
					++role.mIndegree;
				}
				previousRole = role;
			}
		}
		for (int pass = 0; pass < roles.Count; ++pass)
		{
			SparseRoleCandidate selected = null;
			for (int roleIndex = 0; roleIndex < roles.Count; ++roleIndex)
			{
				SparseRoleCandidate role = roles[roleIndex];
				if (!role.mEmitted && role.mIndegree == 0)
				{
					selected = role;
					break;
				}
			}
			if (selected == null)
			{
				return false;
			}
			selected.mEmitted = true;
			orderedRoles.Add(selected);
			for (int edgeIndex = 0; edgeIndex < selected.mOutgoing.Count; ++edgeIndex)
			{
				--selected.mOutgoing[edgeIndex].mIndegree;
			}
		}
		return true;
	}
	private static string getSparseRoleKey(Transform record, Transform elementTransform)
	{
		List<Transform> pathNodes = new();
		Transform current = elementTransform;
		while (current != null && current != record)
		{
			pathNodes.Add(current);
			current = current.parent;
		}
		if (current != record)
		{
			return null;
		}
		if (pathNodes.Count == 0)
		{
			return "$SELF";
		}
		string key = string.Empty;
		for (int i = pathNodes.Count - 1; i >= 0; --i)
		{
			Transform node = pathNodes[i];
			if (key.Length > 0)
			{
				key += "/";
			}
			key += node.name + "#" + getSameNameSiblingOccurrence(node);
		}
		return key;
	}
	private static int getSameNameSiblingOccurrence(Transform node)
	{
		Transform parent = node != null ? node.parent : null;
		if (parent == null)
		{
			return 0;
		}
		int occurrence = 0;
		int siblingIndex = node.GetSiblingIndex();
		for (int i = 0; i < siblingIndex; ++i)
		{
			if (parent.GetChild(i).name == node.name)
			{
				++occurrence;
			}
		}
		return occurrence;
	}
	private static bool hasEnoughSaving(int currentRuns, int soaRuns)
	{
		int saving = currentRuns - soaRuns;
		return saving >= MIN_BATCH_SAVING && soaRuns * 4 <= currentRuns * 3;
	}
	private static void collectRenderElements(Transform root, FastCanvas canvas, List<FastUIRenderElement> result, ref bool hasBarrier)
	{
		if (root == null || hasBarrier)
		{
			return;
		}
		if (root.TryGetComponent(out FastCanvas nestedCanvas) && nestedCanvas != canvas)
		{
			return;
		}
		if (root.GetComponent<FastRectMask2D>() != null || root.GetComponent<FastMask>() != null ||
			root.GetComponent<FastClipStencilGraphic>() != null || root.GetComponent<FastMaskPopGraphic>() != null)
		{
			hasBarrier = true;
			return;
		}
		if (root.TryGetComponent(out FastUIRenderElement element))
		{
			result.Add(element);
		}
		for (int i = 0; i < root.childCount; ++i)
		{
			collectRenderElements(root.GetChild(i), canvas, result, ref hasBarrier);
		}
	}
	private static int countRecordMajorRuns(List<List<FastUIRenderElement>> records)
	{
		int runCount = 0;
		bool hasPrevious = false;
		BatchKey previous = default;
		for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
		{
			List<FastUIRenderElement> elements = records[recordIndex];
			for (int slot = 0; slot < elements.Count; ++slot)
			{
				BatchKey current = new(elements[slot]);
				if (!hasPrevious || !current.equals(previous))
				{
					++runCount;
				}
				previous = current;
				hasPrevious = true;
			}
		}
		return runCount;
	}
	private static int countFixedSOARuns(List<List<FastUIRenderElement>> records, int elementCount)
	{
		int runCount = 0;
		bool hasPrevious = false;
		BatchKey previous = default;
		for (int slot = 0; slot < elementCount; ++slot)
		{
			for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
			{
				BatchKey current = new(records[recordIndex][slot]);
				if (!hasPrevious || !current.equals(previous))
				{
					++runCount;
				}
				previous = current;
				hasPrevious = true;
			}
		}
		return runCount;
	}
	private static int countSparseSOARuns(List<SparseRoleCandidate> orderedRoles)
	{
		int runCount = 0;
		bool hasPrevious = false;
		BatchKey previous = default;
		List<BatchKey> laneKeys = new();
		for (int roleIndex = 0; roleIndex < orderedRoles.Count; ++roleIndex)
		{
			laneKeys.Clear();
			List<FastUIRenderElement> members = orderedRoles[roleIndex].mMembers;
			for (int memberIndex = 0; memberIndex < members.Count; ++memberIndex)
			{
				BatchKey key = new(members[memberIndex]);
				bool exists = false;
				for (int keyIndex = 0; keyIndex < laneKeys.Count; ++keyIndex)
				{
					if (laneKeys[keyIndex].equals(key))
					{
						exists = true;
						break;
					}
				}
				if (!exists)
				{
					laneKeys.Add(key);
				}
			}
			for (int keyIndex = 0; keyIndex < laneKeys.Count; ++keyIndex)
			{
				BatchKey current = laneKeys[keyIndex];
				if (!hasPrevious || !current.equals(previous))
				{
					++runCount;
				}
				previous = current;
				hasPrevious = true;
			}
		}
		return runCount;
	}
	private static bool hasSOAAncestor(Transform root, FastCanvas canvas)
	{
		Transform current = root != null ? root.parent : null;
		while (current != null)
		{
			if (current.TryGetComponent(out FastSOARenderGroup group) && group.isActiveAndEnabled)
			{
				return true;
			}
			if (current == canvas.transform)
			{
				break;
			}
			current = current.parent;
		}
		return false;
	}
	private static bool hasSOADescendant(Transform root)
	{
		if (root == null)
		{
			return false;
		}
		for (int i = 0; i < root.childCount; ++i)
		{
			Transform child = root.GetChild(i);
			if (child.TryGetComponent(out FastSOARenderGroup group) && group.isActiveAndEnabled)
			{
				return true;
			}
			if (hasSOADescendant(child))
			{
				return true;
			}
		}
		return false;
	}
	private static string getHierarchyPath(Transform transform)
	{
		if (transform == null)
		{
			return "null";
		}
		string path = transform.name;
		Transform current = transform.parent;
		while (current != null)
		{
			path = current.name + "/" + path;
			current = current.parent;
		}
		return transform.gameObject.scene.name + ":" + path;
	}
}
