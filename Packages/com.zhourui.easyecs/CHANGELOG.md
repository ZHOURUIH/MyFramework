# Changelog

本文档记录 EasyECS 的主要版本变化。

## [1.5.0] - 2026-08-21

### Added

- 新增通用 Burst Chunk API。
- 新增 `DefaultBurstChunkSize`，当前默认值为 `8192`。
- 新增 `BurstView.GetChunkCount(...)`。
- 新增 `BurstView.GetChunkRange(...)`。
- 新增 `ScheduleBurstChunk<TJob>(...)`。
- 新增带 `JobHandle dependsOn` 的 `ScheduleBurstChunk<TJob>(...)` 重载。
- `ECSDictionary` 支持将 Chunk Job 调度转发到底层 Dense ECSList。
- Chunk 调度接入现有 EasyECS Burst Job 依赖管理。
- 增加 Chunk Tail 正确性验证。
- 增加 Burst Chunk 专项 Benchmark。

### Changed

- 正式性能架构调整为：

```text
Unsafe SoA
+ 64-byte Alignment
+ BurstView
+ Burst
+ Burst Auto Vectorization
+ Burst Chunk
```

- 将 Chunk 从原先 SIMD 相关概念中独立出来，成为通用 Burst 并行能力。
- 保留原有 `ScheduleBurst<TJob>`，不改变其逐元素 `Execute(index)` 语义。
- 明确区分 Unity `innerloopBatchCount` 与 EasyECS 逻辑 Chunk。
- 大规模连续批处理推荐优先使用 `ScheduleBurstChunk`。
- 热点字段访问继续推荐 Direct / Column / BurstView。
- EasyECS.Runtime 不再为了批处理 API 强制引用 Unity Burst；Burst 由实际业务 Job 按需使用。

### Removed

- 移除正式 Generator 中的显式 SIMD 专用 API：

```text
SIMDAdd
SIMDSubtract
SIMDMultiply
SIMDDivide
SIMDMad
SIMDFill
ParallelSIMD*
ScheduleSIMD*
ScheduleBurstSIMD
GetSIMDChunkCount
GetSIMDChunkRange
```

- 移除 Generator 中 SIMD TypeKind、SIMD MethodSuffix、SIMD Kernel 等专用生成逻辑。
- 移除 `__SIMDKernels`。
- 移除针对 AVX / AVX2 / SSE / NEON / FMA 的显式 Intrinsics 实验路径。
- 移除为 SIMD Kernel 单独维护的 Burst `OptimizeFor` 生成逻辑。

### Performance

最终 Burst Chunk Benchmark：

```text
Unity:       6000.3.21f1
EntityCount: 2,000,000
ChunkSize:   8192
```

结果：

| Test | Burst Single | Element Parallel | Chunk Container | Chunk / Single | Chunk / Element |
|---|---:|---:|---:|---:|---:|
| Float MAD | 0.205 ms | 0.079 ms | 0.075 ms | 2.740x | 1.055x |
| Int Add | 0.204 ms | 0.068 ms | 0.043 ms | 4.766x | 1.585x |
| Vector2 MAD | 0.413 ms | 0.097 ms | 0.076 ms | 5.454x | 1.280x |
| Color32 Add | 0.204 ms | 0.074 ms | 0.044 ms | 4.597x | 1.655x |

几何平均：

```text
Chunk Direct / Single         : 4.029x
Chunk Container / Single      : 4.254x
Chunk Container / Element     : 1.372x
```

显式 SIMD 最终验证结果约为：

```text
Explicit SIMD / Burst Auto ≈ 1.00x
```

因此 1.5.0 不再继续维护显式 SIMD Intrinsics，统一依赖 Burst 自动向量化。

### Fixed

- 修正 Chunk API 从 SIMD 命名迁移后的生成结果。
- 修正 Built-in ECSList Benchmark 对 `EasyECS` 命名空间的引用。
- 清理旧 SIMD Benchmark 与正式 API 之间的耦合。
- 验证 `GetChunkCount / GetChunkRange / ScheduleBurstChunk / Tail` 正确性。
- 验证 Runtime Unit Test 在 1.5.0 架构下完整通过。

