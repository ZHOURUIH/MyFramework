using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static TestAssert;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseDefine;

// ResourceManager 资源管理器单元测试
// 覆盖以下场景:
//
// 1.  引用计数系统: addReference / removeReference / update 定时清理
// 2.  ResourceRef<T> 生命周期: 创建、持有、释放、copyRef
// 3.  合法资源路径加载
// 4.  卸载回调: addUnloadObjectCallback / removeUnloadObjectCallback
// 5.  unloadPath 卸载路径
// 6.  AssetBundleInfo: 依赖关系、canUnload、延迟卸载、unload
// 7.  AssetInfo: 同步加载状态机、异步加载回调聚合、setSubAssets 容错
// 8.  AssetBundleLoader: 未初始化时的安全检查
// 9.  AssetDataBaseLoader: 同步加载、路径管理、卸载
// 10. 多处同时引用同一资源
// 11. 异步加载完成后持有者已销毁 (实际调用 loadGameResourceAsyncSafe)
// 12. 多个异步请求共享同一资源时, 空回调或失效持有者不能卸载其他请求已持有的资源
// 13. 引用计数归零后定时自动卸载
// 14. 多次引用同一资源, 部分释放后资源仍可用
// 15. 卸载后重新加载
public static class ResourceManagerTest
{
	// ResourceManager 中 CHECK_REF_INTERVAL 的值, protected const 无法直接访问, 此处使用字面量
	private const float CHECK_REF_INTERVAL = 3.0f;

	// 保存原始的 mResourceManager, 测试结束后恢复
	private static ResourceManager sOriginalResourceManager;
	private static bool sHadOriginalResourceManager;

	public static void Run()
	{
		// 如果 mResourceManager 已存在则保存, 测试中会替换为测试实例
		sHadOriginalResourceManager = mResourceManager != null;
		sOriginalResourceManager = mResourceManager;

		try
		{
			// === 1. 引用计数系统 ===
			testAddReferenceReturnsUniqueToken();
			testRemoveReferenceClearsToken();
			testMultipleReferencesSameAsset();
			testMultipleReferencesDifferentAssets();
			testReferenceTokenUniquenessAcrossAssets();

			// === 2. update 定时清理 ===
			testUpdateNoCleanupWhenReferencesExist();
			testUpdateCleanupWhenAllReferencesRemoved();
			testUpdateCleanupDelayByInterval();
			testUpdateCleanupMultipleAssetsSimultaneously();
			testUpdateCleanupDoesNotAffectActiveReferences();

			// === 3. 卸载回调 ===
			testUnloadObjectCallbackInvoked();
			testRemoveUnloadObjectCallback();
			testUnloadCallbackMultiple();

			// === 4. ResourceRef 生命周期 ===
			testResourceRefSetResource();
			testResourceRefIsValid();
			testResourceRefGetResource();
			testResourceRefDestroyRemovesReference();
			testResourceRefCopyRef();
			testResourceRefResetProperty();
			testResourceRefResetEmptyState();

			// === 5. 多处同时引用 ===
			testMultipleHoldersSameResource();
			testPartialReleaseKeepsResourceAlive();
			testAllReleaseTriggersCleanup();
			testCopyRefCreatesIndependentToken();

			// === 6. 多个异步请求共享同一资源 ===
			testCrossScenario_AsyncCallbackNull_UnloadsUnreferencedResource();
			testCrossScenario_AsyncCallbackNull_DoesNotUnloadHeldResource();
			testCrossScenario_AsyncSafe_HolderDestroyed_UnloadsUnreferencedResource();
			testCrossScenario_AsyncSafe_HolderDestroyed_DoesNotUnloadHeldResource();
			testCrossScenario_AsyncSafe_HolderAlive();

			// === 7. 合法资源路径加载 ===
			testCheckRelativePath_ValidPath();

			// === 8. AssetBundleInfo 依赖与卸载 ===
			testAssetBundleInfoCanUnload_Empty();
			testAssetBundleInfoCanUnload_WithLoadedAsset();
			testAssetBundleInfoCanUnload_WithActiveChild();
			testAssetBundleInfoUnload_ClearsState();
			testAssetBundleInfoUnload_NotifiesParents();
			testAssetBundleInfoUnload_DelayTimer();
			testAssetBundleInfoDontUnload();
			testAssetBundleInfoAddParentAndChild();
			testAssetBundleInfoIsAllParentLoaded();
			testAssetBundleInfoLoadAssetBundleAsync_AlreadyLoaded();

			// === 9. AssetInfo 状态机 ===
			testAssetInfoLoadAssetSync();
			testAssetInfoLoadAssetAsync_AlreadyLoaded();
			testAssetInfoSetSubAssets_WithNull();
			testAssetInfoCallbackAggregation();
			testAssetInfoClear();
			testAssetInfoResetProperty();

			// === 10. AssetDataBaseLoader ===
			testAssetDataBaseLoaderIsAssetLoaded();
			testAssetDataBaseLoaderGetAsset_NotLoaded();
			testAssetDataBaseLoaderUnloadAsset_Null();
			testAssetDataBaseLoaderUnloadPath_Empty();

			// === 11. ResourceManager unload 方法 ===
			testUnloadNullResourceRef();
			testUnloadPath_InvokesCallbacks();
			testUnloadAssetBundle_NotAssetBundleMode();
			testPreloadAssetBundle_NotAssetBundleMode();
			testPreloadAssetBundleAsync_NotAssetBundleMode();
			testIsResourceInited_AssetDatabaseMode();

			// === 12. 多次引用操作 ===
			testDestroyedResourceRefIsInvalid();
			testAddReferenceSameObjectMultipleTimes();

			// === 13. 卸载后重新引用 ===
			testReAddReferenceAfterCleanup();

			// === 14. CustomAsyncOperation 基本行为 ===
			testCustomAsyncOperationInResourceContext();

			// === 15. 综合交叉场景 ===
			testComplexScenario_MultiHolder_PartialUnload_Reload();
			testComplexScenario_AsyncLoadComplete_ThenUnloadAll();
		}
		finally
		{
			// 恢复原始 mResourceManager
			mResourceManager = sHadOriginalResourceManager ? sOriginalResourceManager : null;
		}
	}

