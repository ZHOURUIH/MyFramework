using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

// EasyECS BurstView Position计算。
// Position批处理按计算类型使用独立并行阈值；达到阈值后固定Chunk1024，OuterBatch保持1。
// 当前保留Offset与Delta Queue两条实际运行路径。
public static class FastUIBurstPositionJobs
{
	public const int OFFSET_MIN_SLOT_COUNT = 20000;
	public const int DELTA_QUEUE_MIN_COUNT = 15000;
	public const int PRODUCTION_CHUNK_SIZE = 1024;
	public const int PARALLEL_OUTER_BATCH_COUNT = 1;
	public const int DEFAULT_CHUNK_SIZE = 0;
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct OffsetSlotSingleJob : IJob
	{
		[NativeDisableUnsafePtrRestriction] public FastUIGeometryRangeData_ECSList.BurstView mGeometry;
		[NativeDisableUnsafePtrRestriction] public FastUIPositionData_ECSList.BurstView mPosition;
		public int mStartSlot;
		public int mSlotCount;
		public float mDeltaX;
		public float mDeltaY;
		public float mDeltaZ;
		public readonly void Execute()
		{
			int endSlot = mStartSlot + mSlotCount;
			for (int slot = mStartSlot; slot < endSlot; ++slot)
			{
				int vertexStart = mGeometry.mVertexStart[slot];
				int vertexCount = mGeometry.mVertexCount[slot];
				if (vertexStart < 0 || vertexCount <= 0 || vertexStart + vertexCount > mPosition.Count)
				{
					continue;
				}
				int vertexEnd = vertexStart + vertexCount;
				for (int positionIndex = vertexStart; positionIndex < vertexEnd; ++positionIndex)
				{
					Vector3 position = mPosition.mPosition[positionIndex];
					position.x += mDeltaX;
					position.y += mDeltaY;
					position.z += mDeltaZ;
					mPosition.mPosition[positionIndex] = position;
				}
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct PositionDeltaQueueSingleJob : IJob
	{
		[NativeDisableUnsafePtrRestriction] public FastUIPositionDeltaData_ECSList.BurstView mDelta;
		[NativeDisableUnsafePtrRestriction] public FastUIGeometryRangeData_ECSList.BurstView mGeometry;
		[NativeDisableUnsafePtrRestriction] public FastUIPositionData_ECSList.BurstView mPosition;
		public int mDeltaCount;
		public readonly void Execute()
		{
			for (int index = 0; index < mDeltaCount; ++index)
			{
				int slot = mDelta.mVertexSlot[index];
				if ((uint)slot >= (uint)mGeometry.Count)
				{
					continue;
				}
				int vertexStart = mGeometry.mVertexStart[slot];
				int vertexCount = mGeometry.mVertexCount[slot];
				if (vertexStart < 0 || vertexCount <= 0 || vertexStart + vertexCount > mPosition.Count)
				{
					continue;
				}
				Vector3 delta = mDelta.mDelta[index];
				int vertexEnd = vertexStart + vertexCount;
				for (int positionIndex = vertexStart; positionIndex < vertexEnd; ++positionIndex)
				{
					Vector3 position = mPosition.mPosition[positionIndex];
					position.x += delta.x;
					position.y += delta.y;
					position.z += delta.z;
					mPosition.mPosition[positionIndex] = position;
				}
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct OffsetSlotChunkJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public FastUIGeometryRangeData_ECSList.BurstView mGeometry;
		[NativeDisableUnsafePtrRestriction] public FastUIPositionData_ECSList.BurstView mPosition;
		public int mStartSlot;
		public int mSlotCount;
		public int mChunkSize;
		public float mDeltaX;
		public float mDeltaY;
		public float mDeltaZ;
		public readonly void Execute(int chunkIndex)
		{
			mGeometry.GetChunkRange(chunkIndex, mChunkSize, out int localStart, out int localCount);
			if (localStart >= mSlotCount)
			{
				return;
			}
			int localEnd = localStart + localCount;
			if (localEnd > mSlotCount)
			{
				localEnd = mSlotCount;
			}
			int endSlot = mStartSlot + localEnd;
			for (int slot = mStartSlot + localStart; slot < endSlot; ++slot)
			{
				int vertexStart = mGeometry.mVertexStart[slot];
				int vertexCount = mGeometry.mVertexCount[slot];
				if (vertexStart < 0 || vertexCount <= 0 || vertexStart + vertexCount > mPosition.Count)
				{
					continue;
				}
				int vertexEnd = vertexStart + vertexCount;
				for (int positionIndex = vertexStart; positionIndex < vertexEnd; ++positionIndex)
				{
					Vector3 position = mPosition.mPosition[positionIndex];
					position.x += mDeltaX;
					position.y += mDeltaY;
					position.z += mDeltaZ;
					mPosition.mPosition[positionIndex] = position;
				}
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct PositionDeltaQueueChunkJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public FastUIPositionDeltaData_ECSList.BurstView mDelta;
		[NativeDisableUnsafePtrRestriction] public FastUIGeometryRangeData_ECSList.BurstView mGeometry;
		[NativeDisableUnsafePtrRestriction] public FastUIPositionData_ECSList.BurstView mPosition;
		public int mDeltaCount;
		public int mChunkSize;
		public readonly void Execute(int chunkIndex)
		{
			mDelta.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			if (start >= mDeltaCount)
			{
				return;
			}
			int end = start + count;
			if (end > mDeltaCount)
			{
				end = mDeltaCount;
			}
			for (int index = start; index < end; ++index)
			{
				int slot = mDelta.mVertexSlot[index];
				if ((uint)slot >= (uint)mGeometry.Count)
				{
					continue;
				}
				int vertexStart = mGeometry.mVertexStart[slot];
				int vertexCount = mGeometry.mVertexCount[slot];
				if (vertexStart < 0 || vertexCount <= 0 || vertexStart + vertexCount > mPosition.Count)
				{
					continue;
				}
				Vector3 delta = mDelta.mDelta[index];
				int vertexEnd = vertexStart + vertexCount;
				for (int positionIndex = vertexStart; positionIndex < vertexEnd; ++positionIndex)
				{
					Vector3 position = mPosition.mPosition[positionIndex];
					position.x += delta.x;
					position.y += delta.y;
					position.z += delta.z;
					mPosition.mPosition[positionIndex] = position;
				}
			}
		}
	}
	public static bool isAvailable()
	{
		return BurstCompiler.IsEnabled && SystemInfo.processorCount > 1 && FastUIPositionData_ECSList.IsUnsafeBackend && FastUIGeometryRangeData_ECSList.IsUnsafeBackend && FastUIPositionDeltaData_ECSList.IsUnsafeBackend;
	}
	public static bool shouldUseOffset(int slotCount)
	{
		return slotCount >= OFFSET_MIN_SLOT_COUNT && isAvailable();
	}
	public static bool shouldUseDeltaQueue(int deltaCount)
	{
		return deltaCount >= DELTA_QUEUE_MIN_COUNT && isAvailable();
	}
	public static void applyOffsetSlotRange(FastUIGeometryRangeData_ECSList geometry, FastUIPositionData_ECSList position, int startSlot, int slotCount, Vector3 delta, int innerLoopBatchCount = DEFAULT_CHUNK_SIZE)
	{
		if (geometry == null || position == null || slotCount <= 0)
		{
			return;
		}
		int safeStart = Mathf.Max(startSlot, 0);
		int safeCount = Mathf.Min(slotCount - (safeStart - startSlot), geometry.Count - safeStart);
		if (safeCount <= 0)
		{
			return;
		}
		FastUIGeometryRangeData_ECSList.BurstView geometryView = geometry.GetBurstView();
		FastUIPositionData_ECSList.BurstView positionView = position.GetBurstView();
		JobHandle dependency = JobHandle.CombineDependencies(geometry.GetBurstDependency(), position.GetBurstDependency());
		JobHandle handle;
		if (safeCount >= OFFSET_MIN_SLOT_COUNT)
		{
			int chunkSize = resolveChunkSize(innerLoopBatchCount);
			int chunkCount = calculateChunkCount(safeCount, chunkSize);
			handle = new OffsetSlotChunkJob
			{
				mGeometry = geometryView,
				mPosition = positionView,
				mStartSlot = safeStart,
				mSlotCount = safeCount,
				mChunkSize = chunkSize,
				mDeltaX = delta.x,
				mDeltaY = delta.y,
				mDeltaZ = delta.z
			}.Schedule(chunkCount, PARALLEL_OUTER_BATCH_COUNT, dependency);
		}
		else
		{
			handle = new OffsetSlotSingleJob
			{
				mGeometry = geometryView,
				mPosition = positionView,
				mStartSlot = safeStart,
				mSlotCount = safeCount,
				mDeltaX = delta.x,
				mDeltaY = delta.y,
				mDeltaZ = delta.z
			}.Schedule(dependency);
		}
		geometry.RegisterBurstJob(handle);
		position.RegisterBurstJob(handle);
		complete(geometry, position);
	}
	public static void applyPositionDeltaQueue(FastUIPositionDeltaData_ECSList delta, FastUIGeometryRangeData_ECSList geometry, FastUIPositionData_ECSList position,
												int deltaCount, int innerLoopBatchCount = DEFAULT_CHUNK_SIZE)
	{
		if (delta == null || geometry == null || position == null || deltaCount <= 0)
		{
			return;
		}
		int count = Mathf.Min(deltaCount, delta.Count);
		if (count <= 0)
		{
			return;
		}
		FastUIPositionDeltaData_ECSList.BurstView deltaView = delta.GetBurstView();
		FastUIGeometryRangeData_ECSList.BurstView geometryView = geometry.GetBurstView();
		FastUIPositionData_ECSList.BurstView positionView = position.GetBurstView();
		JobHandle dependency = JobHandle.CombineDependencies(delta.GetBurstDependency(), geometry.GetBurstDependency());
		dependency = JobHandle.CombineDependencies(dependency, position.GetBurstDependency());
		JobHandle handle;
		if (count >= DELTA_QUEUE_MIN_COUNT)
		{
			int chunkSize = resolveChunkSize(innerLoopBatchCount);
			int chunkCount = calculateChunkCount(count, chunkSize);
			handle = new PositionDeltaQueueChunkJob
			{
				mDelta = deltaView,
				mGeometry = geometryView,
				mPosition = positionView,
				mDeltaCount = count,
				mChunkSize = chunkSize
			}.Schedule(chunkCount, PARALLEL_OUTER_BATCH_COUNT, dependency);
		}
		else
		{
			handle = new PositionDeltaQueueSingleJob
			{
				mDelta = deltaView,
				mGeometry = geometryView,
				mPosition = positionView,
				mDeltaCount = count
			}.Schedule(dependency);
		}
		delta.RegisterBurstJob(handle);
		geometry.RegisterBurstJob(handle);
		position.RegisterBurstJob(handle);
		position.CompleteBurstJobs();
		geometry.CompleteBurstJobs();
		delta.CompleteBurstJobs();
	}
	public static int getProductionChunkSize()
	{
		return PRODUCTION_CHUNK_SIZE;
	}
	public static int calculateChunkCount(int entityCount, int chunkSize)
	{
		if (entityCount <= 0)
		{
			return 0;
		}
		int safeChunkSize = Mathf.Max(chunkSize, 1);
		return (entityCount + safeChunkSize - 1) / safeChunkSize;
	}
	private static int resolveChunkSize(int requestedChunkSize)
	{
		return requestedChunkSize > 0 ? requestedChunkSize : getProductionChunkSize();
	}
	private static void complete(FastUIGeometryRangeData_ECSList geometry, FastUIPositionData_ECSList position)
	{
		position.CompleteBurstJobs();
		geometry.CompleteBurstJobs();
	}
}