### Compatibility

- 原有基础 ECSList / ECSDictionary 使用方式保持不变。
- 原有 `ScheduleBurst<TJob>` 保留。
- Unsafe / Direct / Column / BurstView / Job 依赖管理继续保留。
- 1.5.0 删除 SIMD 专用 API，因此使用过实验阶段 `SIMD*` API 的代码需要迁移到普通 Burst 循环或 Burst Chunk。

推荐迁移：

```text
SIMD* 批处理
→ BurstView + 普通连续循环

ParallelSIMD* / ScheduleSIMD*
→ IJobParallelFor + ScheduleBurstChunk
```

## Release Summary

EasyECS 1.5.0 的核心变化不是继续增加 SIMD 指令层，而是确定最终性能路线：

```text
数据布局交给 EasyECS
向量化交给 Burst
多核批处理交给 Chunk
```

该版本作为当前 Burst / Jobs / Chunk 性能架构的稳定基线。

## 1.4.0 - 2026-08-19

### Added
- 增加 BuiltIn 基础类型 ECS 容器，不再要求用户为了常用值类型额外声明简单包装 `[ECS] struct`。
- 增加 `<TypeName>_ECSList` 命名的 BuiltIn List，例如 `Int_ECSList`、`Vector2_ECSList`、`Vector2Int_ECSList`、`Color32_ECSList`。
- 增加 `<TypeName>_ECSDictionary<TKey>` 命名的 BuiltIn Dictionary，例如 `Int_ECSDictionary<TKey>`、`Vector2_ECSDictionary<TKey>`。
- BuiltIn 类型由 Source Generator 的内置元数据直接生成，不引入 `ECSBuiltinXXX` 隐藏包装 struct，也不增加 Runtime 泛型容器基类。
- 当前 BuiltIn 覆盖：`Byte`、`SByte`、`Short`、`UShort`、`Int`、`UInt`、`Long`、`ULong`、`Float`、`Double`、`Bool`、`Char`、`Decimal`、`Vector2`、`Vector2Int`、`Vector3`、`Vector3Int`、`Vector4`、`Quaternion`、`Color`、`Color32`、`Rect`、`RectInt`、`Bounds`、`BoundsInt`、`Matrix4x4`。
- BuiltIn ECSList 提供接近 `List<T>` 的普通使用方式，并复用 EasyECS 已有的 Add / AddRange / Insert / Remove / Range / Sort / BinarySearch / CopyTo / ToArray / Capacity 等能力。
- BuiltIn Dictionary 采用 Key -> DenseIndex -> BuiltIn ECSList 的连续存储结构，并提供 `GetIndex` / `TryGetIndex` / `GetOrAddIndex`、Direct Column 以及可用时的 Burst API。
- 为 BuiltIn 增加专门的 Runtime correctness、普通 parity、Burst parity 与 Generator Test 覆盖。

### SoA Layout
- `Vector2` 拆分为 `float x[] + float y[]`。
- `Vector2Int` 拆分为 `int x[] + int y[]`。
- `Vector3` / `Vector3Int` 分别拆分为 3 个 float / int Column。
- `Vector4` / `Quaternion` 拆分为 `x/y/z/w` 四个 float Column。
- `Color` 拆分为 `r/g/b/a` 四个 float Column，`Color32` 拆分为四个 byte Column。
- `Rect` / `RectInt` 拆分为 `x/y/width/height` 标量 Column。
- `Bounds` 拆分为 `centerX/centerY/centerZ + sizeX/sizeY/sizeZ` 六个 float Column。
- `BoundsInt` 拆分为 `positionX/positionY/positionZ + sizeX/sizeY/sizeZ` 六个 int Column。
- `Matrix4x4` 拆分为 `m00 ... m33` 共 16 个 float Column。
- 普通 API 仍然以原始 Unity 类型读写，Direct / BurstView 则直接访问拆分后的标量 Column。

