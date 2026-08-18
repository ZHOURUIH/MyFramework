# Changelog

本文档记录 EasyECS 的主要版本变化。

## 1.3.0 - 2026-08-18

### Added
- 增加可选 Burst Integration。只有当前 Unity 编译环境能够解析 Burst / Jobs 相关类型时，Source Generator 才会生成 Burst API；未安装 Burst 的项目继续使用原有 EasyECS，不增加强制依赖。
- Unsafe 后端为 Burst-compatible ECS 字段生成 `BurstView`，直接暴露原生 SoA Column 指针与 `Count`，不进行数据复制。
- Managed Hybrid 支持部分 Burst：可 Burst 的 unmanaged ECS 字段进入 `BurstView`，`string`、`object` 等 managed 字段继续走原 EasyECS 路径。
- 增加 `ScheduleBurst<TJob>`，用于直接调度 `IJobParallelFor`，并自动串联当前 ECSList 已登记的 Burst Job 依赖。
- 增加 `GetBurstDependency()`，允许外部 Job 获取当前 EasyECS Burst 依赖。
- 增加 `RegisterBurstJob(JobHandle)`，允许外部自行 Schedule 后重新交给 EasyECS 跟踪生命周期。
- 增加 `CompleteBurstJobs()`，用于显式等待当前 ECSList 已登记的 Burst Job。
- `ECSDictionary<TKey>` 在 Unsafe + Burst-compatible 数据下同步开放 `GetBurstView`、`ScheduleBurst`、`GetBurstDependency`、`RegisterBurstJob`、`CompleteBurstJobs`。
- 增加 `RoleDataBurstBenchmark`，对比 EasyECS Direct C#、Burst `IJob`、Burst `IJobParallelFor`，并验证 Burst 原地修改 Column、Job 跟踪和 Resize 自动 Complete。
- Generator Test 增加 Burst 不存在、Unsafe BurstView、Managed Hybrid 字段过滤、Safe 后端 fallback、Burst API 编译等覆盖。

### Performance
- 50 万 `RoleData` 连续数值更新实测：
  - EasyECS Direct C#：`0.264 ms`
  - EasyECS Burst `IJob`：`0.363 ms`
  - EasyECS Burst `IJobParallelFor`：`0.066 ms`
- 在该测试场景中，Burst `IJobParallelFor` 相比 EasyECS Direct C# 约快 `4.01x`。
- 单线程 Burst `IJob` 在该轻量测试中反而慢于 Direct C#，因此 1.3.0 的推荐策略是：小规模/轻量循环继续使用 Direct，大规模可并行纯数据计算优先使用 Burst `IJobParallelFor`。
- Burst 直接处理 EasyECS 原有 native SoA Column，不维护第二份数据，也不需要 ECS → NativeArray → ECS 的来回复制。

### Safety
- Unsafe Burst Job 直接持有 EasyECS native Column 指针，因此 Job 运行期间不得让相关 Column 地址失效。
- EasyECS 会跟踪通过 `ScheduleBurst` / `RegisterBurstJob` 登记的 Job。
- `resize` 和 `Dispose` 在迁移或释放 native memory 前会自动完成已登记的 Burst Job，避免 Job 持有失效指针。
- Burst Job 运行期间不要同时使用普通 EasyECS API 对同一批数据执行冲突读写、Insert、Remove、Sort 等操作；切回普通访问前应调用 `CompleteBurstJobs()`。
- 使用外部自行 Schedule 的 Job 时，必须通过 `GetBurstDependency()` 串联已有依赖，并通过 `RegisterBurstJob(handle)` 登记回 EasyECS；未登记的裸 Job 无法获得 EasyECS 生命周期保护。
- `Dispose()` 的正常主线程路径会先完成已登记 Job 再释放 native memory。
- Finalizer 不会调用 Job System API。若对象在仍存在 pending Burst Job 时进入 Finalizer，为避免后台 Job 访问已释放内存，会保守地不释放对应 native pointers；这属于未正确 Dispose 的误用。
- SafeSpan / SafeRegistry 不增加 NativeArray 中转，不强行使用 Burst，继续保持原有 EasyECS 路径。

### Compatibility
- Burst 不是 EasyECS 的强制依赖。
- 没有 Burst 的项目仍可完整使用 Unsafe、SafeSpan、SafeRegistry、Direct、ByXXX、DenseIndex 等原有功能。
- Burst API 只在 Unsafe 后端且存在至少一个 Burst-compatible ECS 字段时生成。
- `string`、`object`、`char`、`decimal` 等不进入 `BurstView`；支持的基础数值类型、枚举、指针以及由支持字段组成的 unmanaged struct 可进入 BurstView。
- 修改 Source Generator 后必须重新编译并替换 `Analyzers/ECSGenerator.dll`，Unity 才会使用新的生成逻辑。

