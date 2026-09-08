using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// EasyECS Column到NativeArray的零拷贝桥接层。
// FastUI手写unsafe只允许集中在这里，Renderer和各运行时System不直接操作裸指针。

public static class FastUINativeArrayBridge
{
	public static unsafe NativeArray<T> createView<T>(ref T first, int count) where T : unmanaged
	{
		void* pointer = UnsafeUtility.AddressOf(ref first);
		NativeArray<T> view = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(pointer, count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
		NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref view, AtomicSafetyHandle.GetTempMemoryHandle());
#endif
		return view;
	}
}