### Performance
- 单标量 BuiltIn 增加直接底层 Column 快路径，避免不必要的完整 value 重建和通用字段访问链。
- 单标量 BuiltIn indexer 可直接引用底层连续存储，普通赋值/读取语法保持自然，同时减少 compound read/write 开销。
- `Contains` / `IndexOf` / `LastIndexOf` 对单标量 BuiltIn 直接扫描底层 Column；可使用强类型 `Equals(T)` 时避免通用比较器额外分发。
- `BinarySearch` 对单标量 BuiltIn 使用直接 Column 二分路径。
- 单标量 `AddRange(T[])` / `CopyTo` 保留整段 MemoryCopy / Array.Copy 快路径。
- 复合 BuiltIn 的 `AddRange(T[])` 使用一次源数组扫描同时写入全部标量 Column，避免按字段多次重复扫描 AoS 源数组。
- `Vector2`、`Vector2Int`、`Color32` 的 Direct SoA 路径已与等价手写拆字段 `[ECS] struct` 进行 parity 验证；批量导入与部分搜索路径在当前 Benchmark 中优于通用手写基线。
- 对微秒级 Insert / Resize 等低频结构操作保留简单、统一的生成结构，不为了少量绝对时间差继续增加 BuiltIn 特殊分支；已知规模仍建议优先预留 Capacity。

### Burst / Jobs
- BuiltIn 在 Unsafe Backend 下复用 EasyECS 原有零拷贝 `BurstView` / `ScheduleBurst` / Job dependency 跟踪机制。
- 复合 BuiltIn 的 BurstView 直接暴露标量指针，例如 `Vector2_ECSList.BurstView` 为独立 `float* x` / `float* y`，可只处理实际需要的 Column。
- BuiltIn Dictionary 在可用环境下将 Burst API 转发到内部 Dense BuiltIn ECSList。
- Jobs 能力检测不再错误依赖可选的 `NativeDisableUnsafePtrRestrictionAttribute`；该属性不可见时仍可生成可用的 Jobs/Burst 集成，只是不输出对应可选属性。

### Compatibility
- `EasyECS.Runtime` 保持 `allowUnsafeCode: true`，使 Package 内 BuiltIn 默认获得 Unsafe native SoA / Direct pointer / BurstView 等最高性能能力。
- 业务程序集不需要开启 `allowUnsafeCode` 即可正常使用 `Int_ECSList`、`Vector2_ECSList` 等 BuiltIn 容器。
- 用户自己声明的 `[ECS] struct` 仍按所在程序集能力自动选择 Unsafe / SafeSpan / SafeRegistry，不改变 EasyECS 原有 fallback 原则。
- Burst / Jobs 继续是可选能力；缺少 Burst 不影响 BuiltIn 和普通 EasyECS API 的使用。
- 修改 Source Generator 后仍需重新编译并替换 `Analyzers/ECSGenerator.dll`。

### Fixed
- 修复 BuiltIn Burst API 在 Runtime assembly 未开启 Unsafe 时被降级、无法生成期望 BurstView 的集成问题；正式 Runtime asmdef 明确开启 Unsafe。
- 修复 Jobs 检测把可选 pointer attribute 当成必需条件，导致部分环境错误抑制 Burst/Jobs API 的问题。
- 修复 BuiltIn 单标量 value indexer 在 compound read/write 中存在不必要 get + set 路径的问题。
- 优化 BuiltIn `SetValue` / `TrySetValue` 等 Dictionary 写路径，避免不必要的完整 value 中转。
- Runtime Unit Test 在 1.4.0 封版验证中保持 `77/77` 通过。

### Recommended usage
- 普通基础类型集合：优先直接使用 `<Type>_ECSList` / `<Type>_ECSDictionary<TKey>`，不再创建只有一个简单字段的包装 struct。
- `Vector2` / `Vector3` / `Color32` 等连续热点：优先获取拆分后的 Direct Column，只访问实际需要的字段。
- 普通、偶发访问继续使用 List / Dictionary 风格 API。
- 单字段随机热点：自定义 `[ECS] struct` 继续优先 `ByXXX`；BuiltIn 直接使用其原生 value API / Direct Column。
- 多字段随机热点：优先 DenseIndex + Direct Column。
- 大规模可并行纯数据计算：优先 BuiltIn 或普通 ECS 的 `BurstView + IJobParallelFor`。

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