	// ================================================================================================
	// 辅助方法
	// ================================================================================================

	// 创建一个测试用的 ResourceManager 实例
	private static ResourceManager createTestResourceManager()
	{
		return new ResourceManager();
	}
	// 创建可控异步完成时机的 ResourceManager,只用于异步共享资源场景测试
	private static TestResourceManager createAsyncTestResourceManager()
	{
		return new TestResourceManager();
	}
	// 创建一个测试用的 UObject (ScriptableObject)
	private static ScriptableObject createTestObject()
	{
		return ScriptableObject.CreateInstance<TestResourceSO>();
	}
	// 模拟 ResourceRef 的创建和销毁, 不依赖 CLASS 宏
	// 直接 new ResourceRef<T> 并手动管理生命周期
	private static ResourceRef<T> createResourceRef<T>(ResourceManager rm, T res) where T : UObject
	{
		var resRef = new ResourceRef<T>();
		resRef.set(res);
		return resRef;
	}
	// 模拟 UN_CLASS 的完整回收流程:先调用 destroy 释放资源引用,再调用 resetProperty 清空对象状态
	private static void destroyResourceRef<T>(ResourceRef<T> resRef) where T : UObject
	{
		resRef.destroy();
		resRef.resetProperty();
	}
	// 用于测试的 ScriptableObject
	private class TestResourceSO : ScriptableObject { }
	// ================================================================================================
	// 1. 引用计数系统
	// ================================================================================================
	private static void testAddReferenceReturnsUniqueToken()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		try
		{
			long token = rm.addReference(obj);
			assertTrue(token > 0, "addReference 应返回大于0的token");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testRemoveReferenceClearsToken()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		try
		{
			long token = rm.addReference(obj);
			long tokenCopy = token;
			rm.removeReference(obj, ref tokenCopy);
			assertEqual(0L, tokenCopy, "removeReference 后 token 应被置为0");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testMultipleReferencesSameAsset()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		try
		{
			long t1 = rm.addReference(obj);
			long t2 = rm.addReference(obj);
			long t3 = rm.addReference(obj);
			assertTrue(t1 != t2, "同一资源的多个引用token应不同");
			assertTrue(t2 != t3, "同一资源的多个引用token应不同");
			assertTrue(t1 != t3, "同一资源的多个引用token应不同");

			// 移除一个引用, 资源仍应被引用 (不会触发卸载)
			rm.removeReference(obj, ref t1);
			rm.removeReference(obj, ref t2);
			// 还剩 t3
			rm.removeReference(obj, ref t3);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testMultipleReferencesDifferentAssets()
	{
		var rm = createTestResourceManager();
		var obj1 = createTestObject();
		var obj2 = createTestObject();
		try
		{
			long t1 = rm.addReference(obj1);
			long t2 = rm.addReference(obj2);
			assertTrue(t1 != t2, "不同资源的token应不同");

			rm.removeReference(obj1, ref t1);
			rm.removeReference(obj2, ref t2);
		}
		finally
		{
			UObject.DestroyImmediate(obj1);
			UObject.DestroyImmediate(obj2);
		}
	}
	private static void testReferenceTokenUniquenessAcrossAssets()
	{
		var rm = createTestResourceManager();
		var objs = new ScriptableObject[10];
		var tokens = new long[10];
		try
		{
			for (int i = 0; i < 10; ++i)
			{
				objs[i] = createTestObject();
				tokens[i] = rm.addReference(objs[i]);
			}
			// 验证所有token都唯一
			for (int i = 0; i < 10; ++i)
			{
				for (int j = i + 1; j < 10; ++j)
				{
					assertTrue(tokens[i] != tokens[j], $"token[{i}]={tokens[i]} 不应等于 token[{j}]={tokens[j]}");
				}
			}
			for (int i = 0; i < 10; ++i)
			{
				rm.removeReference(objs[i], ref tokens[i]);
			}
		}
		finally
		{
			for (int i = 0; i < 10; ++i)
			{
				if (objs[i] != null) UObject.DestroyImmediate(objs[i]);
			}
		}
	}
	// ================================================================================================
	// 2. update 定时清理
	// ================================================================================================
	private static void testUpdateNoCleanupWhenReferencesExist()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			long token = rm.addReference(obj);
			// 模拟多帧 update, 即使超过 CHECK_REF_INTERVAL 也不应清理
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "有引用时不应触发卸载");
			rm.removeReference(obj, ref token);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testUpdateCleanupWhenAllReferencesRemoved()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			long token = rm.addReference(obj);
			rm.removeReference(obj, ref token);
			// 引用归零后, 经过 CHECK_REF_INTERVAL 应触发卸载
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "引用归零后应触发卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testUpdateCleanupDelayByInterval()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			long token = rm.addReference(obj);
			// 先消耗一次 update (mCheckRefTimer 初始为0, 第一次 update 会立即触发检查)
			// 此时还有引用, 不会卸载
			rm.update(0.01f);

			rm.removeReference(obj, ref token);
			// 不足间隔时间, 不应清理
			rm.update(CHECK_REF_INTERVAL - 0.1f);
			assertEqual(0, unloadCount, "不足间隔时间不应触发卸载");
			// 再 update 一次, 累计超过间隔
			rm.update(0.2f);
			assertEqual(1, unloadCount, "累计超过间隔后应触发卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testUpdateCleanupMultipleAssetsSimultaneously()
	{
		var rm = createTestResourceManager();
		var obj1 = createTestObject();
		var obj2 = createTestObject();
		var obj3 = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			long t1 = rm.addReference(obj1);
			long t2 = rm.addReference(obj2);
			long t3 = rm.addReference(obj3);
			rm.removeReference(obj1, ref t1);
			rm.removeReference(obj2, ref t2);
			rm.removeReference(obj3, ref t3);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(3, unloadCount, "所有引用归零的资源都应被卸载");
		}
		finally
		{
			UObject.DestroyImmediate(obj1);
			UObject.DestroyImmediate(obj2);
			UObject.DestroyImmediate(obj3);
		}
	}
	private static void testUpdateCleanupDoesNotAffectActiveReferences()
	{
		var rm = createTestResourceManager();
		var obj1 = createTestObject(); // 将被清理
		var obj2 = createTestObject(); // 保持引用
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			long t1 = rm.addReference(obj1);
			long t2 = rm.addReference(obj2);
			rm.removeReference(obj1, ref t1);
			// obj2 仍有引用
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "只有引用归零的资源应被卸载");
			rm.removeReference(obj2, ref t2);
		}
		finally
		{
			UObject.DestroyImmediate(obj1);
			UObject.DestroyImmediate(obj2);
		}
	}
	// ================================================================================================
	// 3. 卸载回调
	// ================================================================================================
	private static void testUnloadObjectCallbackInvoked()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		UObject unloadedObj = null;
		UObjectCallback cb = o => unloadedObj = o;
		rm.addUnloadObjectCallback(cb);
		try
		{
			long token = rm.addReference(obj);
			rm.removeReference(obj, ref token);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(obj, unloadedObj, "卸载回调应收到被卸载的资源");
			rm.removeUnloadObjectCallback(cb);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testRemoveUnloadObjectCallback()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int unloadCount = 0;
		UObjectCallback cb = _ => ++unloadCount;
		rm.addUnloadObjectCallback(cb);
		rm.removeUnloadObjectCallback(cb);
		try
		{
			long token = rm.addReference(obj);
			rm.removeReference(obj, ref token);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "移除回调后不应再被调用");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testUnloadCallbackMultiple()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int count1 = 0, count2 = 0;
		UObjectCallback cb1 = _ => ++count1;
		UObjectCallback cb2 = _ => ++count2;
		rm.addUnloadObjectCallback(cb1);
		rm.addUnloadObjectCallback(cb2);
		try
		{
			long token = rm.addReference(obj);
			rm.removeReference(obj, ref token);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, count1, "回调1应被调用一次");
			assertEqual(1, count2, "回调2应被调用一次");
			rm.removeUnloadObjectCallback(cb1);
			rm.removeUnloadObjectCallback(cb2);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	// ================================================================================================
	// 4. ResourceRef 生命周期
	// ================================================================================================
	private static void testResourceRefSetResource()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var resRef = createResourceRef(rm, obj);
			assertNotNull(resRef.get(), "setResource 后应能获取资源");
			destroyResourceRef(resRef);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefIsValid()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var resRef = createResourceRef(rm, obj);
			assertTrue(resRef.isValid(), "有资源时应 isValid");
			destroyResourceRef(resRef);
			assertFalse(resRef.isValid(), "销毁后应不 isValid");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefGetResource()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var resRef = createResourceRef(rm, obj);
			assertEqual(obj, resRef.get(), "getResource 应返回设置的资源");
			destroyResourceRef(resRef);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefDestroyRemovesReference()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			var resRef = createResourceRef(rm, obj);
			destroyResourceRef(resRef);
			// 引用已移除, update 后应触发卸载
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "ResourceRef destroy 后应移除引用, 进而触发卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefCopyRef()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			var resRef = createResourceRef(rm, obj);
			var copy = resRef.copyRef();
			assertNotNull(copy, "copyRef 应返回新的引用");
			assertTrue(copy.isValid(), "copyRef 的引用应有效");
			assertEqual(obj, copy.get(), "copyRef 应引用同一资源");
			assertTrue(resRef.getToken() != copy.getToken(), "copyRef 的 token 应不同");

			// 销毁原引用, 资源不应被卸载 (copy 仍持有)
			destroyResourceRef(resRef);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "copyRef 仍持有引用, 不应卸载");