### Fixed
- 修复生成的 `BurstView` 构造函数访问级别过低，导致外层 `GetBurstView()` 调用时报 `CS0122` 的问题。
- 清理旧 Benchmark 对已删除 `KeyEnumerationStrategy` 诊断常量的引用，避免正式 API 被阶段性 Benchmark 字符串污染。
- 保持 Runtime Test `74/74` 全部通过。
- Burst Correctness Test 已验证：原地 Column 修改、Job 跟踪、Resize 自动 Complete 均正常。

### Recommended usage
- 普通业务访问：使用现有 List / Dictionary 风格 API。
- 单字段热点随机访问：优先 `ByXXX`。
- 多字段随机热点访问：优先 `GetIndex` / `TryGetIndex` / `GetOrAddIndex` + Direct Column。
- 连续热点遍历：优先 Direct Column。
- 大规模、纯数据、可并行计算：优先 `BurstView + IJobParallelFor`。
- Managed / UnityEngine.Object / 非 Burst-compatible 数据：继续使用原 EasyECS 路径。

## 1.2.0 - 2026-08-18

### Added
- 为 `ECSList` 补齐常用 List 风格扩展 API，包括容量管理、批量添加/插入/删除、查找、排序、二分查找、复制与数组转换等。
- 为真实 ECS 字段生成字段级高速 API：`ContainsByXXX`、`IndexOfByXXX`、`LastIndexOfByXXX`、`ExistsByXXX`、`FindIndexByXXX`、`RemoveAllByXXX`、`SortByXXX`、`BinarySearchByXXX`。
- 为 `ECSDictionary<TKey>` 增加字段级直接访问 API：`GetValueByXXX`、`TryGetValueByXXX`、`SetValueByXXX`、`TrySetValueByXXX`。
- 为 `ECSDictionary<TKey>` 增加 DenseIndex API：`GetIndex`、`TryGetIndex`、`GetOrAddIndex`，用于一次 Key 查找后通过 Direct Column 连续访问多个字段。
- 为 `ECSDictionary<TKey>` 补充 `SetValue`、`TrySetValue`、`SetOrAdd`、`GetOrAdd`、`ContainsValue`、`Remove(key,out value)`、`EnsureCapacity`、`TrimExcess` 等接口。
- 增加 Dictionary Keys / Values / KeyValue 枚举支持。

### Changed
- `SortByXXX` 根据数据布局自适应选择排序实现：纯 unmanaged 数据使用 DirectSwap；包含 managed ECS / NotECS 字段时使用 permutation 排序，减少整行反复搬移。
- `BinarySearchByXXX` 和字段级查找 API 直接读取目标 Column，避免构造完整结构体。
- `RemoveAllByXXX` 使用字段 Column 判断并同步压缩整行数据。
- `InsertRange`、`RemoveRange` 等结构移动使用 overlap-safe 批量移动路径。
- Managed Hybrid 的 `T[] -> ECSList` 使用 RowMajor 导入。
- Managed Hybrid 的 `ECSList -> T[]` 使用 CachedDirectDestination，缓存 Column 后直接写目标数组元素。
- `ECSDictionary.TryAdd` 使用单次 `Dictionary.TryAdd` 路径，消除 `ContainsKey + Add` 的重复哈希。
- `ECSDictionary.Remove` / `Remove(key,out value)` 使用单次 remove-out 路径，减少重复 Key 查找。

### Performance
- `SetValueByXXX` / `TrySetValueByXXX` 在单字段写入场景避免完整 struct 读改写。
- `GetIndex + Direct Column` 在一次 Key 查找后访问多个字段，显著优于连续多次 `ByXXX` 查找。
- `GetOrAddIndex` 已有 Key 路径接近普通 Dictionary 查询成本。
- `CopyTo` / `ToArray` 的剩余成本主要来自 SoA → AoS 的完整结构体重组。

### Fixed
- 修复 Unsafe Dictionary 字段级 API 缺少 `unsafe` 上下文导致的 `CS0214`。
- 修复 Managed Hybrid 数组转换 Generator Test 的类型名称断言问题。
- 修复批量移动源/目标区间重叠时可能产生的数据覆盖。
- 保证 Sort、RemoveAll、Range 操作、Dictionary SwapBack 删除后各 ECS / NotECS 字段保持行同步。

### Cleanup
- 移除优化阶段仅用于诊断的公开 `*Strategy` 字符串常量。
- Generator Test 改为验证真实生成结构和行为，不依赖诊断字符串。
- Runtime Test 保留行为正确性验证。