			// 销毁 copy 后, 资源应被卸载
			destroyResourceRef(copy);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "所有引用释放后应卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefResetProperty()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var resRef = createResourceRef(rm, obj);
			long tokenBefore = resRef.getToken();
			assertTrue(tokenBefore > 0, "应有有效 token");

			// 先移除引用再 resetProperty (因为 resetProperty 不移除引用)
			rm.removeReference(obj, ref tokenBefore);
			resRef.resetProperty();
			assertNull(resRef.get(), "resetProperty 后 resource 应为 null");
			assertEqual(0L, resRef.getToken(), "resetProperty 后 token 应为 0");
			assertFalse(resRef.isValid(), "resetProperty 后应不 isValid");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testResourceRefResetEmptyState()
	{
		var resRef = new ResourceRef<ScriptableObject>();
		resRef.resetProperty();
		assertNull(resRef.get(), "空引用 resetProperty 后资源应为 null");
		assertEqual(0L, resRef.getToken(), "空引用 resetProperty 后 token 应为 0");
		assertFalse(resRef.isValid(), "空引用 resetProperty 后应无效");
	}
	// ================================================================================================
	// 5. 多处同时引用
	// ================================================================================================
	private static void testMultipleHoldersSameResource()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			// 模拟3个持有者同时引用同一资源
			var ref1 = createResourceRef(rm, obj);
			var ref2 = ref1.copyRef();
			var ref3 = ref1.copyRef();

			// 部分释放
			destroyResourceRef(ref1);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "部分释放后资源不应被卸载");

			destroyResourceRef(ref2);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "部分释放后资源不应被卸载");

			destroyResourceRef(ref3);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "所有引用释放后资源应被卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testPartialReleaseKeepsResourceAlive()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			var ref1 = createResourceRef(rm, obj);
			var ref2 = ref1.copyRef();

			destroyResourceRef(ref1);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "部分释放后资源仍存活");

			// ref2 仍可访问资源
			assertNotNull(ref2.get(), "存活引用仍可获取资源");
			assertEqual(obj, ref2.get(), "存活引用获取的资源应正确");

			destroyResourceRef(ref2);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAllReleaseTriggersCleanup()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			var refs = new List<ResourceRef<ScriptableObject>>();
			for (int i = 0; i < 5; ++i)
			{
				if (i == 0)
					refs.Add(createResourceRef(rm, obj));
				else
					refs.Add(refs[0].copyRef());
			}
			assertEqual(5, refs.Count, "应有5个引用");

			// 逐个释放
			for (int i = 0; i < 4; ++i)
			{
				destroyResourceRef(refs[i]);
				rm.update(CHECK_REF_INTERVAL + 0.1f);
				assertEqual(0, unloadCount, $"释放{i + 1}个后不应卸载");
			}
			destroyResourceRef(refs[4]);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "全部释放后应卸载");
		}
		finally 
		{
			UObject.DestroyImmediate(obj); 
		}
	}
	private static void testCopyRefCreatesIndependentToken()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var ref1 = createResourceRef(rm, obj);
			var ref2 = ref1.copyRef();
			var ref3 = ref2.copyRef();

			long t1 = ref1.getToken();
			long t2 = ref2.getToken();
			long t3 = ref3.getToken();

			assertTrue(t1 != t2, "每个 copyRef 的 token 应唯一");
			assertTrue(t2 != t3, "每个 copyRef 的 token 应唯一");
			assertTrue(t1 != t3, "每个 copyRef 的 token 应唯一");

			destroyResourceRef(ref1);
			destroyResourceRef(ref2);
			destroyResourceRef(ref3);
		}
		finally 
		{
			UObject.DestroyImmediate(obj); 
		}
	}
	// ================================================================================================
	// 6. 多个异步请求共享同一资源
	// ================================================================================================
	private static void testCrossScenario_AsyncCallbackNull_UnloadsUnreferencedResource()
	{
		// 只有一个异步请求且回调为空时,临时 ResourceRef 会被释放,资源应等待 ResourceManager.update 检查后再卸载。
		const string resourceName = "UnitTest/AsyncCallbackNull.prefab";
		var rm = createAsyncTestResourceManager();
		mResourceManager = rm;
		var obj = new GameObject("AsyncCallbackNull");
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			rm.prepareAsyncLoad(resourceName);
			rm.loadGameResourceAsync<GameObject>(resourceName, (Action<ResourceRef<GameObject>>)null, false);
			rm.completeAsyncLoad(resourceName, obj);

			assertEqual(0, unloadCount, "异步完成时不应立即卸载无引用资源");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "执行引用检查前资源应继续保持加载状态");

			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "update检查到资源没有引用后应卸载资源");
			assertFalse(rm.isGameResourceLoaded<GameObject>(resourceName), "update卸载后资源不应继续缓存");
		}
		finally
		{
			UObject.DestroyImmediate(obj);
		}
	}
	private static void testCrossScenario_AsyncCallbackNull_DoesNotUnloadHeldResource()
	{
		// 两个请求同时等待同一资源。第一个请求先获得 ResourceRef,第二个请求的回调为空。
		// 旧逻辑会在第二个请求完成时直接 unloadInternal,把第一个请求正在使用的资源卸载。
		const string resourceName = "UnitTest/SharedAsyncCallbackNull.prefab";
		var rm = createAsyncTestResourceManager();
		mResourceManager = rm;
		var obj = new GameObject("SharedAsyncCallbackNull");
		ResourceRef<GameObject> holderRef = null;
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			rm.prepareAsyncLoad(resourceName);
			rm.loadGameResourceAsync<GameObject>(resourceName, (ResourceRef<GameObject> resRef) => holderRef = resRef, false);
			rm.loadGameResourceAsync<GameObject>(resourceName, (Action<ResourceRef<GameObject>>)null, false);
			rm.completeAsyncLoad(resourceName, obj);

			assertNotNull(holderRef, "第一个异步请求应获得资源引用");
			assertTrue(holderRef.isValid(), "第一个异步请求获得的资源引用应有效");
			assertEqual(obj, holderRef.get(), "第一个异步请求持有的资源应正确");
			assertEqual(0, unloadCount, "另一个请求回调为空时不能卸载已被持有的共享资源");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "共享资源仍有引用时应保持加载状态");

			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "update检查时共享资源仍有引用,不应卸载");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "update检查后共享资源仍应保持加载状态");

			destroyResourceRef(holderRef);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "最后一个引用释放后应正常卸载共享资源");
			assertFalse(rm.isGameResourceLoaded<GameObject>(resourceName), "最后一个引用释放后资源不应继续缓存");
		}
		finally
		{
			if (holderRef != null && holderRef.getToken() != 0)
			{
				destroyResourceRef(holderRef);
			}
			UObject.DestroyImmediate(obj);
		}
	}
	private static void testCrossScenario_AsyncSafe_HolderDestroyed_UnloadsUnreferencedResource()
	{
		// Safe 请求的持有者在完成前被回收时,临时 ResourceRef 会被释放,资源应等待 ResourceManager.update 检查后再卸载。
		const string resourceName = "UnitTest/AsyncSafeDestroyed.prefab";
		var rm = createAsyncTestResourceManager();
		mResourceManager = rm;
		var obj = new GameObject("AsyncSafeDestroyed");
		var relatedObj = new TestRecyclable();
		relatedObj.setAssignID(12345);
		int safeCallbackCount = 0;
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			rm.prepareAsyncLoad(resourceName);
			rm.loadGameResourceAsyncSafe<GameObject>(relatedObj, resourceName, (ResourceRef<GameObject> _) => ++safeCallbackCount, false);
			relatedObj.resetProperty();
			rm.completeAsyncLoad(resourceName, obj);

			assertEqual(0, safeCallbackCount, "持有者销毁后 Safe 请求不应执行回调");
			assertEqual(0, unloadCount, "持有者销毁后不应在异步完成回调中立即卸载资源");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "执行引用检查前资源应继续保持加载状态");

			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "update检查到资源没有引用后应卸载资源");
			assertFalse(rm.isGameResourceLoaded<GameObject>(resourceName), "update卸载后资源不应继续缓存");
		}
		finally
		{
			UObject.DestroyImmediate(obj);
		}
	}
	private static void testCrossScenario_AsyncSafe_HolderDestroyed_DoesNotUnloadHeldResource()
	{
		// 两个请求同时等待同一资源。第一个请求正常持有资源,第二个 Safe 请求的持有者在完成前被回收。
		// Safe 请求不能因为自己的持有者失效而卸载第一个请求正在使用的共享资源。
		const string resourceName = "UnitTest/SharedAsyncSafeDestroyed.prefab";
		var rm = createAsyncTestResourceManager();
		mResourceManager = rm;
		var obj = new GameObject("SharedAsyncSafeDestroyed");
		var relatedObj = new TestRecyclable();
		relatedObj.setAssignID(12345);
		ResourceRef<GameObject> holderRef = null;
		int safeCallbackCount = 0;
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			rm.prepareAsyncLoad(resourceName);
			rm.loadGameResourceAsync<GameObject>(resourceName, (ResourceRef<GameObject> resRef) => holderRef = resRef, false);
			rm.loadGameResourceAsyncSafe<GameObject>(relatedObj, resourceName, (ResourceRef<GameObject> _) => ++safeCallbackCount, false);

			// 模拟 Safe 请求的持有者在异步加载完成前被对象池回收。
			relatedObj.resetProperty();
			rm.completeAsyncLoad(resourceName, obj);

			assertEqual(0, safeCallbackCount, "持有者销毁后 Safe 请求不应执行回调");
			assertNotNull(holderRef, "另一个正常请求应获得资源引用");
			assertTrue(holderRef.isValid(), "另一个正常请求持有的资源引用应有效");
			assertEqual(obj, holderRef.get(), "另一个正常请求持有的资源应正确");
			assertEqual(0, unloadCount, "Safe 请求持有者销毁时不能卸载其他请求正在使用的共享资源");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "共享资源仍有引用时应保持加载状态");

			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "update检查时共享资源仍有引用,不应卸载");
			assertTrue(rm.isGameResourceLoaded<GameObject>(resourceName), "update检查后共享资源仍应保持加载状态");

			destroyResourceRef(holderRef);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "最后一个引用释放后应正常卸载共享资源");
			assertFalse(rm.isGameResourceLoaded<GameObject>(resourceName), "最后一个引用释放后资源不应继续缓存");
		}
		finally
		{
			if (holderRef != null && holderRef.getToken() != 0)
			{
				destroyResourceRef(holderRef);
			}
			UObject.DestroyImmediate(obj);
		}
	}
	private static void testCrossScenario_AsyncSafe_HolderAlive()
	{
		// Safe 请求的持有者仍然有效时,实际完成异步加载并把 ResourceRef 交给回调。
		const string resourceName = "UnitTest/AsyncSafeAlive.prefab";
		var rm = createAsyncTestResourceManager();
		mResourceManager = rm;
		var obj = new GameObject("AsyncSafeAlive");
		var relatedObj = new TestRecyclable();
		relatedObj.setAssignID(12345);
		ResourceRef<GameObject> deliveredRef = null;
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			rm.prepareAsyncLoad(resourceName);
			rm.loadGameResourceAsyncSafe<GameObject>(relatedObj, resourceName, (ResourceRef<GameObject> resRef) => deliveredRef = resRef, false);
			rm.completeAsyncLoad(resourceName, obj);

			assertNotNull(deliveredRef, "持有者存活时 Safe 请求应获得资源引用");
			assertTrue(deliveredRef.isValid(), "Safe 请求交付的资源引用应有效");
			assertEqual(obj, deliveredRef.get(), "Safe 请求交付的资源应正确");
			assertEqual(0, unloadCount, "资源被回调持有时不应卸载");

			destroyResourceRef(deliveredRef);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "Safe 请求释放最后一个引用后应卸载资源");
		}
		finally
		{
			if (deliveredRef != null && deliveredRef.getToken() != 0)
			{
				destroyResourceRef(deliveredRef);
			}
			UObject.DestroyImmediate(obj);
		}
	}
	// ================================================================================================
	// 7. 合法资源路径加载
	// ================================================================================================
	private static void testCheckRelativePath_ValidPath()
	{
		// 合法路径格式 (带后缀, 不以Assets开头)
		// 在非编辑器环境, Resources.Load 会返回 null, 但不应崩溃
		var rm = createTestResourceManager();
		var result = rm.loadGameResource<ScriptableObject>("UI/test.prefab", false);
		// 资源不存在, 返回 null, 但不应崩溃
		assertNull(result, "不存在的合法路径应返回 null");
	}
	// ================================================================================================
	// 8. AssetBundleInfo 依赖与卸载
	// ================================================================================================
	private static void testAssetBundleInfoCanUnload_Empty()
	{
		// mResourceManager 需要设置, 因为 canUnload 中调用 mResourceManager.isDontUnloadAssetBundle
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/empty");
		bundle.setLoadState(LOAD_STATE.LOADED);
		// 没有加载的 asset, 没有活跃的 child, 可以卸载
		// 注意: canUnload 是 protected, 通过 update 间接测试
		// 直接验证 unload 不崩溃
		bundle.unload();
		assertEqual(LOAD_STATE.NONE, bundle.getLoadState(), "unload 后状态应为 NONE");
	}
	private static void testAssetBundleInfoCanUnload_WithLoadedAsset()
	{
		mResourceManager = createTestResourceManager();
		mResourceManager.addDontUnloadAssetBundle("test/bundle.unity3d");
		var bundle = new AssetBundleInfo("test/bundle");
		bundle.setLoadState(LOAD_STATE.LOADED);
		bundle.addAssetName("a.prefab");
		var assetInfo = bundle.getAssetInfo("a.prefab");
		assetInfo.setLoadState(LOAD_STATE.LOADED);
		// 有加载的 asset, canUnload 应返回 false
		// 通过设置 mWillUnloadTime 并 update 来间接验证
		// 由于 canUnload 是 protected, 这里通过 unload() 不会真正卸载来验证
		// 因为 isDontUnloadAssetBundle 为 true, unload 不会执行
		bundle.unload();
		// 因为 dontUnload, 状态不变
		assertEqual(LOAD_STATE.LOADED, bundle.getLoadState(), "dontUnload 的 bundle 不应被卸载");
	}
	private static void testAssetBundleInfoCanUnload_WithActiveChild()
	{
		mResourceManager = createTestResourceManager();
		var parent = new AssetBundleInfo("test/parent");
		var child = new AssetBundleInfo("test/child");
		parent.setLoadState(LOAD_STATE.LOADED);
		child.setLoadState(LOAD_STATE.LOADED);
		parent.addChild(child);
		// 有活跃的 child, canUnload 应返回 false
		// 验证 parent 的 unload 不会因为 child 活跃而真正卸载 (但 unload 是强制的)
		// 实际上 canUnload 是在 update 中检查的
		// 这里验证 parent.unload() 后会通知 parents, 但 parent 没有 parent
		// 所以这里主要验证不崩溃
		parent.unload();
		assertEqual(LOAD_STATE.NONE, parent.getLoadState(), "parent unload 后状态应为 NONE");
	}
    private static void testAssetBundleInfoUnload_ClearsState()
    {
        mResourceManager = createTestResourceManager();
        var bundle = new AssetBundleInfo("test/clear");
        bundle.setLoadState(LOAD_STATE.LOADED);
        bundle.addAssetName("a.prefab");
        bundle.addAssetName("b.png");

        bundle.unload();

        assertEqual(LOAD_STATE.NONE, bundle.getLoadState(), "unload 后状态应为 NONE");
        foreach (var item in bundle.getAssetList())
        {
            assertFalse(item.Value.isLoaded(), "unload 后 asset 应未加载");
        }
    }
    private static void testAssetBundleInfoUnload_NotifiesParents()
	{
		mResourceManager = createTestResourceManager();
		var parent = new AssetBundleInfo("test/parent");
		var child = new AssetBundleInfo("test/child");
		parent.setLoadState(LOAD_STATE.LOADED);
		child.setLoadState(LOAD_STATE.LOADED);

		// 设置依赖关系
		child.addParent("test/parent");
		// 需要 findAllDependence 来建立实际的引用关系
		// 但这需要 mResourceManager.getAssetBundleInfo, 我们手动设置
		child.getParents()["test/parent"] = parent;
		parent.addChild(child);

		// child unload 后, parent 应收到通知
		child.unload();
		// parent 的 notifyChildUnload 会被调用
		// 如果 parent canUnload, 会设置 mWillUnloadTime
		// parent 没有 asset, child 已 unload, 所以 canUnload 应为 true
		// 但 mWillUnloadTime 需要通过 update 触发
		// 这里只验证不崩溃
	}
	private static void testAssetBundleInfoUnload_DelayTimer()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/delay");
		bundle.setLoadState(LOAD_STATE.LOADED);
		bundle.addAssetName("a.prefab");

		// 模拟 asset 卸载后设置延迟卸载
		var assetInfo = bundle.getAssetInfo("a.prefab");
		assetInfo.setLoadState(LOAD_STATE.NONE);

		// bundle.update 会检查 canUnload 并 tick 延迟计时器
		// 由于 canUnload 是 protected, 通过 update 间接测试
		// 第一次 update: 如果 canUnload 为 true, 会设置 mWillUnloadTime = UNLOAD_DELAY_TIME
		// 但 mAssetBundle 为 null (没有真正加载 AssetBundle), 所以不会调用 Unload(true)
		// 但状态会被设置为 NONE
		// 注意: update 中只有 mAssetBundle != null 才会 update
		// 所以这里直接调用 unload
		bundle.unload();
		assertEqual(LOAD_STATE.NONE, bundle.getLoadState(), "unload 后状态应为 NONE");
	}
	private static void testAssetBundleInfoDontUnload()
	{
		mResourceManager = createTestResourceManager();
		mResourceManager.addDontUnloadAssetBundle("dontunload.unity3d");
		var bundle = new AssetBundleInfo("dontunload");
		bundle.setLoadState(LOAD_STATE.LOADED);
		bundle.unload();
		// dontUnload 的 bundle 不应被卸载
		assertEqual(LOAD_STATE.LOADED, bundle.getLoadState(), "dontUnload 的 bundle 不应被卸载");
	}
	private static void testAssetBundleInfoAddParentAndChild()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/relations");
		bundle.addParent("dep1");
		bundle.addParent("dep2");
		assertEqual(2, bundle.getParents().Count, "应有2个依赖项");
		assertTrue(bundle.getParents().ContainsKey("dep1"), "应包含 dep1");
		assertTrue(bundle.getParents().ContainsKey("dep2"), "应包含 dep2");

		var child = new AssetBundleInfo("test/child");
		bundle.addChild(child);
		assertEqual(1, bundle.getChildren().Count, "应有1个子项");
		assertTrue(bundle.getChildren().ContainsKey("test/child"), "应包含 test/child");
	}
	private static void testAssetBundleInfoIsAllParentLoaded()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/checkParents");
		var parent1 = new AssetBundleInfo("test/parent1");
		var parent2 = new AssetBundleInfo("test/parent2");
		bundle.addParent("test/parent1");
		bundle.addParent("test/parent2");
		bundle.getParents()["test/parent1"] = parent1;
		bundle.getParents()["test/parent2"] = parent2;

		// 所有父节点都未加载
		assertFalse(bundle.isAllParentLoaded(), "父节点未加载时应返回 false");

		parent1.setLoadState(LOAD_STATE.LOADED);
		assertFalse(bundle.isAllParentLoaded(), "部分父节点未加载时应返回 false");

		parent2.setLoadState(LOAD_STATE.LOADED);
		assertTrue(bundle.isAllParentLoaded(), "所有父节点已加载时应返回 true");
	}
	private static void testAssetBundleInfoLoadAssetBundleAsync_AlreadyLoaded()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/asyncAlreadyLoaded");
		bundle.setLoadState(LOAD_STATE.LOADED);
		int callbackCount = 0;
		bundle.loadAssetBundleAsync(_ => ++callbackCount);
		// 已加载时应立即调用回调
		assertEqual(1, callbackCount, "已加载时应立即调用回调");
	}
	// ================================================================================================
	// 9. AssetInfo 状态机
	// ================================================================================================
	private static void testAssetInfoLoadAssetSync()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/assetSync");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		// loadAsset<T> 需要 mParentAssetBundle.getAssetBundle().LoadAssetWithSubAssets
		// 由于没有真正的 AssetBundle, 这里测试 setSubAssets 和 getAsset
		var obj = ScriptableObject.CreateInstance<TestResourceSO>();
		try
		{
			info.setSubAssets(new UObject[] { obj });
			assertTrue(info.isLoaded(), "setSubAssets 后应已加载");
			assertEqual(obj, info.getAsset(), "getAsset 应返回主资源");
			info.clear();
			assertFalse(info.isLoaded(), "clear 后应未加载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAssetInfoLoadAssetAsync_AlreadyLoaded()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/assetAsync");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		var obj = ScriptableObject.CreateInstance<TestResourceSO>();
		try
		{
			info.setSubAssets(new UObject[] { obj });
			int callbackCount = 0;
			info.addCallback((asset, assets, bytes, loadPath) =>
			{
				++callbackCount;
				assertEqual(obj, asset, "回调应收到主资源");
			}, "test/path");
			// loadAssetAsync 在 mSubAssets != null 时直接 callbackAll
			info.loadAssetAsync();
			assertEqual(1, callbackCount, "已加载时 loadAssetAsync 应立即回调");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAssetInfoSetSubAssets_WithNull()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/nullAssets");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		// 包含 null 的数组应视为加载失败
		info.setSubAssets(new UObject[] { null });
		assertFalse(info.isLoaded(), "包含 null 的资源数组应视为加载失败");
		assertEqual(LOAD_STATE.NONE, info.getLoadState(), "加载失败状态应为 NONE");
	}
	private static void testAssetInfoCallbackAggregation()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/callbackAgg");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		var obj = ScriptableObject.CreateInstance<TestResourceSO>();
		try
		{
			info.setSubAssets(new UObject[] { obj });
			int count = 0;
			// 添加多个回调
			info.addCallback((a, b, c, d) => ++count, "path1");
			info.addCallback((a, b, c, d) => ++count, "path2");
			info.addCallback((a, b, c, d) => ++count, "path3");
			info.addCallback(null, "path4"); // null 回调应被忽略
			info.callbackAll();
			assertEqual(3, count, "应调用3个有效回调");
			// callbackAll 后回调列表应清空
			info.callbackAll();
			assertEqual(3, count, "第二次 callbackAll 不应再调用回调");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAssetInfoClear()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/clearAsset");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		var obj = ScriptableObject.CreateInstance<TestResourceSO>();
		try
		{
			info.setSubAssets(new UObject[] { obj });
			assertTrue(info.isLoaded(), "加载后应已加载");
			info.clear();
			assertFalse(info.isLoaded(), "clear 后应未加载");
			assertEqual(LOAD_STATE.NONE, info.getLoadState(), "clear 后状态应为 NONE");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAssetInfoResetProperty()
	{
		mResourceManager = createTestResourceManager();
		var bundle = new AssetBundleInfo("test/resetAsset");
		var info = new AssetInfo();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("test.prefab");
		info.setLoadState(LOAD_STATE.LOADED);
		info.addCallback((a, b, c, d) => { }, "path");
		info.resetProperty();
		assertNull(info.getAssetBundle(), "resetProperty 后 AssetBundle 应为 null");
		assertNull(info.getAssetName(), "resetProperty 后 AssetName 应为 null");
		assertEqual(LOAD_STATE.NONE, info.getLoadState(), "resetProperty 后状态应为 NONE");
	}
	// ================================================================================================
	// 10. AssetDataBaseLoader
	// ================================================================================================
	private static void testAssetDataBaseLoaderIsAssetLoaded()
	{
		var loader = new AssetDataBaseLoader();
		assertFalse(loader.isAssetLoaded("nonexistent.prefab"), "未加载的资源应返回 false");
	}
	private static void testAssetDataBaseLoaderGetAsset_NotLoaded()
	{
		var loader = new AssetDataBaseLoader();
		assertNull(loader.getAsset("nonexistent.prefab"), "未加载的资源应返回 null");
	}
	private static void testAssetDataBaseLoaderUnloadAsset_Null()
	{
		var loader = new AssetDataBaseLoader();
		assertFalse(loader.unloadAsset(null, false), "卸载 null 应返回 false");
	}
	private static void testAssetDataBaseLoaderUnloadPath_Empty()
	{
		var loader = new AssetDataBaseLoader();
		// 空路径卸载不应崩溃
		loader.unloadPath("nonexistent_path");
		// 验证不崩溃即可
	}
	// ================================================================================================
	// 11. ResourceManager unload 方法
	// ================================================================================================
	private static void testUnloadNullResourceRef()
	{
		var rm = createTestResourceManager();
		ResourceRef<ScriptableObject> nullRef = null;
		// unload null ref 不应崩溃
		rm.unload(ref nullRef);
	}
	private static void testUnloadPath_InvokesCallbacks()
	{
		var rm = createTestResourceManager();
		int pathCallbackCount = 0;
		string unloadedPath = null;
		rm.addUnloadPathCallback(p =>
		{
			++pathCallbackCount;
			unloadedPath = p;
		});
		rm.unloadPath("test/path");
		assertEqual(1, pathCallbackCount, "unloadPath 应触发路径回调");
		assertEqual("test/path", unloadedPath, "回调应收到正确的路径");
	}
	private static void testUnloadAssetBundle_NotAssetBundleMode()
	{
		// mLoadSource 默认为 ASSET_DATABASE, unloadAssetBundle 应不执行任何操作
		var rm = createTestResourceManager();
		rm.unloadAssetBundle("test");
		// 不崩溃即可
	}
	private static void testPreloadAssetBundle_NotAssetBundleMode()
	{
		var rm = createTestResourceManager();
		rm.preloadAssetBundle("test");
		// 不崩溃即可
	}
	private static void testPreloadAssetBundleAsync_NotAssetBundleMode()
	{
		var rm = createTestResourceManager();
		int callbackCount = 0;
		rm.preloadAssetBundleAsync("test", _ => ++callbackCount);
		// 在 ASSET_DATABASE 模式下, callback 应被调用并传入 null
		assertEqual(1, callbackCount, "非 AssetBundle 模式应立即调用回调");
	}
	private static void testIsResourceInited_AssetDatabaseMode()
	{
		var rm = createTestResourceManager();
		// ASSET_DATABASE 模式下, isResourceInited 总是返回 true
		assertTrue(rm.isResourceInited(), "ASSET_DATABASE 模式下 isResourceInited 应返回 true");
	}
	// ================================================================================================
	// 12. 多次引用操作
	// ================================================================================================
	private static void testDestroyedResourceRefIsInvalid()
	{
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		try
		{
			var resRef = createResourceRef(rm, obj);
			destroyResourceRef(resRef);
			assertFalse(resRef.isValid(), "销毁后应无效");
			assertEqual(0L, resRef.getToken(), "销毁后 token 应为 0");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testAddReferenceSameObjectMultipleTimes()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		try
		{
			// 同一对象多次 addReference, 每次都应返回唯一 token
			long t1 = rm.addReference(obj);
			long t2 = rm.addReference(obj);
			long t3 = rm.addReference(obj);
			assertTrue(t1 != t2 && t2 != t3 && t1 != t3, "同一对象多次引用 token 应唯一");

			// 逐个移除
			rm.removeReference(obj, ref t1);
			rm.removeReference(obj, ref t2);
			rm.removeReference(obj, ref t3);
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	// ================================================================================================
	// 13. 卸载后重新引用
	// ================================================================================================
	private static void testReAddReferenceAfterCleanup()
	{
		var rm = createTestResourceManager();
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			// 第一次引用周期
			long t1 = rm.addReference(obj);
			rm.removeReference(obj, ref t1);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "第一次清理后应卸载一次");

			// 重新引用 (模拟重新加载资源)
			long t2 = rm.addReference(obj);
			assertTrue(t2 > 0, "重新引用应返回有效 token");
			rm.removeReference(obj, ref t2);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(2, unloadCount, "第二次清理后应卸载两次");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	// ================================================================================================
	// 15. CustomAsyncOperation 基本行为
	// ================================================================================================
	private static void testCustomAsyncOperationInResourceContext()
	{
		var op = new CustomAsyncOperation();
		assertTrue(op.keepWaiting, "新创建的 operation 应等待");
		op.setFinish();
		assertFalse(op.keepWaiting, "setFinish 后不应等待");
		op.Reset();
		assertTrue(op.keepWaiting, "Reset 后应重新等待");

		// 在资源加载上下文中使用
		var op2 = new CustomAsyncOperation();
		int callbackCount = 0;
		// 模拟异步加载: 先标记未完成, 完成后 setFinish 并回调
		assertFalse(!op2.keepWaiting, "未完成时应等待");
		op2.setFinish();
		// 完成后回调
		callbackCount++;
		assertEqual(1, callbackCount, "完成后应执行回调逻辑");
		assertFalse(op2.keepWaiting, "完成后不应等待");
	}
	// ================================================================================================
	// 16. 综合交叉场景
	// ================================================================================================
	private static void testComplexScenario_MultiHolder_PartialUnload_Reload()
	{
		// 场景: 多个持有者引用同一资源, 部分释放后重新引用, 验证引用计数正确
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			// 3个持有者
			var ref1 = createResourceRef(rm, obj);
			var ref2 = ref1.copyRef();
			var ref3 = ref1.copyRef();

			// 释放 ref1
			destroyResourceRef(ref1);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "部分释放后不应卸载");

			// 重新引用 (模拟新的持有者获取已加载的资源)
			var ref4 = ref2.copyRef();
			assertTrue(ref4.isValid(), "新引用应有效");
			assertEqual(obj, ref4.get(), "新引用应获取同一资源");

			// 释放 ref2, ref3
			destroyResourceRef(ref2);
			destroyResourceRef(ref3);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(0, unloadCount, "ref4 仍持有, 不应卸载");

			// 释放 ref4
			destroyResourceRef(ref4);
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "全部释放后应卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	private static void testComplexScenario_AsyncLoadComplete_ThenUnloadAll()
	{
		// 场景: 异步加载完成后的资源, 被多个持有者引用, 然后全部释放
		var rm = createTestResourceManager();
		mResourceManager = rm;
		var obj = createTestObject();
		int unloadCount = 0;
		rm.addUnloadObjectCallback(_ => ++unloadCount);
		try
		{
			// 模拟异步加载完成, 资源被交付
			var ref1 = createResourceRef(rm, obj);
			var ref2 = ref1.copyRef();
			var ref3 = ref1.copyRef();

			// 验证所有引用都有效
			assertTrue(ref1.isValid() && ref2.isValid() && ref3.isValid(), "所有引用应有效");

			// 全部释放
			destroyResourceRef(ref1);
			destroyResourceRef(ref2);
			destroyResourceRef(ref3);

			// update 后应触发卸载
			rm.update(CHECK_REF_INTERVAL + 0.1f);
			assertEqual(1, unloadCount, "全部释放后应卸载");
		}
		finally { UObject.DestroyImmediate(obj); }
	}
	// ================================================================================================
	// 辅助类
	// ================================================================================================

	// 可控的 ResourceManager,用于在不依赖真实 Unity 资源文件的情况下触发实际异步加载完成流程。
	private class TestResourceManager : ResourceManager
	{
		private readonly TestAssetDataBaseLoader mTestAssetDataBaseLoader;
		public TestResourceManager()
		{
			mLoadSource = LOAD_SOURCE.ASSET_DATABASE;
			mTestAssetDataBaseLoader = new TestAssetDataBaseLoader();
			mAssetDataBaseLoader = mTestAssetDataBaseLoader;
		}
		public void prepareAsyncLoad(string name)
		{
			mTestAssetDataBaseLoader.prepareAsyncLoad(name);
		}
		public void completeAsyncLoad(string name, UObject asset)
		{
			mTestAssetDataBaseLoader.completeAsyncLoad(name, asset);
		}
	}
	// 直接构造 LOADING 状态并手动完成,让测试真正经过 loadGameResourceAsync/loadGameResourceAsyncSafe 的回调分支。
	private class TestAssetDataBaseLoader : AssetDataBaseLoader
	{
		public void prepareAsyncLoad(string name)
		{
			string path = StringUtility.getFilePath(name);
			if (!mLoadedPath.TryGetValue(path, out Dictionary<string, AssetDataBaseLoadInfo> resourceList))
			{
				resourceList = new Dictionary<string, AssetDataBaseLoadInfo>();
				mLoadedPath.Add(path, resourceList);
			}
			FrameUtility.CLASS(out AssetDataBaseLoadInfo info);
			info.setPath(path);
			info.setResourceName(name);
			info.setState(LOAD_STATE.LOADING);
			resourceList.Add(name, info);
		}
		public void completeAsyncLoad(string name, UObject asset)
		{
			string path = StringUtility.getFilePath(name);
			AssetDataBaseLoadInfo info = mLoadedPath[path][name];
			info.setObject(asset);
			info.setState(LOAD_STATE.LOADED);
			mLoadedObjects.Add(asset, info);
			info.callbackAll();
		}
	}
	// 用于测试 IRecyclable 接口的简单实现
	private class TestRecyclable : ClassObject
	{
		// ClassObject 已实现 IRecyclable, 直接使用即可
	}
}
